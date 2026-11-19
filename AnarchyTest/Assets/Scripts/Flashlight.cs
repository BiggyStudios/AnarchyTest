using System.Numerics;

using Vertex;
using Vertex.Input;
using Vertex.Rendering;

namespace AnarchyTest
{
    /// <summary>
    /// A torch held a little to the right and below the eyes: F switches it on and off. It follows where the head looks
    /// a moment late, as a hand would, and sputters as it comes on.
    /// </summary>
    public class Flashlight : Component
    {
        /// <summary>The head it's held in front of (the camera's object).</summary>
        public GameObject? Head { get; set; }

        /// <summary>Where it's held, from the eyes: right, up, forward.</summary>
        public Vector3 Offset { get; set; } = new(0.2f, -0.22f, -0.1f);

        /// <summary>How quickly it catches up with where the head looks. Lower trails further behind.</summary>
        [Range(1f, 30f)]
        public float Follow { get; set; } = 11f;

        public bool On { get; set; } = true;

        private Light? _light;
        private float _brightness;
        private Quaternion _aim = Quaternion.Identity;
        private bool _aimed;
        private float _sputter;

        protected override void Start()
        {
            _light = GetComponent<Light>();
            _brightness = _light?.Intensity ?? 1f;
        }

        /// <summary>Makes it stutter for a moment, as if something got into it.</summary>
        public void Sputter(float seconds) => _sputter = MathF.Max(_sputter, seconds);

        protected override void Update()
        {
            if (_light == null || Head == null) return;

            if (InputSystem.GetKeyDown(Key.F))
            {
                On = !On;
                if (On) _sputter = 0.25f;
            }

            Quaternion look = Head.Transform.WorldRotation;
            _aim = _aimed ? Quaternion.Slerp(_aim, look, MathF.Min(1f, Time.DeltaTime * Follow)) : look;
            _aimed = true;

            // Held in the head's space, but turned to where the hand has got to.
            Transform.Position = Offset;
            Transform.Rotation = Quaternion.Normalize(Quaternion.Inverse(look) * _aim);

            float brightness = On ? _brightness : 0f;
            if (On && _sputter > 0f)
            {
                _sputter -= Time.DeltaTime;
                if (Random.Shared.NextSingle() < 0.45f) brightness *= 0.15f;
            }

            _light.Intensity = brightness;
        }
    }
}
