using System;
using System.Linq;
using haw.pd20.tasksystem;
using haw.pd20.webxr;
using UnityEngine;

namespace haw.pd20.training_modules
{
    [RequireComponent(typeof(WebXRSocketInteractor))]
    public class SocketItemEvaluator : MonoBehaviour
    {
        [SerializeField] private bool active = true;
        [SerializeField] private ItemTag[] allowedItemTags;
        [SerializeField] private TaskNode task;
        [SerializeField] private GameObject helpGameObject;

        private WebXRSocketInteractor socketInteractor;

        public bool IsValid { get; private set; }

        private void Awake()
        {
            socketInteractor = GetComponent<WebXRSocketInteractor>();
            helpGameObject.SetActive(false);
        }

        public void SetActive(bool isActive) => active = isActive;

        public void Eval()
        {
            if (!active)
                return;

            var item = socketInteractor.CurrentInteractable.GetComponent<Item>();
            if (!item)
            {
#if UNITY_EDITOR
                Debug.LogWarning("No Item @" + socketInteractor.CurrentInteractable.name + " | Unable to evaluate.");
#endif
                IsValid = false;
                return;
            }

            IsValid = allowedItemTags.Contains(item.Tag);

            if (!task)
            {
#if UNITY_EDITOR
                Debug.LogWarning("No Task connected with SocketItemEvaluator @" +
                                 socketInteractor.CurrentInteractable.name);
#endif
                return;
            }

            task.Status = IsValid ? TaskStatus.Completed : TaskStatus.Failed;
            task.NotifyExecution();

            // IsValid = AcceptedGameObjectNames.Contains(socketInteractor.AttachTransform
            //     .GetComponentInChildren<WebXRInteractable>().name);
        }

        public void ResetAndNotify()
        {
            if (!active)
                return;

            IsValid = false;

            if (!task)
            {
#if UNITY_EDITOR
                Debug.LogWarning("No Task connected with SocketItemEvaluator @" +
                                 socketInteractor.CurrentInteractable.name);
#endif
                return;
            }

            task.Reset();
            task.NotifyExecution();
        }

        public void DisplayHelpGameObject(bool isEnabled)
        {
            if (!isEnabled)
            {
                helpGameObject.SetActive(false);
                return;
            }

            if (task.Status == TaskStatus.Open)
            {
                helpGameObject.SetActive(true);
            }
        }
    }
}