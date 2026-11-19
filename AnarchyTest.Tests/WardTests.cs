using System.Numerics;

using Vertex;
using Vertex.Input;
using Vertex.Rendering;

// The engine keeps its state in statics (the active scene, input, time), so tests mustn't run at the same time.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace AnarchyTest.Tests
{
    /// <summary>
    /// Plays the ward without a window: the check that it still loads and can be walked through after changes to the
    /// game or the engine.
    /// </summary>
    public class WardTests : IDisposable
    {
        private readonly List<(LogLevel Level, string Message)> _logged = new();

        public WardTests()
        {
            Scene.Active = new Scene();
            Camera.Main = null;
            SimulatedInput.Reset();
            Time.FixedDeltaTime = 1f / 60f;
            Application.AssetFolders = [Path.Combine(AppContext.BaseDirectory, "Assets")];
            Log.Written += Record;
        }

        public void Dispose()
        {
            Log.Written -= Record;
            Scene.Active.Unload();
            Camera.Main = null;
            SimulatedInput.Reset();
        }

        private void Record(LogEntry entry) => _logged.Add((entry.Level, entry.Message));

        private static void RunFrames(GameLoop loop, int frames)
        {
            for (int i = 0; i < frames; i++) loop.Update(1.0 / 60.0, 1280, 720, 720);
        }

        [Fact]
        public void Rivali_stands_in_the_ward_and_can_walk_out_into_the_corridor()
        {
            var loop = new GameLoop(new Game { StartScene = "Scenes/Ward.scene" });
            loop.Load();
            RunFrames(loop, 30);

            GameObject player = GameObject.Find("Player")!;
            Assert.InRange(player.Transform.Position.Y, -0.05f, 0.1f);
            Assert.Equal("", string.Join("\n", _logged.Where(e => e.Level >= LogLevel.Warning).Select(e => e.Message).Distinct()));

            // The dither is on the camera and its shader has nothing wrong with it.
            PostEffect dither = GameObject.Find("Head")!.GetComponent<PostEffect>()!;
            Assert.NotNull(dither.Shader);
            Assert.Null(dither.Shader!.Problem);

            // Turned towards the door and walking, Rivali ends up in the corridor.
            player.Transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, -0.195f);
            SimulatedInput.PressKey(Key.W);
            RunFrames(loop, 420);
            SimulatedInput.ReleaseAll();
            Vector3 at = player.Transform.Position;
            Assert.True(at.Z < -6.4f && at.Z > -9.2f && MathF.Abs(at.X) < 2f, $"Not out in the corridor: at {at}");

            // F switches the torch off and on.
            var torch = GameObject.Find("Flashlight")!.GetComponent<Flashlight>()!;
            Assert.True(torch.On);
            SimulatedInput.PressKey(Key.F);
            RunFrames(loop, 1);
            SimulatedInput.ReleaseAll();
            RunFrames(loop, 1);
            Assert.False(torch.On);
            Assert.Equal(0f, torch.GetComponent<Light>()!.Intensity);
        }
    }
}
