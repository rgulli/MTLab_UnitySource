using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class XMaze_ExperimentCtrl : ExperimentController
{
    // The taskInfo base class is defined as "dynamic" in the experiment
    // controller class, this means that the class is implemented at runtime
    // we therefore need to define it now. 
    // We use a specific sub-class of task info and set it as "taskInfo" so
    // all the base scripts will still work. 
    public XMaze_TaskInfo xMazeTaskInfo;

    // To keep track whether the targets will be displayed at the north or south end
    // of the maze. 
    public bool IsNorth { get; protected set; }

    // This function allows the base-class Experiment Controller to 
    // access values defined here (e.g. the IsNorth boolean). The returned value 
    // needs to be converted into the proper format with (format) before: 
    // bool ReturnedIsNorth = (bool)returnValue("IsNorth");
    public override object ReturnValue(string name)
    {
        return GetType().GetProperty(name).GetValue(this);
    }

    // OnEnable and OnDisable function calls are important because they connect the
    // experiment controller with the events controller for communication across classes. 
    private new void OnEnable()
    {
        // Important, do not remove ---
        base.OnEnable();
        taskInfo = xMazeTaskInfo;
        // ----------------------------

        // First trial is always north, but the value is toggled in 
        // prepare trial
        IsNorth = false;
    }
    
    private void Start()
    {
        // Base function call. Do not remove. 
        base.Initialize();
    }

    public override void PrepareTrial()
    {
        // Most of this is copy pasted from the base function. 

        // get current trial
        _currentTrial = _allTrials[_trialNumber];
        // increment counter after since we number trials 1: but indices are 0:
        _trialNumber++;
        _currentTrial.Trial_Number = _trialNumber;

        // teleport player to the start position if the previous trial was
        // a timeout (the player should be OnBlack at this time).
        if (!taskInfo.ContinuousTrials || _previousTrialError == 2)
        {
            // initial rotation is 90 degrees in the Y axis
            Quaternion initialRotation = Quaternion.identity;
            initialRotation.eulerAngles = new Vector3 { x = 0, y = 90, z = 0 };
            playerController.ToStart(_currentTrial.Start_Position, initialRotation);

            // if we teleport the player to the start postion, this trial is a "North"
            // trial
            IsNorth = true;
        }
        else
        {
            // Toggle North-South Trials
            IsNorth = !IsNorth;
        }

        // Prepare cues and targets. 
        PrepareCues(); // Empty for this example, cue objects remain visible. 

        // In the PrepareTargets/Distractors script, we place the objects at the selected position
        // from the TaskInfo. Our position markers don't differentiate between North/South and are located
        // outside the maze. We need to set the proper X position value. Same with the cue/target onset 
        // triggers. 
        float xPosition;

        if (IsNorth)
        {
            xPosition = taskInfo.NorthMarker.transform.position.x;

            // On a North bound trial, the cue trigger is the south one
            // and the target trigger is the north one
            taskInfo.CueOnsetTriggers = new GameObject[1] { taskInfo.SouthTrigger };
            taskInfo.TargetOnsetTriggers = new GameObject[1] { taskInfo.NorthTrigger };
        }
        else
        {
            xPosition = taskInfo.SouthMarker.transform.position.x;
            taskInfo.CueOnsetTriggers = new GameObject[1] { taskInfo.NorthTrigger };
            taskInfo.TargetOnsetTriggers = new GameObject[1] { taskInfo.SouthTrigger };
        }

        // Set values for current trial.
        for (int i = 0; i < _currentTrial.Target_Positions.Length; i++)
        {
            _currentTrial.Target_Positions[i].x = xPosition;
        }
        for (int i = 0; i < _currentTrial.Distractor_Positions.Length; i++)
        {
            _currentTrial.Distractor_Positions[i].x = xPosition;
        }

        // This will position the game objects and assign their texture, but hide them
        // and disable their collider.
        PrepareTargets();
        PrepareDistractors();

        // Sanity checks
        TrialEnded = false;
        Outcome = "aborted";
    }

    public override void EndTrial()
    {
        // If Graded reward is selected, change outcomes, keep correct/incorrect
        // otherwise. 
        if (xMazeTaskInfo.GradedReward)
        {
            // Check the current context
            if (_currentTrial.Cue_Material == xMazeTaskInfo.Ctx_One)
            {
                // We need to check all trial outcomes since both a "correct" or a 
                // "incorrect" outcome could yield the same reward size, depending 
                // on the presented colors. 

                // correct == highest reward selected
                // correct_mid == middle reward selected when low reward was present
                // correct_low == impossible

                // incorrect_high == impossible
                // incorrect_mid == middle reward selected when high reward was present
                // incorrect == lowest reward selected

                // Subject selected the target object
                if (Outcome == "correct")
                {
                    if (_currentTrial.Target_Materials[0] == xMazeTaskInfo.Mid_CtxOne)
                    {
                        Outcome = "correct_mid";
                    }
                }
                // subject selected the distractor
                else if (Outcome == "incorrect")
                {
                    if (_currentTrial.Distractor_Materials[0] == xMazeTaskInfo.Mid_CtxOne)
                    {
                        Outcome = "incorrect_mid";
                    }
                }
            }
            else if (_currentTrial.Cue_Material == xMazeTaskInfo.Ctx_Two)
            {
                // Subject selected the target object
                if (Outcome == "correct")
                {
                    if (_currentTrial.Target_Materials[0] == xMazeTaskInfo.Mid_CtxTwo)
                    {
                        Outcome = "correct_mid";
                    }
                }
                // subject selected the distractor
                else if (Outcome == "incorrect")
                {
                    if (_currentTrial.Distractor_Materials[0] == xMazeTaskInfo.Mid_CtxTwo)
                    {
                        Outcome = "incorrect_mid";
                    }
                }
            }
        }
        // We don't want to replace the script, only build on it
        // So we call the base function. 
        base.EndTrial();
    }

    public override void FreezePlayer(bool ON)
    {
        // if ON==true then player rotation and backward movement are frozen
        playerController.FreezeRotation(ON);
        playerController.ConstrainForward(ON);
    }
}
