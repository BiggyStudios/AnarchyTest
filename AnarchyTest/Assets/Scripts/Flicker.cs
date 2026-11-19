using Vertex;
using Vertex.Rendering;

namespace AnarchyTest
{
    /// <summary>
    /// A failing ceiling light: on, then every so often a burst of stutters, the light and its panel going out together.
    /// Put it on the light; <see cref="Panel"/> is the glowing panel, which needs a material of its own.
    /// </summary>
    public class Flicker : Component
    {
        /// <summary>The panel that glows while the light's on.</summary>
        public MeshRenderer? Panel { get; set; }

        /// <summary>Seconds between bursts, at most: each wait is somewhere up to this.</summary>
        [Range(0.5f, 30f)]
        public float Calm { get; set; } = 6f;

        /// <summary>How much of the time during a burst it's out, from 0 to 1.</summary>
        [Range(0f, 1f)]
        public float Badness { get; set; } = 0.5f;

        private Light? _light;
        private float _brightness;
        private Color _glow;
        private float _wait;
        private float _burst;
        private float _switch;
        private bool _out;

        protected override void Start()
        {
            _light = GetComponent<Light>();
            _brightness = _light?.Intensity ?? 1f;
            if (Panel != null) _glow = Panel.Material.Emission;
            _wait = Random.Shared.NextSingle() * Calm;
        }

        protected override void Update()
        {
            if (_burst > 0f)
            {
                _burst -= Time.DeltaTime;
                _switch -= Time.DeltaTime;
                if (_switch <= 0f)
                {
                    _out = Random.Shared.NextSingle() < Badness;
                    _switch = 0.03f + Random.Shared.NextSingle() * 0.12f;
                }

                if (_burst <= 0f)
                {
                    _out = false;
                    _wait = 0.5f + Random.Shared.NextSingle() * Calm;
                }
            }
            else
            {
                _wait -= Time.DeltaTime;
                if (_wait <= 0f) _burst = 0.2f + Random.Shared.NextSingle() * 1.2f;
            }

            if (_light != null) _light.Intensity = _out ? 0f : _brightness;
            if (Panel != null) Panel.Material.Emission = _out ? Color.Black : _glow;
        }
    }
}
