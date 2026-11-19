using Vertex.Audio;

namespace AnarchyTest
{
    /// <summary>
    /// The game's sounds, made in code for now: the facility's hum, doors, the lock's refusal, a terminal's chirp, and
    /// the sting when the white figure is seen. Each is made the first time it's wanted.
    /// </summary>
    public static class Sounds
    {
        private const int Rate = 22050;

        public static AudioClip Hum => _hum ??= Make(MakeHum);
        public static AudioClip DoorSlide => _door ??= Make(MakeDoor);
        public static AudioClip Denied => _denied ??= Make(MakeDenied);
        public static AudioClip Chirp => _chirp ??= Make(MakeChirp);
        public static AudioClip Sting => _sting ??= Make(MakeSting);

        private static AudioClip? _hum, _door, _denied, _chirp, _sting;

        private static AudioClip Make(Func<float[]> make) => AudioClip.FromSamples(make(), Rate);

        /// <summary>
        /// Eight seconds that loop without a seam: mains hum and its overtones, breathing slowly, over air moving in the
        /// vents.
        /// </summary>
        private static float[] MakeHum()
        {
            int count = Rate * 8;
            var samples = new float[count];
            var random = new Random(17);
            float low = 0f, lower = 0f;
            float[] air = new float[count];
            for (int i = 0; i < count; i++)
            {
                // Noise smoothed twice: a dull rush rather than a hiss.
                low += (random.NextSingle() * 2f - 1f - low) * 0.05f;
                lower += (low - lower) * 0.05f;
                air[i] = lower;
            }

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                // Whole numbers of cycles in eight seconds, so the end meets the start.
                float breathe = 0.8f + 0.2f * MathF.Sin(MathF.Tau * t / 8f);
                float hum = MathF.Sin(MathF.Tau * 50f * t) * 0.5f + MathF.Sin(MathF.Tau * 100f * t) * 0.3f + MathF.Sin(MathF.Tau * 150f * t) * 0.12f;
                // The air crossfades with itself half a loop on, so it has no seam either.
                float mix = MathF.Sin(MathF.PI * i / count);
                float vent = air[i] * mix + air[(i + count / 2) % count] * (1f - mix);
                samples[i] = (hum * 0.18f * breathe + vent * 2.2f) * 0.6f;
            }

            return samples;
        }

        /// <summary>A sliding door: a hiss of air as it goes, and a dull knock as it stops.</summary>
        private static float[] MakeDoor()
        {
            int count = (int)(Rate * 1.1f);
            var samples = new float[count];
            var random = new Random(5);
            float low = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                low += (random.NextSingle() * 2f - 1f - low) * 0.25f;
                float hiss = low * Envelope(t, 0.05f, 0.8f) * 0.9f;
                float stop = t > 0.8f ? MathF.Sin(MathF.Tau * 70f * (t - 0.8f)) * MathF.Exp(-(t - 0.8f) * 18f) * 0.8f : 0f;
                float motor = MathF.Sin(MathF.Tau * (120f + t * 40f) * t) * 0.06f * Envelope(t, 0.05f, 0.8f);
                samples[i] = hiss + stop + motor;
            }

            return samples;
        }

        /// <summary>Two low, flat beeps: no.</summary>
        private static float[] MakeDenied()
        {
            int count = (int)(Rate * 0.42f);
            var samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                bool on = t < 0.13f || (t > 0.2f && t < 0.33f);
                float square = MathF.Sign(MathF.Sin(MathF.Tau * 196f * t)) * 0.5f + MathF.Sin(MathF.Tau * 196f * t) * 0.5f;
                samples[i] = on ? square * 0.22f : 0f;
            }

            return Smooth(samples, 0.3f);
        }

        /// <summary>A terminal waking: a short rising chirp.</summary>
        private static float[] MakeChirp()
        {
            int count = (int)(Rate * 0.12f);
            var samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                samples[i] = MathF.Sin(MathF.Tau * (900f + t * 2500f) * t) * Envelope(t, 0.005f, 0.11f) * 0.25f;
            }

            return samples;
        }

        /// <summary>
        /// Seeing it: a deep blow, low notes clashing as they swell, and a thin metallic shriek over them, all dying away.
        /// </summary>
        private static float[] MakeSting()
        {
            int count = Rate * 3;
            var samples = new float[count];
            var random = new Random(41);
            float[] cluster = [55f, 58.3f, 82.4f, 87.3f, 116.5f, 233f];
            float low = 0f, band = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float blow = MathF.Sin(MathF.Tau * 38f * t) * MathF.Exp(-t * 3f) * 0.9f;

                float notes = 0f;
                foreach (float note in cluster) notes += MathF.Sin(MathF.Tau * note * t + note);
                notes *= MathF.Min(1f, t * 6f) * MathF.Exp(-t * 0.9f) * 0.12f;

                // Noise through a narrow, rising resonance: the shriek.
                float pitch = 2200f + t * 600f;
                float f = 2f * MathF.Sin(MathF.PI * pitch / Rate);
                low += f * band;
                float high = random.NextSingle() * 2f - 1f - low - band * 0.1f;
                band += f * high;
                float shriek = band * Envelope(t, 0.02f, 2.4f) * 0.08f;

                samples[i] = blow + notes + shriek;
            }

            return Loudest(samples, 0.85f);
        }

        /// <summary>Up over <paramref name="attack"/> seconds, then fading out until <paramref name="end"/>.</summary>
        private static float Envelope(float t, float attack, float end) =>
            t < attack ? t / attack : t > end ? 0f : 1f - (t - attack) / (end - attack);

        /// <summary>Scaled so its loudest moment reaches <paramref name="peak"/>.</summary>
        private static float[] Loudest(float[] samples, float peak)
        {
            float most = samples.Max(MathF.Abs);
            if (most > 0f)
            {
                for (int i = 0; i < samples.Length; i++) samples[i] *= peak / most;
            }

            return samples;
        }

        private static float[] Smooth(float[] samples, float amount)
        {
            float last = 0f;
            for (int i = 0; i < samples.Length; i++) samples[i] = last += (samples[i] - last) * amount;
            return samples;
        }
    }
}
