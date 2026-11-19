# AnarchyTest

A first-person story horror game made with the [Vertex engine](https://github.com/BiggyStudios/Vertex), drawn in
one-bit dithering: big square pixels, black or white. Early days: so far there's the first ward and the corridor
outside it, in greybox.

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

To work on it in the editor, open `AnarchyTest/AnarchyTest.vertexproject` (File › Open Project). The dithering is a
post effect on the player's camera (`Assets/Shaders/Dither.glsl`); the editor's scene view shows it too, and the
button beside the grid's in its toolbar switches that off while you build.

## Tests

`dotnet test AnarchyTest.slnx` builds the engine and the game and plays the ward without a window, along with the
engine's own tests.

## Layout

| Folder | What's in it |
| --- | --- |
| `AnarchyTest/Assets/Scenes` | The scenes, made and changed in the editor. |
| `AnarchyTest/Assets/Scripts` | The game's C# components: the player, the flashlight, flickering lights. |
| `AnarchyTest/Assets/Shaders` | GLSL shaders: the dither post effect. |
| `AnarchyTest/Assets/Textures` | Textures. |
| `AnarchyTest.Tests` | Tests that play the game headless. |
