

///<summary>
/// ToDo: 
/// Check if input configuration transfers across computers with GitHub 
/// Implement true input configuration and not project settings hack
/// Input controller to have a navigation AND and eye controller? 
/// Replace events with direct function calls? 
/// </summary>

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Profiling;
using System.Linq;

public class ExperimentController : MonoBehaviour
{
    // So that other scripts can access the methods here.
    // Such as the various state system states, collider scripts and player controller. 
    #region Access

    public static ExperimentController instance = null; 
    public void Awake() 
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    #endregion Access
    
    // Load the current experiment configuration, and generate trials. 
    #region ExperimentConfig
    // Controllers
    public FirstPerson.PlayerController PlayerController;
    public MonkeyLogicController MonkeyLogicController;
    public EyeLinkController EyeLinkController;
    public UserInputController UserInputController;

    // Data structures
    private FrameData frameData = new FrameData();
    private TrialData currentTrial = new TrialData();
    private List<TrialData> allTrials = new List<TrialData>();

    // Generate Trials
    // Structure holding all the parameters that are set in the Editor. 
    public TaskInfo TaskInfo;

    public void PrepareAllTrials()
    {
        // set the seed
        UnityEngine.Random.InitState(DateTime.Now.Millisecond);

        // In this task we will trigger a Cue on the walls and the correct target is the cube with the same 
        // color as the walls. The target will be cube_1, distractor cube_2 and we will use the textures 
        // according to the context to validate trials. 

        // since the number of Targets and distractors can vary (e.g. can have multiple within a trial)
        // we begin by getting either all combinations (no order) or permutations (with order) of 
        // targets, distractors and positions. 
        // The functions return Lists of arrays containing the possible selections of objects.
        // We use permutations for positions because we get the target and distractors positions at the same
        // time, so if we have 2 targets and 2 distractors [T, T, D, D]: 
        // Positions [1, 2, 3, 4] and [4, 3, 2, 1] mean respectively that targets are at positions 1,2 and 4,3
        List<GameObject[]> all_targets_comb = new List<GameObject[]>();
        List<GameObject[]> all_distractors_comb = new List<GameObject[]>();
        List<GameObject[]> all_positions_perm = new List<GameObject[]>();

        GenerateCombinations(TaskInfo.TargetObjects.ToList(), TaskInfo.NTargets, new List<GameObject>(), all_targets_comb);
        GenerateCombinations(TaskInfo.DistractorObjects.ToList(), TaskInfo.NDistractors, new List<GameObject>(), all_distractors_comb);
        GeneratePermutations(TaskInfo.PossiblePositions.ToList(), TaskInfo.NTargets + TaskInfo.NDistractors, new List<GameObject>(), all_positions_perm);

        // Loop for start positions (only 1 per trial)
        for (int start_index = 0; start_index < TaskInfo.StartPositions.Length; start_index++)
        {
            // Loop through all the conditions defined in the list
            for (int cnd_index = 0; cnd_index < TaskInfo.Conditions.Length; cnd_index++)
            {
                foreach (GameObject[] targs in all_targets_comb)
                {
                    foreach (GameObject[] dists in all_distractors_comb)
                    {
                        // Specify which texture to use for both targets and distractors.
                        //First validate the number of objects and materials. 
                        Material[] targ_mat = new Material[targs.Length];
                        Material[] dist_mat = new Material[dists.Length];

                        for (int ii = 0; ii < targs.Length; ii++)
                        {
                            targ_mat[ii] = 
                                TaskInfo.Conditions[cnd_index].TargetMaterials[Mathf.Min(ii, TaskInfo.Conditions[cnd_index].TargetMaterials.Length-1)];
                        }

                        for (int ii = 0; ii < dists.Length; ii++)
                        {
                            dist_mat[ii] =
                                TaskInfo.Conditions[cnd_index].DistractorMaterials[Mathf.Min(ii, TaskInfo.Conditions[cnd_index].DistractorMaterials.Length - 1)];
                        }

                        foreach (GameObject[] poss in all_positions_perm)
                        {
                            // the positions are game objects, so we need to split between targets and distractors
                            // and get the position values in Vector3 format;
                            List<Vector3> targ_pos = new List<Vector3>();
                            List<Vector3> dist_pos = new List<Vector3>();
                            for(int ii=0; ii<poss.Length; ii++)
                            {
                                if (ii < targs.Length)
                                {
                                    targ_pos.Add(poss[ii].transform.position);
                                }
                                else
                                {
                                    dist_pos.Add(poss[ii].transform.position);
                                }
                            }

                            // At this point we have everything. Add to trial list N times:
                            for (int ii = 0; ii < TaskInfo.NumberOfSets; ii++)
                            {
                                allTrials.Add(
                                    new TrialData
                                    {
                                        Trial_Number = 0,
                                        Start_Position = TaskInfo.StartPositions[start_index].transform.position,
                                        // Current Cue
                                        Cue_Objects = TaskInfo.CueObjects,
                                        Cue_Material = TaskInfo.Conditions[cnd_index].CueMaterial,
                                        // Targets
                                        Target_Objects = targs,
                                        Target_Materials = targ_mat,
                                        Target_Positions = targ_pos.ToArray(),
                                        //Distractors
                                        Distractor_Objects = dists,
                                        Distractor_Materials = dist_mat,
                                        Distractor_Positions = dist_pos.ToArray()

                                    });
                            }
                        }
                    }
                }
            }
        }
        // shuffle trials
        allTrials = allTrials.OrderBy(x => UnityEngine.Random.value).ToList();
    }

