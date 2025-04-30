using UnityEngine;
using UnityEditor; // Required for editor scripting
using UnityEngine.SceneManagement;

[CustomEditor(typeof(DoorController))] // Specify which script this editor is for
[CanEditMultipleObjects] // Optional: Allow editing multiple doors at once
public class DoorControllerEditor : Editor
{
    SerializedProperty modeProp;
    SerializedProperty interactionPromptTextProp;
    SerializedProperty targetSceneNameProp;
    SerializedProperty targetSceneSpawnPositionProp;
    SerializedProperty targetSceneTeleporterNameProp;
    SerializedProperty targetPositionObjectProp;
    SerializedProperty targetRoomCameraProp;
    SerializedProperty teleporterProp;

    void OnEnable()
    {
        // Link serialized properties to the script's variables
        modeProp = serializedObject.FindProperty("mode");
        interactionPromptTextProp = serializedObject.FindProperty("interactionPromptText");
        targetSceneNameProp = serializedObject.FindProperty("targetSceneName");
        targetSceneSpawnPositionProp = serializedObject.FindProperty("targetSceneSpawnPosition");
        targetSceneTeleporterNameProp = serializedObject.FindProperty("targetSceneTeleporterName");
        targetPositionObjectProp = serializedObject.FindProperty("targetPositionObject");
        targetRoomCameraProp = serializedObject.FindProperty("targetRoomCamera");
        teleporterProp = serializedObject.FindProperty("teleporter");
    }

    public override void OnInspectorGUI()
    {
        // Update the serialized object's representation
        serializedObject.ApplyModifiedProperties(); // Apply changes before drawing
        serializedObject.Update(); // Read the latest values from the object

        // Draw the Enum dropdown for the mode
        EditorGUILayout.PropertyField(modeProp);

        // Get the current enum value
        DoorController.DoorMode currentMode = (DoorController.DoorMode)modeProp.enumValueIndex;

        // Draw the interaction prompt text field (always visible)
        EditorGUILayout.PropertyField(interactionPromptTextProp);

        EditorGUILayout.Space(); // Add some visual spacing

        // Conditionally draw fields based on the selected mode
        switch (currentMode)
        {
            case DoorController.DoorMode.DifferentScene:
                EditorGUILayout.LabelField("Mode: Different Scene Options", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(targetSceneNameProp);
                EditorGUILayout.PropertyField(targetSceneSpawnPositionProp);
                EditorGUILayout.PropertyField(targetSceneTeleporterNameProp);
                break;

            case DoorController.DoorMode.SameScene:
                EditorGUILayout.LabelField("Mode: Same Scene Options", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(targetPositionObjectProp);
                EditorGUILayout.PropertyField(targetRoomCameraProp);
                // Only show Teleporter field for Same Scene mode
                EditorGUILayout.PropertyField(teleporterProp);
                break;
        }

        // Apply any changes made in the Inspector back to the serialized object
        serializedObject.ApplyModifiedProperties();
    }
}