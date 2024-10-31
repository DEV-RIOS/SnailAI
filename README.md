# SnailAI

"Realtime" AI imagination for your Rhino model, all within a Rhino viewport, activated via a Display Mode.

**SnailAI** was a project developed for the AEC Tech NYC 2024 Hackathon, winning the award for *Best Overall Hack*.

### Team Members

- **Bob Frederick** - RIOS
- **Jimmy Torres** - RIOS
- **Oscar Borgström** - Foster + Partners
- **Chu Ding, PE, PhD** - RunToSolve
- **Kohei Hayashi** - Shimizu & CORE studio | Thornton Tomasetti
- **Sheng Zheng, PE** - Martin/Martin
- **Kojiro Suzuki** - Shimizu
- **Yutaka Iribe** - Shimizu
- **Tamaho Shigemura** - Algorithm Design Lab

### AECTech
We wanted to thank CORE Studio for hosting this event. 
Here are the slides we presented at the event. 
https://docs.google.com/presentation/d/1IYfWYMRZZFo6tpIs5yYFvRPi0NT_UnSsgiO12qauFYU/edit#slide=id.g2d4d7df7fcd_5_41


---

## Setup Instructions

1. **Create a Fal.AI Account**  
   - Load credits (approximately $5–$10) and import the three included `comfyui` JSON definitions.

2. **Add API Key**  
   - Create a file named `fal_api_key.txt` located at `\SnailAI\fal_api_key.txt`, and paste in your Fal.AI API key.

3. **Configure API Endpoint**  
   - Update `falConfig.json` located at `\SnailAI\falConfig.json` to match your specific API endpoint.  
     - Example: If your endpoint is `"comfy/DEV-RIOS/snail-rhino-to-turbo"`, replace "DEV-RIOS" with your account name. Ensure endpoint names are accurate as they’re a common point of error.

4. **Build and Install**  
   - Build the solution and install `SnailAI.rhp` from the `bin` folder.

5. **Import Display Mode**  
   - Import the included `SnailAI.ini` display mode.

6. **Activate Display Mode**  
   - In the Rhino viewport, activate the display mode to run the plugin.

---

### Tips for Best Performance

- **Viewport Dimensions**  
  Render time is highly sensitive to viewport dimensions. For optimal results, try dimensions 768x768 or 1024x1024.

---
