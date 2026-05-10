using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

namespace haw.pd20.tasksystem
{
    [CreateNodeMenu("Step")]
    public class StepNode : ActionNode<StepNode>
    {
        [Input(typeConstraint = TypeConstraint.Strict)]
        public StepConnection In;

        [Output(connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)]
        public StepConnection Out;

        [Output(connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)]
        public ActivityConnection ActivityConnection;

        [NonSerialized] public ActivityNode Activity;
        [NonSerialized] private readonly List<ActivityNode> activitiesInOrder = new List<ActivityNode>();
        public ReadOnlyCollection<ActivityNode> ActivitiesInOrder => activitiesInOrder.AsReadOnly();

        public TimeSpan Duration
        {
            get
            {
                var duration = new TimeSpan();
                return activitiesInOrder.Aggregate(duration, (current, activity) => current.Add(activity.Duration));
            }
        }

        protected override void Init()
        {
            base.Init();
            // InitFirstActivity();
            var tempActivity = Activity = GetFirstActivity();

            if (!tempActivity)
                return;
            
            activitiesInOrder.Add(tempActivity);

            while (tempActivity.HasNextActivity())
            {
                tempActivity = tempActivity.GetNextActivity();
                activitiesInOrder.Add(tempActivity);
            }
        }

        public bool HasNextStep() => GetPort("Out").IsConnected;

        public StepNode GetNextStep() => GetPort("Out").Connection.node as StepNode;

        public void UpdateActivity()
        {
            if (!Activity.HasNextActivity())
                return;

            Activity = Activity.GetNextActivity();
            Activity.TaskCluster.StartDateTime = DateTime.Now;
        }

        public override void Reset()
        {
            // InitFirstActivity();
            Activity = GetFirstActivity();
            if(Activity)
                Activity.ResetConsecutive();
        }

        private ActivityNode GetFirstActivity()
        {
            var activityPort = GetPort("ActivityConnection");

            if (activityPort == null)
                return null;

            if (activityPort.IsConnected)
            {
                return activityPort.Connection.node as ActivityNode;
            }

            return null;
        }

        public override void RaiseCompletedEvent() => RaiseCompletedEvent(this);

        public override void RaiseErrorEvent()
        {
            RaiseErrorEvent(this);
            Activity.RaiseErrorEvent();
        }

        public override void RaiseHelpActivatedEvent()
        {
            RaiseHelpActivatedEvent(this);
            Activity.RaiseHelpActivatedEvent();
        }

        public override void RaiseHelpDeactivatedEvent()
        {
            RaiseHelpDeactivatedEvent(this);
            Activity.RaiseHelpDeactivatedEvent();
        }

        public override HelpNode GetHelpNode()
        {
            var helpNode = Activity.GetHelpNode();

            if (!helpNode && GetPort("Help").IsConnected)
            {
                helpNode = GetPort("Help").Connection.node as HelpNode;
            }

            return helpNode;
        }
    }
}