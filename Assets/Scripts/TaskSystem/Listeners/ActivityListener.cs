using UnityEngine;
using UnityEngine.Events;

namespace haw.pd20.tasksystem
{
    public class ActivityListener : MonoBehaviour, IActionListener<ActivityNode>
    {
        [SerializeField] private ActivityNode activity;
        public UnityEvent OnCompletedEvent;
        public UnityEvent OnErrorEvent;
        public UnityEvent OnHelpActivatedEvent;
        public UnityEvent OnHelpDeactivatedEvent;

        public void OnActionCompleted(ActivityNode action) => OnCompletedEvent?.Invoke();
        public void OnError(ActivityNode action) => OnErrorEvent?.Invoke();
        public void OnHelpActivated(ActivityNode action) => OnHelpActivatedEvent?.Invoke();
        public void OnHelpDeactivated(ActivityNode action) => OnHelpDeactivatedEvent?.Invoke();

        private void OnEnable() => activity.RegisterCompletedListener(this);
        private void OnDisable() => activity.UnregisterCompletedListener(this);
    }
}