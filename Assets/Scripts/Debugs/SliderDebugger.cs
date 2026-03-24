using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SliderDebugger : MonoBehaviour
{
    private Slider _slider;
    private float _previousValue;
    private Mouse _virtualMouse;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
        _previousValue = _slider.value;
    }

    private void Update()
    {
        // Try to get virtual mouse each frame in case it gets added late
        if (_virtualMouse == null)
            _virtualMouse = (Mouse)InputSystem.GetDevice("VirtualMouse");

        if (_slider.value != _previousValue)
        {
            Vector2 virtualPos = _virtualMouse != null ? _virtualMouse.position.ReadValue() : Vector2.zero;
            Vector2 realPos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            bool isVirtualDragging = _virtualMouse != null && _virtualMouse.leftButton.isPressed;
            bool isRealDragging = Mouse.current != null && Mouse.current.leftButton.isPressed;

            Debug.Log($"Slider: {_slider.value} | " +
                      $"RealMouse pos: {realPos} pressed: {isRealDragging} | " +
                      $"VirtualMouse pos: {virtualPos} pressed: {isVirtualDragging}");

            _previousValue = _slider.value;
        }
    }
}