    // The following scripts (GenerateCombinations and GeneratePermutations) are not intuitive to follow. 
    // This is due to a property of Lists and Arrays in C#: 
    // If we create two lists, where one is a "copy" of another, it does not create 2 lists
    // but simply maps the same memory address to the two variables. 
    // List<GameObject> list_1 = new List<GameObject>();
    // list_1.Add(go_1);
    // list_1.Add(go_2);
    // list_1.Add(go_3);
    // List<GameObject> list_2 = list_1;
    // This means that list_2 will be modified when we change list_1:
    // list_1[2] = go_4;
    // So if we now print the values of the two lists, list_2 will have changed even
    // if we only explicitely changed 1. 
    // list_1[:] => go_1, go_2, go_4
    // list_2[:] => go_1, go_2, go_4
    // To avoid this behavior you need to define list_2 as NEW: 
    //  List<GameObject> list_2 = new List<GameObject>(array_1);
    // For arrays we cast the cloned object to an array: 
    // array_2 = (GameObject[]) array_1.Clone();

    // This means that when we pass the in_array argument to the recursive function calls, it writes
    // to the same arrary without explicitely having a return/out parameter. This is also why we need to 
    // define the temp_obj and temp_list variables as new so we don't overwrite the original ones. 
    
    // Combinations are the possible N groups not taking order into account. For example if N = 2 and in_objs = [A,B,C]
    // we get 3 combinations: [AB, AC, BC] since [AB] == [BA] in this case. 
    private void GenerateCombinations(List<GameObject> in_objs, int N, List<GameObject> in_list, List<GameObject[]> in_array)
    {
        // Missing GameObjects in permutation list
        if (in_list.Count < N)
        {
            List<GameObject> temp_obj = new List<GameObject>(in_objs);
            foreach (GameObject go in in_objs)
            {
                temp_obj.Remove(go);
                List<GameObject> temp_list = new List<GameObject>(in_list);
                temp_list.Add(go);
                GenerateCombinations(temp_obj, N, temp_list, in_array);
            }

        }
        // Permutation list populated, add to out_list
        else if (N != 0)
        {
            in_array.Add(in_list.ToArray());
        }
        else
        {
            in_array.Clear();
        }
    }

    // Unlike Combinations, permutations take order into account, so for the previous example we would get 6
    // possibilities: [AB, BA, AC, CA, BC, CB]
    private void GeneratePermutations(List<GameObject> in_objs, int N, List<GameObject> in_list, List<GameObject[]> in_array)
    {
        // Missing GameObjects in permutation list
        if (in_list.Count < N)
        {

            foreach (GameObject go in in_objs)
            {
                List<GameObject> temp_obj = new List<GameObject>(in_objs);
                temp_obj.Remove(go);
                List<GameObject> temp_list = new List<GameObject>(in_list);
                temp_list.Add(go);
                GeneratePermutations(temp_obj, N, temp_list, in_array);
            }

        }
        // Permutation list populated, add to out_list
        else if (N != 0)
        {

            in_array.Add(in_list.ToArray());
        }
        else
        {
            in_array.Clear();
        }

    }


    private void Start()
    {
        string test = "bob";
        
        // Get Controllers instance
        PlayerController.OnBlack(true);
        PrepareAllTrials();
        Debug.Log("Generated :" + allTrials.Count + " trials. " + (allTrials.Count/TaskInfo.NumberOfSets) + " of which are different.");
    }

    #endregion ExperimentConfig

