using System.Numerics;

using Vertex;
using Vertex.Input;
using Vertex.Rendering;

namespace AnarchyTest.Tests
{
    /// <summary>Using things in the facility, and the white figure.</summary>
    public class FacilityTests : GameTest
    {
        private static SlidingDoor Door(string name) => GameObject.Find("Door " + name)!.GetComponent<SlidingDoor>()!;

        [Fact]
        public void Locked_doors_refuse_and_C302_opens_onto_the_monitoring_room()
        {
            StartWard();
            var hands = Player.GetComponent<Interactor>()!;

            // In front of C-301, which is locked: E shows "Locked" and does nothing.
            Place(-10.5f, -7.9f, 0f);
            Assert.Same(Door("C-301"), hands.Target);
            Assert.Equal("Locked", hands.Target!.Prompt);
            Tap(Key.E);
            Assert.False(Door("C-301").Open);

            // C-302 opens, and Rivali can walk through into the room behind it.
            Place(-5.5f, -7.9f, 0f);
            Assert.Same(Door("C-302"), hands.Target);
            Assert.Equal("Open", hands.Target!.Prompt);
            Tap(Key.E);
            Assert.True(Door("C-302").Open);
            RunFrames(60);
            Assert.Null(hands.Target);

            Hold(Key.W, 150);
            Vector3 at = Player.Transform.Position;
            Assert.True(at.Z < -10.5f, $"Didn't get through the door: at {at}");
            Assert.Equal("", Warnings());
        }

        [Fact]
        public void Reading_a_screen_holds_Rivali_still_until_it_is_put_away()
        {
            StartWard();
            var reader = Player.GetComponent<Reader>()!;
            var player = Player.GetComponent<Player>()!;

            // Beside the bed, facing the screen over it.
            Place(-4.4f, 4.75f, 0.537f);
            Assert.IsType<Terminal>(Player.GetComponent<Interactor>()!.Target);
            Tap(Key.E);
            Assert.NotNull(reader.Open);
            Assert.False(player.CanMove);

            // Walking does nothing while reading, and the same E that opened it didn't close it.
            Vector3 before = Player.Transform.Position;
            Hold(Key.W, 30);
            Assert.True(Vector3.Distance(before, Player.Transform.Position) < 0.01f);

            // Escape puts it away without giving up the mouse, and then Rivali can move again.
            InputSystem.CursorLocked = true;
            Tap(Key.Escape);
            Assert.Null(reader.Open);
            Assert.True(player.CanMove);
            Assert.True(InputSystem.CursorLocked);
        }

        [Fact]
        public void The_sounds_made_in_code_are_neither_silent_nor_clipped()
        {
            foreach (Vertex.Audio.AudioClip clip in new[] { Sounds.Hum, Sounds.DoorSlide, Sounds.Denied, Sounds.Chirp, Sounds.Sting })
            {
                int loudest = clip.Samples.Max(s => Math.Abs((int)s));
                Assert.InRange(loudest, short.MaxValue / 20, short.MaxValue - 1);
            }
        }

        [Fact]
        public void A_screen_whose_document_is_missing_reads_as_corrupted()
        {
            Assert.Same(Document.Corrupted, Document.Load("no such document"));
        }

        [Fact]
        public void The_white_figure_is_waiting_when_Rivali_steps_out_and_goes_when_stared_at()
        {
            StartWard();
            var figure = GameObject.Find("White Figure")!.GetComponent<WhiteFigure>()!;
            PostEffect look = Player.GetComponent<Player>()!.Head!.GetComponent<PostEffect>()!;
            Assert.False(figure.Watching);
            Assert.False(figure.Body!.Active);

            // Out of the ward door, facing the doors opposite: it's there at the west end, behind Rivali's left shoulder.
            Place(0f, -7.4f, 0f);
            Assert.True(figure.Watching);
            Assert.True(figure.Body.Active);
            Assert.False(figure.Seen);
            Assert.True(figure.Roaming);

            // Turning to look at it: seen, and the picture tears.
            Player.Transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI / 2f);
            RunFrames(40);
            Assert.True(figure.Seen);
            Assert.Equal(1, figure.Sightings);
            Assert.True(look.Get("Fear").X > 0.3f);

            // Stared at too long, it goes in a blackout, and the fear fades after.
            RunFrames((int)(figure.StareLimit * 60f));
            Assert.False(figure.Watching);
            Assert.False(figure.Body.Active);
            RunFrames(300);
            Assert.True(look.Get("Fear").X < 0.05f);
            Assert.Equal(0f, look.Get("Blackout").X);
            Assert.Equal("", Warnings());
        }
    }
}
