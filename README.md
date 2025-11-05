# 👁️‍🦾 **RaycastGrasp**  
**RaycastGrasp: Eye-Gaze Interaction with Wearable Devices for Robotic Manipulation**  
📄 [Paper Link](https://arxiv.org/abs/2510.22113)

---

## 🧭 Project Overview

**RaycastGrasp** is a **Mixed Reality (MR)**-based **eye-gaze-guided robotic manipulation system**.  
It allows users to interact with real-world objects through natural gaze fixation.  
Once the user fixates on an object, the system automatically recognizes the target and commands a collaborative robot arm to perform grasping — achieving a truly **hands-free** human–robot interaction experience.

Unlike conventional joystick- or screen-based control methods, RaycastGrasp integrates the **HTC VIVE MR headset** with the **VIVE OpenXR SDK** to implement gaze-based raycasting, projected passthrough alignment, and real-time gaze logging.  
It further combines **YOLOv8 object detection** and a **Franka Emika Panda** robotic arm to enable end-to-end intention understanding and grasp execution.

> 💡 This repository contains the **Unity front-end implementation**, including eye-tracking, passthrough layer control, raycast detection, and gaze data logging.

---

## [1] Method

**System Name:** RaycastGrasp  
**Core Idea:** Enable gaze-driven human–robot collaboration through Mixed Reality interaction.

### 🔧 Key Techniques
- **Eye Tracking and Raycasting**: Utilize the HTC VIVE OpenXR EyeTracker to acquire the right-eye gaze vector and perform real-time raycast.  
- **Passthrough Layer Alignment**: Align virtual passthrough and physical layers for precise spatial consistency.  
- **YOLOv8 Object Detection**: Identify the object label of the user’s gaze target.  
- **Semantic Mapping**: Match user gaze recognition with robot camera detection results.  
- **Franka Emika Panda Execution**: Perform grasping based on the matched semantic label.

The system achieves a **fully hands-free MR–robot interaction**.  
When the user fixates on an object for ≥ 2 seconds, the system automatically recognizes the target and triggers the robot to execute the grasp.

---

## [2] Implementation Details

| Module | Description |
|--------|--------------|
| **Engine** | Unity 2022.3+ (Android build: VIVE Focus 3 / XR Elite) |
| **SDKs** | VIVE OpenXR Plugin v2.5.1 (enable EyeTracker & Passthrough) |
| **Hardware** | HTC VIVE MR headset, Franka Emika Panda robotic arm, PC host |
| **Main Scripts** | `GazeWithPassthrough.cs`, `DataLogger.cs`, `PassthroughController.cs` |
| **Log Output** | `/sdcard/Download/gaze_log.txt` (timestamp, x, y, z, label) |

---

## [3] Quick Start

### Step 1. Clone Repository
```bash
git clone https://github.com/ZitiantaoLin/RaycastGrasp-Eye-Gaze-Interaction-with-Wearable-Devices-for-Robotic-Manipulation.git
