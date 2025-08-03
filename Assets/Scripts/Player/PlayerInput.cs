using sjjasonliu.RTS.Units;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace sjjasonliu.RTS.Player
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera _cinemachineCamera;
        [SerializeField] private Camera _camera;
        [SerializeField] private Rigidbody _cameraTarget;
        [SerializeField] private CameraConfig _cameraConfig;
        [SerializeField] private LayerMask _selectableUnitsLayer;
        [SerializeField] private LayerMask _floorLayer;
        [SerializeField] private RectTransform _selectionBox; // UI element for drag selection

        private Vector2 _startingMousePosition;

        private CinemachineFollow _cinemachineFollow;
        private float _zoomStartTime;
        private float _rotateStartTime;
        private Vector3 _startingFollowOffset;
        private float _maxRotationAmount;
        private ISelectable _selectedUnit;

        private void Awake()
        {
            //check if the component exists
            if (!_cinemachineCamera.TryGetComponent(out _cinemachineFollow))
            {
                Debug.LogError("CinemachineFollow component not found on " + _cinemachineCamera.name);
            }
            _startingFollowOffset = _cinemachineFollow.FollowOffset;
            _maxRotationAmount = Mathf.Abs(_startingFollowOffset.z);
        }

        void Update()
        {
            HandlePanning();
            HandleZooming();
            HandleRotation();
            HandleLeftClick();
            HandleRightClick();
            HandleDragSelect();
        }

        private void HandleDragSelect()
        {
            if(_selectionBox == null) { return; }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                // enable the ui
                _selectionBox.gameObject.SetActive(true);
                // store start position
                _startingMousePosition = Mouse.current.position.ReadValue();
            }
            else if (Mouse.current.leftButton.isPressed && !Mouse.current.leftButton.wasPressedThisFrame) //dragging
            {               
                ResizeSelectionBox();
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                // select new units
                // deselect non-included units
                // disable the ui
                _selectionBox.gameObject.SetActive(false);
            }
        }

        private void ResizeSelectionBox()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            // resize selection box           
            float width = mousePosition.x - _startingMousePosition.x;
            float height = mousePosition.y - _startingMousePosition.y;

            //width and height need to divi
            _selectionBox.anchoredPosition = _startingMousePosition + new Vector2(width / 2, height / 2);
            _selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
        }

        private void HandleRightClick()
        {
            //check if the selected unit is null or not moveable
            if (_selectedUnit == null || _selectedUnit is not IMoveable moveable) { return; }

            Ray cameraRay = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Mouse.current.rightButton.wasPressedThisFrame
                && Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, _floorLayer)) //check if the raycast hits the floor layer
            {
                moveable.MoveTo(hit.point); //move the selected unit to the hit point
            }
        }

        private void HandleLeftClick()
        {
            if (_camera == null) { return; }

            Ray cameraRay = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Mouse.current.leftButton.wasReleasedThisFrame) //check if the left mouse button was released
            {
                if (_selectedUnit != null) // if there is a selected unit, deselect it
                {
                    _selectedUnit.Deselect();
                    _selectedUnit = null;
                }

                // Perform a raycast to check if the click hit a selectable unit, if it does, select it
                if (Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, _selectableUnitsLayer) //check if the raycast hits an object
                    && hit.collider.TryGetComponent(out ISelectable selectable))
                {
                    selectable.Select();
                    _selectedUnit = selectable; // set the selected unit to the newly selected one
                }
            }
        }

        private void HandleRotation()
        {
            //check if the rotation keys are pressed to set the start time
            if (ShouldSetRotateStartTime())
            {
                _rotateStartTime = Time.time;
            }

            //calculate the rotation time
            float rotationTime = Mathf.Clamp01((Time.time - _rotateStartTime) * _cameraConfig.RotateSpeed);
            Vector3 targetFollowOffset;  //store the target follow offset based on rotation

            if (Keyboard.current.pageUpKey.isPressed && !Keyboard.current.pageDownKey.isPressed) //if pressing pageUp, camera rotates right
            {
                targetFollowOffset = new Vector3(
                    _maxRotationAmount,
                    _cinemachineFollow.FollowOffset.y,
                    0
                );
            }
            else if (Keyboard.current.pageDownKey.isPressed && !Keyboard.current.pageUpKey.isPressed)  //if pressing pageDown, camera rotates left
            {
                targetFollowOffset = new Vector3(
                    -_maxRotationAmount,
                    _cinemachineFollow.FollowOffset.y,
                    0
                );
            }
            else
            {
                targetFollowOffset = new Vector3(
                    _startingFollowOffset.x,
                    _cinemachineFollow.FollowOffset.y,
                    _startingFollowOffset.z
                );
            }

            //smoothly interpolate the follow offset towards the target offset
            _cinemachineFollow.FollowOffset = Vector3.Slerp(
                    _cinemachineFollow.FollowOffset,
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
                _zoomStartTime = Time.time;
            }

            //calculate the zoom time
            float zoomTime = Mathf.Clamp01((Time.time - _zoomStartTime) * _cameraConfig.ZoomSpeed);
            Vector3 targetFollowOffset;  //store the target follow offset based on zoom in or out

            if (Keyboard.current.deleteKey.isPressed) //if zooming in, target offset is set to minimum distance
            {
                targetFollowOffset = new(
                    _cinemachineFollow.FollowOffset.x,
                    _cameraConfig.MinZoomDistance,
                    _cinemachineFollow.FollowOffset.z
                );
            }
            else  //if zooming out, target offset is set to the starting follow offset
            {
                targetFollowOffset = new(
                    _cinemachineFollow.FollowOffset.x,
                    _startingFollowOffset.y,
                    _cinemachineFollow.FollowOffset.z
                );
            }

            //smoothly interpolate the follow offset towards the target offset
            _cinemachineFollow.FollowOffset = Vector3.Slerp(
                    _cinemachineFollow.FollowOffset,
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