    #region Trial Flow
    // Experiment Start
    public bool IsRunning { get; private set; }
    public bool IsPaused { get; private set; }

    // Listeners for events. 
    private void StartExperiment()
    {
        IsRunning = true;
        _previousTrialError = 0;
        _previousTrialError = 0;
    }
    private void StopExperiment()
    {
        IsRunning = false;
        PlayerController.OnBlack(true);
    }

    private void PauseExperiment()
    {
        if(IsRunning && !IsPaused)
        {
            IsPaused = true;
            _currentState = "Paused";
            // Need to put absolute value because when trial timer is Infinite, the
            // pause trial timer would be -Infinity.
            _pauseTrialTimer = Mathf.Abs(Time.realtimeSinceStartup - _trialTimer);
            _trialTimer = Mathf.Infinity;
            _pauseStateTimer = Mathf.Abs(Time.realtimeSinceStartup - _stateTimer);
            _stateTimer = Mathf.Infinity;
            PlayerController.OnBlack(true);

        }
    }

    private void ResumeExperiment()
    {
        if (IsRunning && IsPaused)
        {
            _currentState = "Resumed";
            _trialTimer = Mathf.Abs(Time.realtimeSinceStartup - _pauseTrialTimer);
            _pauseTrialTimer = Mathf.Infinity;
            _stateTimer = Mathf.Abs(Time.realtimeSinceStartup - _pauseStateTimer);
            _pauseStateTimer = Mathf.Infinity;
            if(_stateTimer != Mathf.Infinity && _trialTimer != Mathf.Infinity)
                PlayerController.OnBlack(false);
            IsPaused = false;
        }
    }

    // Trial start
    private float _trialTimer = Mathf.Infinity;
    private float _pauseTrialTimer = Mathf.Infinity;
    private float _pauseStateTimer = Mathf.Infinity;
    private int _trialNumber = 0;
    private int _previousTrialError = 0; // 0:hit, 1: error, 2: ignored. 

    // Start Trial will be called after the ITI
    public void PrepareTrial()
    {
        // get current trial
        currentTrial = allTrials[_trialNumber];
        // increment counter after since we number trials 1: but indices are 0:
        _trialNumber++;
        currentTrial.Trial_Number = _trialNumber;

        // Prepare cues and targets. 
        HideCues();
        PrepareCues(); // Empty for this example, cue objects are visible. 
        HideTargets();
        PrepareTargets();
        HideDistractors();
        PrepareDistractors();

        //teleport player to the start position
        if (!TaskInfo.ContinuousTrials)
        {
            PlayerController.ToStart(currentTrial.Start_Position, Quaternion.identity);
        }

        // Sanity checks
        TrialEnded = false;
        Outcome = "aborted";
    }

    public void StartTrial()
    {
        _trialTimer = Time.realtimeSinceStartup;
        PlayerController.OnBlack(false);

    }

    // Cues
    private Material default_cue_material;
    public virtual void PrepareCues() { }
    
    public virtual void ShowCues()
    {
        // loop through all the cues. IN this example we do not set the cues position. 
        foreach (GameObject go in currentTrial.Cue_Objects)
        {
            default_cue_material = go.GetComponentInChildren<MeshRenderer>()?.material;
            if (default_cue_material != null)
                go.GetComponentInChildren<MeshRenderer>().material = currentTrial.Cue_Material;
            if (!go.activeSelf)
                go.SetActive(true);
        }
    }

    public virtual void HideCues()
    {
        // loop through all the cues. IN this example we do not set the cues position. 
        foreach (GameObject go in TaskInfo.CueObjects)
        {
            if (default_cue_material != null)
                go.GetComponentInChildren<MeshRenderer>().material = default_cue_material;
            // Could be used to hide the objects too. 
        }

    }

    // Targets
    public virtual void PrepareTargets()
    {
        int mat_idx;
        int pos_idx;

        for(int ii = 0; ii < currentTrial.Target_Objects.Length; ii++)
        {
            mat_idx = Mathf.Min(currentTrial.Target_Materials.Length-1, ii);
            pos_idx = Mathf.Min(currentTrial.Target_Positions.Length-1, ii);

            currentTrial.Target_Objects[ii].transform.position = currentTrial.Target_Positions[pos_idx];
            currentTrial.Target_Objects[ii].GetComponent<MeshRenderer>().material = currentTrial.Target_Materials[mat_idx];
            currentTrial.Target_Objects[ii].GetComponent<BoxCollider>().enabled = false;
            currentTrial.Target_Objects[ii].GetComponent<Renderer>().enabled = false;
        }
    }
    
