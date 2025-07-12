# Paper-Plane-Controller
https://github.com/user-attachments/assets/14c9e50e-ae36-4022-852a-bc7676cc0f76

# 🚀 Thrust & Boost:
-> Initial launch uses AddForce with an impulse for a strong start.  
-> Boost temporarily increases thrust and reduces drag for high-speed gliding.  

# 🌬️💨  Lift & Glide:  
-> Lift is calculated based on speed using Lift Force = Speed × Lift Multiplier.  
-> If the plane slows down below minGlideSpeed, it starts to lose altitude.  

# 📈 Gravity & Altitude Limits:
-> A custom gravity scale keeps the descent realistic.  
-> At maxAltitude, an artificial downward force ensures the plane doesn't fly infinitely.  

# 🎮 Rotation & Steering:
-> Pitch (Nose Up/Down): Adjusted dynamically based on user input, affecting speed and lift.  
-> Roll (Tilting Left/Right): Impacts lateral drift using Mathf.Sin() for realistic banking turns.  
-> Yaw (Turning): Combined with roll to create natural turns.  

# ⚖️🎢 Drag & Air Resistance:
-> Drag Force = Drag Coefficient × Speed² to simulate real air resistance.  
-> Higher altitude slightly increases drag, mimicking thinner air resistance.  

# 🎯 Dynamic Speed Adjustments:
-> Going upward reduces speed due to gravity and air resistance.  
-> Nose-down maneuvers accelerate descent naturally.
