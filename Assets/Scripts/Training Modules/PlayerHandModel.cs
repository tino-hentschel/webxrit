using System;
using System.Collections.Generic;
using System.Linq;
using haw.pd20.events;
using haw.pd20.tasksystem;
using UnityEngine;

namespace haw.pd20.training_modules
{
    [Serializable]
    public struct HandStateTask
    {
        public HandState HandState;
        public TaskNode Task;
    }

    public class PlayerHandModel : MonoBehaviour
    {
        [SerializeField] private PlayerHandModel otherPlayerHandModel;
        [SerializeField] private HandState defaultHandState;
        [SerializeField] private List<HandStateTask> handStateTasks;

        public HandState HandState { get; private set; }
        private SkinnedMeshRenderer skinnedMeshRenderer;

        public delegate void AppearanceChangedDelegate(PlayerHandModel playerHandModel);

        public event AppearanceChangedDelegate OnAppearanceChanged;

        private void Awake()
        {
            skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

            // setting the default HandState without checking / notifying tasks
            HandState = defaultHandState;
            skinnedMeshRenderer.material = defaultHandState.Material;
        }

        /// <summary>
        /// Will apply the given HandState if it matches the current Task (e.g. The HandState "NonSterileGloves" will be applied if the current Task is "Put On Non Sterile Gloves).
        /// The HandState and Task Combinations have to be setup in the editor the HandStateTasks field of the PlayerHandModel.
        /// If a corresponding HandState and Task Combination (HandStateTask) exists, its Task will be notified whether it is the required current Task or not. 
        /// </summary>
        /// <param name="handState"></param>
        public void TrySetHandStateWithinTask(HandState handState)
        {
            foreach (var handStateTask in handStateTasks.Where(handStateTask =>
                handStateTask.HandState == handState))
            {
                if (handStateTask.Task.IsCurrentTask())
                {
                    SetHandState(handState);

                    if (HandState != otherPlayerHandModel.HandState)
                        return;
                }

                // TODO: This check for Listeners and the Event that is triggered by it could also be moved to the TaskNode itself. But this would couple the EventManager and the TaskSystem.
                if (!handStateTask.Task.HasExecutionListeners)
                {
                    EventManager.Instance.Raise(new DefaultErrorEvent("Die Aufgabe " + handStateTask.Task.Name +
                                                                      " gehört nicht zur aktuellen Aktivität."));
                }
                else
                {
                    handStateTask.Task.NotifyExecution();
                }
            }
        }

        /// <summary>
        /// Sets the HandState which changes the appearance of the PlayHandModel without notifying a task execution.
        /// </summary>
        /// <param name="handState"></param>
        public void SetHandState(HandState handState)
        {
            HandState = handState;
            skinnedMeshRenderer.material = HandState.Material;

            OnAppearanceChanged?.Invoke(this);
        }
    }
}