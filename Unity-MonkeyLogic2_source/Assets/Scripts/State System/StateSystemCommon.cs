using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Misc;

public class StateSystemCommon : StateMachineBehaviour
{
    // Shared by all instances of this class. Used to check experiment status and
    // trigger start / stop events.
    
    static bool wasRunning = false; 

    public StateNames StateName;

    public float MinStateDuration = 0.0f;
    [Tooltip("-1:Infinite; 0:1 frame; >0 duration")]public float MaxStateDuration = 0.0f;
    public bool RequiresFixation = false;
    public enum Triggers
    {
        Cue, 
        Targets, 
        Misc, 
        None
    }
    public Triggers TriggerType = Triggers.None;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
        float duration;
        if (MinStateDuration >= MaxStateDuration && MaxStateDuration > 0)
            duration = Mathf.Max(MaxStateDuration, MinStateDuration);
        else if(MaxStateDuration == 0)
            duration = 0.0f;
        else if (MaxStateDuration < 0)
            duration = Mathf.Infinity;
        else
            duration = Random.Range(MinStateDuration, MaxStateDuration);

        ExperimentController.instance.StartState(StateName, duration, RequiresFixation, TriggerType.ToString());
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    // This script will handle the timing only and will rely on the experiment controller to handle the
    // Gaze and Player collision data. 
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ExperimentController exp = ExperimentController.instance;

        // Just Started
        if (exp.IsRunning && !wasRunning)
        {
            animator.SetTrigger("Run");
            wasRunning = true;
            return;
        }
        // Just Stopped
        else if(!exp.IsRunning && wasRunning)
        {
            animator.SetTrigger("Stop");
            wasRunning = false;
            return;
        }
        // Check for state completion
        else if (exp.IsRunning && wasRunning)
        {
            if (exp.IsTrialOver())
            {
                animator.SetTrigger("TrialOver");
                return;
            }
            
            if (exp.IsStateOver())
            {
                animator.SetTrigger("StateOver");
                return;
            }
        }
        else
        {
            //animator.SetBool("IsTrialOver", true);
            return;
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // reset states. 
        //animator.SetBool("IsStateOver", false);
        animator.ResetTrigger("StateOver");
        // except running and trial over. 
    }

    // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}
}
