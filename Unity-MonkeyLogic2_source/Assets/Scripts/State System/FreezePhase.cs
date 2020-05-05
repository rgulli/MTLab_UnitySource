using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezePhase : StateMachineBehaviour
{
    
    //Freeze player for the duration of the state. 

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ExperimentController.instance.FreezePlayer(true);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // As an example here we have the cues disappearing
        ExperimentController.instance.FreezePlayer(false);
    }


}
