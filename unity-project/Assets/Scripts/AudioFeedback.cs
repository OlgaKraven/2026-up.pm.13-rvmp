using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioFeedback : MonoBehaviour
{
    public AudioClip collectClip, warningClip, successClip, failureClip;
    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        if (collectClip == null) collectClip = Tone(880f);
        if (warningClip == null) warningClip = Tone(220f);
        if (successClip == null) successClip = Tone(1320f);
        if (failureClip == null) failureClip = Tone(110f);
    }

    public void PlayCollect() { source.PlayOneShot(collectClip); }
    public void PlayHazard() { source.PlayOneShot(warningClip); }
    public void PlaySuccess() { source.PlayOneShot(successClip); }
    public void PlayFailure() { source.PlayOneShot(failureClip); }

    private static AudioClip Tone(float frequency)
    {
        const int rate = 44100;
        float[] samples = new float[rate / 6];
        for (int i = 0; i < samples.Length; i++)
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * i / rate) * 0.2f * (1f - (float)i / samples.Length);
        AudioClip clip = AudioClip.Create("Учебный сигнал", samples.Length, 1, rate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
