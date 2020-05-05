using UnityEngine;

public class XMaze_ForceFOV : StateMachineBehaviour
{
    // X, "Z" coordinates for target onset
    // 2D vectors use x and y, we will assing z value to y
    // Remember that values set here are only the "default" values, they 
    // are fully editable in the Inspector GUI as they are "public". 
    public Vector2 N_OnsetPosition = new Vector2 { x = 519.75f, y = 500f };  
    public Vector2 S_OnsetPosition = new Vector2 { x = 480.25f, y = 500f };

    // Orientations at target onset in degrees
    public float N_OnsetRotation = 90;  
    public float S_OnsetRotation = 270;

    // Default rates
    public float default_rotation_rate = 1f;
    public float default_translation_rate = 0.1f;

    // keep current trial targets 
    private Vector2 target_Position;
    private float target_Rotation;

    // Distance at which rates start speeding up
    private float start_DistanceX = 5f;
    
    // Is North?
    private bool isNorth;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // To get the current value for the IsNorth boolean, which is held in the XMaze_ExperimentCtrl class, not
        // in the base ExperimentController, we have to use the ReturnValue function. We want a boolean value
        // so we have to explicitely convert it using (bool) before the returned value
        isNorth = (bool)ExperimentController.instance.ReturnValue("IsNorth");

        // Get the current playercontroller transform and its position
        Transform currentPos = ExperimentController.instance.playerController.transform;

        // Distance is absolute value
        if (isNorth)
        {
            target_Position = N_OnsetPosition;
            target_Rotation = N_OnsetRotation;
        }
        else
        {
            target_Position = S_OnsetPosition;
            target_Rotation = S_OnsetRotation;
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Get the current updated position
        Transform currentPos = ExperimentController.instance.playerController.transform;
        Vector3 angles = currentPos.eulerAngles;
        Vector3 position = currentPos.position;

        // If the current position is > target position, force end state
        if ((isNorth && currentPos.position.x >= target_Position.x) || (!isNorth && currentPos.position.x <= target_Position.x))
        {
            angles.y = target_Rotation;
            position.z = target_Position.y;
            ExperimentController.instance.playerController.transform.eulerAngles = angles;
            ExperimentController.instance.playerController.transform.position = position;

            // replaces the StateSystem common behavior which is set to Infinite time, 
            // no triggers. This is where the state ends. 
            animator.SetTrigger("StateOver");
            return; // stops execution here, and continue to next frame.
        }
        
        // Get distance to target
        float dist_to_targ = Mathf.Abs(currentPos.position.x - target_Position.x);

        // Increase rotation speed as subject gets closer to target onset point
        float modifier = Mathf.Max(1.0f, start_DistanceX / Mathf.Max(0.001f, dist_to_targ));
        angles.y = Mathf.MoveTowardsAngle(angles.y, target_Rotation, default_rotation_rate * modifier);

        // Only change Z translation when distance <= start distance x
        if (dist_to_targ <=5)
        {
            position.z = Mathf.MoveTowards(position.z, target_Position.y, default_translation_rate * modifier);
        }
        
        ExperimentController.instance.playerController.transform.eulerAngles = angles;
        ExperimentController.instance.playerController.transform.position = position;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

}
