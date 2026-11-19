# AnarchyTest

A first-person story horror game made with the [Vertex engine](https://github.com/BiggyStudios/Vertex), in the
style of the first PlayStation: low resolution, wobbling polygons, bending textures, crisp 64-pixel textures and
dithered 15-bit colour, fading into darkness. Everything is white, clean and high-tech, or was.
Early days: so far there's the neural ward Rivali wakes in and the corridor outside it, in greybox.

**Content warning:** graphic violence, gore and disturbing themes. For adults only.

## Getting it

The game builds against the engine it's cloned inside, so clone the engine first and the game into it:

```
git clone https://github.com/BiggyStudios/Vertex.git
cd Vertex
git clone https://github.com/BiggyStudios/AnarchyTest.git
```

The engine repo ignores the `AnarchyTest` folder, so the two stay separate repositories: commit and push each from its
own folder. Changes to the engine show up in the game straight away. With the engine somewhere else, point the build at
it: `dotnet run --project AnarchyTest -p:VertexDir=/path/to/Vertex/`.

## Playing it

```
cd AnarchyTest
dotnet run --project AnarchyTest
```

Click in the window to take the mouse (Escape gives it back). WASD to walk, Shift to hurry, Ctrl or C to crouch, F for
the flashlight.

To work on it in the editor, open `AnarchyTest/AnarchyTest.vertexproject` (File › Open Project). The look comes from two
places: the scene's **Retro** settings in Lighting & Sky (vertex snapping, bending textures, crisp texels) and the PSX
post effect on the player's camera (`Assets/Shaders/PSX.glsl`: the low resolution, colour depth, dither and fog). The
editor's scene view shows the post effect too; the button beside the grid's in its toolbar switches that off.

## Tests

`dotnet test AnarchyTest.slnx` builds the engine and the game and plays the ward without a window, along with the
engine's own tests.

## Layout

| Folder | What's in it |
| --- | --- |
| `AnarchyTest/Assets/Scenes` | The scenes, made and changed in the editor. |
| `AnarchyTest/Assets/Scripts` | The game's C# components: the player, the flashlight, flickering lights. |
| `AnarchyTest/Assets/Shaders` | GLSL shaders: the PSX post effect. |
| `AnarchyTest/Assets/Textures` | Textures, 64 pixels across. |
| `AnarchyTest.Tests` | Tests that play the game headless. |

## License

All rights reserved. You may read the code, build the game and play it yourself, but not sell it, use it commercially,
share or reupload it, or use its code, art, story or characters in anything else without written permission. See
[LICENSE](LICENSE) for the terms. The Vertex engine is under its own license.
