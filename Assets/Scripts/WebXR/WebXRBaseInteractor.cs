using System.Collections.Generic;
using UnityEngine;

namespace haw.pd20.webxr
{
    public class WebXRBaseInteractor : MonoBehaviour
    {
        public bool HasInteractable => CurrentInteractable != null;

        public Transform AttachTransform;

        public WebXRInteractable CurrentInteractable { get; protected set; }
        protected List<WebXRInteractable> contactInteractables = new List<WebXRInteractable>();
        
        // Events
        [SerializeField]
        [Tooltip("Called when this interactor begins selecting an interactable.")]
        protected WebXRInteractorEvent m_OnHoverEntered;

        public WebXRInteractorEvent onHoverEntered
        {
            get => m_OnHoverEntered;
            set => m_OnHoverEntered = value;
        }

        [SerializeField]
        [Tooltip("Called when this interactor begins selecting an interactable.")]
        protected WebXRInteractorEvent m_OnHoverExited;

        public WebXRInteractorEvent onHoverExited
        {
            get => m_OnHoverExited;
            set => m_OnHoverExited = value;
        }
        
        [SerializeField]
        [Tooltip("Called when this interactor begins selecting an interactable.")]
        protected WebXRInteractorEvent m_OnSelectEntered;

        public WebXRInteractorEvent onSelectEntered
        {
            get => m_OnSelectEntered;
            set => m_OnSelectEntered = value;
        }

        [SerializeField]
        [Tooltip("Called when this interactor begins selecting an interactable.")]
        protected WebXRInteractorEvent m_OnSelectExited;

        public WebXRInteractorEvent onSelectExited
        {
            get => m_OnSelectExited;
            set => m_OnSelectExited = value;
        }

        protected virtual void Awake()
        {
            if (AttachTransform != null) 
                return;
            
            CreateAttachTransform();
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            var interactable = other.gameObject.GetComponentInParent<WebXRInteractable>();
            if (interactable == null || contactInteractables.Contains(interactable)) 
                return;
            
            m_OnHoverEntered?.Invoke(interactable);
            contactInteractables.Add(interactable);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            var interactable = other.gameObject.GetComponentInParent<WebXRInteractable>();
            if (interactable == null) 
                return;
            
            m_OnHoverExited?.Invoke(interactable);
            contactInteractables.Remove(interactable);
        }

        private void CreateAttachTransform()
        {
            var attachGO = new GameObject($"[{gameObject.name}] Attach");
            AttachTransform = attachGO.transform;
            
            AttachTransform.SetParent(transform);
            AttachTransform.localPosition = Vector3.zero;
            AttachTransform.localRotation = Quaternion.identity;
            AttachTransform.localScale = Vector3.one;
        }

        protected WebXRInteractable GetNearestInteractable()
        {
            WebXRInteractable nearestInteractable = null;
            var minDistance = float.MaxValue;

            foreach (var interactable in contactInteractables)
            {
                var distance = (interactable.transform.position - transform.position).sqrMagnitude;

                if (!(distance < minDistance)) continue;
                minDistance = distance;
                
                if(interactable.gameObject.activeInHierarchy)
                    nearestInteractable = interactable;
            }

            return nearestInteractable;
        }
        
        protected WebXRInteractable GetNearestInteractableWithAttachState(AttachState attachState)
        {
            WebXRInteractable nearestInteractable = null;
            var minDistance = float.MaxValue;

            foreach (var interactable in contactInteractables)
            {
                if(!interactable.gameObject.activeInHierarchy || interactable.AttachState != attachState)
                    continue;
                
                var distance = (interactable.transform.position - transform.position).sqrMagnitude;

                if (!(distance < minDistance)) 
                    continue;
                
                minDistance = distance;
                nearestInteractable = interactable;
            }

            return nearestInteractable;
        }

        protected void RemoveInactiveGameObjectsFromContactInteractables()
        {
            for (var i = 0; i < contactInteractables.Count; i++)
            {
                if (!contactInteractables[i].gameObject.activeInHierarchy)
                {
                    contactInteractables.RemoveAt(i);
                }
            }
        }
    }
}