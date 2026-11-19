using System.Numerics;

using Vertex;
using Vertex.Audio;
using Vertex.Physics;
using Vertex.Rendering;

namespace AnarchyTest
{
    /// <summary>
    /// The huge white figure that stalks the halls. Mostly it watches: every so often it's standing at one of its
    /// <see cref="Spots"/>, somewhere Rivali isn't looking, facing Rivali, and stays there until it's seen. Once seen,
    /// looking away lets it go; staring at it, or coming too close, makes it go in a blackout. While it's in view the
    /// picture tears and drains (the PSX effect's Fear), and seeing it hits with a sting.
    /// </summary>
    public class WhiteFigure : Component
    {
        /// <summary>Its model, shown while it's there.</summary>
        public GameObject? Body { get; set; }

        /// <summary>Its head, held at a different angle each time it comes.</summary>
        public GameObject? Face { get; set; }

        /// <summary>Where it can stand: empty objects round the facility.</summary>
        public List<GameObject> Spots { get; set; } = new();

        /// <summary>Played when it's seen.</summary>
        public AudioClip? Sting { get; set; }

        /// <summary>Whether it comes by itself yet. Off until it's first been seen where a <see cref="Sighting"/> puts it.</summary>
        public bool Roaming { get; set; }

        /// <summary>Seconds between one time and the next, somewhere between these.</summary>
        [Header("When")]
        public float MinWait { get; set; } = 40f;
        public float MaxWait { get; set; } = 100f;

        /// <summary>How near and far from Rivali a spot it comes to can be, in metres.</summary>
        public float NearestSpot { get; set; } = 6f;
        public float FurthestSpot { get; set; } = 18f;

        /// <summary>How long it waits to be seen before it goes anyway, in seconds.</summary>
        public float MaxWatch { get; set; } = 40f;

        /// <summary>How long it has to be in sight to count as seen, in seconds.</summary>
        [Header("Being seen")]
        public float NoticeTime { get; set; } = 0.3f;

        /// <summary>How long Rivali can stare at it before it goes, in seconds.</summary>
        public float StareLimit { get; set; } = 3.5f;

        /// <summary>How close Rivali can come before it goes, in metres.</summary>
        public float TooClose { get; set; } = 4.5f;

        /// <summary>Whether it's standing somewhere now.</summary>
        public bool Watching { get; private set; }

        /// <summary>Whether it's been seen since it came.</summary>
        public bool Seen { get; private set; }

        /// <summary>How many times it's been seen.</summary>
        public int Sightings { get; private set; }

        /// <summary>How afraid the picture shows Rivali to be, from 0 to 1.</summary>
        public float Fear { get; private set; }

        private Player? _player;
        private Collider? _playerBody;
        private PostEffect? _effect;
        private Flashlight? _torch;
        private GameObject? _spot;
        private float _wait, _watched, _inSight, _outOfSight, _blackout;

        protected override void Start()
        {
            _player = GameObject.Scene.FindComponent<Player>();
            _playerBody = _player?.GetComponent<CharacterController>();
            _effect = _player?.Head?.GetComponent<PostEffect>();
            _torch = GameObject.Scene.FindComponent<Flashlight>();
            if (!Watching) Hide();
            _wait = NextWait();
        }

        /// <summary>Puts it at <paramref name="spot"/>, facing Rivali, unless it's already somewhere.</summary>
        public bool Appear(GameObject spot)
        {
            if (Watching) return false;

            Transform.Position = spot.Transform.WorldPosition;
            FacePlayer();
            if (Face != null)
            {
                // Tipped to one side, or down at Rivali, or both: never quite the same twice.
                float roll = (Random.Shared.NextSingle() * 2f - 1f) * 0.45f;
                float nod = Random.Shared.NextSingle() * 0.35f;
                Face.Transform.Rotation = Quaternion.CreateFromYawPitchRoll(0f, -nod, roll);
            }

            if (Body != null) Body.Active = true;
            Watching = true;
            Seen = false;
            _spot = spot;
            _watched = _inSight = _outOfSight = 0f;
            return true;
        }

        protected override void Update()
        {
            float delta = Time.DeltaTime;
            bool inSight = false;

            if (Watching && _player != null)
            {
                inSight = InSight();
                Watch(inSight, delta);
            }
            else if (Roaming)
            {
                _wait -= delta;
                if (_wait <= 0f)
                {
                    GameObject? spot = ChooseSpot();
                    if (spot != null) Appear(spot);
                    else _wait = 3f;
                }
            }

            Frighten(inSight && Watching, delta);
        }

