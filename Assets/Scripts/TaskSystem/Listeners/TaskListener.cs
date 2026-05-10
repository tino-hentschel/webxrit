using UnityEngine;
using UnityEngine.Events;

namespace haw.pd20.tasksystem
{
    public class TaskListener : MonoBehaviour, IActionListener<TaskNode>
    {
        [SerializeField] private TaskNode task;
        public UnityEvent OnCompletedEvent;
        public UnityEvent OnErrorEvent;
        public UnityEvent OnHelpActivatedEvent;
        public UnityEvent OnHelpDeactivatedEvent;

        public virtual void OnActionCompleted(TaskNode action) => OnCompletedEvent?.Invoke();
        public virtual void OnError(TaskNode action) => OnErrorEvent?.Invoke();
        public virtual void OnHelpActivated(TaskNode action) => OnHelpActivatedEvent?.Invoke();
        public virtual void OnHelpDeactivated(TaskNode action) => OnHelpDeactivatedEvent?.Invoke();

        private void OnEnable() => task.RegisterCompletedListener(this);
        private void OnDisable() => task.UnregisterCompletedListener(this);
    }
}