    public virtual void ShowTargets()
    {
        foreach (GameObject go in currentTrial.Target_Objects)
        {
            go.GetComponent<BoxCollider>().enabled = true;
            go.GetComponent<Renderer>().enabled = true;
        }
    }

    public virtual void HideTargets()
    {
        foreach (GameObject go in TaskInfo.TargetObjects)
        {
            go.GetComponent<BoxCollider>().enabled = false;
            go.GetComponent<Renderer>().enabled = false;
        }
    }

    private bool _responseOK = false;
    public void CanRespond(bool OK)
    {
        _responseOK = OK;
    }

    // Distractors
    public virtual void PrepareDistractors()
    {
        int mat_idx;
        int pos_idx;

        for (int ii = 0; ii < currentTrial.Distractor_Objects.Length; ii++)
        {
            mat_idx = Mathf.Min(currentTrial.Distractor_Materials.Length - 1, ii);
            pos_idx = Mathf.Min(currentTrial.Distractor_Positions.Length - 1, ii);

            currentTrial.Distractor_Objects[ii].transform.position = currentTrial.Distractor_Positions[pos_idx];
            currentTrial.Distractor_Objects[ii].GetComponent<MeshRenderer>().material = currentTrial.Distractor_Materials[mat_idx];
            currentTrial.Distractor_Objects[ii].GetComponent<BoxCollider>().enabled = false;
            currentTrial.Distractor_Objects[ii].GetComponent<Renderer>().enabled = false;
        }
    }
    public virtual void ShowDistractors()
    {
        foreach (GameObject go in currentTrial.Distractor_Objects)
        {
            go.GetComponent<BoxCollider>().enabled = true;
            go.GetComponent<Renderer>().enabled = true;
        }
    }

    public virtual void HideDistractors()
    {
        foreach (GameObject go in TaskInfo.DistractorObjects)
        {
            go.GetComponent<BoxCollider>().enabled = false;
            go.GetComponent<Renderer>().enabled = false;
        }
    }

    // MISC
    public void FreezePlayer(bool ON)
    {
        PlayerController.Freeze(ON);
    }


    // End of trial
    private bool TrialEnded = false;
    private string Outcome = "";

    public void EndTrial()
    {
        _trialTimer = Mathf.Infinity;
        _responseOK = false;
        
        if (!TaskInfo.ContinuousTrials)
        {
            PlayerController.OnBlack(true);
        }
        // When switching to onblack and teleporting to the start position the trigger volume
        // is not "exited" properly so we need to clear the state. If not it triggers a end of trial
        // as soon as the trial starts. 
        PlayerController.ClearCollisionStatus();

        HideCues();
        HideTargets();
        HideDistractors();
        // Publish DATA
        TrialEnded = true;
    }

    // Will check whether the TRIAL is over (fixation break, time run out)
    [HideInInspector]
    public bool IsTrialOver
    {
        get
        {
            bool targ = false;
            bool dist = false;
            // check if target or distractor
            if (_trialTimer != Mathf.Infinity)
            {
                foreach (GameObject go in currentTrial.Target_Objects)
                {
                    if (frameData.Player_State == go.name)
                    {
                        targ = true;
                    }
                }

                foreach (GameObject go in currentTrial.Distractor_Objects)
                {
                    if (frameData.Player_State == go.name)
                    {
                        dist = true;
                    }

                }

                if (_responseOK && targ)
                {
                    Outcome = "correct";
                    _previousTrialError = 0;
                    return true;
                }
                else if (_responseOK && dist)
                {
                    Outcome = "distractor";
                    _previousTrialError = 1;
                    return true;
                }
                else if (!_responseOK && (targ || dist))
                {
                    Outcome = "early_response";
                    _previousTrialError = 1;
                    return true;
                }
                else if (_responseOK && (Time.realtimeSinceStartup - _trialTimer) > TaskInfo.MaxTrialTime)
                {
                    Outcome = "no_response";
                    _previousTrialError = 2;
                    return true;
                }
                else if (!_responseOK && (Time.realtimeSinceStartup - _trialTimer) > TaskInfo.MaxTrialTime)
                {
                    Outcome = "ignored";
                    _previousTrialError = 2;
                    return true;
                }
                else if (_fixRequired && !_isFixating)
                {
                    Outcome = "break_fixation";
                    _previousTrialError = 1;
                    return true;
                }
                else
                {
                    return false; 
                }
            }
            else if(currentTrial != null)
            {
                return false;
            }
            else
            {
                return false;
            }
        }
    }
    #endregion Trial Flow

