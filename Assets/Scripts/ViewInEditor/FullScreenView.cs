using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FullScreenView : MonoBehaviour
{
    public int ScreenWidth = 1920;
    public int ScreenHeight = 1080;
    public int ScreenOffset = 0;
    public int MenuBarHeight = 21;
    public bool AutoLaunch = true;
    public float CameraFOV = 90; 

    public static int ResolutionX;
    public static int ResolutionY;
    public static int XOffset;
    // The menu bar is exactly 21 pixels in height
    public static int MenuOffset;

    private void OnValidate()
    {
        ResolutionX = ScreenWidth;
        ResolutionY = ScreenHeight;
        XOffset = ScreenOffset;
        MenuOffset = MenuBarHeight;
        
        // The camera FOV value is for the Vertical FOV, convert to Horizontal
        Camera.main.fieldOfView = 2 * Mathf.Atan(Mathf.Tan(CameraFOV * Mathf.Deg2Rad * 0.5f) / Camera.main.aspect) * Mathf.Rad2Deg;
    }

    // FullScreen Game Window
    private EditorWindow win;

    private void Start()
    {
        if (AutoLaunch)
        {
            win = (EditorWindow)ScriptableObject.CreateInstance("UnityEditor.GameView");
            win.name = "FullScreenView";
            win.ShowUtility();

            win.minSize = new Vector2 { x = ResolutionX, y = ResolutionY + MenuOffset };
            win.position = new Rect
            {
                x = XOffset,
                y = -MenuOffset,
                width = ResolutionX,
                height = ResolutionY + MenuOffset
            };
        }
    }

    private void OnDisable()
    {
        if (win != null) win.Close();
    }

}
