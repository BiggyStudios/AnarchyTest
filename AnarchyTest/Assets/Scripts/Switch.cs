using Vertex;
using Vertex.Audio;
using Vertex.Rendering;

namespace AnarchyTest
{
    /// <summary>A lever or button that unlocks and opens <see cref="Doors"/> once it's used: a blast door's override.</summary>
    public class Switch : Interactable
    {
        /// <summary>What E does, shown on screen ("Pull the override").</summary>
        public string Action { get; set; } = "Use";

        public List<SlidingDoor> Doors { get; set; } = new();

        /// <summary>Shown at the bottom of the screen once it's used.</summary>
        public string Message { get; set; } = "";

        /// <summary>A light that goes from red to blue once it's used. It needs a material of its own.</summary>
        public MeshRenderer? Light { get; set; }

        public AudioClip? Sound { get; set; }

        public bool Used { get; private set; }

        public override string Prompt => Action;
        public override bool Available => !Used;

        public override void Use(Interactor by)
        {
            Used = true;
            foreach (SlidingDoor door in Doors) door?.OpenNow();
            if (Light != null) Light.Material.Emission = new Color(0.3f, 0.85f, 1f);
            if (Sound != null) Vertex.Audio.Sound.PlayAt(Sound, FocusPoint, 0.8f);
            by.GetComponent<Inventory>()?.Notice(Message);
        }
    }
}
