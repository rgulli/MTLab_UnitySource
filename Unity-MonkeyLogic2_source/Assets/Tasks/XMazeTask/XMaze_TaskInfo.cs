using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XMaze_TaskInfo : TaskInfo
{
    // Graded or all/nothing reward
    public bool GradedReward = true;

    // Position Markers
    public GameObject NorthMarker;
    public GameObject SouthMarker;
    public GameObject NorthTrigger;
    public GameObject SouthTrigger;

    // Materials for context 2 and color hierarchy
    public Material Ctx_One;
    public Material High_CtxOne;
    public Material Mid_CtxOne;
    public Material Low_CtxOne;
    // Materials for context 2 and color hierarchy
    public Material Ctx_Two;
    public Material High_CtxTwo;
    public Material Mid_CtxTwo;
    public Material Low_CtxTwo;
    // On Validate is called when the public properties are either changed or loaded. 
    // It serves to make sure the data is entered properly and can be used to generate
    // grids for target placement for example. 
    private void OnValidate()
    {
        // Call the validate function of base-class TaskInfo.
        // Note that we do not use base.OnValidate() as this function
        // is not "virtual" so we are not "overridding" it. 
        BaseValidate();

        // Generate the Condition list for the two contexts. 
        // The possible trial conditions are: 
        //    Ctx 1/2: High-Low, High-Mid, Mid-Low
        // Yielding 6 possible types. Keeping in mind that East-West positions are
        // randomized during trial preparation. 
        // The "Condition" structure is defined in TaskInfo.cs and it contains
        // a material variable for the Context Texture, as well as material arrays
        // for targets and distractors textures. The "Conditions" variable in an array
        // of conditions. We need to populate these arrays:
        Conditions = new Condition[6];

        // Context 1; 
        // Create a new Condition structure and define its values between the { }
        Conditions[0] = new Condition
        {
            CueMaterial = Ctx_One,
            TargetMaterials = new Material[1] { High_CtxOne },
            DistractorMaterials = new Material[1] { Low_CtxOne }
        };

        Conditions[1] = new Condition
        {
            CueMaterial = Ctx_One,
            TargetMaterials = new Material[1] { High_CtxOne },
            DistractorMaterials = new Material[1] { Mid_CtxOne }
        };

        Conditions[2] = new Condition
        {
            CueMaterial = Ctx_One,
            TargetMaterials = new Material[1] { Mid_CtxOne },
            DistractorMaterials = new Material[1] { Low_CtxOne }
        };

        Conditions[3] = new Condition
        {
            CueMaterial = Ctx_Two,
            TargetMaterials = new Material[1] { High_CtxTwo },
            DistractorMaterials = new Material[1] { Low_CtxTwo }
        };

        Conditions[4] = new Condition
        {
            CueMaterial = Ctx_Two,
            TargetMaterials = new Material[1] { High_CtxTwo },
            DistractorMaterials = new Material[1] { Mid_CtxTwo }
        };

        Conditions[5] = new Condition
        {
            CueMaterial = Ctx_Two,
            TargetMaterials = new Material[1] { Mid_CtxOne },
            DistractorMaterials = new Material[1] { Low_CtxTwo }
        };
    }
}
