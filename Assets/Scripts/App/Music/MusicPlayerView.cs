using UnityEngine;

public class MusicPlayerView : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            return;
        }

        audioSource.loop = true;
        audioSource.Play();
    }

    public void Play(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void Stop()
    {
        audioSource.Stop();
        audioSource.clip = null;
    }
}
