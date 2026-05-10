using UnityEngine;
using UnityEngine.Events;

namespace haw.pd20.tasksystem
{
    public class TaskClusterListener : MonoBehaviour, IActionListener<TaskClusterNode>
    {
        [SerializeField] private TaskClusterNode taskCluster;
        public UnityEvent OnCompletedEvent;
        public UnityEvent OnErrorEvent;
        public UnityEvent OnHelpActivatedEvent;
        public UnityEvent OnHelpDeactivatedEvent;

        public void OnActionCompleted(TaskClusterNode action) => OnCompletedEvent?.Invoke();
        public void OnError(TaskClusterNode action) => OnErrorEvent?.Invoke();
        public void OnHelpActivated(TaskClusterNode action) => OnHelpActivatedEvent?.Invoke();
        public void OnHelpDeactivated(TaskClusterNode action) => OnHelpDeactivatedEvent?.Invoke();
        
        private void OnEnable() => taskCluster.RegisterCompletedListener(this);
        private void OnDisable() => taskCluster.UnregisterCompletedListener(this);
    }
}