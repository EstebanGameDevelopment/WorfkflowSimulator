# **Workflow Simulator**

**Workflow Simulator** is an open-source platform that enables the creation of a **digital twin office** — a fully simulated work environment where teams, processes, and meetings are powered by AI.
It allows users to experiment safely with organizational workflows, evaluate candidate performance, and optimize collaboration dynamics before applying changes to real workplaces.

---

## 🏗️ **Overview**

Workflow Simulator operates across **three main layers**:

1. **Project Management Layer** – Provides task boards, calendars, and team structures similar to modern project management systems.
2. **3D Visualization Layer** – Renders a living office environment where users can visualize operations, meetings, and team interactions in real time.
3. **AI Simulation Layer** – Integrates large language models (LLMs) to simulate realistic decision-making, communication, and productivity behaviors among virtual agents.

The result is a realistic sandbox for **training, experimentation, and workflow optimization** in organizations of any scale.

---

## ⚙️ **System Requirements**

* **Unity Version:** Recommended 6000.0.59f2
* **Backend:** PHP + MySQL [(XAMPP recommended)](https://www.apachefriends.org/download.html).
* **AI Backend:** Python server for AI and LLM operations.
* **Supported Build Target:** WebGL.

---

## 📦 **Required Unity Assets**

To run the project successfully, the following Unity Asset Store packages are required:

* [**Date Time Picker**](https://assetstore.unity.com/packages/tools/gui/date-time-picker-253354) – clock and schedule system
* [**Calendar Scheduler UI**](https://assetstore.unity.com/packages/tools/gui/calendar-scheduler-ui-261219) – meeting and event organization
* [**File Browser PRO**](https://assetstore.unity.com/packages/tools/utilities/file-browser-pro-98713) – in-editor file management
* [**InGame Code Editor**](https://assetstore.unity.com/packages/tools/gui/ingame-code-editor-144254) – embedded code input tool
* [**Simple Sign-In with Google**](https://assetstore.unity.com/packages/tools/integration/simple-sign-in-with-google-250663) – authentication system
* [**UI Color Picker**](https://assetstore.unity.com/packages/tools/gui/ui-color-picker-62874) – project and group color customization
* [**Volumetric Lines**](https://assetstore.unity.com/packages/tools/particles-effects/volumetric-lines-29160) – 3D visualization of layouts
* [**Net Checkout**](https://assetstore.unity.com/packages/tools/integration/net-checkout-176354) – payment and checkout flow
* [**Simple File Browser for WebGL**](https://assetstore.unity.com/packages/tools/integration/simple-file-browser-for-webgl-234993) – file browser for WebGL builds

Some of these assets must be **modified** for compatibility.
After importing, replace the scripts from the following packages:

* `Calendar Scheduler UI`
* `Date Time Picker`
* `UI Color Picker`

To obtain the modified versions of these scripts, please [contact the development team](mailto:assets.request@workflowsimulator.com?subject=[GitHub]%20Assets%20Request) and provide **proof of asset ownership** (e.g., video verification).

---

## 🔧 **Installation Instructions**

1. **Clone the repository**

   ```bash
   git clone https://github.com/EstebanGameDevelopment/workflow-simulator.git
   ```

2. **Open the project in Unity**
   Use the correct Unity version as indicated above.

3. **Import the required assets**
   Purchase and import all required assets from the Unity Asset Store.

4. **Replace modified scripts**
   Overwrite the necessary scripts with the provided modified versions.

5. **Configure Unity build settings**

   * Enable **HTTP downloads**.
   * Set **API Compatibility Level** to `.NET Framework`.
   * Ensure the **Build Platform** is **WebGL**.
   * Verify **preprocessor constants** for local testing or production deployment.

6. **Set up the database (MySQL)**

   * Launch **XAMPP** or another local web server.
   * Create a new database named `workflowsimulator`.
   * Import the included `.sql` file.
   * Copy the provided **PHP endpoints** to a new folder named `workflowsimulator` within your server directory.
   * Update the configuration file paths accordingly.

7. **Deploy the asset bundles**

   * Create a folder named `webgl` in your web server root.
   * Copy all asset bundles into this folder.

8. **Configure AI backend services**

   * Copy the Python service files to the designated AI server directory.
   * Run the Python service that processes AI requests (English mode by default).
   * For production, use the main script that handles concurrent user sessions.

9. **Run the Unity project**

   * Use the provided default user account to log in.
   * Enter your **OpenAI API key** or local **LLM instance** address.
   * Load the default office simulation data.
   * Start the simulation and verify that AI endpoints respond correctly.

---

## 🧠 **Simulation Features**

* Realistic **AI-driven meetings** and **task management**.
* Full **project management** suite with integrated calendar, documents, and team structure.
* AI-based **performance and soft skills evaluation** for candidates.
* Dynamic **office lifecycle** simulation (meetings, work sessions, breaks, and daily cycles).
* Adjustable **time speed** controls and cost tracking for AI operations.

---

## 🧪 **Candidate Evaluation Mode**

The **Candidate Test** module allows organizations to simulate professional environments and evaluate both **technical** and **soft skills**.

* Candidates interact with AI team members during simulated Sprints.
* Meetings and document creation are tracked automatically.
* AI analyzes candidate performance through prompt-based evaluation.
* Results include a breakdown of productivity, communication, and teamwork indicators.

---

## 🎬 **Video Tutorials**

* [📹 **Project Presentation**](https://youtu.be/_MSsaI-L6T4) 
* [📹 **Installation Guide**](https://youtu.be/0MxYODuZ_nc)
* [📹 **Project Code Structure Walkthrough**](https://youtu.be/qyK3UiZHiZ0)
* [📹 **Project Demo**](https://youtu.be/EAPTDztlE08)
* [📹 **Candidate Test Simulation**](https://youtu.be/p3GtG-Wbcv8)
* [📹 **Build office from Scratch**](https://youtu.be/qWDFTqthzow)
* [📹 **Master the control of time**](https://youtu.be/gQd0HTuhmkg)
  
---

## 🤝 **Contributing**

Contributions are welcome!
If you’d like to submit bug fixes, improvements, or new features:

1. Fork the repository.
2. Create a feature branch.
3. Commit and push your changes.
4. Open a pull request describing your modifications.

---

## 🪪 **License**

This project is released under the [**MIT License**](https://github.com/EstebanGameDevelopment/WorfkflowSimulator/blob/master/LICENSE.txt).
You are free to use, modify, and distribute it for personal and commercial purposes, provided proper attribution is given.
