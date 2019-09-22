# PumkinsAvatarTools (BETA)
An editor script that adds tools to help you setup avatars faster and easier. It includes a component copier that makes it a lot easier to reimport your avatars, and tools to make your thumbnails nicer.

To launch the tool go to `Tools > Pumkin > Avatar Tools` 

Also check out the new [Wiki](https://github.com/rurre/PumkinsAvatarTools/wiki) for a more detailed explanation of what everything does. (Not yet updated to v0.6) 
I now have a [Discord Server](https://discord.gg/7vyekJv)! Join it to stay up to date with this tool's updates. At least until I make an auto updater.

## New this version (v0.6b):
Lost of stuff! Now where to begin.. 
**Component Copier:**
- Added support for more components! Including: `Mesh Renderers`, `Trail Renderers`, `Particle Systems`, `Rigid Bodies`, `Audio Sources` and more! No Joints yet though, sorry.
- Added the long requested ability to **copy Game Objects when copying around most components.** Finally!
- **Added the ability to copy avatar scale to the transforms menu.** Don't ask why it took so long.
- Added `Select All` and `Select None` buttons as there was too much clicking. 
**Tools:**
- Fixed `Edit Viewpoint` breaking if your avatar is not at 0,0,0 in the scene. Rookie mistake.
- Fixed `Edit Viewpoint` being really laggy if you moved around the viewpoint quickly. The view ball has also been upgraded to yellow!
- Fixed `Fill Visemes` failing to.. fill the visemes! You had one job, button.
- 'Fill Visemes' should now select the correct mesh if you have multiple SkinnedMeshRenderers. Hopefully.
- Renamed the `ResetBlendshapes` button to `ZeroBlendshapes`. This will reset all your blendshapes to 0.
- Added a 'Revert Blendshapes' button. This will revert your blendshapes to prefab, in case you changed your default facial expression.
- Renamed the `Reset Pose` button to `Revert Pose`. This will revert your pose to prefab.
- Added a `Reset to T-Pose` button which will force your avatar into a T-Pose. Might not work for all avatars.
- Added buttons to remove even more components. Everything that's supported by the copier should be here.
- **Added a button to remove empty Game Objects**. A lot more useful than it might seem. 
**Avatar Stats:**
- Added performance ranks for most stats. The ranks are the same as the SDK, but the numbers can differ.
- Added a `Copy to Clipboard` button to the avatar stats panel. Now you don't need to screenshot it anymore! But you can, if you want. I won't judge. You weirdo. 
**Thumbnails:**
- Added a new thumbnail menu with some cool stuff.
- Added the ability to overlay or replace the thumbnail with an image of your choice.
- Added ability to easily change the camera background color. No more default unity skybox thumbnails. Please.
- Added button to center the thumbnail on your face. No more thumbnails looking at your characters crotch. Unless you're into that. Weirdo. 
**UI and Misc:**
- Added a `Select from Scene` checkbox that lets you select an avatar but just selecting something in the scene. Doesn't seem to work if you click it inside the hierarchy though.
- Prettyfied the UI by adding icons and separator lines. Woah! The buttons are still not properly centered tho, sorry.
- Added a super secret and experimental work in progress Pose Editor to the credits screen. Don't tell anyone.
- Added buttons to: `Open Github page`, `Open Help page` and `Join Discord Server`.
- Made a Discord sever. This Discord server. Our Discord server.
- Added a button to buy a me a ko-fi. No pressure~ 
============================================= 
## New that version (v0.5.3b):
- **Fixed dependency checker being stuck in a read-write loop** - Should no longer lag and freeze unity for no reason.
- **Fixed SkinnedMeshRenderer breaking avatar bounds** - Root bones are now correctly assigned. 
Screenshot: [Here](https://puu.sh/C3GGb/5d96267e73.png) 
(Very) Outdated Video Example: [Here](https://puu.sh/BZMGY/53e5dad7c3.mp4) 
- **Tools menu** - Allows you to do stuff including dragging your avatar's viewpoint around, auto filling in visemes and resetting your avatar's transform, as well as quickly removing dynamic bones and colliders from your avatar
- **Component Copier** - Allows you to copy over components from one avatar to another
- **Avatar Info** - Gives you a short rundown of your avatar stats, including trinagles, materials, shaders, dynamic bones and so on.
- **Thumbnail Menu** - Allows you to replace or overlay a thumbnail with an image, set the background to a color and center the thumbnail on your face. 
## Components Supported by the Copier:
- **Transforms** - Can fix broken poses left behind after animating an avatar
- **Dynamic Bones** - Can copy Dynamic Bones from one avatar to another, if the bone and parent names are the same
- **Avatar Descriptors** - Avatar descriptors, their viseme setup, animation override controllers and pipeline IDs
- **Colliders** - Box, Capsule, Sphere and Mesh Colliders
- **SkinnedMesh Renderers** - Copy materials, BlendShape values and Settings between SkinnedMeshRenderers

**And more!** (Most components you might want are supported. Not Joints tho.) 

If you like this project and would like to give something back, consider [buying me a ko-fi!](https://ko-fi.com/notpumkin). Thanks~ 

**Important:** Since this is an editor script it needs to be in a folder called `Editor`anywhere in your project.
**Disclaimer:** This tool is in beta and could *break everything*. Please backup your avatars. And use at your own risk.
