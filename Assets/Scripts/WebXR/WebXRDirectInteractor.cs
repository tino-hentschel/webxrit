using System;
using System.Collections.Generic;
using UnityEngine;
using WebXR;

namespace haw.pd20.webxr
{
    /// <summary>
    /// <c>WebXRDirectInteractor</c>: Component that is attached to the user’s virtual hands and enables them to select ("grab") and manipulate virtual objects that have the corresponding <see cref="WebXRInteractable"/> components attached.
    /// In a typical IVR training scenario such objects can be task-related tools or equipment.<br/>
    /// <para>
    /// A WebXRDirectInteractor enables users to perform direct selection of interactable components in the virtual environment by “grabbing” them with their virtual hands, typically by pressing the grip button on the controller.
    /// </para>
    /// <para>
    /// Before an interactable is selected, the <c>WebXRDirectInteractor</c> enters a hover state in which it uses a Unity <c>Collider</c> component to detect <see cref="WebXRInteractable"/> within range, i.e., those overlapping with its collider boundaries.
    /// By default, the closest <see cref="WebXRInteractable"/> within range is targeted for selection.
    /// </para>
    /// <para>
    /// <see cref="WebXRInteractable"/>s currently held by the user can also be manipulated or “activated” by pressing the trigger button on the controller.
    /// When activated, a dedicated <c>OnActivation</c> event may be executed if one is defined for the interactable.
    /// For example, a pair of medical scissors held by the user could trigger a cutting animation when activated.
    /// </para>
    /// </summary>
    public class WebXRDirectInteractor : WebXRBaseInteractor, IWebXRController
    {
        public WebXRController Controller { get; private set; }
        
        public FixedJoint AttachJoint { get; private set; }
        // public WebXRControllerHand Hand => Controller.hand;
        // public bool IsPoseGrabbing => CurrentInteractable && CurrentInteractable.GrabType == GrabType.KinematicPose;

        [SerializeField] private WebXRDirectInteractor otherHandInteractor;

        private PullInteractor currentPullInteractor;
        private List<PullInteractor> contactPullinteractors = new List<PullInteractor>();

        public WebXRInteractable NearestInteractable { get; protected set; }
        public PullInteractor NearestPullInteractor { get; protected set; }

        public event Action OnButtonAUp;
        public event Action OnButtonADown;
        public event Action OnButtonBUp;
        public event Action OnButtonBDown;


        public enum InReach
        {
            Nothing,
            Interactable,
            PullInteractor
        }

        public InReach CurrentInReach { get; protected set; }
        
        
        protected override void Awake()
        {
            base.Awake();

            AttachJoint = GetComponent<FixedJoint>();
            Controller = GetComponent<WebXRController>();
        }

        void Update()
        {
            UpdateInReach();

            if (Controller.GetButtonDown(WebXRController.ButtonTypes.Grip))
            {
                Grip();
            }

            if (Controller.GetButtonUp(WebXRController.ButtonTypes.Grip))
            {
                ReleaseGrip();
            }

            if (Controller.GetButtonDown(WebXRController.ButtonTypes.Trigger))
            {
                Trigger();
            }

            if (Controller.GetButtonUp(WebXRController.ButtonTypes.Trigger))
            {
                ReleaseTrigger();
            }

            // TODO There should be a better way to delegate this
            if (Controller.GetButtonDown(WebXRController.ButtonTypes.ButtonA))
            {
                OnButtonADown?.Invoke();
            }

            if (Controller.GetButtonUp(WebXRController.ButtonTypes.ButtonA))
            {
                OnButtonAUp?.Invoke();
            }

            if (Controller.GetButtonDown(WebXRController.ButtonTypes.ButtonB))
            {
                OnButtonBDown?.Invoke();
            }

            if (Controller.GetButtonUp(WebXRController.ButtonTypes.ButtonB))
            {
                OnButtonBUp?.Invoke();
            }
        }

