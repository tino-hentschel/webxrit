using XNode;

namespace haw.pd20.tasksystem
{
    [CreateNodeMenu("Start")]
    public class StartNode : Node
    {
        [Output(connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)]
        public StepConnection Out;
        
        public override object GetValue(NodePort port) => null;
    }
}