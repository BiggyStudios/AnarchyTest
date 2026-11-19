using Vertex;
using Vertex.Audio;

namespace AnarchyTest
{
    /// <summary>The facility's hum, always there under everything else.</summary>
    public class Ambience : Component
    {
        [Range(0f, 1f)]
        public float Volume { get; set; } = 0.35f;

        private AudioSource? _source;

        protected override void Start()
        {
            _source = AddComponent<AudioSource>();
            _source.PlayOnStart = false;
            _source.InWorld = false;
            _source.Loop = true;
            _source.Sound = Sounds.Hum;
            _source.Volume = Volume;
            _source.Play();
        }

        protected override void Update()
        {
            if (_source != null) _source.Volume = Volume;
        }
    }
}
