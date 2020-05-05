/*
    Downloaded from the Unity Technologies Standard Assets pack. 

    Modified to remove unnecessary code. 

    We have no running, no jumping, no bobbing, no animations

*/

using UnityEngine;
using UnityEngine.InputSystem;

namespace FirstPerson
{
    [RequireComponent(typeof (CharacterController))]
    [RequireComponent(typeof (AudioSource))]
    public class PlayerController : MonoBehaviour
    {

        // Movement properties
        private float m_WalkSpeed = 5.0f;
        private float m_StickToGroundForce = 10.0f;
        private float m_GravityMultiplier = 2.0f;
        private Vector3 m_MoveDirection = Vector3.zero;
        private bool m_CanMove = true; // All Movements
        private bool m_CanTurn = true; // Rotation only
        private bool m_CanBack = true; // Backwards movement
        private bool m_PreviouslyGrounded;

        private float speed;

        // Camera properties
        private Camera m_Camera;
        private int m_CullMask;

        // Misc
        private CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;

        // Set from the editor
        public UserInputController inputCtrl;
        
        // Save the input data used to compute movement during each frame. To be
        // sent during LateUpdate to the MonkeyLogicController. 
        // We don't need to create position/rotation variables as those are accessible 
        // from the gameobject transform.
        private float _hInput;
        private float _vInput;
        // -1: Nothing; -Infinity: OnBlack; [0 Infinity]: object instance ID
        private float _CollisionStatus = -1;
        
        // Use this for initialization
        private void OnEnable()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_Camera = Camera.main;
            m_Camera.backgroundColor = Color.black;
            m_CullMask = m_Camera.cullingMask;
            m_Camera.targetDisplay = 0;
            // Default start on black

        }

        private void Update()
        {

            // Manual On Black
            if (Keyboard.current.vKey.wasPressedThisFrame)
                OnBlack(false);
            if (Keyboard.current.bKey.wasPressedThisFrame)
                OnBlack(true);
            Vector2 move = inputCtrl.ReadAxes();

            if (m_CanMove)
            {
                
                // set the desired speed to be walking or running
                //speed = m_WalkSpeed * inputCtrl.Move_Sensitivity;

                if (m_CharacterController.isGrounded)
                {
                    // Read input
                    //_vInput = Input.GetAxis(inputCtrl.InputV);
                    if (!m_CanBack && move.y < 0)
                    {
                        m_MoveDirection = new Vector3(0, 0, 0);
                    }
                    else
                    {
                        m_MoveDirection = new Vector3(0, 0, move.y);
                    }
                    m_MoveDirection = transform.TransformDirection(m_MoveDirection);
                    m_MoveDirection *= m_WalkSpeed;

                    m_MoveDirection.y = -m_StickToGroundForce;

                }
                else
                {

                    m_MoveDirection += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
                }

                m_CollisionFlags = m_CharacterController.Move(m_MoveDirection * Time.deltaTime);

                // Copied from MouseLook.LookRotation
                // No pitch rotation either. 
                //_hInput = Input.GetAxis(inputCtrl.InputH);
                if (m_CanTurn)
                    transform.localRotation *= Quaternion.Euler(0f, move.x * inputCtrl.Turn_Sensitivity, 0f);
            }
            // Update values to experiment controller
            if (EventsController.instance != null)
                EventsController.instance.SendPlayerLateUpdateEvent(transform.position, transform.rotation.eulerAngles.y, _CollisionStatus, move.y, move.x);
        }

        public void ToStart(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
        }

        // This is to clear the value at the end of the trial as hiding/disabling the trigger
        // volumes does not call a trigger exit. 
        public void ClearCollisionStatus()
        {
            _CollisionStatus = -1;
        }

        // To detect collisions with trigger volumes
        private void OnTriggerEnter(Collider other)
        {
            _CollisionStatus = other.gameObject.GetInstanceID();
            
        }
        
        private void OnTriggerExit(Collider other)
        {
            _CollisionStatus = -1;
        }
       
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
           /* Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(m_CharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);*/
        }
        
        // For ITI, Pause and before the experiment starts. 
        public void OnBlack(bool OnOff)
        {
            if (OnOff == true)
            {
                // Disable movement and render only solid black
                m_CanMove = false;
                m_Camera.cullingMask = 0;
                m_Camera.clearFlags = CameraClearFlags.SolidColor;
                _CollisionStatus = Mathf.NegativeInfinity;
            }
            else
            {
                // Enable movement and render view
                m_CanMove = true;
                m_Camera.cullingMask = m_CullMask; // what to render
                m_Camera.clearFlags = CameraClearFlags.Skybox; // what to render when no geometry present
                _CollisionStatus = -1;
            }
        }

        public void Freeze(bool OnOff)
        {
            m_CanMove = !OnOff;
        }
        public void FreezeRotation(bool OnOff)
        {
            m_CanTurn = !OnOff;
        }
        public void ConstrainForward(bool OnOff)
        {
            m_CanBack = !OnOff;
        }


    }
}