        private void Watch(bool inSight, float delta)
        {
            _watched += delta;
            if (inSight)
            {
                _inSight += delta;
                _outOfSight = 0f;
            }
            else
            {
                _outOfSight += delta;
                if (!Seen) _inSight = 0f;
                // It turns to keep facing Rivali, but never while it's watched.
                FacePlayer();
            }

            if (!Seen && _inSight >= NoticeTime)
            {
                Seen = true;
                Sightings++;
                Fear = 1f;
                if (Sting != null) Sound.Play(Sting, 0.9f);
            }

            float distance = HorizontalDistance();
            if (Seen && inSight && (_inSight > StareLimit || distance < TooClose))
            {
                Vanish(blackout: true);
            }
            else if (!inSight && ((Seen && _outOfSight > 0.5f) || _watched > MaxWatch || distance < TooClose * 0.6f))
            {
                Vanish(blackout: false);
            }
        }

        /// <summary>Gone: in a blackout when it's being looked at, so it's never seen going.</summary>
        private void Vanish(bool blackout)
        {
            if (blackout)
            {
                _blackout = 0.18f;
                _torch?.Sputter(0.5f);
            }

            Hide();
            Watching = false;
            _wait = NextWait();
        }

        private void Hide()
        {
            if (Body != null) Body.Active = false;
        }

        private float NextWait() => MinWait + Random.Shared.NextSingle() * MathF.Max(0f, MaxWait - MinWait);

        /// <summary>Drives the picture: interference while it's in sight, worse the longer it's stared at, easing off after.</summary>
        private void Frighten(bool inSight, float delta)
        {
            float target = !inSight ? 0f : Seen ? 0.35f + 0.65f * MathF.Min(1f, _inSight / StareLimit) : 0.18f;
            Fear = target > Fear ? MathF.Min(target, Fear + delta * 3f) : MathF.Max(target, Fear - delta * 0.45f);
            _blackout = MathF.Max(0f, _blackout - delta);

            if (_effect == null) return;
            _effect.Set("Fear", Fear);
            _effect.Set("Blackout", _blackout > 0f ? 1f : 0f);
        }

        /// <summary>Whether Rivali's looking its way and nothing solid is in between (glass doesn't count).</summary>
        private bool InSight()
        {
            GameObject? head = _player?.Head;
            if (head == null || Body == null) return false;

            Vector3 eye = head.Transform.WorldPosition;
            Vector3 forward = head.Transform.Forward;
            foreach (float height in new[] { 2.4f, 1.4f })
            {
                Vector3 to = Transform.WorldPosition + new Vector3(0f, height, 0f) - eye;
                float distance = to.Length();
                if (distance < 0.01f) return true;
                if (Vector3.Dot(to / distance, forward) < MathF.Cos(28f * MathF.PI / 180f)) continue;
                if (ClearLine(eye, to / distance, distance)) return true;
            }

            return false;
        }

        private bool ClearLine(Vector3 from, Vector3 direction, float distance)
        {
            // Through up to a few panes of glass.
            for (int i = 0; i < 4; i++)
            {
                if (!GameObject.Scene.Physics.Raycast(from, direction, distance, out RaycastHit hit, _playerBody)) return true;
                if (hit.GameObject.GetComponent<SeeThrough>() == null) return false;
                from = hit.Point + direction * 0.02f;
                distance -= hit.Distance + 0.02f;
                if (distance <= 0f) return true;
            }

            return false;
        }

        /// <summary>A spot at the right distance, out of Rivali's view, and not the last one.</summary>
        private GameObject? ChooseSpot()
        {
            if (_player?.Head == null) return null;
            Vector3 eye = _player.Head.Transform.WorldPosition;
            Vector3 forward = _player.Head.Transform.Forward;

            var choices = new List<GameObject>();
            foreach (GameObject spot in Spots)
            {
                if (spot == null || spot == _spot) continue;
                Vector3 to = spot.Transform.WorldPosition + new Vector3(0f, 1.6f, 0f) - eye;
                float distance = to.Length();
                if (distance < NearestSpot || distance > FurthestSpot) continue;
                if (Vector3.Dot(to / distance, forward) > MathF.Cos(55f * MathF.PI / 180f)) continue;
                choices.Add(spot);
            }

            return choices.Count == 0 ? null : choices[Random.Shared.Next(choices.Count)];
        }

        private void FacePlayer()
        {
            if (_player == null) return;
            Vector3 to = _player.Transform.WorldPosition - Transform.WorldPosition;
            if (MathF.Abs(to.X) + MathF.Abs(to.Z) < 0.001f) return;
            Transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.Atan2(-to.X, -to.Z));
        }

        private float HorizontalDistance()
        {
            if (_player == null) return float.MaxValue;
            Vector3 to = _player.Transform.WorldPosition - Transform.WorldPosition;
            return new Vector2(to.X, to.Z).Length();
        }
    }
}
