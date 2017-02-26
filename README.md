# AltspaceVR Programming Project - Impostor Rendering
Solution code and scene file from Jong Pil Park(jpster99@gmail.com)

# Part 1 - Realtime Impostor

## Goal : Realtime generation and rendering of Impostor texture

Dynamically generates impostor textures from given mesh object(Skateboard). 
The impostor texture is updated due to relative movement of impostor target object from the viewing camera.

Impostor mesh is not updated when the camera viewing angle to the impostor does not exceed the defined threshold.
Original threshold is set to 0.02. As the threshold is increased, the impostor does not be updated frequently and cause some artifacts which comes from angle difference between the camera angle of current view and camera angle of render texture.

Target object which is to be rendered with the impostor is initially set to the Skateboard object, but you can change the target object using parameter box of Inspector or source code of _ImpostorController.cs_.

## Parameters
* Angle Threshold(float) : Threshold for late update. Until the angle difference of impostor view is exceed this threshold, rendering update is not processed.
* Texture Size(int) : Size of RenderTexture

## Files Included : 
* TextureManager.cs : Container class of RenderTexture and Camera for Texture rendering
* ImpostorController.cs : Main Controller class which assigns TextureManager and update camera position
* AlphaShader.shader : Alpha shader for the render texture composition

## Usage
1. Create Quad object to be rendered with impostor
2. Link to the _ImpostorController.cs_ script to the quad object
3. Set the parameter and excute

## Issues
When the camera is closer than certain distance, imposter rendered object can be vanished. This is due to the render texture property of unity engine, but this does not happens when at least 2m distance is allowed(which is mentioned in problem condition).

If you set the texture resolution too low, some artifact like aliasing and blur can be seen. In my calculation, over than 400 pixels render texture does not show distinct artifacts.

# Part 2 - Enhancements : Massive objects Generation using Faster Impostor Rendering
## Goal 

## Enhancement Algorithm

## Parameters
* Angle Threshold(float) : Threshold for late update. Until the angle difference of impostor view is exceed this threshold, rendering update is not processed.
* Texture Size(int) : Size of RenderTexture

## Files Included : 
* TextureManager.cs : Container class of RenderTexture and Camera for Texture rendering
* ImpostorController.cs : Main Controller class which assigns TextureManager and update camera position
* AlphaShader.shader : Alpha shader for the render texture composition

## Usage


## Issues



## Deliverable

    
## Acknowledgements

## Contact

