using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource _audioSource;

    private void Awake()
    {
        // If an instance already exists and it's not this one, destroy this duplicate
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Optional: auto-play on start
        _audioSource.Play();
    }

    public void PlayMusic()
    {
        if (!_audioSource.isPlaying)
            _audioSource.Play();
    }

    public void StopMusic()
    {
        _audioSource.Stop();
    }

    public void ChangeMusic(AudioClip newClip)
    {
        _audioSource.clip = newClip;
        _audioSource.Play();
    }
}   