# WebXRIT - WebXR Interaction Toolkit

The WebXR Interaction Toolkit (WebXRIT) is a lightweight framework for creating [WebXR](https://immersiveweb.dev/)-based training applications with the Unity Engine.

It provides a set of Unity components for implementing essential interaction functionality, such as the selection and manipulation of virtual objects, while minimizing runtime overhead in WebXR.

In its architecture and component structure, WebXRIT follows the conventions of Unity’s XR Interaction Toolkit (XRIT). This design was chosen to provide familiar code structures and interaction patterns for Unity developers and to simplify the process of porting existing Unity XR projects to WebXR.

## Version Info - Upcoming Releases

WebXRIT is still under active development, and several technical limitations remain that will be addressed in these upcoming versions.

At the moment, this repository contains a Unity project with the complete functional source code of an initial alpha version of WebXRIT. However, it does not yet include tutorials, detailed documentation, or example scenes.

Planned upcoming releases in 2026 will include:

- a getting-started guide on how to use WebXRIT
- an example Unity scene
- documentation (wiki)


## Acknowledgement

### Funding

WebXRIT was developed as part of the research project [PflegeDigital 2.0](https://www.pflegedigital20.de/) at the [Hamburg University of Applied Sciences](https://www.haw-hamburg.de/). The project focused on developing and evaluating digital solutions for nursing education and training and was funded by the [German Federal Ministry of Health](https://www.bundesgesundheitsministerium.de/en/).

### WebXR Export Plugin

WebXRIT uses the [WebXR Export plugin](https://github.com/De-Panther/unity-webxr-export) by [Oren Weizman (De-Panther)](https://github.com/De-Panther). This plugin adds WebXR as a build target to Unity and and allows WebXRIT to access the position and orientation of the HMD and its controllers, as well as to process user input from the controllers.

### Node-based Task System

WebXRIT was developed to create Immersive Virtual Reality (IVR) training applications with Unity for WebXR. For this purpose, it uses a custom node-based task system as an authoring tool.

The task system was built with the open-source plugin [xNode](https://github.com/Siccity/xNode) by [Thor Brigsted (Siccity)](https://github.com/Siccity), which adds a visual node graph to the Unity Editor. WebXRIT extends this node graph with custom components and scripts for controlling training workflows.

The architecture of the task system uses a custom implementation of the [Event Listener Response Pattern](https://github.com/roboryantron/Unite2017) by [Ryan Hipple](https://github.com/roboryantron).

Upcoming releases of the WebXRIT will include examples and detailed documentation for the task system, explaining features such as:

- visual modeling of process workflows within the Unity Editor
- controlling training sequences in Unity scenes
- responding to events during the training process (e.g. successfull task completion) 
- displaying support information through the Unity UI when needed

### XR-HandPoser

WebXRIT uses a custom version of the open-source project [XR-HandPoser](https://github.com/VRwithAndrew/XR-HandPoser) by [VRwithAndrew](https://github.com/VRwithAndrew).

This plugin enables the 3D model of the user’s virtual hand to assume one or more predefined poses when interacting with an object in a Unity scene, for example when grabbing a virtual tool.