using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource sfxSource;
    private AudioSource ambientSource;

    // Generated clips
    private AudioClip clipJewel;
    private AudioClip clipDetected;
    private AudioClip clipVictory;
    private AudioClip clipAmbient;
    private AudioClip clipFootstep;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        sfxSource     = gameObject.AddComponent<AudioSource>();
        ambientSource = gameObject.AddComponent<AudioSource>();

        clipJewel    = GenerateJewelPickup();
        clipDetected = GenerateDetected();
        clipVictory  = GenerateVictory();
        clipAmbient  = GenerateAmbient();
        clipFootstep = GenerateFootstep();

        ambientSource.clip   = clipAmbient;
        ambientSource.loop   = true;
        ambientSource.volume = 0.15f;
        ambientSource.Play();
    }

    public void PlayJewelPickup()    => sfxSource.PlayOneShot(clipJewel, 0.8f);
    public void PlayPlayerDetected() => sfxSource.PlayOneShot(clipDetected, 1f);
    public void PlayVictory()        => sfxSource.PlayOneShot(clipVictory, 0.9f);
    public void PlayFootstep()       => sfxSource.PlayOneShot(clipFootstep, 0.3f);

    // ── Jewel pickup: ascending arpeggio ────────────────────────────────
    private AudioClip GenerateJewelPickup()
    {
        int sr = 44100; float dur = 0.5f;
        int n = (int)(sr * dur);
        float[] data = new float[n];
        float[] freqs = { 523f, 659f, 784f, 1047f };
        int step = n / freqs.Length;
        for (int i = 0; i < n; i++)
        {
            int fi = Mathf.Min(i / step, freqs.Length - 1);
            float env = 1f - (float)(i % step) / step;
            data[i] = Mathf.Sin(2 * Mathf.PI * freqs[fi] * i / sr) * env * 0.5f;
        }
        return MakeClip("Jewel", data, sr);
    }

    // ── Detected: harsh descending buzz ─────────────────────────────────
    private AudioClip GenerateDetected()
    {
        int sr = 44100; float dur = 0.6f;
        int n = (int)(sr * dur);
        float[] data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t   = (float)i / sr;
            float env = Mathf.Exp(-t * 4f);
            float freq = 220f - t * 80f;
            // Saw wave for harsh buzz
            float phase = (freq * t) % 1f;
            data[i] = (phase * 2f - 1f) * env * 0.6f;
        }
        return MakeClip("Detected", data, sr);
    }

    // ── Victory: short fanfare ───────────────────────────────────────────
    private AudioClip GenerateVictory()
    {
        int sr = 44100; float dur = 1.2f;
        int n = (int)(sr * dur);
        float[] data = new float[n];
        float[] notes  = { 523f, 659f, 784f, 1047f, 1319f };
        float[] timing = { 0f, 0.15f, 0.30f, 0.45f, 0.60f };
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / sr;
            float s = 0f;
            for (int ni = 0; ni < notes.Length; ni++)
            {
                float tl = t - timing[ni];
                if (tl < 0f) continue;
                float env = Mathf.Exp(-tl * 5f);
                s += Mathf.Sin(2 * Mathf.PI * notes[ni] * tl) * env;
            }
            data[i] = s * 0.2f;
        }
        return MakeClip("Victory", data, sr);
    }

    // ── Ambient: low drone ───────────────────────────────────────────────
    private AudioClip GenerateAmbient()
    {
        int sr = 44100; float dur = 4f;
        int n = (int)(sr * dur);
        float[] data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / sr;
            data[i] = (Mathf.Sin(2 * Mathf.PI * 55f * t) * 0.3f
                     + Mathf.Sin(2 * Mathf.PI * 82.5f * t) * 0.15f
                     + Mathf.Sin(2 * Mathf.PI * 110f * t) * 0.1f)
                     * (0.5f + 0.5f * Mathf.Sin(2 * Mathf.PI * 0.2f * t));
        }
        return MakeClip("Ambient", data, sr);
    }

    // ── Footstep: short thud ─────────────────────────────────────────────
    private AudioClip GenerateFootstep()
    {
        int sr = 44100; float dur = 0.08f;
        int n = (int)(sr * dur);
        float[] data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / sr;
            float env = Mathf.Exp(-t * 60f);
            data[i] = (Random.value * 2f - 1f) * env * 0.5f;
        }
        return MakeClip("Footstep", data, sr);
    }

    private AudioClip MakeClip(string name, float[] data, int sr)
    {
        var clip = AudioClip.Create(name, data.Length, 1, sr, false);
        clip.SetData(data, 0);
        return clip;
    }
}