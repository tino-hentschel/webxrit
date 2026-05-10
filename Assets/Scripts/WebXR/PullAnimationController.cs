using UnityEngine;

namespace haw.pd20.webxr
{
    [RequireComponent(typeof(PullInteractor))]
    public class PullAnimationController : MonoBehaviour
    {
        private enum AnimationDirection
        {
            UpperToLowerLimit,
            LowerToUpperLimit
        }

        [SerializeField] private AnimationDirection animationDirection;

        [SerializeField] private Animator animator;
        private AnimationClip animationClip;

        private float minAnimPos;
        private float maxAnimPos;

        private PullInteractor pullInteractor;

        private void Awake()
        {
            animator.speed = 0; // Animation Speed is also set to 0 (in case this component is deactivated on Awake)
            animationClip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
            pullInteractor = GetComponent<PullInteractor>();
        }

        private void Start()
        {
            minAnimPos = animationDirection == AnimationDirection.LowerToUpperLimit
                ? pullInteractor.LowerLimit
                : pullInteractor.UpperLimit;
            
            maxAnimPos = animationDirection == AnimationDirection.UpperToLowerLimit
                ? pullInteractor.LowerLimit
                : pullInteractor.UpperLimit;
        }

        private void Update()
        {
            // TODO this is a bad style performance wise ... rework
            animator?.Play(animationClip.name, 0, (float)System.Math.Round(MathUtil.Map(minAnimPos, maxAnimPos, animationClip.length, 0, pullInteractor.PullAxisValue), 3));
        }
    }
}