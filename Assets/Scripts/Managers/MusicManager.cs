using System;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance{ get; private set;}

    private const int MUSIC_VOLUME_MAX = 10;

    private static float musicTime;
    private static int musicVolume = 4; 
    private AudioSource musicAudioSource;

    public event EventHandler onMusicVolumeChanged;

    private void Awake()
    {
        Instance = this;
        musicAudioSource = GetComponent<AudioSource>();
        musicAudioSource.time = musicTime;
    }

    private void Start()
    {
        musicAudioSource.volume = GetMusicVolumeNormalized();
    }

    private void Update()
    {
        musicTime = musicAudioSource.time;
    }

    public void ChangeMusicVolume()
    {
        musicVolume = (musicVolume + 1) % MUSIC_VOLUME_MAX;
        musicAudioSource.volume = GetMusicVolumeNormalized();
        onMusicVolumeChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetMusicVolume()
    {
        return musicVolume;
    }

    public float GetMusicVolumeNormalized()
    {
        return ((float)musicVolume) / MUSIC_VOLUME_MAX;
    } 
}
