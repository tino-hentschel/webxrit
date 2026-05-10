using System.Collections.Generic;
using UnityEngine;

namespace haw.pd20.events
{
    public class EventManager : MonoBehaviour
    {
        // private static EventManager instanceInternal = null;
        // public static EventManager Instance => instanceInternal ?? (instanceInternal = new EventManager());

        public delegate void EventDelegate<T>(T e) where T : GameEvent;

        private delegate void EventDelegate(GameEvent e);

        private readonly Dictionary<System.Type, EventDelegate>
            delegates = new Dictionary<System.Type, EventDelegate>();

        private readonly Dictionary<System.Delegate, EventDelegate> delegateLookup =
            new Dictionary<System.Delegate, EventDelegate>();
        
        private static EventManager instanceInternal = null;

        // override so we don't have to typecast the object
        public static EventManager Instance
        {
            get
            {
                if (instanceInternal == null)
                {
                    instanceInternal = GameObject.FindObjectOfType(typeof(EventManager)) as EventManager;
                }
                return instanceInternal;
            }
        }

        public void AddListener<T>(EventDelegate<T> del) where T : GameEvent
        {
            // Early-out if we've already registered this delegate
            if (delegateLookup.ContainsKey(del))
                return;

            // Create a new non-generic delegate which calls our generic one.
            // This is the delegate we actually invoke.
            void InternalDelegate(GameEvent e) => del((T) e);
            delegateLookup[del] = InternalDelegate;

            if (delegates.TryGetValue(typeof(T), out var tempDel))
            {
                delegates[typeof(T)] = tempDel += InternalDelegate;
            }
            else
            {
                delegates[typeof(T)] = InternalDelegate;
            }
        }

        public void RemoveListener<T>(EventDelegate<T> del) where T : GameEvent
        {
            if (delegateLookup.TryGetValue(del, out var internalDelegate))
            {
                if (delegates.TryGetValue(typeof(T), out var tempDel))
                {
                    tempDel -= internalDelegate;
                    if (tempDel == null)
                    {
                        delegates.Remove(typeof(T));
                    }
                    else
                    {
                        delegates[typeof(T)] = tempDel;
                    }
                }

                delegateLookup.Remove(del);
            }
        }

        public void Raise(GameEvent e)
        {
            if (delegates.TryGetValue(e.GetType(), out var del))
            {
                del.Invoke(e);
            }
        }
    }
}