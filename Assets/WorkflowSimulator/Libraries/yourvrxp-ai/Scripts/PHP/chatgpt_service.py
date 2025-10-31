from lwe import ApiBackend
from lwe.core.config import Config
from lwe.backends.api.user import UserManager
from flask import Flask, request, jsonify

app = Flask(__name__)
# config = Config()
# config.set('chat.model', 'gpt-3.5-turbo-16k')
# chatGPT = ApiBackend(config)
chatGPT = ApiBackend()
chatGPT.set_model('gpt-3.5-turbo-16k')

def loginUserChatGPT(userID, userName, userPassword):        
        if (chatGPT.current_user == None) or (chatGPT.current_user.id != userID):
                success, userid, message = chatGPT.user_manager.login(userName, userPassword)
                if success:
                        chatGPT.set_current_user(userid) 

@app.route("/chatgpt/question", methods=["POST"])
def question():
        args = request.args
        prompt = request.json
        userID = int(prompt["userid"])
        userName = prompt["username"]
        userPassword = prompt["password"]
        conversationID = int(prompt["conversationid"])
        question = prompt["question"]

        if args.get("debug", default=False, type=bool):
                print("ChatGPT question received...")
                print("ChatGPT question is {}".format(question))

        loginUserChatGPT(userID, userName, userPassword)

        chatGPT.conversation_id = conversationID

        success, response, message = chatGPT.ask(question)        

        if args.get("debug", default=False, type=bool):
                print("ChatGPT response received...")
                print(response)

        return response

@app.route("/chatgpt/users/login", methods=["GET"])
def login_user():
        args = request.args
        username = args.get("user", default="", type=str)
        password = args.get("password", default="", type=str)

        if args.get("debug", default=False, type=bool):
                print("Login requested. User("+username+"), Psw("+password+")")

        success, userid, message = chatGPT.user_manager.login(username, password)
        if success:
                return jsonify({"success": True, "user_id": userid.id})
        else:
                return jsonify({"success": False, "user_id": -1})

@app.route("/chatgpt/conversations/new", methods=["GET"])
def new_conversation():
        args = request.args        
        userID = args.get("userid", default="", type=int)
        userName = args.get("username", default="", type=str)
        userPassword = args.get("password", default="", type=str)
        nameScript = args.get("namescript", default="None", type=str)

        loginUserChatGPT(userID, userName, userPassword)

        llm = chatGPT.llm
        provider = chatGPT.provider
        model_name = getattr(llm, provider.model_property_name)
        preset_name = chatGPT.active_preset or ''
        success, conversationData, message = chatGPT.conversation.add_conversation(chatGPT.current_user.id, title=nameScript)

        if not success:
                raise Exception(message)

        print("New conversation with id("+str(conversationData.id)+"),provider("+provider.name+"),model("+model_name+"),preset("+preset_name+")::max_tokens("+str(chatGPT.max_submission_tokens)+")")

        return jsonify({"success": True, "conversation_id": conversationData.id})

@app.route("/chatgpt/conversations/get", methods=["GET"])
def get_conversation():
        args = request.args        
        userID = args.get("userid", default="", type=int)
        userName = args.get("username", default="", type=str)
        userPassword = args.get("password", default="", type=str)
        conversationID = int(args.get("conversationid", default="", type=str))

        loginUserChatGPT(userID, userName, userPassword)

        if (conversationID != -1):
                if args.get("debug", default=False, type=bool):
                        print("Getting conversation with id("+str(conversationID)+")")

                success, conversationData, message = chatGPT.get_conversation(conversationID)
                if success:
                        return conversationData
                else:
                        return ""
        else:
                return ""

@app.route("/chatgpt/conversations/delete", methods=["GET"])
def delete_conversation():
        args = request.args        
        userID = args.get("userid", default="", type=int)
        userName = args.get("username", default="", type=str)
        userPassword = args.get("password", default="", type=str)
        conversationID = int(args.get("conversationid", default="", type=str))

        loginUserChatGPT(userID, userName, userPassword)

        if (conversationID != -1):
                if args.get("debug", default=False, type=bool):
                        print("Deleting conversation with id("+str(conversationID)+")")

                success, conversationData, message = chatGPT.delete_conversation(conversationID)
                if success:
                        return jsonify({"success": True})
                else:
                        return jsonify({"success": False})
        else:
                return jsonify({"success": False})

@app.route("/chatgpt/conversations/delete_all", methods=["GET"])
def delete_all_conversations():
        args = request.args        
        userID = args.get("userid", default="", type=int)
        userName = args.get("username", default="", type=str)
        userPassword = args.get("password", default="", type=str)
        conversationIDs = args.get("conversationids", default="", type=str)

        loginUserChatGPT(userID, userName, userPassword)

        if args.get("debug", default=False, type=bool):
                print("+++++++++++++userName["+userName+"] Conversations to delete("+conversationIDs+")")

        ids = conversationIDs.split(',')

        for id in ids:
                if len(id) > 0:
                        convID = int(id)
                        if (convID != -1):
                                if args.get("debug", default=False, type=bool):
                                        print("Deleting conversation with conversation ID("+str(convID)+")")

                                success, conversationData, message = chatGPT.delete_conversation(convID)
        return jsonify({"success": True})

@app.route("/chatgpt/stop", methods=["GET"])
def stop():
        return jsonify(status="ok")

@app.route("/chatgpt/status", methods=["GET"])
def status():
        return jsonify(status="ok")


if __name__ == "__main__":
# app.run(host='0.0.0.0', threaded=False, ssl_context=('cert.pem', 'key.pem'))
        app.run(host='0.0.0.0', threaded=False)
