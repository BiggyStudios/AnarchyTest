using System.Numerics;

using Vertex;

namespace AnarchyTest
{
    /// <summary>
    /// A place that, the first time Rivali walks into it, puts the white figure at <see cref="Spot"/>, and from then on
    /// lets it come by itself. The place is a box of <see cref="Size"/> round this object.
    /// </summary>
    public class Sighting : Component
    {
        public WhiteFigure? Figure { get; set; }

        /// <summary>Where the figure stands.</summary>
        public GameObject? Spot { get; set; }

        /// <summary>How big the box is that Rivali walks into, in metres.</summary>
        public Vector3 Size { get; set; } = new(2f, 3f, 2f);

        public bool Done { get; private set; }

        protected override void Update()
        {
            if (Done || Figure == null || Spot == null) return;
            Player? player = GameObject.Scene.FindComponent<Player>();
            if (player == null) return;

            Vector3 from = player.Transform.WorldPosition - Transform.WorldPosition;
            Vector3 half = Size * 0.5f;
            if (MathF.Abs(from.X) > half.X || MathF.Abs(from.Y) > half.Y || MathF.Abs(from.Z) > half.Z) return;

            if (!Figure.Appear(Spot)) return;
            Done = true;
            Figure.Roaming = true;
        }
    }
}
