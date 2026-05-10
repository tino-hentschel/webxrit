using UnityEngine;

namespace haw.pd20.training_modules
{
    /// <summary>
    /// Helper component. Can be used to play an animation (defined by the animationName)
    /// when a Help Request is issued by the trainee (help button on the controller is pressed).
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class HelpAnimation : MonoBehaviour
    {
        [SerializeField] public string AnimationStateName;
        
        private Animator animator;

        private void Awake() => animator = GetComponent<Animator>();
        public void ResetAndPlay() => animator.Play(AnimationStateName, -1, 0f);
    }
}