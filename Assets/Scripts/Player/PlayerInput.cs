using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private Transform _cameraTarget;
    [SerializeField] private float _keyboardPanSpeed = 5f;

    // Update is called once per frame
    void Update()
    {
        Vector2 moveAmount = Vector2.zero;

        if(Keyboard.current.upArrowKey.isPressed)
        {
            moveAmount.y += _keyboardPanSpeed;
        }
        if(Keyboard.current.leftArrowKey.isPressed)
        {
            moveAmount.x -= _keyboardPanSpeed;
        }
        if(Keyboard.current.downArrowKey.isPressed)
        {
            moveAmount.y -= _keyboardPanSpeed;
        }
        if(Keyboard.current.rightArrowKey.isPressed)
        {
            moveAmount.x += _keyboardPanSpeed;
        }

        moveAmount *= Time.deltaTime;
        _cameraTarget.position += new Vector3(moveAmount.x, 0, moveAmount.y);
    }
}