    #region State Handling
    // These properties will determine whether the current frame information triggers a state 
    // change in the state system. 
    private string _currentState;
    private float _stateDuration;
    private float _stateTimer;

    private bool _fixRequired;
    private bool _isFixating = false;

    private List<string> triggerGroup = new List<string>();
    public bool IsTouchingTrigger
    {
        get
        {
            if (frameData.Player_State != "" && triggerGroup.Count > 0)
            {
                return triggerGroup.IndexOf(frameData.Player_State) != -1;
            }
            else
            {
                return false;
            }
        }
    }

    public void StartState(string name, float duration, bool fixation, string triggers)
    {
        _currentState = name;
        _stateDuration = duration;
        _fixRequired = fixation;

        triggerGroup.Clear();
        GameObject[] temp_array;
        switch (triggers)
        {
            case "Cue":
                temp_array = TaskInfo.CueOnsetTriggers;
                break;
            case "Targets":
                temp_array = TaskInfo.TargetOnsetTriggers;
                break;
            case "Misc":
                temp_array = TaskInfo.MiscTriggers;
                break;
            case "None":
                temp_array = new GameObject[0];
                break;
            default:
                temp_array = new GameObject[0];
                break;
        }
        foreach (GameObject go in temp_array)
            triggerGroup.Add(go.name);

        _stateTimer = Time.realtimeSinceStartup;
        // Add ITI penalties
        if (_currentState == "ITI")
        {
            if (_previousTrialError == 1)
                _stateTimer += TaskInfo.ErrorPenalty;
            if (_previousTrialError == 2)
                _stateTimer += TaskInfo.IgnorePenalty;
        }
    }

    // Check to see if the current state is over, NOT the trial (e.g. fixation break or time run out).
    // Will check if
    //  Duration has elapsed.
    //  Triggers were touched. 
    [HideInInspector]
    public bool IsStateOver
    {
        get
        {
            if (IsPaused)
                return false;
            else
                return (Time.realtimeSinceStartup - _stateTimer) > _stateDuration || IsTouchingTrigger;
        }
    }
    #endregion State Handling

    #region Event handling
    private void OnEnable()
    {
        // Add listener for Update Events 
        EventsController.OnPlayerLateUpdate += UpdatePlayer;
        EventsController.OnEyeLateUpdate += UpdateEye;
        EventsController.OnBegin += StartExperiment;
        EventsController.OnEnd += StopExperiment;
        EventsController.OnPause += PauseExperiment;
        EventsController.OnResume += ResumeExperiment;

    }

    private void OnDisable()
    {
        // Remove listeners
        EventsController.OnPlayerLateUpdate -= UpdatePlayer;
        EventsController.OnEyeLateUpdate -= UpdateEye;
        EventsController.OnBegin -= StartExperiment;
        EventsController.OnEnd -= StopExperiment;
        EventsController.OnPause -= PauseExperiment;
        EventsController.OnResume -= ResumeExperiment;
    }

    void UpdatePlayer(Vector3 position, float rotation, string status, float hInput, float vInput)
    {
        frameData.Position = position;
        frameData.Rotation = rotation;
        frameData.Player_State = status;
        frameData.JoystickPosition.x = hInput;
        frameData.JoystickPosition.y = vInput;
    }

    void UpdateEye(Vector2 gazePosition, string gazeTargets)
    {
        frameData.GazePosition = gazePosition;
        frameData.GazeTargets = gazeTargets;
    }
    #endregion Event handling

    #region Frame Publish
    // Update is called once per frame
    void Update()
    {
        // Coroutine to publish at the end of frame. 
        StartCoroutine(WriteMarkerAfterImageIsRendered());
    }

    // End of frame publishing
    // Frame Publish
    IEnumerator WriteMarkerAfterImageIsRendered()
    {
        yield return new WaitForEndOfFrame();
        if (TrialEnded)
        {
            currentTrial.Outcome = Outcome;
            MonkeyLogicController.PublishTrial(currentTrial.GetData(MonkeyLogicController.GetLSLTime()));
            TrialEnded = false;
        }

        // Read data at the last minute to make sure every other controller has updated the values. 
        // Time information in defined in the _frameData.GetData() script; 
        // Set current frame trial state; 
        frameData.Trial_State = _currentState;
        MonkeyLogicController.PublishFrame(frameData.GetData(MonkeyLogicController.GetLSLTime()));
        frameData.Clear();

        yield return null;
    }
    #endregion Frame Publish
}

