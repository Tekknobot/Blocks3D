using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource audioSource; // Reference to the AudioSource component

    private List<AudioClip> musicClips = new List<AudioClip>();

    void Start()
    {
        LoadMusicClips();
        PlayRandomSong();
    }

    // Load all audio clips from the Music folder
    private void LoadMusicClips()
    {
        // Load all audio clips from the specified folder
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Audio/Music");
        musicClips.AddRange(clips);

        if (musicClips.Count == 0)
        {
            Debug.LogError("No music found in the Assets/Resources/Audio/Music folder!");
        }
    }

    // Play a random song
    private void PlayRandomSong()
    {
        if (musicClips.Count == 0) return;

        // Pick a random clip
        AudioClip randomClip = musicClips[Random.Range(0, musicClips.Count)];

        // Assign and play the random clip
        audioSource.clip = randomClip;
        audioSource.Play();

        // Schedule the next song to play when this one ends
        StartCoroutine(PlayNextSongAfterDelay(randomClip.length));
    }

    // Coroutine to play the next song after the current one ends
    private IEnumerator PlayNextSongAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayRandomSong();
    }
}
