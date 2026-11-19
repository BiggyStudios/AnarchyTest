using Vertex;
using Vertex.Audio;

namespace AnarchyTest
{
    /// <summary>A keycard lying somewhere: E takes it, and doors that want it open (see <see cref="SlidingDoor.Card"/>).</summary>
    public class Pickup : Interactable
    {
        /// <summary>The card's name, as doors ask for it ("staff").</summary>
        public string Card { get; set; } = "";

        /// <summary>What it's called on screen when it's taken ("staff keycard").</summary>
        public string Label { get; set; } = "";

        public AudioClip? Sound { get; set; }

        public override string Prompt => "Take";

        public override void Use(Interactor by)
        {
            Inventory? inventory = by.GetComponent<Inventory>();
            if (inventory == null) return;
            inventory.Add(Card);
            inventory.Notice($"Took the {Label}.");
            if (Sound != null) Vertex.Audio.Sound.PlayAt(Sound, FocusPoint, 0.6f);
            GameObject.Active = false;
        }
    }
}
