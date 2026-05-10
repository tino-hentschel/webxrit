using System;
using System.Collections.Generic;
using XNode;

namespace haw.pd20.tasksystem
{
    public abstract class ActionNode<T> : Node
    {
        [NonSerialized] private readonly List<IActionListener<T>> listeners = new List<IActionListener<T>>();

        public string Name;

        [Output(typeConstraint = TypeConstraint.Strict, connectionType = ConnectionType.Override)]
        public HelpConnection Help;

        public abstract void Reset();
        public abstract void RaiseCompletedEvent();
        public abstract void RaiseErrorEvent();
        public abstract void RaiseHelpActivatedEvent();
        public abstract void RaiseHelpDeactivatedEvent();
        public abstract HelpNode GetHelpNode();

        protected void RaiseCompletedEvent(T action)
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnActionCompleted(action);
            }
        }

        protected void RaiseErrorEvent(T action)
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnError(action);
            }
        }

        protected void RaiseHelpActivatedEvent(T action)
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnHelpActivated(action);
            }
        }
        
        protected void RaiseHelpDeactivatedEvent(T action)
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnHelpDeactivated(action);
            }
        }

        public void RegisterCompletedListener(IActionListener<T> listener)
        {
            if (!listeners.Contains(listener))
                listeners.Add(listener);
        }

        public void UnregisterCompletedListener(IActionListener<T> listener)
        {
            if (listeners.Contains(listener))
                listeners.Remove(listener);
        }

        public override object GetValue(NodePort port) => null;
    }
}