using System.Linq;

namespace haw.pd20.tasksystem
{
    [CreateNodeMenu("Task Cluster/Set Cluster")]
    public class SetTaskClusterNode : TaskClusterNode
    {
        public override void RaiseHelpActivatedEvent()
        {
            RaiseHelpActivatedEvent(this);

            foreach (var task in tasks.Where(t => t.Status == TaskStatus.Open))
            {
                task.RaiseHelpActivatedEvent();
            }
        }

        public override void RaiseHelpDeactivatedEvent()
        {
            RaiseHelpDeactivatedEvent(this);
            
            foreach (var task in tasks.Where(t => t.Status == TaskStatus.Open))
            {
                task.RaiseHelpDeactivatedEvent();
            }
        }

        public override bool HasOpenTasks() => tasks.Any(t => t.Status == TaskStatus.Open);

        public override HelpNode GetHelpNode()
        {
            if (GetPort("Help").IsConnected)
            {
                return GetPort("Help").Connection.node as HelpNode;
            }

            return null;
        }
    }
}