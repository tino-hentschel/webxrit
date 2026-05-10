using UnityEngine;
using WebXR;

namespace haw.pd20.webxr
{
    [RequireComponent(typeof(CharacterController))]
    public class ContinuousMovement : MonoBehaviour
    {
        public WebXRController Controller;
        public GameObject CameraGameObjectEditor;
        public GameObject CameraGameObjectWebXR;
        public float Speed;
        public float Gravity = -9.81f;
        public LayerMask GroundLayer;
        public float EyesToTopOfHeadDistance = 0.12f;

        // private XRRig rig;
        // private InputDevice inputDevice;
        private GameObject cameraGameObject;
        private Vector2 inputAxis;
        private CharacterController characterController;
        private float fallingSpeed;

        private bool axisInputEnabled;

        void Awake()
        {
            // rig = GetComponent<XRRig>();
            characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
#if UNITY_EDITOR
            cameraGameObject = CameraGameObjectEditor;
#endif
            
#if UNITY_WEBGL && !UNITY_EDITOR
            cameraGameObject = CameraGameObjectWebXR;
#endif
        }

        void Update()
        {
            // At the moment we only use continuous movement as a debug feature.
            // To activate it users have to press the thumbstick of the controller.
            if (Controller.GetButtonDown(WebXRController.ButtonTypes.Thumbstick))
                axisInputEnabled = !axisInputEnabled;

            inputAxis = axisInputEnabled ? Controller.GetAxis2D(WebXRController.Axis2DTypes.Thumbstick) : Vector2.zero;
        }

        void FixedUpdate()
        {
            CharacterControllerFollowHeadset();

            Quaternion headYaw = Quaternion.Euler(0.0f, cameraGameObject.transform.eulerAngles.y, 0.0f);
            Vector3 direction = headYaw * new Vector3(inputAxis.x, 0.0f, inputAxis.y);
            characterController.Move(direction * (Time.fixedDeltaTime * Speed));

            // AffectCharacterByGravity();
        }

        private void AffectCharacterByGravity()
        {
            if (Grounded())
            {
                fallingSpeed = 0;
            }
            else
            {
                fallingSpeed += Gravity * Time.fixedDeltaTime;
                characterController.Move(Vector3.up * (fallingSpeed * Time.fixedDeltaTime));
            }
        }

        private bool Grounded()
        {
            // get center of the character controller in world space.
            Vector3 rayStart = transform.TransformPoint(characterController.center);
            // the ray length only have to exceed the height of the character controller by a tiny amount (0.01f) for a ground check. 
            float rayLength = characterController.center.y + 0.01f;

            return Physics.SphereCast(rayStart, characterController.radius, Vector3.down, out RaycastHit hitInfo,
                rayLength,
                GroundLayer);
        }

        private void CharacterControllerFollowHeadset()
        {
            Vector3 cameraPosInRigLocalSpace = transform.InverseTransformPoint(cameraGameObject.transform.position);
            characterController.height = cameraPosInRigLocalSpace.y + EyesToTopOfHeadDistance;

            characterController.center = new Vector3(cameraPosInRigLocalSpace.x,
                characterController.height / 2 + characterController.skinWidth, cameraPosInRigLocalSpace.z);
        }
    }
}