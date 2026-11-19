using System.Numerics;

using Vertex;
using Vertex.Input;
using Vertex.Rendering;

namespace AnarchyTest.Tests
{
    /// <summary>
    /// Plays the ward without a window: the check that it still loads and can be walked through after changes to the
    /// game or the engine.
    /// </summary>
    public class WardTests : GameTest
    {
        [Fact]
        public void Rivali_stands_in_the_ward_and_can_walk_out_into_the_corridor()
        {
            StartWard();

            GameObject player = Player;
            Assert.InRange(player.Transform.Position.Y, -0.05f, 0.1f);
            Assert.Equal("", Warnings());

            // The PlayStation look is on the camera and its shader has nothing wrong with it.
            PostEffect look = Player.GetComponent<Player>()!.Head!.GetComponent<PostEffect>()!;
            Assert.NotNull(look.Shader);
            Assert.Null(look.Shader!.Problem);

            // Turned towards the door and walking, Rivali ends up in the corridor.
            player.Transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, -0.195f);
            Hold(Key.W, 420);
            Vector3 at = player.Transform.Position;
            Assert.True(at.Z < -6.4f && at.Z > -9.2f && MathF.Abs(at.X) < 2f, $"Not out in the corridor: at {at}");

            // F switches the torch off and on.
            var torch = GameObject.Find("Flashlight")!.GetComponent<Flashlight>()!;
            Assert.True(torch.On);
            Tap(Key.F);
            Assert.False(torch.On);
            Assert.Equal(0f, torch.GetComponent<Light>()!.Intensity);
        }
    }
}
