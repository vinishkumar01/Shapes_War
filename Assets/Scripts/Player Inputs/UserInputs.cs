using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.LowLevel;
using UnityEditorInternal;


public class UserInputs : MonoBehaviour
{
    public static UserInputs instance;

    //This represents the value that the playerControls that pass in (Example: when we press A in keyborad here it passes it as -1 in X)
    [HideInInspector] public Vector2 moveInputs;
    [HideInInspector] public PlayerInputsScheme _playerInputs;
    
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform _canvasRectTransform;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private float _cursorSpeed = 3500;
    [SerializeField] private float _padding = 12f;

    private Mouse _virtualMouse;
    private Mouse _currentMouse;
    private Camera _mainCamera;

    public RectTransform _cursorTransform;

    private string previousControlScheme = "";
    private const string gamepadScheme = "Gamepad";
    private const string mouseScheme = "KeyboardMouse";

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        _playerInputs = new PlayerInputsScheme();

        _playerInputs.Player.Move.performed += ctx => moveInputs = ctx.ReadValue<Vector2>();
    }

    private void OnEnable()
    {
        _mainCamera = Camera.main;
        _currentMouse = Mouse.current;

        _playerInputs.Enable();

        //Creating a Virtual Mouse. Note: The low level of Input system is used here so that we could change the mouse state.
        InputDevice virtualMouseInputDevice = InputSystem.GetDevice("VirtualMouse");

        if (virtualMouseInputDevice == null)
        {
            _virtualMouse = (Mouse)InputSystem.AddDevice("VirtualMouse");
        }
        else if (!virtualMouseInputDevice.added)
        {
            _virtualMouse = (Mouse)InputSystem.AddDevice("VirtualMouse");
        }
        else
        {
            _virtualMouse = (Mouse)virtualMouseInputDevice;
        }

        //Now we are pairing the device to use the playerInput component with the EventSystem and the virtual mouse
        InputUser.PerformPairingWithDevice(_virtualMouse, _playerInput.user);

        //Checking if the RectTransform of the cursor is assigned or present 
        if(_cursorTransform != null)
        {
            //The anchored position is the position of the pivot of the rectTransform relative to the anchor point
            Vector2 position = _cursorTransform.anchoredPosition;
            InputState.Change(_virtualMouse.position, position);
        }

        //This is a event and we are subscribing to the UpdateMotion method
        //onAfterUpdate is an event that is fired after the input system has completed an update and processed all pending events.
        InputSystem.onAfterUpdate += UpdateMotion;
        _playerInput.onControlsChanged += OnControlChanged;

    }

    private void UpdateMotion()
    {
        //Control the cursor using Gamepad
        if (_virtualMouse != null && Gamepad.current != null)
        {
            //Getting the delta value of the right stick of the Gamepad
            Vector2 deltaValue = Gamepad.current.rightStick.ReadValue();
            deltaValue *= _cursorSpeed * Time.deltaTime;

            //Getting the current Position of the cursor and stored and assign it new position by adding the currentPosition and the deltavalue (Stick value)
            Vector2 currenPosition = _virtualMouse.position.ReadValue();
            Vector2 newPosition = currenPosition + deltaValue;

            //Setting bounds so that the cursor doesnt goes off the screen
            newPosition.x = Mathf.Clamp(newPosition.x, _padding, Screen.width - _padding);
            newPosition.y = Mathf.Clamp(newPosition.y, _padding, Screen.height - _padding);

            // all the operation that we have done until now is applied here so the movement of the cursor will be done here
            InputState.Change(_virtualMouse.position, newPosition);
            InputState.Change(_virtualMouse.delta, deltaValue);

            AnchorCursor(newPosition);
        }

       //Control the cursor using the Mouse
       if(_virtualMouse != null && Mouse.current != null)
        {
            //Sync only if the mouse is moved in this frame
            if(Mouse.current.delta.ReadValue() != Vector2.zero)
            {
                Vector2 mousePosition = Mouse.current.position.ReadValue();

                mousePosition.x = Mathf.Clamp(mousePosition.x, _padding, Screen.width - _padding);
                mousePosition.y = Mathf.Clamp(mousePosition.y, _padding, Screen.height - _padding);

                //Update virtual Mouse and cursor
                InputState.Change(_virtualMouse.position, mousePosition);
                AnchorCursor(mousePosition);
            }
        }
    }

    public void AnchorCursor(Vector2 position)
    {
        Vector2 anchoredPosition;

        // we can directly assign the anchored position to the new position but because of the different screen size we might not achieve the result we expect so we use this functionality from unity for it.
        // we will check the render mode in canvas is set to ScreenSpace Overlay or Camera, if its overly we leave the camera null if its set to screenSpaceCamera then we will assign the mainCamera here
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRectTransform,position, _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _mainCamera, out anchoredPosition);

        _cursorTransform.anchoredPosition = anchoredPosition;
    }

    //---We might not need this method for now as we are accessing the cursor using both the Keyboard and Gamepad---
    private void OnControlChanged(PlayerInput input)
    {
        if(_playerInput.currentControlScheme == mouseScheme && previousControlScheme != mouseScheme)
        {
            _cursorTransform.gameObject.SetActive(true);
            Cursor.visible = false;
            InputState.Change(_virtualMouse.position, _currentMouse.position.ReadValue());
            AnchorCursor(_currentMouse.position.ReadValue());
            previousControlScheme = mouseScheme;
        }
        else if (_playerInput.currentControlScheme == gamepadScheme && previousControlScheme != gamepadScheme)
        {
            _cursorTransform.gameObject.SetActive(true);
            Cursor.visible = false;
            InputState.Change(_virtualMouse.position, _currentMouse.position.ReadValue());
            AnchorCursor(_currentMouse.position.ReadValue());
            previousControlScheme = gamepadScheme;
        }
    }

    private void OnDisable()
    {
        //Unsubscribe from this event on Disable of the gameObject that this script is attached
        InputSystem.onAfterUpdate -= UpdateMotion;
        _playerInput.onControlsChanged -= OnControlChanged;

        if (_virtualMouse != null && _virtualMouse.added)
        {
            //remove the virtual mouse on disable
            InputSystem.RemoveDevice(_virtualMouse);
        }

        _playerInputs.Disable();
    }
}
