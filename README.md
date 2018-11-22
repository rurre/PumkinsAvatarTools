# PumkinsAvatarTools (BETA)
This editor script has a bunch of tools that allows you setup avatars easier, as well as to copy components from existing avatars to new ones, making it much faster to reimport avatars from blender or setup new ones.

Check out the [Wiki](https://github.com/rurre/PumkinsAvatarTools/wiki) for a more detailed explanation of what everything does.

## New this version (v0.5.3b):
- **Fixed dependency checker being stuck in a read-write loop** - Should no longer lag and freeze unity for no reason.
- **Fixed SkinnedMeshRenderer breaking avatar bounds** - Root bones are now correctly assigned.

## New that version (v0.5.2b):
- **Fixed OldVersion not existing** - Should no longer break everything, yet again.
- **Empty Material Slots no longer break Avatar Info** - Finally!

## New that other version (v0.5.1b):
- **Removed DynamicBone** - ...dependencies. It's okay if you don't have them now. It's also okay if you do.
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
- **Audio Sources**
- **Some other things I need to decide to add**

To launch the tool go to `Tools > Pumkin > Avatar Tools`

**Important:** Since this is an editor script it needs to be in a folder called `Editor`anywhere in your project.
**Disclaimer:** This tool is in beta and could *break everything*. Please backup your avatars. Als use at your own risk.
