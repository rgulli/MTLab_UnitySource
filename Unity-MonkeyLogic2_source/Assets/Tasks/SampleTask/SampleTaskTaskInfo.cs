using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleTaskTaskInfo : TaskInfo
{
    // On Validate is called when the public properties are either changed or loaded. 
    // It serves to make sure the data is entered properly and can be used to generate
    // grids for target placement for example. 
    private void OnValidate()
    {
        BaseValidate();
    }
}
