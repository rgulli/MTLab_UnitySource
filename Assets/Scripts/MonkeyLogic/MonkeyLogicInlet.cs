///<summary>
/// This implementation is based on the ABaseInlet Class from the LSL library. 
/// </summary>
using System.Collections.Generic;
using System;
using UnityEngine;
using Assets.LSL4Unity.Scripts.AbstractInlets;
using Assets.LSL4Unity.Scripts;
using UnityEngine.Events;

public class MonkeyLogicInlet : InletStringSamples
{
    // Outlets settings
    public string UniqueID;
    private bool pullSamplesContinuously = false;
    
    // override Resolver definition in Base class because the Resolver class had errors.
    // Created error-less version by copy-pasting. Couldn't sub-class because of private 
    // methods. 
    new protected MonkeyLogicResolver resolver;

    // Delegates for inlet control events
    public delegate void EyeCalibrationReceived(EyeCalibrationParameters parameters);
    public EyeCalibrationReceived OnCalibrationReceived;

    public delegate void ML_Command(string command);
    public ML_Command OnCommand;

    // Override the function in the ABaseInlet class: 
    // returns an error if no stream is not found during initialization. 
    // The resolver should still continuously resolve. 
    protected override void registerAndLookUpStream() { }

    // Instead of calling this in the start function, we call it here to pass the parameters of the 
    // expected stream. 
    public void Configure(string Name, string Type, string ID, MonkeyLogicResolver resolve)
    {
        StreamName = Name;
        StreamType = Type;
        UniqueID = ID;
        resolver = resolve;
        resolver.OnStreamFound += AStreamIsFound;
        resolver.OnStreamLost += AStreamGotLost;
        // now that the listeners are set, we can start the Resolver coroutine
        resolver.Run();
    }

    protected override bool isTheExpected(LSLStreamInfoWrapper stream)
    {
        // Checks for name, type and Unique ID?
        bool predicate = StreamName.Equals(stream.Name);
        predicate &= StreamType.Equals(stream.Type);
        predicate &= UniqueID.Equals(stream.SourceID);
        return predicate;
    }

    protected override void Process(string[] newSample, double timeStamp)
    {
        // Process the MonkeyLogic commands that can be: 
        List<string> Commands = new List<string> { "Begin", "End", "Pause", "Resume" }; // + EyeCalibration

        foreach (string samp in newSample)
        {
            InletCommand in_cmd = JsonUtility.FromJson<InletCommand>(samp);
            if (Commands.IndexOf(in_cmd.command_name) != -1)
            {
                OnCommand?.Invoke(in_cmd.command_name);
            }
            else if (in_cmd.command_name == "EyeCalibration") // Eye Calibration
            {
                OnCalibrationReceived?.Invoke(in_cmd.eyecal_parameters);
            }
            else // Nothing
            {

            }
            
        }
    }

    protected override void OnStreamAvailable()
    {
        pullSamplesContinuously = true;
    }

    protected override void OnStreamLost()
    {
        pullSamplesContinuously = false;
    }

    // The inlet will sample at late update to overwrite any
    // states or variables defined in the Update or State Machine Update
    // calls. 
    private void LateUpdate()
    {
        if (pullSamplesContinuously)
            pullSamples();
    }

    private void OnDestroy()
    {
        resolver.Stop();
    }
}

[Serializable]
public class InletCommand
{
    public string command_name;
    public EyeCalibrationParameters eyecal_parameters; 
}