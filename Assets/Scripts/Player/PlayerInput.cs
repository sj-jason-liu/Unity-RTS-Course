using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace sjjasonliu.RTS.Player
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera _cinemachineCamera;
        [SerializeField] private Rigidbody _cameraTarget;
        [SerializeField] private CameraConfig _cameraConfig;

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
            float rotationTime = Mathf.Clamp01((Time.time - rotateStartTime) * _cameraConfig.RotateSpeed);
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
            float zoomTime = Mathf.Clamp01((Time.time - zoomStartTime) * _cameraConfig.ZoomSpeed);
            Vector3 targetFollowOffset;  //store the target follow offset based on zoom in or out

            if (Keyboard.current.deleteKey.isPressed) //if zooming in, target offset is set to minimum distance
            {
                targetFollowOffset = new(
                    cinemachineFollow.FollowOffset.x,
                    _cameraConfig.MinZoomDistance,
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
            Vector2 moveAmount = GetKeyboardMoveAmount();
            moveAmount += GetMouseMoveAmount();

            //set the camera target's velocity based on the move amount
            _cameraTarget.linearVelocity = new Vector3(moveAmount.x, 0, moveAmount.y);
        }

        private Vector2 GetMouseMoveAmount()
        {
            Vector2 moveAmount = Vector2.zero;

            // if edge panning is disabled, return zero
            if (!_cameraConfig.EnableEdgePan) { return moveAmount; }

            Vector2 mousePosition = Mouse.current.position.ReadValue(); // get the current mouse position
            int screenWidth = Screen.width; // get the screen width
            int screenHeight = Screen.height; // get the screen height

            if (mousePosition.x <= _cameraConfig.EdgePanSize) // if mouse is near the left edge
            {
                moveAmount.x -= _cameraConfig.MousePanSpeed;
            }
            else if(mousePosition.x >= screenWidth - _cameraConfig.EdgePanSize) // if mouse is near the right edge
            {
                moveAmount.x += _cameraConfig.MousePanSpeed;
            }

            if (mousePosition.y <= _cameraConfig.EdgePanSize) // if mouse is near the bottom edge
            {
                moveAmount.y -= _cameraConfig.MousePanSpeed;
            }
            else if (mousePosition.y >= screenHeight - _cameraConfig.EdgePanSize) // if mouse is near the top edge
            {
                moveAmount.y += _cameraConfig.MousePanSpeed;
            }

            return moveAmount;
        }

        private Vector2 GetKeyboardMoveAmount()
        {
            Vector2 moveAmount = Vector2.zero;

            if (Keyboard.current.upArrowKey.isPressed)
            {
                moveAmount.y += _cameraConfig.KeyboardPanSpeed;
            }
            if (Keyboard.current.leftArrowKey.isPressed)
            {
                moveAmount.x -= _cameraConfig.KeyboardPanSpeed;
            }
            if (Keyboard.current.downArrowKey.isPressed)
            {
                moveAmount.y -= _cameraConfig.KeyboardPanSpeed;
            }
            if (Keyboard.current.rightArrowKey.isPressed)
            {
                moveAmount.x += _cameraConfig.KeyboardPanSpeed;
            }

            return moveAmount;
        }
    }
}
