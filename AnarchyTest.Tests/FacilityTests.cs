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
        public void Keycards_open_the_doors_that_want_them()
        {
            StartWard();
            var hands = Player.GetComponent<Interactor>()!;
            var inventory = Player.GetComponent<Inventory>()!;

            // Without the card, C-301 says what it wants.
            Place(-10.5f, -7.9f, 0f);
            Tap(Key.E);
            Assert.False(Door("C-301").Open);
            Assert.Contains("staff card", inventory.Shown);

            // The staff card is on the monitoring room's desk; taken, it's gone from there.
            Pickup card = Scene.Active.FindComponents<Pickup>().Single(p => p.Card == "staff");
            card.Use(hands);
            Assert.True(inventory.Has("staff"));
            Assert.False(card.GameObject.Active);
            Assert.Equal("Took the staff keycard.", inventory.Shown);

            // Now the door shows "Open" and opens.
            RunFrames(2);
            Assert.Same(Door("C-301"), hands.Target);
            Assert.Equal("Open", hands.Target!.Prompt);
            Tap(Key.E);
            Assert.True(Door("C-301").Open);

            // Each card is somewhere to be found, and each door that wants one has it somewhere.
            var cards = Scene.Active.FindComponents<Pickup>().Select(p => p.Card).Append("staff").ToHashSet();
            foreach (SlidingDoor door in Scene.Active.FindComponents<SlidingDoor>().Where(d => d.Card.Length > 0 && d.Card != "medical"))
                Assert.Contains(door.Card, cards);
        }

        [Fact]
        public void The_override_raises_the_blast_door_onto_the_lift_lobby()
        {
            StartWard();
            var hands = Player.GetComponent<Interactor>()!;
            SlidingDoor blast = GameObject.Find("Blast Door")!.GetComponent<SlidingDoor>()!;

            // By hand it won't move.
            Place(12.6f, -7.7f, -MathF.PI / 2f);
            Assert.Same(blast, hands.Target);
            Tap(Key.E);
            Assert.False(blast.Open);

            Scene.Active.FindComponent<Switch>()!.Use(hands);
            Assert.True(blast.Open);
            RunFrames(200);

            Hold(Key.W, 120);
            Vector3 at = Player.Transform.Position;
            Assert.True(at.X > 15f, $"Didn't get through the blast door: at {at}");
            Assert.Equal("", Warnings());
        }

        [Fact]
        public void Caught_by_the_hunting_figure_Rivali_sees_the_visions_and_wakes_in_the_ward_further_gone()
        {
            StartWard();
            var figure = GameObject.Find("White Figure")!.GetComponent<WhiteFigure>()!;
            var player = Player.GetComponent<Player>()!;
            Camera eyes = player.Head!.GetComponent<Camera>()!;
            var readout = Scene.Active.FindComponent<TransferReadout>()!;
            Assert.Equal(97, readout.Transfer);

            // In the corridor facing the doors; it comes from the west end, closer whenever it isn't watched.
            Place(0f, -7.4f, 0f);
            Assert.True(figure.Watching);
            Assert.True(figure.StartHunt());
            Visions visions = figure.Visions!;
            int frames = 0;
            while (!visions.Showing && frames < 60 * 45)
            {
                RunFrames(1);
                frames++;
            }

            Assert.True(visions.Showing, "It never caught Rivali");
            Assert.False(figure.Hunting);
            Assert.False(player.CanMove);
            Assert.NotSame(eyes, Camera.Main);

            RunFrames((int)(visions.Length * 60f) + 5);
            Assert.False(visions.Showing);
            Assert.Same(eyes, Camera.Main);
            Assert.True(player.CanMove);
            Assert.True(Vector3.Distance(Player.Transform.Position, new Vector3(-2.2f, 0f, 3.6f)) < 0.3f, $"Woke at {Player.Transform.Position}");
            Assert.Equal(98, readout.Transfer);
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
        public void Doors_screens_the_figure_and_the_hum_all_have_sounds_that_are_neither_silent_nor_clipped()
        {
            StartWard();
            var clips = new List<Vertex.Audio.AudioClip?>();
            foreach (SlidingDoor door in Scene.Active.FindComponents<SlidingDoor>()) clips.AddRange([door.OpenSound, door.LockedSound]);
            foreach (Terminal screen in Scene.Active.FindComponents<Terminal>()) clips.Add(screen.Sound);
            foreach (Pickup card in Scene.Active.FindComponents<Pickup>()) clips.Add(card.Sound);
            foreach (Switch lever in Scene.Active.FindComponents<Switch>()) clips.Add(lever.Sound);
            var figure = GameObject.Find("White Figure")!.GetComponent<WhiteFigure>()!;
            clips.AddRange([figure.Sting, figure.Step, figure.Visions!.Sting, figure.Visions.Noise]);
            clips.Add(GameObject.Find("Ambience")!.GetComponent<Vertex.Audio.AudioSource>()!.Sound);

            Assert.True(clips.Count > 30, $"Only {clips.Count} sounds");
            foreach (Vertex.Audio.AudioClip? clip in clips)
            {
                Assert.NotNull(clip);
                Assert.StartsWith("Sounds", Path.GetRelativePath(Path.Combine(AppContext.BaseDirectory, "Assets"), clip!.Source!));
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
