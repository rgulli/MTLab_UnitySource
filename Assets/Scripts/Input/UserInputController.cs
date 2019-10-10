using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.HID;

public class UserInputController : MonoBehaviour
{
  
    public enum EDevice
    {
        Null,
        Joystick,
        Keyboard,
        Mouse  // here mouse is for navigation (e.g. trackball); TODO: mouse for gaze?
        // TODO: Touchpad
    }

    // Editor editable variables. 
    public EDevice deviceType = EDevice.Null;
    public float Move_Sensitivity = 2.0f;
    public float Turn_Sensitivity = 2.0f;
    
    public Vector2 ReadAxes()
    {
        Vector2 axes;

        switch(deviceType)
        {
            case EDevice.Joystick:
                if (Joystick.current != null)
                    axes = Joystick.current.stick.ReadValue();
                else
                    axes = Vector2.zero;
                break;
            case EDevice.Keyboard:
                axes = new Vector2
                {
                    x = Keyboard.current.rightArrowKey.ReadValue() - Keyboard.current.leftArrowKey.ReadValue(),
                    y = Keyboard.current.upArrowKey.ReadValue() - Keyboard.current.downArrowKey.ReadValue()
                };
                break;
            case EDevice.Mouse:
                axes = Mouse.current.delta.ReadValue();
                break;
            case EDevice.Null:
                axes = Vector2.zero;
                break;
            default:
                axes = Vector2.zero;
                break;
        }
        axes.x *= Turn_Sensitivity;
        axes.y *= Move_Sensitivity;
        return axes;

    }

    private void Start()
    {
       
    
    }
}
