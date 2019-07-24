/*
 * Handles the LSL communication for both the Control Inlet and the publication of frame data on the Frame Outlet. 
 * 
 * the FrameOutlet will publish: 
    * Position X, Y, Z 
    * Rotation
    * ...
 * 
 * the Trial Outlet will publish at the end of the trial and is used by MonkeyLogic to trigger
 * the end of the trial and get the outcome. 
 *
 * This script is based on the LSLMarkerStream.cs script in LSL/Scripts/. 
 * 
 * Notes: 
 * - We will use delegate functions in the MonkeyLogicController children classes that
 *      will call forward functions to trigger Events in the Events controller class instead of sending the events directly from
 *      the children classes. This is to have a better control on the ins and outs of the LSL streams and the events. 
 * - 
 * 
 * */

using UnityEngine;
using LSL;


public class MonkeyLogicController : MonoBehaviour
{
    // Outlets
    private MonkeyLogicOutlet outlets;
    private int frameOutlet; // index in the outlets list. 
    private string _frameOutletName = "ML_FrameData";
    private string _frameOutletType = "Markers";
    private string _frameOutletID = "frame1214";

    private int trialOutlet;
    public string _trialOutletName = "ML_TrialData";
    public string _trialOutletType = "LSL_Marker_Strings";
    public string _trialOutletID = "trial1214";

    // Inlets
    private MonkeyLogicInlet inlet;
    private string _controlInletName = "ML_ControlStream";
    private string _controlInletType = "Markers";
    private string _controlInletID = "control1214";
    private MonkeyLogicResolver _resolver;

    // Start is called before the first frame update
    void Start()
    {
        if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
        {
            Debug.Log("You're running " + SystemInfo.operatingSystem +
                ". Aborting MonkeyLogicController.cs");
            return; // LSL crashes OSX           
        }

        // Create stream descriptors
        outlets = gameObject.AddComponent<MonkeyLogicOutlet>();
        
        // These need to be MonoBehavior so leave unchanged
        _resolver = gameObject.AddComponent<MonkeyLogicResolver>();
        inlet = gameObject.AddComponent<MonkeyLogicInlet>();

        // Configure
        frameOutlet = outlets.Configure(_frameOutletName,
                                _frameOutletType,
                                1,
                                liblsl.IRREGULAR_RATE,
                                liblsl.channel_format_t.cf_string,
                                _frameOutletID);

        trialOutlet = outlets.Configure(_trialOutletName,
                                _trialOutletType,
                                1,
                                liblsl.IRREGULAR_RATE,
                                liblsl.channel_format_t.cf_string,
                                _trialOutletID);

        inlet.Configure(_controlInletName, _controlInletType, _controlInletID, _resolver);

        // add listener for Streams delegates
        inlet.OnCalibrationReceived += ForwardEyecalibration;
        inlet.OnCommand += ForwardCommand;

}
    
    // Forward delegates from children classes to the Events Controller. 
    private void ForwardEyecalibration(EyeCalibrationParameters parameters)
    {
        EventsController.instance.SendEyeCalibrationUpdate(parameters);
    }

    private void ForwardCommand(string command)
    {
        switch (command)
        {
            case "Begin":
                EventsController.instance.SendBegin();
                break;
            case "Pause":
                EventsController.instance.SendPause();
                break;
            case "Resume":
                EventsController.instance.SendResume();
                break;
            case "End":
                EventsController.instance.SendEnd();
                break;
            default:
                break;
        }
    }

    public double GetLSLTime()
    {
        return liblsl.local_clock();
    }

    public void PublishFrame(string to_publish)
    {
        outlets.Write(frameOutlet, to_publish);
    }

    public void PublishTrial(string to_publish)
    {
        outlets.Write(trialOutlet, to_publish);
    }
}
