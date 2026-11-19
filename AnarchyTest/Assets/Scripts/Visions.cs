using System.Numerics;

using Vertex;
using Vertex.Audio;
using Vertex.Rendering;
using Vertex.UI;

namespace AnarchyTest
{
    /// <summary>
    /// What happens when the white figure catches Rivali: a few seconds of hard cuts between <see cref="Shots"/> (places
    /// set up out of sight of the facility, seen through a camera of their own), black frames and words flashing over
    /// them, then Rivali wakes back by the bed with the transfer a step further along. The words are lines of a file in
    /// Assets/Text (<see cref="TextKey"/>).
    /// </summary>
    public class Visions : Component
    {
        /// <summary>The camera the visions are seen through, kept switched off until they start.</summary>
        public GameObject? Eyes { get; set; }

        /// <summary>Where the camera cuts to: empty objects placed and turned as the camera should be.</summary>
        public List<GameObject> Shots { get; set; } = new();

        /// <summary>Where Rivali comes round afterwards.</summary>
        public GameObject? Wake { get; set; }

        /// <summary>The screen over Rivali's bed, which goes up a percent each time.</summary>
        public TransferReadout? Readout { get; set; }

        [Range(1f, 20f)]
        public float Length { get; set; } = 6.5f;

        public string TextKey { get; set; } = "visions";

        [Header("Sounds")]
        public AudioClip? Sting { get; set; }
        public AudioClip? Noise { get; set; }

        /// <summary>Whether they're happening now.</summary>
        public bool Showing { get; private set; }

        /// <summary>How many times Rivali has been caught.</summary>
        public int Times { get; private set; }

        private Player? _player;
        private string[] _lines = [];
        private float _time, _cut;
        private int _shot = -1;
        private bool _black;
        private string? _line;
        private PlayingSound _noise;

        protected override void Start()
        {
            _player = GameObject.Scene.FindComponent<Player>();
            if (Eyes != null) Eyes.Active = false;

            string path = Application.AssetPath("Text", TextKey + ".txt");
            if (File.Exists(path)) _lines = File.ReadAllLines(path).Select(l => l.Trim()).Where(l => l.Length > 0).ToArray();
        }

        public void Begin()
        {
            if (Showing || Eyes == null || Shots.Count == 0) return;
            Showing = true;
            Times++;
            _time = 0f;
            _cut = 0f;
            if (_player != null) _player.CanMove = false;

            Eyes.Active = true;
            Camera.Main = Eyes.GetComponent<Camera>();
            if (Sting != null) Sound.Play(Sting, 1f);
            if (Noise != null) _noise = Sound.Play(Noise, 0.7f);
        }

        protected override void Update()
        {
            if (!Showing) return;

            _time += Time.DeltaTime;
            _cut -= Time.DeltaTime;
            if (_time >= Length)
            {
                End();
                return;
            }

            if (_cut <= 0f) Cut();

            if (_black) Canvas.DrawRect(Vector2.Zero, Canvas.Size, Color.Black);
            if (_line != null)
            {
                const float size = 46f;
                Vector2 measured = Canvas.MeasureText(Font.DefaultBold, _line, size);
                var at = new Vector2((Canvas.Width - measured.X) * 0.5f + Jitter(6f), (Canvas.Height - measured.Y) * 0.5f + Jitter(6f));
                Canvas.DrawText(Font.DefaultBold, _line, at, new Color(0.85f, 0.05f, 0.03f, 0.95f), size);
            }
        }

        /// <summary>The next frame: a black one now and then, otherwise somewhere else, sometimes with words over it.</summary>
        private void Cut()
        {
            // Quicker and quicker towards the end.
            float pace = 1f - _time / Length * 0.6f;
            _cut = (0.08f + Random.Shared.NextSingle() * 0.5f) * pace;
            _black = Random.Shared.NextSingle() < 0.22f;
            _line = _lines.Length > 0 && Random.Shared.NextSingle() < 0.35f ? _lines[Random.Shared.Next(_lines.Length)] : null;
            if (_black || Eyes == null) return;

            int next = Random.Shared.Next(Shots.Count);
            if (Shots.Count > 1 && next == _shot) next = (next + 1) % Shots.Count;
            _shot = next;
            GameObject shot = Shots[next];
            Eyes.Transform.Position = shot.Transform.WorldPosition;
            // Never quite level.
            Eyes.Transform.Rotation = shot.Transform.WorldRotation * Quaternion.CreateFromAxisAngle(-Vector3.UnitZ, Jitter(0.12f));
        }

        private void End()
        {
            Showing = false;
            _black = false;
            _line = null;
            _noise.Stop();
            if (Eyes != null) Eyes.Active = false;

            if (_player != null)
            {
                Camera.Main = _player.Head?.GetComponent<Camera>();
                if (Wake != null)
                {
                    _player.Transform.Position = Wake.Transform.WorldPosition;
                    _player.Transform.Rotation = Wake.Transform.WorldRotation;
                }

                _player.CanMove = true;
            }

            if (Readout != null) Readout.Transfer = Math.Min(99, Readout.Transfer + 1);
        }

        private static float Jitter(float amount) => (Random.Shared.NextSingle() * 2f - 1f) * amount;
    }
}
