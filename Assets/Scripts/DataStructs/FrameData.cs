using UnityEngine;

public class FrameData
{
    // From PlayerController
    public Vector3 Position;
    public float Rotation;
    public Vector2 JoystickPosition;
    public string Player_State; // Collisions

    // From EyeLinkController
    public Vector2 GazePosition;
    public string GazeTargets;

    // From the StateSystemController
    public string Trial_State;

    // From MonkeyLogicController
    public double Unity_Local_Time;

    public void Clear()
    {
        Position = Vector3.zero;
        Rotation = .0f;
        Player_State = "";
        Trial_State = "";
        JoystickPosition = Vector2.zero;
        GazePosition = Vector2.zero;
        GazeTargets = "";
        Unity_Local_Time = .0;
    }

    // Start is called before the first frame update
    void Start()
    {
        Clear();
    }

    public string GetData(double curr_time)
    {
        Unity_Local_Time = curr_time;
        return JsonUtility.ToJson(this);
    }
}