        protected override void OnTriggerEnter(Collider other)
        {
            // base.OnTriggerEnter(other);
            
            var pullInteractor = other.gameObject.GetComponent<PullInteractor>();

            // if (pullInteractor == null || pullInteractor.FollowTransform ||
            //     contactPullinteractors.Contains(pullInteractor))
            //     return;
            //
            // contactPullinteractors.Add(pullInteractor);

            if (pullInteractor)
            {
                if (CurrentInteractable || pullInteractor.FollowTransform || contactPullinteractors.Contains(pullInteractor))
                    return;

                contactPullinteractors.Add(pullInteractor);
            }
            else
            {
                base.OnTriggerEnter(other);
            }

            // var pullInteractor = other.gameObject.GetComponent<PullInteractor>();
            // if (pullInteractor == null || pullInteractor.FollowTransform) 
            //     return;
            //
            // currentPullInteractor = pullInteractor;
        }

        protected override void OnTriggerExit(Collider other)
        {
            // base.OnTriggerExit(other);
            //
            // var pullInteractor = other.gameObject.GetComponent<PullInteractor>();
            // if (pullInteractor == null)
            //     return;
            //
            // contactPullinteractors.Remove(pullInteractor);
            //
            // if (currentPullInteractor == null)
            //     return;
            //
            // if (!currentPullInteractor.Equals(pullInteractor))
            //     return;
            //
            // currentPullInteractor.Detach();
            // currentPullInteractor = null;

            var pullInteractor = other.gameObject.GetComponent<PullInteractor>();

            if (pullInteractor)
            {
                contactPullinteractors.Remove(pullInteractor);

                if (currentPullInteractor == null)
                    return;

                if (!currentPullInteractor.Equals(pullInteractor))
                    return;

                currentPullInteractor.Detach();
                currentPullInteractor = null;
            }
            else
            {
                base.OnTriggerExit(other);
            }

            // if (!other.gameObject.GetComponent<PullInteractor>() || !currentPullInteractor)
            //     return;
            //
            // currentPullInteractor.Detach();
            // currentPullInteractor = null;
        }

        private void UpdateInReach()
        {
            RemoveInactiveGameObjectsFromContactInteractables();
            RemoveInactiveGameObjectsFromContactPullInteractors();
            
            // TODO Remove - Debug Log
            // var cpi_string = "";
            // foreach (var cpi in contactPullinteractors)
            // {
            //     cpi_string += cpi.InteractionText + " | ";
            // }
            //
            // Debug.Log( gameObject.name + "| Contact Pull Interactors " + cpi_string);

            NearestInteractable = GetNearestInteractable();
            NearestPullInteractor = GetNearestPullInteractor();

            bool hasInteractable = NearestInteractable;
            bool hasPullInteractor = NearestPullInteractor;

            switch (hasInteractable)
            {
                case false when !hasPullInteractor:
                    CurrentInReach = InReach.Nothing;
                    return;
                case true when !hasPullInteractor:
                    CurrentInReach = InReach.Interactable;
                    break;
                case false: // when hasPullinteractor: (always true)
                    CurrentInReach = InReach.PullInteractor;
                    break;
                default: // hasInteractable && hasPullInteractor
                    var distanceInteractable =
                        (NearestInteractable.transform.position - transform.position).sqrMagnitude;
                    var distancePullInteractor =
                        (NearestPullInteractor.transform.position - transform.position).sqrMagnitude;

                    CurrentInReach = distanceInteractable <= 2 * distancePullInteractor
                        ? InReach.Interactable
                        : InReach.PullInteractor;
                    break;
            }
        }
        
        // TODO This is a pure code duplicate of RemoveInactiveGameObjectsFromContactInteractables -> PullInteractors and Interactables need to be refactored to derive from one base class (but not by me ...)
        private void RemoveInactiveGameObjectsFromContactPullInteractors()
        {
            for (var i = 0; i < contactPullinteractors.Count; i++)
            {
                if (!contactPullinteractors[i].gameObject.activeInHierarchy)
                {
                    contactPullinteractors.RemoveAt(i);
                }
            }
        }

        private void Grip()
        {
            // var nearestInteractable = GetNearestInteractable();
            // var nearestPullInteractor = GetNearestPullInteractor();
            //
            // bool hasInteractable = nearestInteractable;
            // bool hasPullInteractor = nearestPullInteractor;
            //
            // switch (hasInteractable)
            // {
            //     case false when !hasPullInteractor:
            //         return;
            //     case true when !hasPullInteractor:
            //         TryGrabInteractable();
            //         break;
            //     case false: // when hasPullinteractor: (always true)
            //         TryGrabPullInteractor();
            //         break;
            //     default: // hasInteractable && hasPullInteractor
            //         var distanceInteractable =
            //             (nearestInteractable.transform.position - transform.position).sqrMagnitude;
            //         var distancePullInteractor =
            //             (nearestPullInteractor.transform.position - transform.position).sqrMagnitude;
            //
            //         if (distanceInteractable <= 4 * distancePullInteractor)
            //             TryGrabInteractable();
            //         else
            //             TryGrabPullInteractor();
            //
            //         break;
            // }

            switch (CurrentInReach)
            {
                case InReach.Interactable:
                    TryGrabInteractable();
                    break;
                case InReach.PullInteractor:
                    TryGrabPullInteractor();
                    break;
            }
        }

