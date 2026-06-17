using UnityEngine;

public interface IAudioMixerService
{
    void SetMasterVolume(float value);
    void SetMusicVolume(float value);
    void SetSfxVolume(float value);
}

public class AudioMixerService : IAudioMixerService
{
    private float musicVolume = 1f;
    private float sfxVolume = 1f;

    public void SetMasterVolume(float value)
    {
        AudioListener.volume = Clamp01(value);
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Clamp01(value);
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Clamp01(value);
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
