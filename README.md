# Paper-Plane-Controller

https://github.com/user-attachments/assets/6d6f6b0e-7765-4cfc-bc08-1aeeed8a900b

# Description
This is a Unity-based physics-driven paper plane controller. The plane can be launched, glided, and maneuvered with realistic aerodynamics, including lift, drag, gravity, and boost mechanics.

# Controls
W - Descend (Pitch Down)  
S - Ascend (Pitch Up)   
A - Turn Left (Roll Left)   
D - Turn Right (Roll Right)   
Left Shift - Boost   
Space - Launch the plane  

# Launch Mechanics
The plane is initially stationary and only moves when launched.
Upon pressing Space key, an impulse force is applied forward and slightly upward to simulate a hand throw.
The Rigidbody's gravity is activated upon launch.

# Maneuvering
The plane is controlled via pitch, roll, and yaw.  
Pitch (W/S): Tilts the nose up or down, affecting altitude and speed.  
Roll (A/D): Rolls the plane left or right, affecting lateral movement.  
Yaw: Automatically adjusts when rolling to turn the plane.  
Input is smoothed using Mathf.Lerp() for gradual transitions.  
