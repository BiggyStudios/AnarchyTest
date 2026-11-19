using System.Numerics;

using Vertex;
using Vertex.Input;
using Vertex.Physics;
using Vertex.UI;

namespace AnarchyTest
{
    /// <summary>
    /// Lets Rivali use things: finds the <see cref="Interactable"/> being looked at, near enough and not behind a wall,
    /// shows what E would do with it, and does it when E is pressed. Put it on the player.
    /// </summary>
    public class Interactor : Component
    {
        /// <summary>The eyes it looks from (the camera's object).</summary>
        public GameObject? Head { get; set; }

        /// <summary>How far away things can be used from, in metres.</summary>
        [Range(0.5f, 5f)]
        public float Reach { get; set; } = 2.2f;

        /// <summary>What's being looked at and could be used now, if anything.</summary>
        public Interactable? Target { get; private set; }

        private Player? _player;
        private Collider? _body;
        private float _shown;
        // What the prompt said last, kept while it fades out.
        private string _lastPrompt = "";

        protected override void Start()
        {
            _player = GetComponent<Player>();
            _body = GetComponent<CharacterController>();
        }

        protected override void Update()
        {
            bool free = _player?.CanMove ?? true;
            Target = free && Head != null ? Find(Head.Transform.WorldPosition, Head.Transform.Forward) : null;

            if (Target != null && InputSystem.GetKeyDown(Key.E))
            {
                Target.Use(this);
                // It may have changed what it does ("Open" no more once it's open), so look again next frame.
                Target = null;
            }

            _shown = MathF.Max(0f, MathF.Min(1f, _shown + (Target != null ? 8f : -6f) * Time.DeltaTime));
            if (_shown > 0f) DrawPrompt();
        }

        private Interactable? Find(Vector3 eye, Vector3 look)
        {
            Interactable? best = null;
            float bestAlong = float.MaxValue;
            foreach (Interactable thing in GameObject.Scene.FindComponents<Interactable>())
            {
                if (!thing.Available) continue;
                Vector3 to = thing.FocusPoint - eye;
                float along = Vector3.Dot(to, look);
                if (along <= 0f || along > Reach + thing.Radius) continue;

                // How far the line of sight passes from it.
                float off = (to - look * along).Length();
                if (off > thing.Radius || along >= bestAlong) continue;
                if (Blocked(eye, thing, to.Length())) continue;

                best = thing;
                bestAlong = along;
            }

            return best;
        }

        /// <summary>Whether something solid is between the eyes and the thing (other than the thing itself).</summary>
        private bool Blocked(Vector3 eye, Interactable thing, float distance)
        {
            if (!GameObject.Scene.Physics.Raycast(eye, thing.FocusPoint - eye, distance, out RaycastHit hit, _body)) return false;
            if (hit.Distance >= distance - thing.Radius) return false;
            return !Within(hit.GameObject, thing.GameObject);
        }

        private static bool Within(GameObject part, GameObject whole)
        {
            for (Transform? at = part.Transform; at != null; at = at.Parent)
            {
                if (at.GameObject == whole) return true;
            }

            return false;
        }

        /// <summary>A small dot in the middle, and under it the key and what it does.</summary>
        private void DrawPrompt()
        {
            if (Target != null) _lastPrompt = Target.Prompt;
            var centre = new Vector2(Canvas.Width * 0.5f, Canvas.Height * 0.5f);
            var ink = new Color(0.9f, 0.93f, 0.95f, 0.85f * _shown);

            Canvas.DrawRect(centre - new Vector2(1.5f), new Vector2(3f), ink);

            const float size = 17f;
            Vector2 word = Canvas.MeasureText(Font.Default, _lastPrompt, size);
            const float key = 22f, gap = 8f;
            float width = key + gap + word.X;
            var at = new Vector2(centre.X - width * 0.5f, centre.Y + 34f);
            Canvas.DrawRectOutline(at, new Vector2(key), ink, 1.5f, 3f);
            Vector2 e = Canvas.MeasureText(Font.Default, "E", size);
            Canvas.DrawText(Font.Default, "E", at + new Vector2((key - e.X) * 0.5f, (key - Canvas.LineHeight(Font.Default, size)) * 0.5f), ink, size);
            Canvas.DrawText(Font.Default, _lastPrompt, at + new Vector2(key + gap, (key - Canvas.LineHeight(Font.Default, size)) * 0.5f), ink, size, shadow: true);
        }
    }
}
