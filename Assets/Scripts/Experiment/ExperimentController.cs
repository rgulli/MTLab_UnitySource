using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Profiling;
using System.Linq;
using Misc;

public abstract class ExperimentController : MonoBehaviour
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
    public FirstPerson.PlayerController playerController;
    public MonkeyLogicController monkeyLogicController;
    public EyeLinkController eyeLinkController;
    public UserInputController userInputController;

    // Data structures
    protected FrameData _frameData = new FrameData();
    protected TrialData _currentTrial = new TrialData();
    protected List<TrialData> _allTrials = new List<TrialData>();

    [HideInInspector]
    public IDictionary<string, int> InstanceIDMap = new Dictionary<string, int>();

    // Generate Trials
    // Structure holding all the parameters that are set in the Editor. 
    public dynamic taskInfo;

    public virtual void PrepareAllTrials()
    {
        // set the seed
        UnityEngine.Random.InitState(DateTime.Now.Millisecond);

        // Clear trial list
        _allTrials = new List<TrialData>();

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

        GenerateCombinations(new List<GameObject>(taskInfo.TargetObjects), taskInfo.NTargets, new List<GameObject>(), all_targets_comb);
        GenerateCombinations(new List<GameObject>(taskInfo.DistractorObjects), taskInfo.NDistractors, new List<GameObject>(), all_distractors_comb);
        GeneratePermutations(new List<GameObject>(taskInfo.PossiblePositions), taskInfo.NTargets + taskInfo.NDistractors, new List<GameObject>(), all_positions_perm);

        // Loop for start positions (only 1 per trial)
        for (int start_index = 0; start_index < taskInfo.StartPositions.Length; start_index++)
        {
            // Loop through all the conditions defined in the list
            for (int cnd_index = 0; cnd_index < taskInfo.Conditions.Length; cnd_index++)
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
                                taskInfo.Conditions[cnd_index].TargetMaterials[Mathf.Min(ii, taskInfo.Conditions[cnd_index].TargetMaterials.Length - 1)];
                        }

                        for (int ii = 0; ii < dists.Length; ii++)
                        {
                            dist_mat[ii] =
                                taskInfo.Conditions[cnd_index].DistractorMaterials[Mathf.Min(ii, taskInfo.Conditions[cnd_index].DistractorMaterials.Length - 1)];
                        }

                        foreach (GameObject[] poss in all_positions_perm)
                        {
                            // the positions are game objects, so we need to split between targets and distractors
                            // and get the position values in Vector3 format;
                            List<Vector3> targ_pos = new List<Vector3>();
                            List<Vector3> dist_pos = new List<Vector3>();
                            for (int ii = 0; ii < poss.Length; ii++)
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

                            // Convert fixation screen positions to world positions
                            // This should loop through and generate various trials according
                            // to your experiment. For example purposes we only have 1. 
                            Vector3[] fixPositions;
                            if (taskInfo.WorldFixationOffsets.Length > 0)
                            {
                                fixPositions = taskInfo.WorldFixationOffsets;
                            }
                            else if (taskInfo.ScreenFixationOffsets.Length > 0 )
                            {
                                List<Vector3> lstVec = new List<Vector3>();

                                Vector3[] frustumCorners = new Vector3[4];
                                foreach(Vector2 vct in taskInfo.ScreenFixationOffsets)
                                {
                                    Camera.main.CalculateFrustumCorners(new Rect(vct.x, vct.y, 0, 0),
                                                                    Camera.main.nearClipPlane + (0.5f * taskInfo.FixationObjectSize),
                                                                    Camera.MonoOrStereoscopicEye.Mono, frustumCorners);
                                    lstVec.Add(frustumCorners[0]);
                                }
                                
                                // normalize
                                fixPositions = lstVec.ToArray();

                            }
                            else
                            {
                                fixPositions = new Vector3[1];
                                fixPositions[0] = Vector3.negativeInfinity;
                            }

                            // At this point we have everything. Add to trial list N times:
                            for (int ii = 0; ii < taskInfo.NumberOfSets; ii++)
                            {
                                _allTrials.Add(
                                    new TrialData
                                    {
                                        Trial_Number = 0,
                                        Start_Position = taskInfo.StartPositions[start_index].transform.position,
                                        // Fix point
                                        Fix_Objects = taskInfo.FixationObjects,
                                        Fix_Positions = fixPositions,

                                        // Current Cue
                                        Cue_Objects = taskInfo.CueObjects,
                                        Cue_Material = taskInfo.Conditions[cnd_index].CueMaterial,
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
        _allTrials = _allTrials.OrderBy(x => UnityEngine.Random.value).ToList();
        Debug.Log("Generated :" + _allTrials.Count + " trials. " + (_allTrials.Count / taskInfo.NumberOfSets) + " of which are different.");
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
    protected void GenerateCombinations(List<GameObject> in_objs, int N, List<GameObject> in_list, List<GameObject[]> in_array)
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
        else
        {
            in_array.Add(in_list.ToArray());
        }

    }

    // Unlike Combinations, permutations take order into account, so for the previous example we would get 6
    // possibilities: [AB, BA, AC, CA, BC, CB]
    protected void GeneratePermutations(List<GameObject> in_objs, int N, List<GameObject> in_list, List<GameObject[]> in_array)
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
        else
        {
            in_array.Add(in_list.ToArray());
        }

    }

    private void Start()
    {
        Initialize();
    }

    protected virtual void Initialize()
    {
        QualitySettings.maxQueuedFrames = 0;
        // Coroutine to publish at the end of frame. 
        StartCoroutine("WriteMarkerAfterImageIsRendered");

        // Get Controllers instance
        playerController.OnBlack(true);

        //PrepareAllTrials();
        
    }

    // Generate dictionary mapping object name to instance ID
    protected void GenerateIDMap()
    {
        // Get all colliders and create a dictionary of name:instanceID
        Collider[] colliders = FindObjectsOfType<Collider>();
        foreach (Collider col in colliders)
        {
            string name = col.gameObject.name;
            int ID = col.gameObject.GetInstanceID();
            InstanceIDMap.Add(name, ID);
        }

    }
    public int NameToID(string name)
    {
        if (InstanceIDMap.TryGetValue(name, out int ID))
            return ID;
        else
            return -1;
    }

    public string IDToName(int ID)
    {
        foreach (KeyValuePair<string, int> kv in InstanceIDMap)
        {
            if (kv.Value == ID)
                return kv.Key;
        }
        return null;
    }

    #endregion ExperimentConfig

    #region Trial Flow
    // Experiment Start
    public bool IsRunning { get; private set; }
    public bool IsPaused { get; private set; }

    // Listeners for events. 
    protected void StartExperiment()
    {
        IsRunning = true;
        _previousTrialError = 0;
        _previousTrialError = 0;
    }
    protected void StopExperiment()
    {
        IsRunning = false;
        playerController.OnBlack(true);
    }

    protected void PauseExperiment()
    {
        if (IsRunning && !IsPaused)
        {
            IsPaused = true;
            currentState = StateNames.Pause;
            // Need to put absolute value because when trial timer is Infinite, the
            // pause trial timer would be -Infinity.
            _pauseTrialTimer = Mathf.Abs(Time.realtimeSinceStartup - _trialTimer);
            _trialTimer = Mathf.Infinity;
            _pauseStateTimer = Mathf.Abs(Time.realtimeSinceStartup - _stateTimer);
            _stateTimer = Mathf.Infinity;
            playerController.OnBlack(true);

        }
    }

    protected void ResumeExperiment()
    {
        if (IsRunning && IsPaused)
        {
            currentState = StateNames.Resume;
            _trialTimer = Mathf.Abs(Time.realtimeSinceStartup - _pauseTrialTimer);
            _pauseTrialTimer = Mathf.Infinity;
            _stateTimer = Mathf.Abs(Time.realtimeSinceStartup - _pauseStateTimer);
            _pauseStateTimer = Mathf.Infinity;
            if (_stateTimer != Mathf.Infinity && _trialTimer != Mathf.Infinity)
                playerController.OnBlack(false);
            IsPaused = false;
        }
    }

    // Trial start
    protected float _trialTimer = Mathf.Infinity;
    protected float _pauseTrialTimer = Mathf.Infinity;
    protected float _pauseStateTimer = Mathf.Infinity;
    protected int _trialNumber = 0;
    protected int _previousTrialError = 0; // 0:hit, 1: error, 2: ignored. 

    // Start Trial will be called after the ITI
    public virtual void PrepareTrial()
    {
        // get current trial
        _currentTrial = _allTrials[_trialNumber];
        // increment counter after since we number trials 1: but indices are 0:
        _trialNumber++;
        _currentTrial.Trial_Number = _trialNumber;

        // Prepare cues and targets. 
        HideFixationObject();
        PrepareFixationObject();
        HideCues();
        PrepareCues(); // Empty for this example, cue objects are visible. 
        HideTargets();
        PrepareTargets();
        HideDistractors();
        PrepareDistractors();

        //teleport player to the start position
        if (!taskInfo.ContinuousTrials)
        {
            playerController.ToStart(_currentTrial.Start_Position, Quaternion.identity);
        }

        // Sanity checks
        TrialEnded = false;
        Outcome = "aborted";
    }

    public void StartTrial()
    {
        _trialTimer = Time.realtimeSinceStartup;
        playerController.OnBlack(false);

    }

    // Fixation objects. We use fixation "objects" and not "point" because 
    // fixation can be required on any object. 
    public virtual void PrepareFixationObject()
    {

        int pos_idx;

        for (int ii = 0; ii < _currentTrial.Fix_Objects.Length; ii++)
        {
            pos_idx = Mathf.Min(_currentTrial.Fix_Positions.Length - 1, ii);

            _currentTrial.Fix_Objects[ii].transform.localPosition = _currentTrial.Fix_Positions[pos_idx];
            _currentTrial.Fix_Objects[ii].transform.localScale = new Vector3 { x = taskInfo.FixationObjectSize, y = taskInfo.FixationObjectSize, z = taskInfo.FixationObjectSize };
            _currentTrial.Fix_Objects[ii].GetComponent<SphereCollider>().radius = taskInfo.FixationWindow;

        }

    }

    public virtual void ShowFixationObject()
    {
        // loop through all the cues. IN this example we do not set the cues position. 
        foreach (GameObject go in _currentTrial.Fix_Objects)
        {
            go.GetComponent<BoxCollider>().enabled = true;
            go.GetComponent<Renderer>().enabled = true;
        }
    }

    public virtual void HideFixationObject()
    {
        // loop through all the cues. IN this example we do not set the cues position. 
        foreach (GameObject go in _currentTrial.Fix_Objects)
        {
            go.GetComponent<BoxCollider>().enabled = false;
            go.GetComponent<Renderer>().enabled = false;
        }
    }

    // Cues
    private Material default_cue_material;
    public virtual void PrepareCues() { }
    
    public virtual void ShowCues()
    {
        // loop through all the cues. IN this example we do not set the cues position. 
        foreach (GameObject go in _currentTrial.Cue_Objects)
        {
            default_cue_material = go.GetComponentInChildren<MeshRenderer>()?.material;
            if (default_cue_material != null)
                go.GetComponentInChildren<MeshRenderer>().material = _currentTrial.Cue_Material;
            if (!go.activeSelf)
                go.SetActive(true);
        }
    }

    public virtual void HideCues()
    {
        // loop through all the cues. IN this example we do not set the cues position. 
        foreach (GameObject go in taskInfo.CueObjects)
        {
            // For the sample task "hiding" the cues means setting the default material back
            // on the object, hiding the cue color. 
            if (default_cue_material != null)
                go.GetComponentInChildren<MeshRenderer>().material = default_cue_material;
            
            // Can be set to hide the objects too. 
            //if (!go.activeSelf)
            //    go.SetActive(false);
        }

    }

    // Targets
    public virtual void PrepareTargets()
    {
        int mat_idx;
        int pos_idx;

        for(int ii = 0; ii < _currentTrial.Target_Objects.Length; ii++)
        {
            mat_idx = Mathf.Min(_currentTrial.Target_Materials.Length-1, ii);
            pos_idx = Mathf.Min(_currentTrial.Target_Positions.Length-1, ii);

            _currentTrial.Target_Objects[ii].transform.position = _currentTrial.Target_Positions[pos_idx];
            _currentTrial.Target_Objects[ii].GetComponent<MeshRenderer>().material = _currentTrial.Target_Materials[mat_idx];
            _currentTrial.Target_Objects[ii].GetComponent<BoxCollider>().enabled = false;
            _currentTrial.Target_Objects[ii].GetComponent<Renderer>().enabled = false;
        }
    }
    
    public virtual void ShowTargets()
    {
        foreach (GameObject go in _currentTrial.Target_Objects)
        {
            go.GetComponent<BoxCollider>().enabled = true;
            go.GetComponent<Renderer>().enabled = true;
        }
    }

    public virtual void HideTargets()
    {
        foreach (GameObject go in taskInfo.TargetObjects)
        {
            go.GetComponent<BoxCollider>().enabled = false;
            go.GetComponent<Renderer>().enabled = false;
        }
    }

    private bool ResponseOK = false;
    public void CanRespond(bool OK)
    {
        ResponseOK = OK;
    }

    // Distractors
    public virtual void PrepareDistractors()
    {
        int mat_idx;
        int pos_idx;

        for (int ii = 0; ii < _currentTrial.Distractor_Objects.Length; ii++)
        {
            mat_idx = Mathf.Min(_currentTrial.Distractor_Materials.Length - 1, ii);
            pos_idx = Mathf.Min(_currentTrial.Distractor_Positions.Length - 1, ii);

            _currentTrial.Distractor_Objects[ii].transform.position = _currentTrial.Distractor_Positions[pos_idx];
            _currentTrial.Distractor_Objects[ii].GetComponent<MeshRenderer>().material = _currentTrial.Distractor_Materials[mat_idx];
            _currentTrial.Distractor_Objects[ii].GetComponent<BoxCollider>().enabled = false;
            _currentTrial.Distractor_Objects[ii].GetComponent<Renderer>().enabled = false;
        }
    }
    public virtual void ShowDistractors()
    {
        foreach (GameObject go in _currentTrial.Distractor_Objects)
        {
            go.GetComponent<BoxCollider>().enabled = true;
            go.GetComponent<Renderer>().enabled = true;
        }
    }

    public virtual void HideDistractors()
    {
        foreach (GameObject go in taskInfo.DistractorObjects)
        {
            go.GetComponent<BoxCollider>().enabled = false;
            go.GetComponent<Renderer>().enabled = false;
        }
    }

    // MISC
    public virtual void FreezePlayer(bool ON)
    {
        playerController.Freeze(ON);
    }


    // End of trial
    protected bool TrialEnded = false;
    protected string Outcome = "";

    public virtual void EndTrial()
    {
        _trialTimer = Mathf.Infinity;
        ResponseOK = false;
        
        if (!taskInfo.ContinuousTrials)
        {
            playerController.OnBlack(true);
        }
        // When switching to onblack and teleporting to the start position the trigger volume
        // is not "exited" properly so we need to clear the state. If not it triggers a end of trial
        // as soon as the trial starts. 
        playerController.ClearCollisionStatus();

        HideCues();
        HideTargets();
        HideDistractors();
        // Publish DATA
        TrialEnded = true;
    }

    // Will check whether the TRIAL is over (fixation break, time run out)
    public bool IsTrialOver()
    {
        bool targ = false;
        bool dist = false;
        // check if target or distractor
        if (_trialTimer != Mathf.Infinity)
        {
            foreach (GameObject go in _currentTrial.Target_Objects)
            {
                if (IDToName((int)_frameData.Player_State) == go.name)
                {
                    targ = true;
                }
            }

            foreach (GameObject go in _currentTrial.Distractor_Objects)
            {
                if (IDToName((int)_frameData.Player_State) == go.name)
                {
                    dist = true;
                }

            }

            if (ResponseOK && targ)
            {
                Outcome = "correct";
                _previousTrialError = 0;
                return true;
            }
            else if (ResponseOK && dist)
            {
                Outcome = "distractor";
                _previousTrialError = 1;
                return true;
            }
            else if (!ResponseOK && (targ || dist))
            {
                Outcome = "early_response";
                _previousTrialError = 1;
                return true;
            }
            else if (ResponseOK && (Time.realtimeSinceStartup - _trialTimer) > taskInfo.MaxTrialTime)
            {
                Outcome = "no_response";
                _previousTrialError = 2;
                return true;
            }
            else if (!ResponseOK && (Time.realtimeSinceStartup - _trialTimer) > taskInfo.MaxTrialTime)
            {
                Outcome = "ignored";
                _previousTrialError = 2;
                return true;
            }
            else if (fixRequired && !isFixating())
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
        else if(_currentTrial != null)
        {
            return false;
        }
        else
        {
            return false;
        }
   
    }
    #endregion Trial Flow

    #region State Handling
    // These properties will determine whether the current frame information triggers a state 
    // change in the state system. 
    protected StateNames currentState;
    protected float stateDuration;
    protected float _stateTimer;

    protected bool fixRequired;
    protected virtual bool isFixating()
    {
        // also check whether objects are fixation objects
        return _frameData.GazeTargets.Any(x => _currentTrial.Fix_Objects.Any(y => (float)y.GetInstanceID() == x));
    }
    

    protected List<string> triggerGroup = new List<string>();
    public bool IsTouchingTrigger
    {
        get
        {
            if (_frameData.Player_State != -1 && triggerGroup.Count > 0)
            {
                return triggerGroup.IndexOf(IDToName((int)_frameData.Player_State)) != -1;
            }
            else
            {
                return false;
            }
        }
    }

    public void StartState(StateNames name, float duration, bool fixation, string triggers)
    {
        currentState = name;
        stateDuration = duration;
        fixRequired = fixation;

        triggerGroup.Clear();
        GameObject[] temp_array;
        switch (triggers)
        {
            case "Cue":
                temp_array = taskInfo.CueOnsetTriggers;
                break;
            case "Targets":
                temp_array = taskInfo.TargetOnsetTriggers;
                break;
            case "Misc":
                temp_array = taskInfo.MiscTriggers;
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
        if (currentState == StateNames.ITI)
        {
            if (_previousTrialError == 1)
                _stateTimer += taskInfo.ErrorPenalty;
            if (_previousTrialError == 2)
                _stateTimer += taskInfo.IgnorePenalty;
        }
    }

    // Check to see if the current state is over, NOT the trial (e.g. fixation break or time run out).
    // Will check if
    //  Duration has elapsed.
    //  Triggers were touched. 
    public bool IsStateOver()
    {
        if (IsPaused)
            return false;
        else
            return (Time.realtimeSinceStartup - _stateTimer) > stateDuration || IsTouchingTrigger;
    }
    #endregion State Handling

    #region Event handling
    protected void OnEnable()
    {
        // Colliders name: gameObject instance ID to send to stream
        // Need to be computed BEFORE Start()
        GenerateIDMap();

        // Add listener for Update Events 
        EventsController.OnPlayerLateUpdate += UpdatePlayer;
        EventsController.OnEyeLateUpdate += UpdateEye;
        EventsController.OnPhotoDiodeUpdate += UpdatePhotoDiode;
        EventsController.OnBegin += StartExperiment;
        EventsController.OnEnd += StopExperiment;
        EventsController.OnPause += PauseExperiment;
        EventsController.OnResume += ResumeExperiment;

    }

    protected void OnDisable()
    {
        // Remove listeners
        EventsController.OnPlayerLateUpdate -= UpdatePlayer;
        EventsController.OnEyeLateUpdate -= UpdateEye;
        EventsController.OnPhotoDiodeUpdate -= UpdatePhotoDiode;
        EventsController.OnBegin -= StartExperiment;
        EventsController.OnEnd -= StopExperiment;
        EventsController.OnPause -= PauseExperiment;
        EventsController.OnResume -= ResumeExperiment;
    }

    void UpdatePlayer(Vector3 position, float rotation, string status, float hInput, float vInput)
    {
        _frameData.Position = position;
        _frameData.Rotation = rotation;
        _frameData.Player_State = NameToID(status);
        _frameData.JoystickPosition.x = hInput;
        _frameData.JoystickPosition.y = vInput;
    }

    void UpdateEye(Vector2 gazePosition, string[] gazeTargets, float[] gazeCounts)
    {
        _frameData.GazePosition = gazePosition;
        float[] gazeTargetIDs = new float[] { -1, -1, -1, -1, -1 };
        if (gazeTargets != null)
        {
            for (int i = 0; i < Mathf.Min(5, gazeTargets.Length); i++)
            {
                if (gazeTargets[i] != null)
                {
                    gazeTargetIDs[i] = NameToID(gazeTargets[i]);
                }
            }
        }
        _frameData.GazeTargets = gazeTargetIDs;
        _frameData.GazeRayCounts = gazeCounts;
    }

    void UpdatePhotoDiode(float intensity)
    {
        _frameData.PhotoDiodeIntensity = intensity; 
    }
    #endregion Event handling

    #region Frame Publish

    // End of frame publishing
    // Frame Publish
    IEnumerator WriteMarkerAfterImageIsRendered()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            
            if (TrialEnded)
            {
                _currentTrial.Outcome = Outcome;
                monkeyLogicController.PublishTrial(_currentTrial.GetData(monkeyLogicController.GetLSLTime()));
                TrialEnded = false;
            }

            // Read data at the last minute to make sure every other controller has updated the values. 
            // Time information in defined in the _frameData.GetData() script; 
            // Set current frame trial state; 
            _frameData.Trial_State = currentState;
            monkeyLogicController.PublishFrame(_frameData.GetData(monkeyLogicController.GetLSLTime()));
            _frameData.Clear();
            yield return null;
        }

    }
    #endregion Frame Publish
}
