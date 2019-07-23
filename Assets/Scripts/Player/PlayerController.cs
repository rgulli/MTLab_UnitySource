/*
    Downloaded from the Unity Technologies Standard Assets pack. 

    Modified to remove unnecessary code. 

    We have no running, no jumping, no bobbing, no animations

*/
using System;
using UnityEngine;

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
        private bool m_CanMove = true;
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
        private string _CollisionStatus = "";
        
        // Use this for initialization
        private void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_Camera = Camera.main;
            m_Camera.backgroundColor = Color.black;
            m_CullMask = m_Camera.cullingMask;

            // Default start on black
            OnBlack(true);
        }

        private void Update()
        {
            // Manual On Black
            if (Input.GetKey("v"))
                OnBlack(false);
            if (Input.GetKey("b"))
                OnBlack(true);

            if (m_CanMove)
            {
                
                // set the desired speed to be walking or running
                speed = m_WalkSpeed * inputCtrl.Move_Sensitivity;

                if (m_CharacterController.isGrounded)
                {
                    // Read input
                    _vInput = Input.GetAxis(inputCtrl.InputV);

                    m_MoveDirection = new Vector3(0, 0, _vInput);
                    m_MoveDirection = transform.TransformDirection(m_MoveDirection);
                    m_MoveDirection *= speed;

                    m_MoveDirection.y = -m_StickToGroundForce;

                }
                else
                {

                    m_MoveDirection += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
                }

                m_CollisionFlags = m_CharacterController.Move(m_MoveDirection * Time.deltaTime);

                // Copied from MouseLook.LookRotation
                // No pitch rotation either. 
                _hInput = Input.GetAxis(inputCtrl.InputH);
                transform.localRotation *= Quaternion.Euler(0f, _hInput * inputCtrl.Turn_Sensitivity, 0f);
            }
            // Update values to experiment controller
            EventsController.instance.SendPlayerLateUpdateEvent(transform.position, transform.rotation.eulerAngles.y, _CollisionStatus, _hInput, _vInput);
        }

        public void ToStart(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
        }

        // This is to clear the value at the end of the trial as hiding/disabling the trigger
        // volumes does not call a trigger exit. 
        public void ClearCollisionStatus()
        {
            _CollisionStatus = "";
        }

        // To detect collisions with trigger volumes
        private void OnTriggerEnter(Collider other)
        {
            _CollisionStatus = other.name;
        }
        
        private void OnTriggerExit(Collider other)
        {
            _CollisionStatus = "";
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
                _CollisionStatus = "OnBlack";
            }
            else
            {
                // Enable movement and render view
                m_CanMove = true;
                m_Camera.cullingMask = m_CullMask; // what to render
                m_Camera.clearFlags = CameraClearFlags.Skybox; // what to render when no geometry present
                _CollisionStatus = "";
            }
        }

        public void Freeze(bool OnOff)
        {
            m_CanMove = !OnOff;
        }
    }
}
