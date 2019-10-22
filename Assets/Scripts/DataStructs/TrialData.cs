using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrialData
{
    public int Trial_Number;
    public Vector3 Start_Position;

    [System.NonSerialized] public GameObject[] Fix_Objects;
    public string[] fix_Objects;
    public Vector3[] Fix_Positions;
    
    [System.NonSerialized] public GameObject[] Cue_Objects;
    public string[] cue_Objects;

    [System.NonSerialized] public Material Cue_Material;
    public string cue_Material;

    [System.NonSerialized] public GameObject[] Target_Objects;
    public string[] target_Objects;

    [System.NonSerialized] public Material[] Target_Materials;
    public string[] target_Materials; 

    public Vector3[] Target_Positions;

    [System.NonSerialized] public GameObject[] Distractor_Objects;
    public string[] distractor_Objects;

    [System.NonSerialized] public Material[] Distractor_Materials;
    public string[] distractor_Materials;

    public Vector3[] Distractor_Positions;

    public string Outcome;

    public double Unity_Local_Time;

    public string GetData(double curr_time)
    {
        // Convert game objects to string to replace "instance ID" in the saved data
        // Cue objects
        cue_Objects = new string[Cue_Objects.Length];
        for (int ii = 0; ii < cue_Objects.Length; ii++)
        {
            cue_Objects[ii] = Cue_Objects[ii].name;
        }
        cue_Material = Cue_Material.name;

        // Target objects
        target_Objects = new string[Target_Objects.Length];
        target_Materials = new string[Target_Objects.Length];

        for (int ii = 0; ii < target_Objects.Length; ii++)
        {
            target_Objects[ii] = Target_Objects[ii].name;
            target_Materials[ii] = Target_Materials[Mathf.Min(ii, Target_Materials.Length - 1)].name;
        }

        // Target objects
        distractor_Objects = new string[Distractor_Objects.Length];
        distractor_Materials = new string[Distractor_Objects.Length];

        for (int ii = 0; ii < distractor_Objects.Length; ii++)
        {
            distractor_Objects[ii] = Distractor_Objects[ii].name;
            distractor_Materials[ii] = Distractor_Materials[Mathf.Min(ii, Distractor_Materials.Length - 1)].name;
        }

        Unity_Local_Time = curr_time;
        return JsonUtility.ToJson(this);
    }

}
