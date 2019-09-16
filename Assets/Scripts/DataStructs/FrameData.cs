using UnityEngine;
using Misc;

public class FrameData
{
    // From PlayerController
    public Vector3 Position;
    public float Rotation;
    public Vector2 JoystickPosition;
    public float Player_State; // Collisions instance ID; set in ExpCtrl

    // From EyeLinkController
    public Vector2 GazePosition;
    public float[] GazeTargets;  // instance IDs
    public float[] GazeRayCounts; // number of rays hitting them

    // Will redo gazetargets to only keep 5 objects and sort
    // by the number of rays hitting them. 
    
    // From the StateSystemController
    public StateNames Trial_State;

    // PhotoDiode
    public float PhotoDiodeIntensity; 

    // From MonkeyLogicController
    public double Unity_Local_Time;

    public void Clear()
    {
        Position = Vector3.zero;
        Rotation = .0f;
        Player_State = -1;
        Trial_State = StateNames.Null;
        JoystickPosition = Vector2.zero;
        GazePosition = Vector2.zero;
        GazeTargets = new float[5];
        GazeRayCounts = new float[5];
        Unity_Local_Time = .0;
    }

    // Data to publish will be a float[] containing:
    //  Pos X
    //  Pos Y
    //  Pos Z
    //  Rot
    //  Joystick X
    //  Joystick Y
    //  Collision Object InstanceID
    //  Gaze X
    //  Gaze Y
    //  <= 5 Gaze collision object instanceIDs
    //  <= 5 Gaze ray hit counts (max 33: 1 center and 4x8 circles)
    //  Photo Diode intensity
    //  Trial State
    //  Unity LSL local time
    public double[] GetData(double curr_time)
    {
        double[] sample = new double[22];
        sample[0] = Position.x;
        sample[1] = Position.y;
        sample[2] = Position.z;
        sample[3] = Rotation;
        sample[4] = JoystickPosition.x;
        sample[5] = JoystickPosition.y;
        sample[6] = Player_State;
        sample[7] = GazePosition.x;
        sample[8] = GazePosition.y;
        if (GazeTargets != null)
        {
            for (int i = 0; i < GazeTargets.Length; i++)
            {
                sample[9 + i] = GazeTargets[i];
                sample[14 + i] = GazeRayCounts[i];
            }
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                sample[9 + i] = -1;
                sample[14 + i] = 0;
            }
        }
        sample[19] = (double)Trial_State;
        sample[20] = PhotoDiodeIntensity;
        sample[21] = curr_time;

        return sample; 

        //return JsonUtility.ToJson(this);
    }
}
