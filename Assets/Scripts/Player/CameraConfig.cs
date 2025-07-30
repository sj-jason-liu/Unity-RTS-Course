using UnityEngine;

namespace sjjasonliu.RTS.Player
{
    [System.Serializable] //make this class serializable so it can be used in the inspector
    public class CameraConfig  // Configuration class for camera settings
    {
        [field: SerializeField] public bool EnableEdgePan { get; private set; } = true;
        [field: SerializeField] public float MousePanSpeed { get; private set; } = 5f;
        [field: SerializeField] public float EdgePanSize { get; private set; } = 50f;

        [field: SerializeField] public float KeyboardPanSpeed { get; private set; } = 5f;

        [field: SerializeField] public float ZoomSpeed { get; private set; } = 1f;
        [field: SerializeField] public float MinZoomDistance { get; private set; } = 7.5f;

        [field: SerializeField] public float RotateSpeed { get; private set; } = 1f;
    }
}