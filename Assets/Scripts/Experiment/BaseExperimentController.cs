using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class BaseExperimentController : MonoBehaviour
{
    // List of possible variables
    // Public
    // public TaskInfo taskInfo;
    // public TrialState trialState;
    // public TrialParameters trialParameters; // Instead of TaskParameters for current trial. 
    // public DevelopTools developTools; 

    // public float fixationTime;
    // public float gateTime; // analogous to stateTime

    // public UserInputController inputController; 
    // public 

    // Protected
    // protected List<TaskInfo> _allTrials = new List<TaskInfo>();
    // protected float _fixationTimer;
    // protected float _gateTimer;

    // Private



    // NOTES: 
    //  - Separate Gaze and User input? As subjects might have both gaze and button/controller in the future? 
    //  - Separate any parameters in the Experiment controller (e.g. number of trials) and place in task info.
    //  - Keep some parameters in the Experiment controller that are common across tasks (e.g. Fixation window)? Or put in proper controller? like gaze
    //  - Separate experimental environments from the experiment controller
    //  - 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
