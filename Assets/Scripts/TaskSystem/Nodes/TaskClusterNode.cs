using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace haw.pd20.tasksystem
{
    public abstract class TaskClusterNode : ActionNode<TaskClusterNode>
    {
        [Input(typeConstraint = TypeConstraint.Strict)]
        public TaskClusterConnection In;

        [Output(dynamicPortList = true)] public List<TaskConnection> TaskConnections;
        [NonSerialized] protected readonly List<TaskNode> tasks = new List<TaskNode>();
        public ReadOnlyCollection<TaskNode> Tasks => tasks.AsReadOnly();

        [NonSerialized] private readonly List<ErrorData> errorData = new List<ErrorData>();
        public ReadOnlyCollection<ErrorData> Errors => errorData.AsReadOnly();

        [NonSerialized] public DateTime StartDateTime;
        [NonSerialized] public DateTime EndDateTime;
        [NonSerialized] public int HelpCount;

        public TimeSpan Duration => EndDateTime.Subtract(StartDateTime);

        protected override void Init()
        {
            base.Init();

            foreach (var port in DynamicOutputs)
            {
                if (port.IsConnected)
                {
                    tasks.Add(port.Connection.node as TaskNode);
                }
            }
        }

        public void AddErrorData(params ErrorData[] errors) => errorData.AddRange(errors);

        public override void Reset()
        {
            foreach (var task in tasks)
            {
                task.Reset();
            }
        }

        public override void RaiseCompletedEvent() => RaiseCompletedEvent(this);

        public override void RaiseErrorEvent() => RaiseErrorEvent(this);

        // public override void RaiseHelpActivatedEvent() => RaiseHelpActivatedEvent(this);

        public abstract bool HasOpenTasks();

        // public void RegisterTaskExecutionListener(TaskNode.NotifyExecutionDelegate onExecutionListener)
        // {
        //     foreach (var task in tasks)
        //     {
        //         task.OnNotifyExecutionListeners += onExecutionListener;
        //     }
        // }
        //
        // public void UnregisterTaskExecutionListener(TaskNode.NotifyExecutionDelegate onExecutionListener)
        // {
        //     foreach (var task in tasks)
        //     {
        //         task.OnNotifyExecutionListeners -= onExecutionListener;
        //     }
        // }
    }
}