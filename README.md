# PumkinsAvatarTools
This editor script tool allows you to copy components from existing avatars to new ones, making it much faster to reimport avatars from blender or setup new ones.

## New this version:
- **Select Avatar from Scene Button** - Gone are the days of manually dragging things around
- **A brand new Tools menu** - Bunch of never-before-seen-tools-that-nobody-has-done-before to do things with including, dragging your avatar's viewpoint around, auto filling in visemes and resetting your avatar's transform, as well as quickly removing dynamic bones and colliders from your avatar
- **An even newer Avatar Info Menu** - For when you want to know exactly how unoptimized your avatar is
- **More Untested changes** - Even more Hype! (Please give feedback)

Screenshot: [Here](https://puu.sh/C3GGb/5d96267e73.png)

(Very) Outdated Video Example: [Here](https://puu.sh/BZMGY/53e5dad7c3.mp4)

- **Tools menu** - Allows you to do stuff including dragging your avatar's viewpoint around, auto filling in visemes and resetting your avatar's transform, as well as quickly removing dynamic bones and colliders from your avatar
- **Component Copier** - Allows you to copy over components from one avatar to another
- **Avatar Info** - Gives you a short rundown of your avatar stats, including trinagles, materials, shaders, dynamic bones and so on.

## Components Supported by the Copier:
- **Transforms** - Can fix broken poses left behind after animating an avatar
- **Dynamic Bones** - Can copy Dynamic Bones from one avatar to another, if the bone and parent names are the same
- **Avatar Descriptors** - Avatar descriptors, their viseme setup, animation override controllers and pipeline IDs
- **Colliders** - Box, Capsule, Sphere and Mesh Colliders
- **SkinnedMeshRenderers** - Copy materials, BlendShape values and Settings between SkinnedMeshRenderers

### Not yet supported:
- **Particle Systems**
- **Audio Sorces**
- **Some other things I need to decide to add**

To launch the tool go to `Tools > Pumkin > Avatar Tools`

**Important:** Since this is an editor script it needs to be in a folder called `Editor`anywhere in your project.
