using UnityEngine;
using Unity.Cinemachine;
using MoreMountains.Tools; // For Corgi Engine tools like MMSceneLoadingManager
using MoreMountains.CorgiEngine; // For Corgi Engine components like Character, LevelManager
using UnityEngine.SceneManagement;
using System.Collections;

// Require necessary components to ensure they exist on the GameObject
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(ButtonActivated))]
public class DoorController : MonoBehaviour
{
    // Enum to define the door's behavior mode
    public enum DoorMode
    {
        DifferentScene,
        SameScene
    }

    [Header("Door Configuration")]
    public DoorMode mode = DoorMode.DifferentScene; // Default mode

    [Header("Interaction")]
    [Tooltip("Text displayed next to the interaction prompt (e.g., 'Enter Dungeon')")]
    public string interactionPromptText = "Enter";

    [Header("Mode: Different Scene")]
    [Tooltip("Name of the scene to load when activated.")]
    public string targetSceneName;
    [Tooltip("Where the player should spawn in the new scene.")]
    public Vector3 targetSceneSpawnPosition;
    [Tooltip("Name of the Teleporter in the target scene to use as the arrival point.")]
    public string targetSceneTeleporterName;

    [Header("Mode: Same Scene")]
    [Tooltip("The empty GameObject marking the position where the player should teleport.")]
    public Transform targetPositionObject;
    [Tooltip("The CinemachineVirtualCamera for the destination room.")]
    public CinemachineCamera targetRoomCamera;

    [Header("Teleporter")]
    [SerializeField] private Teleporter teleporter;

    // Internal references
    private Animator _animator;
    private ButtonActivated _buttonActivated;
    private bool _canInteract = false; // To prevent accidental double interaction

    // Animator parameter hash for efficiency
    private readonly int _openParameter = Animator.StringToHash("Open");
    private readonly int _closeParameter = Animator.StringToHash("Close");

    void Awake()
    {
        // Get references to required components
        _animator = GetComponent<Animator>();
        _buttonActivated = GetComponent<ButtonActivated>();

        // Ensure ButtonActivated component exists
        if (_buttonActivated == null)
        {
            Debug.LogError("DoorController requires a ButtonActivated component!", this);
            enabled = false; // Disable script if setup is incorrect
            return;
        }
        
        // Ensure LevelManager exists (needed for scene transitions)
        if (LevelManager.Instance == null && mode == DoorMode.DifferentScene)
        {
            Debug.LogError("DoorController: LevelManager instance not found. Scene transitions will not work.", this);
        }
    }

    void Start()
    {
        // Set the custom prompt text - we'll do this in the inspector manually
        // Since we can't find the right property name programmatically
        
        // Initial check for required fields based on mode
        ValidateConfiguration();
    }

    // This will be called by ButtonActivated when the button is pressed
    // Make sure to connect this in the Inspector
    public void ButtonActivated()
    {
        Debug.Log("$ ButtonActivated...");
        ActivateDoor();
    }
    
    // Keep the original Interact method as a backup
    public void Interact()
    {
        Debug.Log("$ Interact...");
        // Play animation
        // _animator.SetTrigger(_closeParameter);
        // After animation (or after a short delay), activate the teleporter
        StartCoroutine(ActivateTeleporterAfterDelay());
    }
    
    // Centralized method to avoid code duplication
    private void ActivateDoor()
    {
        Debug.Log("$ ActivateDoor...");
        if (!_canInteract) return;
        
        // Play the opening animation
        _animator.SetTrigger(_openParameter);
        
        // Start the transition process
        StartCoroutine(TransitionRoutine());
    }

    private System.Collections.IEnumerator TransitionRoutine()
    {
        // Wait for a fixed short delay
        yield return new WaitForSeconds(0.2f); // Adjust delay as needed
        PerformTransition();
    }

