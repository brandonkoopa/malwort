using UnityEngine;

public class MoonFollowCamera : MonoBehaviour
{
    [Header("Position Settings")]
    [Tooltip("Set to true if you want the object at a fixed screen position")]
    public bool useScreenPosition = true;
    
    [Tooltip("Screen position (0,0 is bottom-left, 1,1 is top-right)")]
    [Range(0, 1)] public float screenPositionX = 0.25f;
    [Range(0, 1)] public float screenPositionY = 0.8f;
    
    [Tooltip("Distance from camera for non-screen positioning")]
    public float distanceFromCamera = 10f;
    
    [Header("Movement Settings")]
    [Tooltip("How much the object follows camera movement (0 = fixed in world, 1 = fixed to camera)")]
    [Range(0, 1)] public float followFactor = 0.05f;
    
    private Camera mainCamera;
    private Vector3 initialPosition;
    
    void Start()
    {
        mainCamera = Camera.main;
        initialPosition = transform.position;
    }
    
    void LateUpdate()
    {
        float originalZ = transform.position.z;
        
        if (useScreenPosition)
        {
            // Place object at a fixed screen position
            Vector3 viewportPosition = new Vector3(screenPositionX, screenPositionY, distanceFromCamera);
            Vector3 worldPosition = mainCamera.ViewportToWorldPoint(viewportPosition);
            
            // Preserve original Z position
            transform.position = new Vector3(worldPosition.x, worldPosition.y, originalZ);
        }
        else
        {
            // Parallax effect - object slowly follows the camera
            Vector3 cameraPosition = mainCamera.transform.position;
            Vector3 targetPosition = new Vector3(
                cameraPosition.x * followFactor + initialPosition.x * (1 - followFactor),
                cameraPosition.y * followFactor + initialPosition.y * (1 - followFactor),
                originalZ
            );
            
            transform.position = targetPosition;
        }
    }
}