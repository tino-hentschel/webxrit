using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using XNode;

namespace haw.pd20.tasksystem
{
    [CreateNodeMenu("Activity")]
    public class ActivityNode : ActionNode<ActivityNode>
    {
        [Input()] //typeConstraint = TypeConstraint.Strict)]
        public ActivityConnection In;

        [Output()] public ActivityConnection Out;

        [Output(dynamicPortList = true)] public TaskClusterConnection TaskClusters;

        [NonSerialized] private List<TaskClusterNode> taskClusters = new List<TaskClusterNode>();
        public ReadOnlyCollection<TaskClusterNode> TaskClustersInOrder => taskClusters.AsReadOnly();

        public ReadOnlyCollection<TaskClusterNode> OpenTaskClustersInOrder
        {
            get
            {
                var openTaskClusters = new List<TaskClusterNode>();

                for (int i = taskClusterIndex; i < taskClusters.Count; i++)
                {
                    openTaskClusters.Add(taskClusters[i]);
                }
                
                return openTaskClusters.AsReadOnly();
            }
        }

        [NonSerialized] private int taskClusterIndex;

        public bool RegisterTaskListenersInAllClusters;

        public TaskClusterNode TaskCluster => taskClusters[taskClusterIndex];

        public TimeSpan Duration =>
            taskClusters[taskClusters.Count - 1].EndDateTime.Subtract(taskClusters[0].StartDateTime);

        protected override void Init()
        {
            base.Init();

            taskClusterIndex = 0;

            foreach (var port in DynamicOutputs)
            {
                if (port.IsConnected)
                {
                    taskClusters.Add(port.Connection.node as TaskClusterNode);
                }
            }
        }

        public bool HasNextActivity() => GetPort("Out").IsConnected;

        public ActivityNode GetNextActivity() => GetPort("Out").Connection.node as ActivityNode;

        public bool HasNextCluster() => taskClusterIndex + 1 < taskClusters.Count;

        public void UpdateCluster()
        {
            if (!HasNextCluster())
                return;

            taskClusterIndex++;
            TaskCluster.StartDateTime = DateTime.Now;
        }

        public override void Reset()
        {
            foreach (var cluster in taskClusters)
            {
                cluster.Reset();
            }
        }

        public void ResetConsecutive()
        {
            Reset();
            if (HasNextActivity())
                GetNextActivity().ResetConsecutive();
        }

        public override void RaiseCompletedEvent() => RaiseCompletedEvent(this);

        public override void RaiseErrorEvent()
        {
            RaiseErrorEvent(this);
            TaskCluster.RaiseErrorEvent();
        }

        public override void RaiseHelpActivatedEvent()
        {
            RaiseHelpActivatedEvent(this);
            TaskCluster.RaiseHelpActivatedEvent();
        }

        public override void RaiseHelpDeactivatedEvent()
        {
            RaiseHelpDeactivatedEvent(this);
            TaskCluster.RaiseHelpDeactivatedEvent();
        }

        public override HelpNode GetHelpNode()
        {
            var helpNode = TaskCluster.GetHelpNode();

            if (!helpNode && GetPort("Help").IsConnected)
            {
                helpNode = GetPort("Help").Connection.node as HelpNode;
            }

            return helpNode;
        }
    }
}