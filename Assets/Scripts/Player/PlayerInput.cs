using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _cinemachineCamera;
    [SerializeField] private Transform _cameraTarget;
    [SerializeField] private float _keyboardPanSpeed = 5f;
    [SerializeField] private float _zoomSpeed = 1f;
    [SerializeField] private float _rotateSpeed = 1f;
    [SerializeField] private float minZoomDistance = 7.5f;

    private CinemachineFollow cinemachineFollow;
    private float zoomStartTime;
    private float rotateStartTime;
    private Vector3 startingFollowOffset;
    private float maxRotationAmount;

    private void Awake()
    {
        //check if the component exists
        if (!_cinemachineCamera.TryGetComponent(out cinemachineFollow))
        {
            Debug.LogError("CinemachineFollow component not found on " + _cinemachineCamera.name);
        }
        startingFollowOffset = cinemachineFollow.FollowOffset;
        maxRotationAmount = Mathf.Abs(startingFollowOffset.z);
    }

    void Update()
    {
        HandlePanning();
        HandleZooming();
        HandleRotation();
    }

    private void HandleRotation()
    {
        //check if the rotation keys are pressed to set the start time
        if (ShouldSetRotateStartTime())
        {
            rotateStartTime = Time.time;
        }

        //calculate the rotation time
        float rotationTime = Mathf.Clamp01((Time.time - rotateStartTime) * _rotateSpeed);
        Vector3 targetFollowOffset;  //store the target follow offset based on rotation

        if (Keyboard.current.pageUpKey.isPressed && !Keyboard.current.pageDownKey.isPressed) //if pressing pageUp, camera rotates right
        {
            targetFollowOffset = new Vector3(
                maxRotationAmount,
                cinemachineFollow.FollowOffset.y,
                0
            );
        }
        else if (Keyboard.current.pageDownKey.isPressed && !Keyboard.current.pageUpKey.isPressed)  //if pressing pageDown, camera rotates left
        {
            targetFollowOffset = new Vector3(
                -maxRotationAmount,
                cinemachineFollow.FollowOffset.y,
                0
            );
        }
        else
        {
            targetFollowOffset = new Vector3(
                startingFollowOffset.x,
                cinemachineFollow.FollowOffset.y,
                startingFollowOffset.z
            );
        }

        //smoothly interpolate the follow offset towards the target offset
        cinemachineFollow.FollowOffset = Vector3.Slerp(
                cinemachineFollow.FollowOffset,
                targetFollowOffset,
                rotationTime
            );
    }

    private bool ShouldSetRotateStartTime()
    {
        return Keyboard.current.pageUpKey.wasPressedThisFrame
            || Keyboard.current.pageDownKey.wasReleasedThisFrame
            || Keyboard.current.pageUpKey.wasReleasedThisFrame
            || Keyboard.current.pageDownKey.wasPressedThisFrame;
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

        if (Keyboard.current.deleteKey.isPressed) //if zooming in, target offset is set to minimum distance
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
        return Keyboard.current.deleteKey.wasPressedThisFrame
            || Keyboard.current.deleteKey.wasReleasedThisFrame;
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
