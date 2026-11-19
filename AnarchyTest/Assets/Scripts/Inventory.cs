using System.Numerics;

using Vertex;
using Vertex.UI;

namespace AnarchyTest
{
    /// <summary>
    /// What Rivali's carrying (keycards, for now), and a line of text at the bottom of the screen for what just
    /// happened: "Took the staff keycard", "Locked". Put it on the player.
    /// </summary>
    public class Inventory : Component
    {
        /// <summary>The keycards picked up, by name ("staff", "theatre").</summary>
        public List<string> Cards { get; set; } = new();

        /// <summary>The line being shown, if any.</summary>
        public string Shown => _notice;

        private string _notice = "";
        private float _noticeLeft;

        public bool Has(string card) => card.Length > 0 && Cards.Contains(card);

        public void Add(string card)
        {
            if (card.Length > 0 && !Cards.Contains(card)) Cards.Add(card);
        }

        /// <summary>Shows <paramref name="text"/> at the bottom of the screen for a few seconds.</summary>
        public void Notice(string text)
        {
            if (text.Length == 0) return;
            _notice = text;
            _noticeLeft = 3.5f;
        }

        protected override void Update()
        {
            if (_noticeLeft <= 0f) return;
            _noticeLeft -= Time.DeltaTime;
            float fade = MathF.Min(1f, _noticeLeft / 0.6f);

            const float size = 18f;
            Vector2 measured = Canvas.MeasureText(Font.Default, _notice, size);
            var at = new Vector2((Canvas.Width - measured.X) * 0.5f, Canvas.Height - 96f);
            Canvas.DrawText(Font.Default, _notice, at, new Color(0.88f, 0.92f, 0.95f, 0.9f * fade), size, shadow: true);
        }
    }
}
