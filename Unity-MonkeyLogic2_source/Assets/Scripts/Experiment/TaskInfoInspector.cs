using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TaskInfo))]
public class TaskInfoInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        TaskInfo ti = (TaskInfo)target;
        if(GUILayout.Button("Generate Trials."))
        {
            ti.GenerateTrials();
        }
    }
}
