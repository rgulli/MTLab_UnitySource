using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
///  This is a sub-class of the base Experiment controller. It can be used to 
///  override the basic functions to suit your own experiment. To replace the function
///  you need to define it with: 
///  ProtectionLevel KeyWord OutputType FunctionName(Function, Parameters)
///  Such as: 
///  public override void HideTargets()
///  
///  Wich is already defined in the ExperimentController class with a "virtual" 
///  keyword. Functions that can be overridden should be either: 
///     virtual: meaning that there is a default behavior (i.e. code is defined) 
///              which can be overriden in a sub-class.
///     abstract: meaning that the function NEEDS to be overriden or it will trigger
///               an error when compiling. There is no default behavior and the function
///               must be left empty. 
///  Any public or protected (i.e. only accessible from base and sub classes) functions can 
///  be overriden by the sub-class so be careful when naming them. If you 
///  add a function in your sub-class with the same name as a base class function you will 
///  get a warning and will need to add a "new" keyword to remove it. Private functions in the base
///  class are not accessible in the sub-class so there is no need for the new keyword. This means
///  that the base function will NOT be executed anymore, hence the use of "base.OnEnable();" in 
///  the code. See the OnEnable() function below. In this case these are MonoBehavior functions so 
///  we can't rename them. This is why we use the "new" keyword.
///  
/// When you override a function in a sub-class, the new code will be executed even if the function
/// is called from the base class. 
/// Experiment Controller virtual/abstract functions are: 
///     protected virtual void Initialize()
///     public virtual void PrepareTrial()
///     public virtual void PrepareCues() 
///     public virtual void ShowCues()
///     public virtual void HideCues()
///     public virtual void PrepareTargets()
///     public virtual void ShowTargets()
///     public virtual void HideTargets()
///     public virtual void PrepareDistractors()
///     public virtual void ShowDistractors()
///     public virtual void HideDistractors()
///     public virtual void FreezePlayer(bool ON)
///     public virtual void EndTrial()
///
/// tl;dr; you can replace some functions in the base ExperimentController class with your own code.
/// </summary>


public class SampleTaskExpController : ExperimentController
{
    // The taskInfo base class is defined as "dynamic" in the experiment
    // controller class, this means that the class is implemented at runtime
    // we therefore need to define it now. 
    // We use a specific sub-class of task info and set it as "taskInfo" so
    // all the base scripts will still work. 
    public SampleTaskTaskInfo sTaskInfo;
    
    // OnEnable and OnDisable function calls are important because they connect the
    // experiment controller with the events controller for communication across classes. 
    private new void OnEnable()
    {
        // Important, do not remove ---
        base.OnEnable();
        taskInfo = sTaskInfo;
        // ----------------------------
    }

    private void Start()
    {
        // Base function call. Do not remove. 
        base.Initialize();
    }

}
