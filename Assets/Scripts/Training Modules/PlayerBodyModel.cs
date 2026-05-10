using System.Collections.Generic;
using UnityEngine;

namespace haw.pd20.training_modules
{
// TODO This whole script can be extended to support a full IK Body animation. For Reference: https://www.youtube.com/watch?v=tBYl-aSxUe0&
    public class PlayerBodyModel : MonoBehaviour
    {
        [Header("Body Rig")] [SerializeField] private Transform headConstraintEditor;
        [SerializeField] private Transform headConstraintWebXR;
        [SerializeField] private float neckRotationXMin;
        [SerializeField] private float neckRotationXMax;
        [SerializeField] private GameObject bodyRoot;
        [SerializeField] private bool deactivateBodyRootWhenNotInView = true;

        [Header("Hand State Tasks")] [SerializeField]
        private PlayerHandModel leftPlayerHandModel;

        [SerializeField] private PlayerHandModel rightPlayerHandModel;

        [SerializeField] private List<HandStateTask> handStateTasks;

        private Transform headConstraint;

        private void Start()
        {
#if UNITY_EDITOR
            headConstraint = headConstraintEditor;
#endif
#if UNITY_WEBGL && !UNITY_EDITOR
            headConstraint = headConstraintWebXR;
#endif
        }

        private void Update() => MoveBodyWithHeadWhenInView();

        /// <summary>
        /// Orientates the player body according to the headConstraint when the eulerAngle of the x-axis of the head constraint is within the range of a vertical field of view (between neckRotationXMin and neckRotationXMax).
        /// This is used to prevent the body from rotating in extreme angles when the rotation of the headConstraint reaches certain angles.
        /// When the flag deactivateBodyRootWhenNotInView is true the bodyRoot of the play body will be set inactive (hiding the player body when it is not in view)
        /// </summary>
        private void MoveBodyWithHeadWhenInView()
        {
            transform.position = headConstraint.position;

            if (headConstraint.eulerAngles.x > neckRotationXMin && headConstraint.eulerAngles.x < neckRotationXMax)
            {
                bodyRoot.SetActive(true);
                transform.forward = Vector3.ProjectOnPlane(headConstraint.up, Vector3.up).normalized;
            }
            else if (deactivateBodyRootWhenNotInView)
            {
                bodyRoot.SetActive(false);
            }
        }
    }
}