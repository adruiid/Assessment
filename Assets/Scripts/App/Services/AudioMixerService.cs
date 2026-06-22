using UnityEngine;
using UnityEngine.Audio;

public interface IAudioMixerService
{
    void SetMasterVolume(float value);
    void SetMusicVolume(float value);
    void SetSfxVolume(float value);
}

public class AudioMixerService : IAudioMixerService
{
    private const string MasterVolumeParameter = "MasterVolume";
    private const string MusicVolumeParameter = "MusicVolume";
    private const string SfxVolumeParameter = "SfxVolume";
    private const float MutedDecibels = -80f;

    private readonly AudioMixer audioMixer;

    public AudioMixerService(AudioMixer audioMixer)
    {
        this.audioMixer = audioMixer;
    }

    public void SetMasterVolume(float value)
    {
        SetMixerVolume(MasterVolumeParameter, value);
    }

    public void SetMusicVolume(float value)
    {
        SetMixerVolume(MusicVolumeParameter, value);
    }

    public void SetSfxVolume(float value)
    {
        SetMixerVolume(SfxVolumeParameter, value);
    }

    private void SetMixerVolume(string parameterName, float value)
    {
        if (audioMixer == null)
        {
            return;
        }

        audioMixer.SetFloat(parameterName, ToDecibels(Clamp01(value)));
    }

    private float ToDecibels(float value)
    {
        if (value <= 0.0001f)
        {
            return MutedDecibels;
        }

        return Mathf.Log10(value) * 20f;
    }

    private float Clamp01(float value)
    {
        if (value < 0f)
        {
            return 0f;
        }

        if (value > 1f)
        {
            return 1f;
        }

        return value;
    }
}
