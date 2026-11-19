using System.Numerics;

using Vertex;
using Vertex.Input;
using Vertex.UI;

namespace AnarchyTest
{
    /// <summary>
    /// Shows a <see cref="Document"/> over the game, a page at a time, as an old screen would: A and D (or the arrow keys)
    /// turn the pages, and E or Escape puts it away. Rivali stands still while reading. Put it on the player.
    /// </summary>
    public class Reader : Component
    {
        /// <summary>What's being read, if anything.</summary>
        public Document? Open { get; private set; }

        public int Page { get; private set; }

        private const float Width = 620f, Height = 440f, TextSize = 17f;
        private static readonly Color Ink = new(0.8f, 0.9f, 0.95f);
        private static readonly Color Glow = new(0.3f, 0.85f, 1f);

        private Player? _player;
        private bool _justOpened;
        private bool _closing;
        private float _shown;

        protected override void Start() => _player = GetComponent<Player>();

        public void Show(Document document)
        {
            Open = document;
            Page = 0;
            _justOpened = true;
            _closing = false;
            if (_player != null) _player.CanMove = false;
        }

        protected override void Update()
        {
            // Rivali can move again the frame after it's put away, so the key that put it away does nothing else.
            if (_closing)
            {
                _closing = false;
                if (_player != null) _player.CanMove = true;
            }

            if (Open != null && !_justOpened) Turn(Open);
            _justOpened = false;

            _shown = MathF.Max(0f, MathF.Min(1f, _shown + (Open != null ? 10f : -10f) * Time.DeltaTime));
            if (Open != null) Draw(Open);
        }

        private void Turn(Document document)
        {
            if (InputSystem.GetKeyDown(Key.E) || InputSystem.GetKeyDown(Key.Escape))
            {
                Open = null;
                _closing = true;
                return;
            }

            if ((InputSystem.GetKeyDown(Key.D) || InputSystem.GetKeyDown(Key.Right)) && Page < document.Pages.Count - 1) Page++;
            if ((InputSystem.GetKeyDown(Key.A) || InputSystem.GetKeyDown(Key.Left)) && Page > 0) Page--;
        }

        private void Draw(Document document)
        {
            float fade = _shown;
            Canvas.DrawRect(Vector2.Zero, Canvas.Size, new Color(0f, 0f, 0f, 0.55f * fade));

            var at = new Vector2((Canvas.Width - Width) * 0.5f, (Canvas.Height - Height) * 0.5f);
            Canvas.DrawRect(at, new Vector2(Width, Height), new Color(0.02f, 0.05f, 0.07f, 0.94f * fade));
            Canvas.DrawRectOutline(at, new Vector2(Width, Height), Tint(Glow, 1f, 0.6f * fade), 1.5f);

            // The title in a bar across the top.
            Canvas.DrawRect(at + new Vector2(1.5f), new Vector2(Width - 3f, 34f), Tint(Glow, 0.25f, fade));
            Canvas.DrawText(Font.DefaultBold, document.Title, at + new Vector2(18f, 7f), Tint(Glow, 1f, fade), 17f);

            float y = at.Y + 52f;
            float lineHeight = Canvas.LineHeight(Font.Default, TextSize);
            foreach (string line in Wrap(document.Pages[Page], Width - 40f))
            {
                if (y + lineHeight > at.Y + Height - 40f) break;
                Canvas.DrawText(Font.Default, line, new Vector2(at.X + 20f, y), Tint(Ink, 1f, fade), TextSize);
                y += lineHeight;
            }

            // Scan lines over it all.
            for (float line = at.Y + 2f; line < at.Y + Height - 2f; line += 3f)
                Canvas.DrawRect(new Vector2(at.X + 2f, line), new Vector2(Width - 4f, 1f), new Color(0f, 0f, 0f, 0.18f * fade));

            string footer = document.Pages.Count > 1 ? $"{Page + 1} / {document.Pages.Count}      A  D  turn page      E  close" : "E  close";
            Vector2 size = Canvas.MeasureText(Font.Default, footer, 14f);
            Canvas.DrawText(Font.Default, footer, new Vector2(at.X + Width - 18f - size.X, at.Y + Height - 28f), Tint(Glow, 0.8f, fade), 14f);
        }

        /// <summary><paramref name="colour"/> made <paramref name="brightness"/> as bright, and see-through by <paramref name="alpha"/>.</summary>
        private static Color Tint(Color colour, float brightness, float alpha) =>
            new(colour.R * brightness, colour.G * brightness, colour.B * brightness, alpha);

        /// <summary>The text broken into lines no wider than <paramref name="width"/>, keeping its own line breaks.</summary>
        private static IEnumerable<string> Wrap(string text, float width)
        {
            foreach (string paragraph in text.Split('\n'))
            {
                string line = "";
                foreach (string word in paragraph.Split(' '))
                {
                    string longer = line.Length == 0 ? word : line + " " + word;
                    if (line.Length > 0 && Canvas.MeasureText(Font.Default, longer, TextSize).X > width)
                    {
                        yield return line;
                        line = word;
                    }
                    else
                    {
                        line = longer;
                    }
                }

                yield return line;
            }
        }
    }
}