    private void PerformTransition()
    {
        switch (mode)
        {
            case DoorMode.DifferentScene:
                Debug.Log("PerformTransition...");
                Debug.Log("Different Scene...");
                Debug.Log("Target Scene Name: " + targetSceneName);
                if (!string.IsNullOrEmpty(targetSceneName))
                {
                    Debug.Log("$, ok we found the Scene...");
                    // Store the target position and teleporter name in the GameManager
                    GameManager.Instance.StorePointsOfEntry(targetSceneName, 0, Character.FacingDirections.Right);
                    Debug.Log("$, about to load scene!...");
                    
                    // Use LevelManager to handle the scene transition if available
                    if (LevelManager.Instance != null)
                    {
                        LevelManager.Instance.GotoLevel(targetSceneName);
                    }
                    else
                    {
                        MMSceneLoadingManager.LoadScene(targetSceneName);
                    }
                }
                else
                {
                    Debug.LogError($"DoorController: Target scene name is not set for door '{gameObject.name}'.", this);
                }
                break;
            case DoorMode.SameScene:
                if (targetPositionObject != null)
                {
                    Character playerCharacter = null;
                    try { playerCharacter = FindFirstObjectByType<Character>(); }
                    catch { playerCharacter = FindObjectOfType<Character>(); }

                    if (playerCharacter != null)
                    {
                        playerCharacter.RespawnAt(targetPositionObject, Character.FacingDirections.Right);

                        // Camera handling
                        if (targetRoomCamera != null)
                        {
                            // Only deactivate the target camera if it's active
                            if (targetRoomCamera.gameObject.activeInHierarchy)
                            {
                                targetRoomCamera.gameObject.SetActive(false);
                            }
                            // Activate the target camera
                            targetRoomCamera.gameObject.SetActive(true);
                            Debug.Log($"Player teleported to {targetPositionObject.name} and camera '{targetRoomCamera.name}' activated.");
                        }
                        else
                        {
                            Debug.LogWarning($"DoorController: No CinemachineVirtualCamera assigned for this door. Player teleported, but camera was not switched.", this);
                        }
                    }
                    else
                    {
                        Debug.LogError($"DoorController: Could not find Player Character to teleport on door '{gameObject.name}'.", this);
                    }
                }
                else
                {
                    Debug.LogError($"DoorController: Target Position Object is not set for door '{gameObject.name}' in SameScene mode.", this);
                }
                break;
        }
    }

    // These methods are called by the ButtonActivated component's internal trigger checks
    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            _canInteract = true;
            _animator.SetTrigger("Open"); // Play Door_Opening
        }
    }

    public void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            _canInteract = false;
            _animator.SetTrigger("Close"); // Play Door_Closed
        }
    }

    // Called when the script is loaded or a value is changed in the inspector
    void OnValidate()
    {
        // Removed the text setting logic since we can't determine the right property programmatically
        // You'll need to set the text in the Inspector manually
        
        ValidateConfiguration(); // Still validate the configuration
    }

    // Helper to check if required fields are set based on the mode
    private void ValidateConfiguration()
    {
        if (mode == DoorMode.DifferentScene && string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning($"DoorController on '{gameObject.name}' is in 'Different Scene' mode but 'Target Scene Name' is not set.", this);
        }
        else if (mode == DoorMode.SameScene && targetPositionObject == null)
        {
            Debug.LogWarning($"DoorController on '{gameObject.name}' is in 'Same Scene' mode but 'Target Position Object' is not set.", this);
        }
    }

    private IEnumerator ActivateTeleporterAfterDelay()
    {
        Debug.Log("ActivateTeleporterAfterDelay...");
        yield return new WaitForSeconds(0.2f); // or wait for animation event

        if (teleporter != null)
        {
            Debug.Log("teleporter != null...");
            var playerCharacter = FindObjectOfType<Character>();
            if (playerCharacter != null)
            {
                Debug.Log("playerCharacter != null...");
                teleporter.TriggerButtonAction(playerCharacter.gameObject);
            }
            else
            {
                Debug.Log("else...");
                Debug.LogError("DoorController: Could not find Player Character to teleport!", this);
            }
        }
        else
        {
            Debug.LogError("DoorController: Teleporter component is not assigned!", this);
        }
    }
}