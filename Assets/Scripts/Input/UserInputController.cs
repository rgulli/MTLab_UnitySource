using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInputController : MonoBehaviour
{
    // Read-only params. 
    private string _inputH = "Null";
    public string InputH { 
        get { return _inputH; }
        set {}
    }

    private string _inputV = "Null";
    public string InputV
    {
        get { return _inputV; }
        set {}
    }

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

    // In order for this to work, the Axis need to be properly labelled in the Edit/Project Settings... 
    // Inputs section. If your task was created from the original one, this shouldn't be an issue. 
    private void OnEnable()
    {
        switch(deviceType)
        {
            case EDevice.Joystick:
                _inputH = "Joystick_H";
                _inputV = "Joystick_V";
                break;
            case EDevice.Keyboard:
                _inputH = "Keyboard_H";
                _inputV = "Keyboard_V";
                break;
            case EDevice.Mouse:
                _inputH = "Mouse_H";
                _inputV = "Mouse_V";
                break;
            case EDevice.Null:
                _inputH = "Null";
                _inputV = "Null";
                break;
            default:
                _inputH = "Null";
                _inputV = "Null";
                break;
        }
           
    }

    void Start()
    {

    }

    void Update()
    {

    }
}
