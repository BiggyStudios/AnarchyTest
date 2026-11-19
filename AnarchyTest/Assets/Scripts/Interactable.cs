using System.Numerics;

using Vertex;

namespace AnarchyTest
{
    /// <summary>
    /// Something Rivali can use by looking at it, close enough, and pressing E: a door, a terminal. The
    /// <see cref="Interactor"/> on the player finds it and shows what E would do.
    /// </summary>
    public abstract class Interactable : Component
    {
        /// <summary>Where the thing is, from this object: what Rivali has to look at.</summary>
        public Vector3 Focus { get; set; }

        /// <summary>How far round <see cref="Focus"/> still counts as looking at it, in metres.</summary>
        [Range(0.05f, 1.5f)]
        public float Radius { get; set; } = 0.35f;

        /// <summary>What E does, shown under the middle of the screen ("Open", "Read").</summary>
        public abstract string Prompt { get; }

        /// <summary>Whether there's anything to do with it now: an open door has nothing left.</summary>
        public virtual bool Available => true;

        /// <summary>Where <see cref="Focus"/> is in the world.</summary>
        public Vector3 FocusPoint => Vector3.Transform(Focus, Transform.WorldMatrix);

        /// <summary>Done when E is pressed while Rivali's looking at it.</summary>
        public abstract void Use(Interactor by);
    }
}
