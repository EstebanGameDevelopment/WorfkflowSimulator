import os
import subprocess
import multiprocessing
import time
from flask import Flask, request, jsonify
import hashlib
import base64
import time
from celery import Celery
from celery.app.control import Control
from celery.utils.log import get_task_logger
import redis
import json
import requests
from flask import Flask, request, jsonify
import openai

SECRET_BASE_KEY = "WBj5172128t"
SECRET_KEY = "GEw8129312uNCSOP2829taWGkhc"

class ScreenManager:
    def __init__(self, redis_client):
        self.TIME_WINDOW = 600  # 10(600) minutes: Time window for verification
        self.TOTAL_SESSION_SLOTS = 70
        self.env = os.environ.copy()
        self.env["PATH"] = "/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin:/usr/games:/usr/local/games:/snap/bin"
        self.redis_client = redis_client
        self.messages_key = "messages_list"  # Key for storing message data

    def check_total_session_limit(self):
        return self.count_sessions_reddis() > self.TOTAL_SESSION_SLOTS
        
    def generate_timestamp(self):
        return str(int(time.time()))

    def count_sessions_reddis(self):
        messages = self.redis_client.lrange(self.messages_key, 0, -1)
        return len(messages)

    def generate_combined_salt(self, salt, timestamp):
        # Combine salt and timestamp
        salt_with_timestamp = f"{salt}{timestamp}"
        # Hash the combination
        hashed = hashlib.sha256(salt_with_timestamp.encode('utf-8')).digest()
        return base64.b64encode(hashed).decode('utf-8')

    def hash_with_salt(self, input_str, combined_salt):
        # Hash the input with the combined salt
        combined = (input_str + combined_salt).encode('utf-8')
        hashed = hashlib.sha256(combined).digest()
        return base64.b64encode(hashed).decode('utf-8')

    def verify_user_hash(self, user_id, provided_hash, original_salt, timestamp):
        # Validate the timestamp
        current_time = int(time.time())
        if abs(current_time - int(timestamp)) > self.TIME_WINDOW:
            return False  # Timestamp expired

        # Recompute combined salt and hash
        recomputed_salt = self.generate_combined_salt(original_salt, timestamp)
        computed_hash = self.hash_with_salt(user_id, recomputed_salt)
        return computed_hash == provided_hash    

    def get_max_id_message_data(self):
        """Find and return the message with the maximum unique_id."""
        messages = self.get_all_messages()
        if not messages:
            return None  # Return None if the list is empty

        # Find the message with the maximum unique_id
        max_message = max(messages, key=lambda msg: msg["identification"])
        return max_message        
            
    def get_all_messages(self):
        """Retrieve all messages from Redis."""
        # Get the list of messages and decode them
        messages = self.redis_client.lrange(self.messages_key, 0, -1)
        return [json.loads(msg.decode()) for msg in messages]
        
    def get_total_time(self, session_name):
        """Return the total time for a specific session_name."""
        messages = self.get_all_messages()
        for message in messages:
            if message["session_name"] == session_name:
                return message["total_time"]
        return None  # Return None if the session_name is not found

    def get_start_time(self, session_name):
        """Return the start time for a specific session_name."""
        messages = self.get_all_messages()
        for message in messages:
            if message["session_name"] == session_name:
                return message["start_time"]
        return None  # Return None if the session_name is not found

    def get_celery_task_id(self, session_name):
        """Return the celery task id for a specific session_name."""
        messages = self.get_all_messages()
        for message in messages:
            if message["session_name"] == session_name:
                return message["task_id"]
        return None  # Return None if the session_name is not found

    def set_start_time(self, session_name, new_start_time):
        """Update the total time for a specific session_name."""
        messages = self.get_all_messages()
        updated_messages = []
        updated = False

        for message in messages:
            if message["session_name"] == session_name:
                message["start_time"] = new_start_time
                updated = True
            updated_messages.append(message)

        # Overwrite the list in Redis if the session_name was found
        if updated:
            self.redis_client.delete(self.messages_key)
            for msg in updated_messages:
                self.redis_client.rpush(self.messages_key, json.dumps(msg))
        return updated  # Return True if updated, False otherwise

    def exists(self, session_name):
        """Check if a session_name exists in the list."""
        messages = self.get_all_messages()
        return any(message["session_name"] == session_name for message in messages)
            
    def remove_redis_message(self, session_name):
        # Remove the message from the list
        messages = self.get_all_messages()
        updated_messages = [m for m in messages if m["session_name"] != session_name]
        
        # Overwrite the messages list in Redis
        self.redis_client.delete(self.messages_key)
        for msg in updated_messages:
            self.redis_client.rpush(self.messages_key, json.dumps(msg))    
            
    def create_session(self, session_name, script_path, timeout_seconds):
        if self.exists(session_name):
            return {"error": f"Session '{session_name}' already exists."}

        max_item_id = self.get_max_id_message_data()
        if max_item_id is None:
            max_id = 1
        else:
            max_id = int(max_item_id["identification"]) + 1
        
        final_port = 9001 + max_id
            
         # Start the screen session
        subprocess.run([
            "screen", "-dmS", session_name, 
            "bash", "-c", f"/home/sammy/workflowsimulator/myprojectenv/bin/python {script_path} --port {final_port} > session.log 2>&1"
        ], env=self.env)

        # Schedule the task to delete the message
        result_celery = delete_message.apply_async((session_name,))

        message_data = {
            "identification": max_id,
            "session_name": session_name,
            "start_time": time.time(),
            "total_time": timeout_seconds,
            "task_id": result_celery.id 
        }
        # Append the message data to the list in Redis
        self.redis_client.rpush(self.messages_key, json.dumps(message_data))
    
        return {"session_name": session_name, "port_number": final_port}

    def destroy_session(self, session_name):
        """Terminate a screen session."""
        if not self.exists(session_name):
            return False

        # Terminate the screen session
        subprocess.run(["screen", "-S", session_name, "-X", "quit"], env=self.env)
   
        # Remove from the redis session manager
        self.remove_redis_message(session_name)
        
        return True

    def destroy_all_session(self):
        messages = self.get_all_messages()
        for message in messages:          
            try:
                # Terminate the screen session
                subprocess.run(["screen", "-S", message["session_name"], "-X", "quit"], env=self.env)
            except subprocess.CalledProcessError:
                print("No active screen sessions found or pkill command failed.")
            except Exception as e:
                print(f"An error occurred: {e}")
            
        # Remove all sessions
        self.redis_client.delete(self.messages_key)
        
        return True

    def refesh_time_to_session(self, session_name, additional_seconds):
        """Refresh the timeout for a session."""
        if not self.exists(session_name):
            return {"error": f"Session '{session_name}' does not exist."}

        self.set_start_time(session_name, time.time())
        return {
            "session_name": session_name,
            "start_time": self.get_start_time(session_name),
        }

