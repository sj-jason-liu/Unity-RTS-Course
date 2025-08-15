using System.Collections.Generic;
using sjjasonliu.RTS.Commands;
using sjjasonliu.RTS.EventBus;
using sjjasonliu.RTS.Events;
using sjjasonliu.RTS.Units;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
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
        private int _currentZoomLevel = 0; // Current zoom level
        private int _maxZoomLevels = 5; // Maximum zoom levels
        private float[] _zoomDistances; // Array to store zoom distances for each level
        private HashSet<AbstractUnit> _aliveUnits = new(100);
        private HashSet<AbstractUnit> _addedUnits = new(24);
        private List<ISelectable> _selectedUnits = new(12);

        private void Awake()
        {
            //check if the component exists
            if (!_cinemachineCamera.TryGetComponent(out _cinemachineFollow))
            {
                Debug.LogError("CinemachineFollow component not found on " + _cinemachineCamera.name);
            }
            _startingFollowOffset = _cinemachineFollow.FollowOffset;
            _maxRotationAmount = Mathf.Abs(_startingFollowOffset.z);

            Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected; //listener for unit selection events
            Bus<UnitDeselectedEvent>.OnEvent += HandleUnitDeselected;
            Bus<UnitSpawnEvent>.OnEvent += HandleUnitSpawn;

            InitializeZoomLevels();
        }

        private void InitializeZoomLevels()
        {
            _zoomDistances = new float[_maxZoomLevels];
            float minDistance = _cameraConfig.MinZoomDistance;
            float maxDistance = _startingFollowOffset.y;

            // 計算各級別的縮放距離
            for (int i = 0; i < _maxZoomLevels; i++)
            {
                float t = (float)i / (_maxZoomLevels - 1);
                _zoomDistances[i] = Mathf.Lerp(minDistance, maxDistance, t);
            }

            // 設定初始縮放級別為最遠
            _currentZoomLevel = _maxZoomLevels - 1;
        }

        private void OnDestroy()
        {
            Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected; // Unsubscribe from the event to prevent memory leaks
            Bus<UnitDeselectedEvent>.OnEvent -= HandleUnitDeselected;
            Bus<UnitSpawnEvent>.OnEvent -= HandleUnitSpawn;
        }

        //When Worker is selected, it will be passed to this method by the event system
        private void HandleUnitSelected(UnitSelectedEvent evt) => _selectedUnits.Add(evt.Unit);

        // clear the selected unit when it is deselected
        private void HandleUnitDeselected(UnitDeselectedEvent evt) => _selectedUnits.Remove(evt.Unit);

        private void HandleUnitSpawn(UnitSpawnEvent evt) => _aliveUnits.Add(evt.Unit);

        void Update()
        {
            HandlePanning();
            HandleZooming();
            HandleRotation();
            HandleRightClick();
            HandleDragSelect();
        }

        private void HandleDragSelect()
        {
            if(_selectionBox == null) { return; }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                ResetSelectionBox();
            }
            else if (Mouse.current.leftButton.isPressed && !Mouse.current.leftButton.wasPressedThisFrame) //dragging
            {
                DraggingSelectionBox();
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                ReleaseSelectionBox();
            }
        }

        private void ReleaseSelectionBox()
        {
            if (!Keyboard.current.shiftKey.isPressed)
            {
                // deselect non-included units
                DeselectAllUnits();
            }           
            HandleLeftClick();
            // select new units
            foreach (AbstractUnit unit in _addedUnits)
            {
                unit.Select(); // select the units that were added in this frame
            }
            // disable the ui
            _selectionBox.gameObject.SetActive(false);
        }

        private void DeselectAllUnits()
        {
            ISelectable[] currentSelectedUnits = _selectedUnits.ToArray();
            foreach (ISelectable selectable in currentSelectedUnits)
            {
                selectable.Deselect(); // deselect all currently selected units
            }
        }

        private void DraggingSelectionBox()
        {
            Bounds selectionBoxBounds = ResizeSelectionBox();
            foreach (AbstractUnit unit in _aliveUnits)
            {
                //get the screen position of the unit
                Vector2 unitPosition = _camera.WorldToScreenPoint(unit.transform.position);

                if (selectionBoxBounds.Contains(unitPosition))
                {
                    _addedUnits.Add(unit); // add the unit to the added units set
                }
            }
        }

        private void ResetSelectionBox()
        {
            // store start position
            _startingMousePosition = Mouse.current.position.ReadValue();
            // reset selection box size
            _selectionBox.sizeDelta = Vector2.zero;
            // set the anchored position to the starting mouse position
            _selectionBox.anchoredPosition = _startingMousePosition;
            _selectionBox.gameObject.SetActive(true); // enable the ui
            _addedUnits.Clear(); // clear the added units for this frame
        }

        private Bounds ResizeSelectionBox()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            // resize selection box           
            float width = mousePosition.x - _startingMousePosition.x;
            float height = mousePosition.y - _startingMousePosition.y;

            //width and height need to divi
            _selectionBox.anchoredPosition = _startingMousePosition + new Vector2(width / 2, height / 2);
            _selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));

            return new Bounds(_selectionBox.anchoredPosition, _selectionBox.sizeDelta);
        }

        private void HandleRightClick()
        {
            //check if the selected unit is null or not moveable
            if (_selectedUnits.Count == 0) { return; }

            Ray cameraRay = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Mouse.current.rightButton.wasPressedThisFrame
                && Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, _floorLayer)) //check if the raycast hits the floor layer
            {
                //find applicable command
                //issue that command to all units

                List<AbstractUnit> abstractUnits = new(_selectedUnits.Count);
                foreach (ISelectable selectable in _selectedUnits)
                {
                    if (selectable is AbstractUnit unit)
                    {
                        abstractUnits.Add(unit);
                    }
                }

                int unitsOnLayer = 0;
                int maxUnitsOnLayer = 1;
                float circleRadius = 0f;
                float radialOffset = 0f;

                foreach (AbstractUnit unit in abstractUnits)
                {
                    foreach (ICommand command in unit.AvailableCommands)
                    {
                        if (command.CanHandle(unit, hit))
                        {
                            command.Handle(unit, hit);
                        }
                    }

                    // // calculate the radial offset based on the number of units on this layer
                    //     float angle = radialOffset * unitsOnLayer;

                    // Vector3 targetPosition = new(
                    //     hit.point.x + circleRadius * Mathf.Cos(angle), // calculate the x position based on the angle and radius
                    //     hit.point.y,
                    //     hit.point.z + circleRadius * Mathf.Sin(angle) // calculate the z position based on the angle and radius
                    // );

                    // unit.MoveTo(targetPosition);
                    // unitsOnLayer++;

                    // // if the number of units on this layer exceeds the maximum, reset the counter and increase the radial offset
                    // if (unitsOnLayer >= maxUnitsOnLayer)
                    // {
                    //     unitsOnLayer = 0;
                    //     // increase the radius for the next layer
                    //     circleRadius += unit.AgentRadius * 3.5f;
                    //     // calculate the maximum number of units on this layer based on the circumference
                    //     maxUnitsOnLayer = Mathf.FloorToInt(2 * Mathf.PI * circleRadius / (unit.AgentRadius * 2));
                    //     radialOffset = 2 * Mathf.PI / maxUnitsOnLayer; // calculate the radial offset for the next unit
                    // } 
                }



                // foreach (ISelectable selectable in _selectedUnits)
                // {
                //     if (selectable is IMoveable moveable)
                //     {
                //         moveable.MoveTo(hit.point); //move the selected unit to the hit point
                //     }
                // }                
            }
        }

        private void HandleLeftClick()
        {
            if (_camera == null) { return; }

            Ray cameraRay = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            // Perform a raycast to check if the click hit a selectable unit, if it does, select it
            if (Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, _selectableUnitsLayer)
                && hit.collider.TryGetComponent(out ISelectable selectable))
            {
                selectable.Select();                    
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
            // 檢測滑鼠滾輪輸入
            float scrollValue = Mouse.current.scroll.ReadValue().y;

            if (scrollValue != 0)
            {
                _zoomStartTime = Time.time;

                // 根據滾輪方向調整縮放級別
                if (scrollValue > 0) // 向上滾動，放大
                {
                    _currentZoomLevel = Mathf.Max(0, _currentZoomLevel - 1);
                }
                else // 向下滾動，縮小
                {
                    _currentZoomLevel = Mathf.Min(_maxZoomLevels - 1, _currentZoomLevel + 1);
                }
            }

            // 計算縮放時間
            float zoomTime = Mathf.Clamp01((Time.time - _zoomStartTime) * _cameraConfig.ZoomSpeed);

            // 設定目標縮放距離
            Vector3 targetFollowOffset = new(
                _cinemachineFollow.FollowOffset.x,
                _zoomDistances[_currentZoomLevel],
                _cinemachineFollow.FollowOffset.z
            );

            // 平滑插值到目標位置
            _cinemachineFollow.FollowOffset = Vector3.Slerp(
                _cinemachineFollow.FollowOffset,
                targetFollowOffset,
                zoomTime
            );

            // if (ShouldSetZoomStartTime()) // if the zoom key is pressed or the scroll value is not zero, set the zoom start time
            // {
            //     _zoomStartTime = Time.time;
            // }

            // //calculate the zoom time
            // float zoomTime = Mathf.Clamp01((Time.time - _zoomStartTime) * _cameraConfig.ZoomSpeed);
            // Vector3 targetFollowOffset;  //store the target follow offset based on zoom in or out

            // if (Keyboard.current.deleteKey.isPressed) //if zooming in, target offset is set to minimum distance
            // {
            //     targetFollowOffset = new(
            //         _cinemachineFollow.FollowOffset.x,
            //         _cameraConfig.MinZoomDistance,
            //         _cinemachineFollow.FollowOffset.z
            //     );
            // }
            // else  //if zooming out, target offset is set to the starting follow offset
            // {
            //     targetFollowOffset = new(
            //         _cinemachineFollow.FollowOffset.x,
            //         _startingFollowOffset.y,
            //         _cinemachineFollow.FollowOffset.z
            //     );
            // }

            // //smoothly interpolate the follow offset towards the target offset
            // _cinemachineFollow.FollowOffset = Vector3.Slerp(
            //         _cinemachineFollow.FollowOffset,
            //         targetFollowOffset,
            //         zoomTime
            //     );
        }

        private bool ShouldSetZoomStartTime()
        {
            float scrollValue = Mouse.current.scroll.ReadValue().y;
            return scrollValue != 0;

            // return Keyboard.current.deleteKey.wasPressedThisFrame
            //     || Keyboard.current.deleteKey.wasReleasedThisFrame;
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
