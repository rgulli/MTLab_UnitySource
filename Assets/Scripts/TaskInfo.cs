///<summary>
/// This class holds all the parameters for the specific task you are running. You can modify at will. 
/// The Tooltips are not necessary, they just add a tooltip window in the editor GUI. 
/// Same with Headers, they just make things prettier. 
/// 
/// What you need to know for this is:
///     - Variables need to be public to be accessible from the editor and other classes. 
///     - The opposite behavior is a private variable. 
///     - The variables here will be read by the Experiment Controller to generate trials. 
///     - Setting values here only means setting their default values. During a trial, the value used will
///         be the one set in the editor. 
///     - 
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskInfo : MonoBehaviour
{
    // Objects will appear in this order in the Inspector panel of the Editor GUI. 
    [Header("Session Parameters")]
    // Will generate X number of sets containing all the possible conditions and shuffle them 
    // to get randomized trials. Will auto pause when completed. 
    public int NumberOfSets = 15;

    [Header("Trials")]
    public GameObject[] StartPositions;
    public bool ContinuousTrials = true;
    [Tooltip("In seconds.")] public float MaxTrialTime = 50.0f;
    [Tooltip("Extra time added to ITI in seconds.")] public float ErrorPenalty = 0.0f;
    [Tooltip("Penalize out of time trials.")] public float IgnorePenalty = 0.0f;

    [Header("Cues/Contexts")]
    // This class (defined at the bottom), can be expanded a lot to add condition specific start positions, 
    // target positions,... 
    public GameObject[] CueOnsetTriggers;
    public GameObject[] CueObjects;
    
    [Header("Targets/Distractors")]
    public GameObject[] TargetOnsetTriggers;
    public int NTargets = 1;
    [Tooltip("Set to 0 to not use Distractors")]public int NDistractors = 1; 
    public GameObject[] TargetObjects;
    public GameObject[] DistractorObjects;
    public GameObject[] PossiblePositions;
    [Tooltip("Response time (s).")] public float ResponseTime = 1.0f;

    [Header("Conditions")]
    public Condition[] Conditions;

    [Header("Misc Objects")]
    public GameObject[] MiscTriggers;

    // Options validation
    private void OnValidate()
    {
        // Can't penalize errors if trials are continuous. 
        if (ErrorPenalty > 0)
        {
            ContinuousTrials = false;
        }

        // Can't have more objects in a trial than there are in the scene. 
        if (NTargets > TargetObjects.Length)
        {
            NTargets = TargetObjects.Length;
        }

        if (NDistractors > DistractorObjects.Length)
        {
            NDistractors = DistractorObjects.Length;
        }

        // Also if we have NTargets + NDistractors objects, we need at least that
        // number of positions
        int diff = (NDistractors + NTargets) - PossiblePositions.Length;
        if (diff > 0)
        {
            NTargets = Mathf.Max(0, NTargets - (diff - NDistractors));
            NDistractors = Mathf.Max(0, (NDistractors - diff));
        }

    }
}

// To create the mappings between cues/contexts and texture values
[System.Serializable]
public struct Condition
{
    public Material CueMaterial;
    public Material[] TargetMaterials;
    public Material[] DistractorMaterials;
}
