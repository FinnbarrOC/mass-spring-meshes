using System;
using Mirror;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Vehicles.Aeroplane
{
    [RequireComponent(typeof (AeroplaneController))]
    public class AeroplaneUserControl4Axis : NetworkBehaviour
    {
        // these max angles are only used on mobile, due to the way pitch and roll input are handled
        public float maxRollAngle = 80;
        public float maxPitchAngle = 80;
        public bool invertVerticalInput = true;
        private float _verticalInputMultiplier = -1;

        // reference to the aeroplane that we're controlling
        private AeroplaneController m_Aeroplane;
        private float m_Throttle;
        private bool m_AirBrakes;
        private float m_Yaw;
        private float m_Strafe;

        [SerializeField] private GameObject bulletPrefab;
        private void OnValidate()
        {
            if (invertVerticalInput)
            {
                _verticalInputMultiplier = -1;
            }
            else
            {
                _verticalInputMultiplier = 1;
            }
        }


        private void Awake()
        {
            // Set up the reference to the aeroplane controller.
            m_Aeroplane = GetComponent<AeroplaneController>();

            if (!isLocalPlayer) return;
            
            Cursor.lockState = CursorLockMode.Locked; 
            Cursor.visible = false;
        }


        private void FixedUpdate()
        {
            if (!isLocalPlayer) return;
            
            float pitch = 0;
            
            if (Input.GetKey(KeyCode.Space))
            {
                // Block these inputs while in turret mode
                m_Yaw = 0;
                m_Throttle = 0;
                m_AirBrakes = false;
            }

            else
            {
                // Read input for the pitch, yaw, roll and throttle of the aeroplane.
                pitch = _verticalInputMultiplier * 
                        CrossPlatformInputManager.GetAxis("Mouse Y");
                m_Yaw = CrossPlatformInputManager.GetAxis("Mouse X");
                m_Throttle = CrossPlatformInputManager.GetAxis("Vertical");
            
                m_AirBrakes = m_Throttle < 0;
            }

            // Pass the input to the aeroplane
            m_Strafe = CrossPlatformInputManager.GetAxis("Horizontal");
            m_Aeroplane.Move(0, pitch, m_Yaw, m_Throttle, m_AirBrakes, m_Strafe);
        }

        [Command]
        public void CmdSpawnObject(Vector3 pos, Quaternion rot, Vector3 velocity)
        {
            GameObject obj = Instantiate(bulletPrefab, pos, rot);
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            rb.velocity += velocity;
            NetworkServer.Spawn(obj);
        }


        private void AdjustInputForMobileControls(ref float roll, ref float pitch, ref float throttle)
        {
            // because mobile tilt is used for roll and pitch, we help out by
            // assuming that a centered level device means the user
            // wants to fly straight and level!

            // this means on mobile, the input represents the *desired* roll angle of the aeroplane,
            // and the roll input is calculated to achieve that.
            // whereas on non-mobile, the input directly controls the roll of the aeroplane.

            float intendedRollAngle = roll*maxRollAngle*Mathf.Deg2Rad;
            float intendedPitchAngle = pitch*maxPitchAngle*Mathf.Deg2Rad;
            roll = Mathf.Clamp((intendedRollAngle - m_Aeroplane.RollAngle), -1, 1);
            pitch = Mathf.Clamp((intendedPitchAngle - m_Aeroplane.PitchAngle), -1, 1);
        }
    }
}
