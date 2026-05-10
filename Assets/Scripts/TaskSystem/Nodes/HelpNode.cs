using XNode;

namespace haw.pd20.tasksystem
{
    [CreateNodeMenu("Help")]
    public class HelpNode : Node
    {
        [Input(typeConstraint = TypeConstraint.Strict)]
        public HelpConnection In;

        public string Message;

        public override object GetValue(NodePort port) => null;
    }
}