# Flask app to manage sessions
app = Flask(__name__)

# Configure Flask app and Celery
app.config['CELERY_BROKER_URL'] = 'redis://localhost:6379/0'
app.config['CELERY_RESULT_BACKEND'] = 'redis://localhost:6379/0'

celery = Celery(app.name, broker=app.config['CELERY_BROKER_URL'])
celery.conf.update(app.config)
logger = get_task_logger(__name__)

redis_client = redis.StrictRedis(host="localhost", port=6379, db=0)

# Initialize the ScreenManager
screen_manager = ScreenManager(redis_client)

@celery.task(bind=True)
def delete_message(self, session_name):
    should_run = True    
    while should_run:
        time.sleep(30)
        
        if not screen_manager.exists(session_name):
            should_run = False
            return

        session_task_id = screen_manager.get_celery_task_id(session_name)
        if session_task_id != self.request.id:
            should_run = False
            return
        
        end_time = screen_manager.get_total_time(session_name)
        elapsed_time = time.time() - screen_manager.get_start_time(session_name)

        if elapsed_time >= end_time:
            if should_run:
                should_run = False
                headers = {"Authorization": "Bearer WBj5172128t"}
                response = requests.post(
                    "https://www.workflowsimulator.site/delete_local_session",
                    json={"session_name": session_name},
                    headers=headers
                )        
                if response.status_code == 200:
                    print(f"Message {session_name} removed successfully via the service.")
                else:
                    print(f"Failed to remove message {session_name}: {response.text}")            
                return            
        
