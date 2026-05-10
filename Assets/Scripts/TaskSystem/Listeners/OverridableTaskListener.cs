using System;
using UnityEngine;
using UnityEngine.Events;

namespace haw.pd20.tasksystem
{
    public class OverridableTaskListener : TaskListener
    {
        [Serializable]
        public struct TaskListenerOverride
        {
            public int TaskIndex;
            public UnityEvent OnCompletedEvent;
            public UnityEvent OnErrorEvent;
            public UnityEvent OnHelpActivatedEvent;
            public UnityEvent OnHelpDeactivatedEvent;
        }

        private int CurrentTaskIndex = 0;

        public TaskListenerOverride[] TaskListenerOverrides;

        public override void OnActionCompleted(TaskNode action)
        {
            // Debug.Log("Current Task Index: " + CurrentTaskIndex);
            foreach (var taskListenerOverride in TaskListenerOverrides)
            {
                if (CurrentTaskIndex != taskListenerOverride.TaskIndex)
                    continue;

                // Debug.Log("Task Listener Override Invoked " + CurrentTaskIndex);
                taskListenerOverride.OnCompletedEvent?.Invoke();
                CurrentTaskIndex++;
                return;
            }

            OnCompletedEvent?.Invoke();
            CurrentTaskIndex++;
        }

        public override void OnError(TaskNode action)
        {
            foreach (var taskListenerOverride in TaskListenerOverrides)
            {
                if (CurrentTaskIndex != taskListenerOverride.TaskIndex)
                    continue;

                taskListenerOverride.OnErrorEvent?.Invoke();
                return;
            }

            OnErrorEvent?.Invoke();
        }

        public override void OnHelpActivated(TaskNode action)
        {
            foreach (var taskListenerOverride in TaskListenerOverrides)
            {
                if (CurrentTaskIndex != taskListenerOverride.TaskIndex)
                    continue;

                taskListenerOverride.OnHelpActivatedEvent?.Invoke();
                return;
            }

            OnHelpActivatedEvent?.Invoke();
        }

        public override void OnHelpDeactivated(TaskNode action)
        {
            foreach (var taskListenerOverride in TaskListenerOverrides)
            {
                if (CurrentTaskIndex != taskListenerOverride.TaskIndex)
                    continue;

                taskListenerOverride.OnHelpDeactivatedEvent?.Invoke();
                return;
            }

            OnHelpDeactivatedEvent?.Invoke();
        }
    }
}