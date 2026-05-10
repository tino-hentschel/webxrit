using UnityEngine;
using UnityEngine.Events;

namespace haw.pd20.tasksystem
{
    public class StepListener : MonoBehaviour, IActionListener<StepNode>
    {
        [SerializeField] private StepNode step;
        public UnityEvent OnCompletedEvent;
        public UnityEvent OnErrorEvent;
        public UnityEvent OnHelpActivatedEvent;
        public UnityEvent OnHelpDeactivatedEvent;

        public void OnActionCompleted(StepNode action) => OnCompletedEvent?.Invoke();
        public void OnError(StepNode action) => OnErrorEvent?.Invoke();
        public void OnHelpActivated(StepNode action) => OnHelpActivatedEvent?.Invoke();
        public void OnHelpDeactivated(StepNode action) => OnHelpDeactivatedEvent?.Invoke();

        private void OnEnable() => step.RegisterCompletedListener(this);
        private void OnDisable() => step.UnregisterCompletedListener(this);
    }
}