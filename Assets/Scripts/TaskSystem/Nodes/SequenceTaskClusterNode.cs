using System;

namespace haw.pd20.tasksystem
{
    [CreateNodeMenu("Task Cluster/Sequence Cluster")]
    public class SequenceTaskClusterNode : TaskClusterNode
    {
        [NonSerialized] private int taskIndex = 0;

        public TaskNode CurrentTask
        {
            get
            {
                if (tasks.Count <= 0 || taskIndex >= tasks.Count)
                    return null;

                return tasks[taskIndex];
            }
        }

        public override void Reset()
        {
            base.Reset();
            taskIndex = 0;
        }

        public override void RaiseHelpActivatedEvent()
        {
            RaiseHelpActivatedEvent(this);
            CurrentTask.RaiseHelpActivatedEvent();
        }

        public override void RaiseHelpDeactivatedEvent()
        {
            RaiseHelpDeactivatedEvent(this);
            CurrentTask.RaiseHelpDeactivatedEvent();
        }

        public override bool HasOpenTasks() => taskIndex + 1 < tasks.Count;

        public override HelpNode GetHelpNode()
        {
            var helpNode = CurrentTask.GetHelpNode();

            if (!helpNode && GetPort("Help").IsConnected)
            {
                helpNode = GetPort("Help").Connection.node as HelpNode;
            }

            return helpNode;
        }

        public void UpdateTask()
        {
            if (HasOpenTasks())
                taskIndex++;
        }
    }
}