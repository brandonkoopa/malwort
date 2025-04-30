using UnityEngine;

public static class DoorTransitionData
{
    public static Vector3 TargetPosition;
    public static string TargetCameraName;
    public static string TargetTeleporterName;

    public static void Clear()
    {
        TargetPosition = Vector3.zero;
        TargetCameraName = "";
        TargetTeleporterName = "";
    }
} 