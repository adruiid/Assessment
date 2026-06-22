using UnityEngine;

public class SfxPlayerView : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void PlayOnce()
    {
        if (audioSource == null)
        {
            return;
        }

        audioSource.Stop();
        audioSource.loop = false;
        audioSource.Play();
    }
}