@app.route("/create_session", methods=["POST"])
def create_session():
    """Endpoint to create a new screen session."""
    data = request.json
    session_name = data.get("session_name")
    script_path = data.get("script_path")
    timeout_seconds = data.get("timeout_seconds", 60)
    salt = data.get("salt")
    user_hash = data.get("user_hash")
    timestamp = data.get("timestamp")

    # Verify the hash    
    if screen_manager.verify_user_hash(session_name, user_hash, salt, timestamp) is False:
        return jsonify({"error": "User verification invalid"}), 400

    # Check reached the limit of total number of session    
    if screen_manager.check_total_session_limit():
        return jsonify({"error": "Session limit reached"}), 400

    if screen_manager.exists(session_name):
        screen_manager.destroy_session(session_name)

    return jsonify(screen_manager.create_session(session_name, script_path, timeout_seconds))
	
@app.route("/refresh_time_session", methods=["POST"])
def refresh_time_session():
    """Endpoint to refresh time to an existing session."""
    data = request.json
    session_name = data.get("session_name")
    additional_seconds = data.get("additional_seconds")
    salt = data.get("salt")
    user_hash = data.get("user_hash")
    timestamp = data.get("timestamp")
    
    # Verify the hash    
    if screen_manager.verify_user_hash(session_name, user_hash, salt, timestamp) is False:
        return jsonify({"error": "User verification invalid"}), 400

    if not isinstance(additional_seconds, (int, float)) or additional_seconds <= 0:
        return jsonify({"error": "additional_seconds must be a positive number"}), 400

    result = screen_manager.refesh_time_to_session(session_name, additional_seconds)
    return jsonify(result)

@app.route("/destroy_session", methods=["POST"])
def destroy_session():
    """Endpoint to destroy a screen session."""
    data = request.json
    session_name = data.get("session_name")
    salt = data.get("salt")
    user_hash = data.get("user_hash")
    timestamp = data.get("timestamp")
    
    # Verify the hash    
    if screen_manager.verify_user_hash(session_name, user_hash, salt, timestamp) is False:
        return jsonify({"error": "User verification invalid"}), 400
    
    result = screen_manager.destroy_session(session_name)
    return jsonify(result)

@app.route('/delete_local_session', methods=['POST'])
def decrease_message():
    data = request.json
    session_name = data.get('session_name')
        
    key = request.headers.get('Authorization')
    if key != f"Bearer {SECRET_BASE_KEY}":
        abort(403)  # Forbidden access
        
    # Call decrease_number
    if screen_manager.destroy_session(session_name):
        return jsonify({"success": True, "message": f"Message {session_name} removed successfully."})
    else:
        return jsonify({"success": True, "message": f"ERROR TO LOCALLY REMOVE {session_name}"})

@app.route("/session_exists/<session_name>", methods=["GET"])
def session_with_param(session_name):
    key = request.headers.get('Authorization')
    if key != f"Bearer {SECRET_KEY}":
        abort(403)  # Forbidden access

    output = "<html><body>"
    output += "<h1 style='color:blue'>Get session["+str(session_name)+"].</h1>"
    output += "<p>"
    if screen_manager.exists(session_name):
        output += "Session["+session_name+"] EXISTS+++"
    else:
        output += "Session["+session_name+"] DOES NOT EXIST-----"
    output += "<p>"
    output += summary_sessions()
    output += "</body></html>"
    return output    

@app.route("/session_deletes/<session_name>", methods=["GET"])
def session_with_deletes(session_name):
    key = request.headers.get('Authorization')
    if key != f"Bearer {SECRET_KEY}":
        abort(403)  # Forbidden access

    output = "<html><body>"
    output += "<h1 style='color:red'>Delete session["+str(session_name)+"].</h1>"
    output += "<p>"
    if screen_manager.exists(session_name):
        if screen_manager.destroy_session(session_name):
            output += "Session["+session_name+"] destroyed succesfully+++++"
        else:
            output += "Session["+session_name+"] HAS NOT BEEN DELETED---"
    else:
        output += "Session["+session_name+"] DOES NOT EXIST-----"
    output += "<p>"
    output += summary_sessions()
    output += "</body></html>"
    return output

@app.route("/session_delete_all", methods=["GET"])
def session_delete_all():
    key = request.headers.get('Authorization')
    if key != f"Bearer {SECRET_KEY}":
        abort(403)  # Forbidden access

    screen_manager.destroy_all_session()

    output = "<html><body>"
    output += "<h1 style='color:red'>Delete all sessions.</h1>"
    output += "<p>"
    output += summary_sessions()
    output += "</body></html>"
    return output
    
