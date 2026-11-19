using System.Numerics;

using Vertex;
using Vertex.Input;
using Vertex.Rendering;

// The engine keeps its state in statics (the active scene, input, time), so tests mustn't run at the same time.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace AnarchyTest.Tests
{
    /// <summary>
    /// Plays the game without a window: starts the ward, runs frames and presses keys, and keeps what was logged so a
    /// test can check nothing went wrong.
    /// </summary>
    public abstract class GameTest : IDisposable
    {
        protected readonly List<(LogLevel Level, string Message)> Logged = new();
        protected GameLoop Loop = null!;

        protected GameTest()
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

        private void Record(LogEntry entry) => Logged.Add((entry.Level, entry.Message));

        /// <summary>Starts the game in the ward and lets it settle.</summary>
        protected void StartWard()
        {
            Loop = new GameLoop(new Game { StartScene = "Scenes/Ward.scene" });
            Loop.Load();
            RunFrames(30);
        }

        protected void RunFrames(int frames)
        {
            for (int i = 0; i < frames; i++) Loop.Update(1.0 / 60.0, 1280, 720, 720);
        }

        /// <summary>Presses a key for one frame, then lets it go.</summary>
        protected void Tap(Key key)
        {
            SimulatedInput.PressKey(key);
            RunFrames(1);
            SimulatedInput.ReleaseAll();
            RunFrames(1);
        }

        protected void Hold(Key key, int frames)
        {
            SimulatedInput.PressKey(key);
            RunFrames(frames);
            SimulatedInput.ReleaseAll();
        }

        protected static GameObject Player => GameObject.Find("Player")!;

        /// <summary>Puts Rivali at (<paramref name="x"/>, <paramref name="z"/>), turned <paramref name="yaw"/> radians left from facing north.</summary>
        protected void Place(float x, float z, float yaw)
        {
            Player.Transform.Position = new Vector3(x, 0f, z);
            Player.Transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, yaw);
            RunFrames(5);
        }

        protected string Warnings() =>
            string.Join("\n", Logged.Where(e => e.Level >= LogLevel.Warning).Select(e => e.Message).Distinct());
    }
}
