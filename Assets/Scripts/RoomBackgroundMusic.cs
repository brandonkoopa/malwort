using UnityEngine;

// [RequireComponent(typeof(Room))]
public class RoomBackgroundMusic : MonoBehaviour
{
    [Tooltip("The music to play when this room becomes active.")]
    public AudioClip roomMusic; // Drag your mp3 or audio clip here in the Inspector

    private MoreMountains.CorgiEngine.Room _room;
    private bool _wasActive = false;

    void Awake()
    {
        Debug.Log($"[{name}] RoomBackgroundMusic.Awake()");
        _room = GetComponent<MoreMountains.CorgiEngine.Room>();
        if (_room == null)
        {
            Debug.LogError($"[{name}] RoomBackgroundMusic: Room component NOT FOUND on this GameObject!");
        }
        else
        {
            Debug.Log($"[{name}] RoomBackgroundMusic: Room component found.");
        }
    }

    void Update()
    {
        if (_room == null) return;

        bool isActive = _room.CurrentRoom;
        Debug.Log($"[{name}] RoomBackgroundMusic.Update() - Room.CurrentRoom: {isActive}, _wasActive: {_wasActive}");

        if (isActive && !_wasActive)
        {
            Debug.Log($"[{name}] Room just became ACTIVE. Playing music.");
            PlayRoomMusic();
        }
        else if (!isActive && _wasActive)
        {
            Debug.Log($"[{name}] Room just became INACTIVE. Stopping music.");
            StopRoomMusic();
        }

        _wasActive = isActive;
    }

    // Call this when the room becomes active
    public void PlayRoomMusic()
    {
        Debug.Log($"[{name}] PlayRoomMusic() called. roomMusic assigned: {roomMusic != null}");
        if (roomMusic != null && RoomMusicManager.Instance != null)
        {
            RoomMusicManager.Instance.PlayRoomMusic(roomMusic);
        }
        else
        {
            Debug.LogWarning($"[{name}] PlayRoomMusic() - Missing roomMusic or RoomMusicManager.Instance");
        }
    }

    public void StopRoomMusic()
    {
        Debug.Log($"[{name}] StopRoomMusic() called.");
        if (RoomMusicManager.Instance != null)
        {
            RoomMusicManager.Instance.PlayRoomMusic(null); // Stops music
        }
        else
        {
            Debug.LogWarning($"[{name}] StopRoomMusic() - RoomMusicManager.Instance is null");
        }
    }
} 