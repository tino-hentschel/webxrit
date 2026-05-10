using TMPro;
using UnityEngine;

namespace haw.pd20.training_modules
{
    /// <summary>
    /// A world space canvas displayed at the position of the player hand that is used to show interaction texts when the players hand hovers over Interactables or PullInteractors.
    /// The canvas is always facing a "Look At"-target which usually should be a camera representing the players head/eyes.
    /// In the WebXR Interactions framework this camera is different for the Editor (lookAtTargetEditor) and the WebGL Build (LookAtTargetWebGL).
    /// The canvas is not parented under the player hand gameobject so that it not receives its rotation. It should instead be parented below the WebXR Rig. 
    /// </summary>
    public class HandInteractionCanvas : MonoBehaviour
    {
        [SerializeField] private Transform handTransform;
        [SerializeField] private Transform LookAtTargetWebGL;
        [SerializeField] private Transform lookAtTargetEditor;

        [SerializeField] private TextMeshProUGUI interactionLabel;
        [SerializeField] private Vector3 offset;

        public bool IsHidden { get; private set; }

        private Transform lookAtTarget;


        private void Start()
        {
            lookAtTarget = lookAtTargetEditor;

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                lookAtTarget = LookAtTargetWebGL;
            }
        }

        private void Update()
        {
            if (IsHidden)
                return;

            transform.position = handTransform.position + offset;
            transform.rotation = Quaternion.LookRotation(transform.position - lookAtTarget.transform.position);
        }

        public void Show(string interactionText)
        {
            IsHidden = false;
            interactionLabel.text = interactionText;
        }

        public void Hide()
        {
            IsHidden = true;
            interactionLabel.text = "";
        }
    }
}