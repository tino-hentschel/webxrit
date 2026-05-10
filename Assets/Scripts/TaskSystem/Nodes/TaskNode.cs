using System;

namespace haw.pd20.tasksystem
{
    [CreateNodeMenu("Task")]
    public class TaskNode : ActionNode<TaskNode>
    {
        [Input(typeConstraint = TypeConstraint.Strict)]
        public TaskConnection In;

        [NonSerialized] public TaskStatus Status;

        public bool NotificationEnabled { get; private set; }

        public delegate void NotifyExecutionDelegate(TaskNode task);

        public event NotifyExecutionDelegate OnNotifyExecutionListeners;

        public delegate void NotifyExecutionErrorDelegate(TaskNode task, string errorMessage);

        public event NotifyExecutionErrorDelegate OnNotifyExecutionErrorListeners;

        public delegate bool CheckIsCurrentTaskDelegate(TaskNode task);

        public event CheckIsCurrentTaskDelegate OnCheckIsCurrentTaskListeners;

        protected override void Init()
        {
            base.Init();
            Reset();
            NotificationEnabled = true;
        }

        public override void Reset()
        {
            Status = TaskStatus.Open;
        }

        public override void RaiseCompletedEvent() => RaiseCompletedEvent(this);
        public override void RaiseErrorEvent() => RaiseErrorEvent(this);
        public override void RaiseHelpActivatedEvent() => RaiseHelpActivatedEvent(this);
        public override void RaiseHelpDeactivatedEvent() => RaiseHelpDeactivatedEvent(this);

        public override HelpNode GetHelpNode()
        {
            if (GetPort("Help").IsConnected)
            {
                return GetPort("Help").Connection.node as HelpNode;
            }

            return null;
        }

        public void EnableTaskNotification(bool notificationEnabled)
        {
            NotificationEnabled = notificationEnabled;
        }

        public bool HasExecutionListeners => OnNotifyExecutionListeners != null;

        public void NotifyExecution()
        {
            if (!NotificationEnabled)
                return;

            OnNotifyExecutionListeners?.Invoke(this);
        }

        public void NotifyExecution(TaskStatus status)
        {
            if (!NotificationEnabled)
                return;

            Status = status;

            OnNotifyExecutionListeners?.Invoke(this);
        }

        public void NotifyExecution(string statusStr)
        {
            if (!NotificationEnabled)
                return;

            Status = statusStr switch
            {
                "Completed" => TaskStatus.Completed,
                "Open" => TaskStatus.Open,
                "Failed" => TaskStatus.Failed,
                "Default" => TaskStatus.Default,
                _ => Status
            };

            OnNotifyExecutionListeners?.Invoke(this);
        }
        
        public bool HasExecutionErrorListeners => OnNotifyExecutionErrorListeners != null;

        public void NotifyExecutionError(string errorMessage)
        {
            if (!NotificationEnabled)
                return;

            OnNotifyExecutionErrorListeners?.Invoke(this, errorMessage);
        }

        public bool IsCurrentTask()
        {
            return OnCheckIsCurrentTaskListeners != null && OnCheckIsCurrentTaskListeners(this);
        }
    }
}