using UnityEngine;

public class RoomMusicManager : MonoBehaviour
{
    public static RoomMusicManager Instance { get; private set; }
    private AudioSource _audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                Debug.LogError("RoomMusicManager requires an AudioSource component on the same GameObject.");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayRoomMusic(AudioClip clip)
    {
        if (_audioSource == null || clip == null) return;
        if (_audioSource.clip == clip && _audioSource.isPlaying) return; // Already playing
        _audioSource.clip = clip;
        _audioSource.Play();
    }
} 