using Vertex;
using Vertex.Rendering;

namespace AnarchyTest
{
    /// <summary>
    /// Writing in the world (on a wall, a sign) whose words come from a file in Assets/Text, named by <see cref="Key"/>,
    /// the way screens' do. Put it beside a World Text. With no file, there's nothing written.
    /// </summary>
    public class LocalText : Component
    {
        public string Key { get; set; } = "";

        protected override void Start()
        {
            WorldText? text = GetComponent<WorldText>();
            if (text == null) return;
            string path = Application.AssetPath("Text", Key + ".txt");
            text.Text = Key.Length > 0 && File.Exists(path) ? File.ReadAllText(path).Replace("\r\n", "\n").Trim() : "";
        }
    }
}
