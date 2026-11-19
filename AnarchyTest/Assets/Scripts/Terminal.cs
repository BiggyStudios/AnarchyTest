using Vertex;
using Vertex.Audio;

namespace AnarchyTest
{
    /// <summary>A screen or tablet with something on it to read. Its text is a file in Assets/Text (see <see cref="Document"/>).</summary>
    public class Terminal : Interactable
    {
        /// <summary>The name of its text file in Assets/Text, without ".txt".</summary>
        public string Document { get; set; } = "";

        /// <summary>Played as it wakes up.</summary>
        public AudioClip? Sound { get; set; }

        public override string Prompt => "Read";

        public override void Use(Interactor by)
        {
            Reader? reader = by.GetComponent<Reader>();
            if (reader == null) return;
            if (Sound != null) Vertex.Audio.Sound.PlayAt(Sound, FocusPoint, 0.5f);
            reader.Show(AnarchyTest.Document.Load(Document));
        }
    }
}
