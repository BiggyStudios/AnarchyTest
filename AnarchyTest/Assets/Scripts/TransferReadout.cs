using Vertex;
using Vertex.Rendering;

namespace AnarchyTest
{
    /// <summary>The screen over Rivali's bed: how far the transfer has gone. It goes up each time the white figure catches Rivali.</summary>
    public class TransferReadout : Component
    {
        [Range(0, 100)]
        public int Transfer { get; set; } = 97;

        private WorldText? _text;

        protected override void Start() => _text = GetComponent<WorldText>();

        protected override void Update()
        {
            if (_text != null) _text.Text = $"SUBJECT 0417\nNEURAL TRANSFER  {Transfer}%";
        }
    }
}