def summary_sessions():
    output = "<h1 style='color:blue'>Total sessions "+str(screen_manager.count_sessions_reddis())+"</h1>"
    output += "<p>"
    messages = screen_manager.get_all_messages()
    for message in messages:
        output += str(message)
        output += "<br>"    
    return output

@app.route("/tasks", methods=["GET"])
def list_tasks():
    key = request.headers.get('Authorization')
    if key != f"Bearer {SECRET_KEY}":
        abort(403)  # Forbidden access

    # Get active tasks using Celery inspect
    i = celery.control.inspect()
    active_tasks = i.active()  # Returns tasks in each worker

    tasks_list = []

    output = "<html><body>"
    if active_tasks is None:
        output += "<h1 style='color:blue'>Tasks Empty</h1>"
    else:
        key = 'celery@workflowsimulator'
        elements = active_tasks.get(key, [])  # Use .get() to safely access the key
        # Check if the list is empty
        if not elements:
            output += "<h1><b>No elements found.</b></h1><br>"
        else:
            output += "<h1 style='color:blue'>Tasks ["+str(len(elements))+"]</h1><p>"
            
            for i, element in enumerate(elements, start=1):
                output += f"Element {i}:<br>"
                for prop, value in element.items():
                    output += f"  {prop}: {value} <br>"
                output += "<br>"            

    output += "<p>"
    output += "</body></html>"

    return output

@app.route('/validate-api-key', methods=['POST'])
def validate_api_key():
    data = request.json
    provider = data.get("provider", 0)
    api_key = data.get("api_key")

    if not api_key:
        return jsonify({"valid": False, "error": "No API key provided"}), 400

    if provider == 0:#OPEN AI API KEY
        client = openai.OpenAI(api_key=api_key)  # New OpenAI client format
        try:
            response = client.models.list()  # Correct method for OpenAI >=1.0.0
            return jsonify({"valid": True})
        except openai.AuthenticationError:
            return jsonify({"valid": False, "error": "Invalid API key"}), 401
        except Exception as e:
            return jsonify({"valid": False, "error": str(e)}), 500

    elif provider == 1:  # MISTRAL API KEY
        headers = { "Authorization": f"Bearer {api_key}" }

        response = requests.get("https://api.mistral.ai/v1/models", headers=headers)

        if response.status_code == 200:
            return jsonify({"valid": True})
        else:
            return jsonify({"valid": False, "error": "Invalid API key"}), 401

    elif provider == 2:  # DEEPSEEK
        headers = { "Authorization": f"Bearer {api_key}" }

        response = requests.get("https://api.deepseek.com/v1/models", headers=headers)

        if response.status_code == 200:
            return jsonify({"valid": True})
        else:
            return jsonify({"valid": False, "error": "Invalid API key"}), 401

    elif provider == 3:  # GEMINI	
        url = f"https://generativelanguage.googleapis.com/v1/models?key={api_key}"

        response = requests.get(url)

        if response.status_code == 200:
            return jsonify({"valid": True})
        else:
            return jsonify({"valid": False, "error": "Invalid API key"}), 401

    elif provider == 4:  # OPENROUTER
        headers = { "Authorization": f"Bearer {api_key}" }
        response = requests.get("https://openrouter.ai/api/v1/models", headers=headers)

        if response.status_code == 200:
            return jsonify({"valid": True})
        else:
            return jsonify({"valid": False, "error": "Invalid API key"}), 401

    elif provider == 5:  # Stability
        headers = {"Authorization": f"Bearer {api_key}"}

        response = requests.get("https://api.stability.ai/v1/engines/list", headers=headers)

        if response.status_code == 200:
            return jsonify({"valid": True})
        else:
            return jsonify({"valid": False, "error": "Invalid API key"}), 401
            
    elif provider == 6:  # GROK API KEY
        headers = { "Authorization": f"Bearer {api_key}" }

        response = requests.get("https://api.x.ai/v1/models", headers=headers)

        if response.status_code == 200:
            return jsonify({"valid": True})
        else:
            return jsonify({"valid": False, "error": "Invalid API key"}), 401

@app.route("/")
def hello():
    key = request.headers.get('Authorization')
    if key != f"Bearer {SECRET_KEY}":
        abort(403)  # Forbidden access

    output = "<html><body>"
    output += "<h1 style='color:blue'>WorkflowSimulator AI Screen Server Online!!! Step Debug.</h1>"
    output += "<p>"
    output += "<p>"
    output += summary_sessions()
    output += "</body></html>"
    return output

if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5000)
