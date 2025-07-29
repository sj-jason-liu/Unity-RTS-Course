using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _cinemachineCamera;
    [SerializeField] private Transform _cameraTarget;
    [SerializeField] private float _keyboardPanSpeed = 5f;
    [SerializeField] private float _zoomSpeed = 1f;
    [SerializeField] private float minZoomDistance = 7.5f;

    private CinemachineFollow cinemachineFollow;
    private float zoomStartTime;
    private Vector3 startingFollowOffset;

    private void Awake()
    {
        //check if the component exists
        if (!_cinemachineCamera.TryGetComponent(out cinemachineFollow))
        {
            Debug.LogError("CinemachineFollow component not found on " + _cinemachineCamera.name);
        }
        startingFollowOffset = cinemachineFollow.FollowOffset;
    }

    void Update()
    {
        HandlePanning();
        HandleZooming();
    }

    private void HandleZooming()
    {
        if (ShouldSetZoomeStartTime())
        {
            zoomStartTime = Time.time;
        }

        //calculate the zoom time
        float zoomTime = Mathf.Clamp01((Time.time - zoomStartTime) * _zoomSpeed);
        Vector3 targetFollowOffset;  //store the target follow offset based on zoom in or out

        if (Keyboard.current.endKey.isPressed) //if zooming in, target offset is set to minimum distance
        {
            targetFollowOffset = new(
                cinemachineFollow.FollowOffset.x,
                minZoomDistance,
                cinemachineFollow.FollowOffset.z
            );
        }
        else  //if zooming out, target offset is set to the starting follow offset
        {
            targetFollowOffset = new(
                cinemachineFollow.FollowOffset.x,
                startingFollowOffset.y,
                cinemachineFollow.FollowOffset.z
            );
        }

        //smoothly interpolate the follow offset towards the target offset
        cinemachineFollow.FollowOffset = Vector3.Slerp(
                cinemachineFollow.FollowOffset,
                targetFollowOffset,
                zoomTime
            );
    }

    private bool ShouldSetZoomeStartTime()
    {
        return Keyboard.current.endKey.wasPressedThisFrame
            || Keyboard.current.endKey.wasReleasedThisFrame;
    }

    private void HandlePanning()
    {
        Vector2 moveAmount = Vector2.zero;

        if (Keyboard.current.upArrowKey.isPressed)
        {
            moveAmount.y += _keyboardPanSpeed;
        }
        if (Keyboard.current.leftArrowKey.isPressed)
        {
            moveAmount.x -= _keyboardPanSpeed;
        }
        if (Keyboard.current.downArrowKey.isPressed)
        {
            moveAmount.y -= _keyboardPanSpeed;
        }
        if (Keyboard.current.rightArrowKey.isPressed)
        {
            moveAmount.x += _keyboardPanSpeed;
        }

        moveAmount *= Time.deltaTime;
        _cameraTarget.position += new Vector3(moveAmount.x, 0, moveAmount.y);
    }
}
