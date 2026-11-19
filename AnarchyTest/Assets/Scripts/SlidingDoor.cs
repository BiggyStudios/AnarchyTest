using System.Numerics;

using Vertex;
using Vertex.Audio;
using Vertex.Rendering;

namespace AnarchyTest
{
    /// <summary>
    /// A door that slides aside into the wall when it's used, unless it's locked: then its lock light blinks and it
    /// refuses. Put it on the door's object, with the part that moves (and anything that moves with it) as
    /// <see cref="Panel"/>.
    /// </summary>
    public class SlidingDoor : Interactable
    {
        /// <summary>The part that slides: the door itself, with its collider.</summary>
        public GameObject? Panel { get; set; }

        /// <summary>How far the panel moves to open, and which way, from where it starts.</summary>
        public Vector3 Slide { get; set; } = new(1.1f, 0f, 0f);

        /// <summary>How long it takes to open, in seconds.</summary>
        [Range(0.1f, 5f)]
        public float OpenTime { get; set; } = 0.9f;

        public bool Locked { get; set; }

        /// <summary>The little light by the door: red while it's locked, blue once it isn't. It needs a material of its own.</summary>
        public MeshRenderer? LockLight { get; set; }

        [Header("Sounds")]
        public AudioClip? OpenSound { get; set; }
        public AudioClip? LockedSound { get; set; }

        public bool Open { get; private set; }

        public override string Prompt => Locked ? "Locked" : "Open";
        public override bool Available => !Open;

        private static readonly Color LockedColour = new(1f, 0.12f, 0.08f);
        private static readonly Color OpenColour = new(0.3f, 0.85f, 1f);

        private Vector3 _closed;
        private float _opened;
        private float _blink;

        protected override void Start()
        {
            if (Panel != null) _closed = Panel.Transform.Position;
        }

        public override void Use(Interactor by)
        {
            if (Locked)
            {
                _blink = 0.5f;
                if (LockedSound != null) Sound.PlayAt(LockedSound, FocusPoint, 0.7f);
                return;
            }

            Open = true;
            if (OpenSound != null) Sound.PlayAt(OpenSound, FocusPoint, 0.9f);
        }

        /// <summary>Lets it open: for a keycard, a switch, a scripted moment.</summary>
        public void Unlock() => Locked = false;

        protected override void Update()
        {
            if (Panel != null && Open && _opened < 1f)
            {
                _opened = MathF.Min(1f, _opened + Time.DeltaTime / OpenTime);
                // Quick to start and easing in at the end, as a motor would.
                float eased = 1f - (1f - _opened) * (1f - _opened);
                Panel.Transform.Position = _closed + Slide * eased;
            }

            if (LockLight != null)
            {
                _blink = MathF.Max(0f, _blink - Time.DeltaTime);
                bool dark = _blink > 0f && (int)(_blink * 12f) % 2 == 0;
                LockLight.Material.Emission = dark ? Color.Black : Locked ? LockedColour : OpenColour;
            }
        }
    }
}
