using Vertex;

namespace AnarchyTest
{
    /// <summary>
    /// Something written that Rivali can read: a log on a terminal, a message on a tablet. Each is a text file in
    /// Assets/Text, named by its key: the first line is its title, the rest its text, and a line of just "---" starts a
    /// new page. One that can't be found reads as corrupted.
    /// </summary>
    public sealed record Document(string Title, IReadOnlyList<string> Pages)
    {
        public static Document Load(string key)
        {
            string path = Application.AssetPath("Text", key + ".txt");
            if (key.Length == 0 || !File.Exists(path)) return Corrupted;

            string[] lines = File.ReadAllText(path).Replace("\r\n", "\n").Split('\n');
            var pages = new List<string>();
            var page = new List<string>();
            foreach (string line in lines.Skip(1))
            {
                if (line.Trim() == "---")
                {
                    pages.Add(string.Join("\n", page).Trim('\n'));
                    page.Clear();
                }
                else
                {
                    page.Add(line);
                }
            }

            pages.Add(string.Join("\n", page).Trim('\n'));
            return new Document(lines[0].Trim(), pages);
        }

        /// <summary>What's shown when the file isn't there.</summary>
        public static readonly Document Corrupted = new("FILE ERROR", ["The record can't be read.\n\nSECTOR CHECKSUM FAILED\nDATA CORRUPTED"]);
    }
}
