using System.Linq;
using UnityEngine;

namespace haw.pd20.webxr
{
    /// <summary>
    /// The <c>WebXRSocketInteractor</c> is used to hold <see cref="WebXRInteractable"/>s via a socket and serves as a virtual storage or placement area for objects.
    /// Unlike the <see cref="WebXRDirectInteractor"/>, it is not connected to any <c>Controller</c> and can be attached to any <c>GameObject</c> equipped with a <c>Collider</c> component.<br/>
    /// <para>
    /// An active <c>WebXRSocketInteractor</c> automatically selects and attaches the closest <see cref="WebXRInteractable"/> located within the boundaries of its <c>Collider</c>
    /// (e.g., a keyhole that automatically holds a key when it is placed inside it). 
    /// </para>
    /// <para>
    /// The position and orientation of an attached <see cref="WebXRInteractable"/> can be defined by an optional <c>Transform</c> component,
    /// otherwise the <c>Transform</c> of the <c>WebXRSocketInteractor</c> itself is used. 
    /// </para>
    /// <para>
    /// When a <see cref="WebXRInteractable"/> held by a <see cref="WebXRDirectInteractor"/> (i.e., the user’s virtual hand) enters the socket’s <c>Collider</c>,
    /// a preview of the interactable can be displayed at the socket’s attach position.
    /// </para>
    /// <para>
    /// If the user releases the <see cref="WebXRInteractable"/> by releasing the grip button of the <c>Controller</c>, it automatically snaps into place and becomes attached to the socket. 
    /// </para>
    /// <para>
    /// A <c>WebXRSocketInteractor</c> can hold only one <see cref="WebXRInteractable"/> at a time.
    /// </para>
    /// <para>
    /// <see cref="WebXRInteractable"/>s can be removed from sockets simply by grabbing them again with a <see cref="WebXRDirectInteractor"/>.
    /// </para>
    /// </summary>
    public class WebXRSocketInteractor : WebXRBaseInteractor
    {
        public bool SocketActive = true;
        public WebXRInteractable StartingSelectedInteractable;

        public GameObject[] allowedObjects;

        private void Start()
        {
            if (!StartingSelectedInteractable) 
                return;
            
            CurrentInteractable = StartingSelectedInteractable;
            CurrentInteractable.AttachTo(this);
        }

        private void Update()
        {
            // If an interactable is attached to this socket we monitor if its state has changed (e.g. if it was grabbed by the user)
            if (CurrentInteractable)
            {
                CheckAttachState();
                return;
            }

            // otherwise we look for any interactables within the collider of this socket an try to attach them.
            TryAttachInteractable();
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (allowedObjects.Length > 0 && !allowedObjects.Contains(other.gameObject))
                return;
            
            if(CurrentInteractable)
                return;

            base.OnTriggerEnter(other);
        }

        protected override void OnTriggerExit(Collider other)
        {
            if (allowedObjects.Length > 0 && !allowedObjects.Contains(other.gameObject))
                return;

            base.OnTriggerExit(other);
        }

        private void CheckAttachState()
        {
            switch (CurrentInteractable.AttachState)
            {
                case AttachState.Hand:
                    m_OnSelectExited?.Invoke(CurrentInteractable);
                    // TODO Add call to CurrentInteractable.DetachFrom(this)
                    CurrentInteractable = null;
                    return;
                case AttachState.Socket when !SocketActive:
                    ForceReleaseInteractable();
                    break;
            }
        }

        /// <summary>
        /// Completely detaches the current Interactable from this Socket for the current frame only.
        /// The Interactable needs to be removed out of the Box Collider of the Socket directly afterwards or it will be re-attached in the next frame.
        /// </summary>
        public void ForceReleaseInteractable()
        {
            if (!CurrentInteractable)
                return;
            
            CurrentInteractable.Detach();
            m_OnSelectExited?.Invoke(CurrentInteractable);
            CurrentInteractable = null;
        }

        private void TryAttachInteractable()
        {
            if (!SocketActive || contactInteractables.Count == 0)
                return;

            var nearestInteractable = GetNearestInteractableWithAttachState(AttachState.None);
            
            if (!nearestInteractable)
                return;

            if (allowedObjects.Length > 0 && !allowedObjects.Contains(nearestInteractable.gameObject))
                return;

            CurrentInteractable = nearestInteractable;
            CurrentInteractable.AttachTo(this);
            m_OnSelectEntered?.Invoke(CurrentInteractable);
        }

        public void SetSocketActive(bool active) => SocketActive = active;
    }
}