        private void TryGrabInteractable()
        {
            // var nearestInteractable = GetNearestInteractable();
            //
            // if (!nearestInteractable) //|| nearestInteractable.AttachState == AttachState.Hand)
            //     return;

            if (!NearestInteractable.Grabbable)
            {
                // We only raise a TrySelect event if the interactable is not Grabbable but displays an interaction text.
                if(NearestInteractable.DisplayInteractionText)
                    NearestInteractable.RaiseTrySelectEvent(this);
                return;
            }

            if (NearestInteractable.AttachState == AttachState.Hand)
            {
                if (!NearestInteractable.GrabbableWhenHeld)
                    return;

                otherHandInteractor.ReleaseGrip();
            }

            CurrentInteractable = NearestInteractable;

            m_OnSelectEntered?.Invoke(CurrentInteractable);
            CurrentInteractable.AttachTo(this);

            // CurrentInteractable.MovePosition(transform.position);
            // attachJoint.connectedBody = CurrentInteractable;    
        }

        private void TryGrabPullInteractor()
        {
            // var nearestPullInteractor = GetNearestPullInteractor();
            //
            // if (!nearestPullInteractor)
            //     return;

            currentPullInteractor = NearestPullInteractor;
            currentPullInteractor.Attach(transform);


            // if(!currentPullInteractor)
            //     return;
            //
            // currentPullInteractor.Attach(transform);
        }

        private void ReleaseGrip()
        {
            ReleaseTrigger();
            TryDropInteractable();
            TryDropPullInteractor();
        }

        private void TryDropInteractable()
        {
            if (!CurrentInteractable)
                return;

            AttachJoint.connectedBody = null;
            m_OnSelectExited?.Invoke(CurrentInteractable);
            CurrentInteractable.DetachFrom(this);
            CurrentInteractable = null;
        }

        private void TryDropPullInteractor()
        {
            if (!currentPullInteractor)
                return;

            currentPullInteractor.Detach();
        }

        private void Trigger()
        {
            if (!CurrentInteractable)
                return;

            CurrentInteractable.OnActivate(this);
        }

        private void ReleaseTrigger()
        {
            if (!CurrentInteractable)
                return;

            CurrentInteractable.OnDeactivate(this);
        }

        private PullInteractor GetNearestPullInteractor()
        {
            PullInteractor nearestPullInteractor = null;
            var minDistance = float.MaxValue;

            foreach (var pullInteractor in contactPullinteractors)
            {
                var distance = (pullInteractor.transform.position - transform.position).sqrMagnitude;

                if (!(distance < minDistance)) continue;
                minDistance = distance;
                if (pullInteractor.gameObject.activeInHierarchy)
                    nearestPullInteractor = pullInteractor;
            }

            return nearestPullInteractor;
        }

        public void ExternalAttach(WebXRInteractable interactable)
        {
            if (!interactable.Grabbable)
                return;

            if (currentPullInteractor)
                TryDropPullInteractor();

            if (CurrentInteractable)
                TryDropInteractable();

            CurrentInteractable = interactable;

            m_OnSelectEntered?.Invoke(CurrentInteractable);
            CurrentInteractable.AttachTo(this);
        }
        

        #region Exposed Controller Interface

        public ControllerHand GetHand()
        {
            return Controller.hand == WebXRControllerHand.LEFT ? ControllerHand.Left : ControllerHand.Right;
        }

        public Vector2 ThumbstickAxis()
        {
            return Controller.GetAxis2D(WebXRController.Axis2DTypes.Thumbstick);
        }

        public void Vibrate(float intensity, float durationMilliseconds)
        {
            Controller.Pulse(intensity, durationMilliseconds);
        }

        #endregion
    }
}