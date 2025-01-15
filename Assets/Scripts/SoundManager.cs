using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Sound Effects")]
    public AudioClip moveSound;
    public AudioClip rotateSound;
    public AudioClip rowClearSound;
    public AudioClip tetrimino_4_RowSound;
    public AudioClip dropSound;
    public AudioClip gameOverSound;

    private AudioSource audioSource;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void PlayBackgroundMusic(AudioClip music, bool loop = true)
    {
        if (audioSource != null)
        {
            audioSource.loop = loop;
            audioSource.clip = music;
            audioSource.Play();
        }
    }
}
