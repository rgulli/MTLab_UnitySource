using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleTaskExpController : ExperimentController
{
    // The taskInfo base class is defined as "dynamic" in the experiment
    // controller class, this means that the class is implemented at runtime
    // we therefore need to define it now. 
    // We use a specific sub-class of task info and set it as "taskInfo" so
    // all the base scripts will still work. 
    public SampleTaskTaskInfo sTaskInfo;
    private new void OnEnable()
    {
        base.OnEnable();
        taskInfo = sTaskInfo;
    }

    private new void OnDisable()
    { 
        base.OnDisable();
    }

    private void Start()
    {
        // Base function call. 
        base.Initialize();
        
    }
}
