using UnityEngine;
using Unity.Cinemachine;
using MoreMountains.CorgiEngine;

public class PlayerSceneSpawnHandler : MonoBehaviour
{
    void Start()
    {
        // Move player to the correct position if set
        var player = FindObjectOfType<Character>();
        if (player != null)
        {
            // First try to find the target teleporter if specified
            if (!string.IsNullOrEmpty(DoorTransitionData.TargetTeleporterName))
            {
                var teleporters = FindObjectsOfType<Teleporter>();
                foreach (var teleporter in teleporters)
                {
                    if (teleporter.name == DoorTransitionData.TargetTeleporterName)
                    {
                        Debug.Log($"Found target teleporter: {teleporter.name}, moving player there.");
                        player.transform.position = teleporter.transform.position;
                        break;
                    }
                }
            }
            // If no teleporter found or specified, use the direct position
            else if (DoorTransitionData.TargetPosition != Vector3.zero)
            {
                Debug.Log($"Moving player to target position: {DoorTransitionData.TargetPosition}");
                player.transform.position = DoorTransitionData.TargetPosition;
            }
            else
            {
                Debug.LogWarning("No spawn position or teleporter specified in DoorTransitionData");
            }
        }
        else
        {
            Debug.LogWarning("Could not find player character to reposition");
        }

        // Activate the correct camera
        if (!string.IsNullOrEmpty(DoorTransitionData.TargetCameraName))
        {
            foreach (var cam in FindObjectsOfType<CinemachineVirtualCamera>())
            {
                cam.gameObject.SetActive(cam.name == DoorTransitionData.TargetCameraName);
            }
        }

        // Clear the transition data after use
        DoorTransitionData.Clear();
    }
} 