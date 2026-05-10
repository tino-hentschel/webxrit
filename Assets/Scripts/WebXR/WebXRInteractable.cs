using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace haw.pd20.webxr
{
    public enum AttachState
    {
        None,
        Hand, // DirectInteractor
        Socket // SocketInteractor
    }

    public enum GrabType
    {
        KinematicPose,
        PhysicsJoint
    }

    /// <summary>
    /// A <c>WebXRInteractable</c> is a component that can be selected and manipulated by any interactor (e.g., grabbed by a <see cref="WebXRDirectInteractor"/>).<br/>
    /// <para>
    /// During selection, the interactable becomes a child of the selecting interactor within the scene hierarchy,
    /// meaning it follows the interactor’s movement by matching its local position and rotation,
    /// for instance, following the user’s hand movements while being held. 
    /// </para>
    /// <para>
    /// Each <c>WebXRInteractable</c> requires both a <c>Collider</c> and a <c>Rigidbody</c> to detect interactions through Unity’s physics engine.
    /// However, once an interactable is selected, its physics simulation is disabled by default to avoid unnecessary processing
    /// and unintended collisions, and it becomes a kinematic object.  Upon release, the parent–child relationship
    /// between the interactor and the interactable is removed, and the physics simulation is reenabled
    /// </para>
    /// <para>
    /// The <c>WebXRInteractable</c> supports custom responses to selection and manipulation events through its <c>OnSelect</c> and <c>OnActivate</c> properties.
    /// These properties are implemented as <c>Unity Events</c> that can hold multiple serialized function calls to other <c>GameObjects</c> or components within the <c>Scene</c>.
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// Typical uses include triggering Unity API functions such as playing <c>Animations</c> or <c>AudioClips</c>,
    /// which can be configured directly in the Unity Editor without additional scripting.
    /// </item>
    /// <item>
    /// For example, when the user holds a pair of medical scissors during a nursing training scenario,
    /// activating the interactable could play both a cutting animation and a corresponding sound effect.
    /// </item>
    /// </list>
    /// <para>
    /// Furthermore, the <c>OnSelect</c> and <c>OnActivate</c> properties can call functions on custom components attached to the interactable,
    /// thus providing interfaces for easily extending interaction behaviors through user-defined scripts if required.
    /// As an example see <see cref="PoseContainer"/>.
    /// </para>
    ///  </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class WebXRInteractable : MonoBehaviour
    {
        // TODO check if we can make them private serialized fields
        // public bool IsKinematicWhenAttached = true;
        [Tooltip("When the interactable is dropped it will be parented to the parent transform it had when it was picked up. Otherwise its parent transform will be the scene root.")]
        public bool RetainTransformParent;
        
        [FormerlySerializedAs("Grabbable")] [SerializeField] private bool grabbable;
        public bool Grabbable => grabbable;
        public void SetGrabbable(bool isGrabbable) => grabbable = isGrabbable;
        
        [Tooltip("If checked an Interactable can be grabbed by one hand when the player holds it in the other hand. The interactable will switch its location to the new hand that grabbed it.")]
        public bool GrabbableWhenHeld;
        public GrabType GrabType;
        
        [SerializeField] private bool displayInteractionText;
        public bool DisplayInteractionText => displayInteractionText;
        public void EnableInteractionText(bool isEnabled) => displayInteractionText = isEnabled;
        public void SetGrabbableAndEnableInteractionText(bool isEnabled) =>
            grabbable = displayInteractionText = isEnabled;
        public string InteractionText;
        
        private Rigidbody _rigidbody;
        private Transform originalSceneParent;

        public AttachState AttachState { get; private set; }
        public IWebXRController Controller { get; private set; }
        
        [SerializeField]
        [Tooltip("Called when a directInteractor begins selecting this interactable.")]
        WebXRInteractableEvent m_OnSelectEntered = new WebXRInteractableEvent();

        /// <summary>
        /// Called when a directInteractor begins selecting this interactable.
        /// </summary>
        public WebXRInteractableEvent onSelectEntered
        {
            get => m_OnSelectEntered;
            set => m_OnSelectEntered = value;
        }
        
        [SerializeField]
        [Tooltip("Called when a directInteractor stops selecting this interactable.")]
        WebXRInteractableEvent m_OnSelectExited = new WebXRInteractableEvent();

        // TODO Raise Event
        /// <summary>
        /// Called when a directInteractor stops selecting this interactable.
        /// </summary>
        public WebXRInteractableEvent onSelectExited
        {
            get => m_OnSelectExited;
            set => m_OnSelectExited = value;
        }
        
        [SerializeField]
        [Tooltip("Called when a directInteractor tries to select (grab) this interactable when it is not grabbable.")]
        WebXRInteractableEvent m_OnTrySelect = new WebXRInteractableEvent();

        // TODO Raise Event
        /// <summary>
        /// Called when a directInteractor tries to select (grab) this interactable when it is not grabbable.
        /// </summary>
        public WebXRInteractableEvent onTrySelect
        {
            get => m_OnTrySelect;
            set => m_OnTrySelect = value;
        }

        [SerializeField]
        [Tooltip("Called when an Interactor activates this selected Interactable.")]
        WebXRInteractableEvent m_OnActivate = new WebXRInteractableEvent();

        /// <summary>
        /// Called when an Interactor activates this selected Interactable.
        /// </summary>
        public WebXRInteractableEvent onActivate
        {
            get => m_OnActivate;
            set => m_OnActivate = value;
        }
        
        [SerializeField]
        [Tooltip("Called when an Interactor deactivates this selected Interactable.")]
        WebXRInteractableEvent m_OnDeactivate = new WebXRInteractableEvent();
        
        /// <summary>
        /// Called when an Interactor deactivates this selected Interactable.
        /// </summary>
        public WebXRInteractableEvent onDeactivate
        {
            get => m_OnDeactivate;
            set => m_OnDeactivate = value;
        }

        [SerializeField] private UnityEvent onAttachedToSocket;
        [SerializeField] private UnityEvent onDetachedFromSocket;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            originalSceneParent = transform.parent;
            // AttachTransformLeft = AttachTransformLeft == null ? transform : AttachTransformLeft;
            // AttachTransformRight = AttachTransformRight == null ? transform : AttachTransformRight;
        }
        
        public void AttachTo(WebXRDirectInteractor directInteractor)
        {
            switch (GrabType)
            {
                case GrabType.KinematicPose:
                    transform.position = directInteractor.AttachTransform.position;
                    transform.rotation = directInteractor.AttachTransform.rotation;
                    transform.SetParent(directInteractor.transform);
                    _rigidbody.isKinematic = true;
                    _rigidbody.useGravity = false;
                    break;

                case GrabType.PhysicsJoint:
                    transform.parent = RetainTransformParent ? originalSceneParent : null;
                    directInteractor.AttachJoint.connectedBody = _rigidbody;
                    _rigidbody.isKinematic = false;
                    _rigidbody.useGravity = true;
                    break;
            }

            // SetTransformToInteractorAttachPoint(directInteractor);
            // transform.position = directInteractor.AttachTransform.position;
            // transform.rotation = directInteractor.AttachTransform.rotation;
            //
            // if (IsKinematicWhenAttached)
            // {
            //     transform.parent = directInteractor.transform;
            //     _rigidbody.isKinematic = true;
            // }
            // else
            // {
            //     directInteractor.AttachJoint.connectedBody = _rigidbody;
            // }
            
            // if the previous Attach State was a Socket
            if (AttachState == AttachState.Socket)
            {
                // we call the detached from Socket Event
                onDetachedFromSocket?.Invoke();
            }

            AttachState = AttachState.Hand;
            Controller = directInteractor;
            onSelectEntered?.Invoke(directInteractor);
        }

        public void AttachTo(WebXRSocketInteractor socketInteractor)
        {
            if (AttachState == AttachState.Socket)
                return;

            // SetTransformToInteractorAttachPoint(directInteractor);
            transform.position = socketInteractor.AttachTransform.position;
            transform.rotation = socketInteractor.AttachTransform.rotation;

            transform.SetParent(socketInteractor.AttachTransform);
            
            _rigidbody.isKinematic = true;
            _rigidbody.useGravity = false;

            AttachState = AttachState.Socket;
            
            onAttachedToSocket?.Invoke();
        }
        
        public void DetachFrom(WebXRDirectInteractor directInteractor)
        {
            // onSelectExited?.Invoke(directInteractor);
            Detach();
            // TODO Changed to Invoke AFTER Interactable has been detached
            onSelectExited?.Invoke(directInteractor);
            Controller = null;
        }
        
        public void DetachFrom(WebXRSocketInteractor socketInteractor)
        {
            // TODO When reworking an object placement task (module 16) make sure to refactor the "old" Detach method
            // TODO => make it private and only use DetachFrom. Adjust and call the onDetachedFromSocket Event here. Make sure all serialized calls in the scene are changed. 
            //  onDetachedFromSocket?.Invoke(socketInteractor);
            
            // If the next Attach State after a detachment from a socket is "Hand" this means we grabbed the interactable from a socket
            // so we only call the detachFromSocket Event an don't drop (Detach) that sh*t.
            if (AttachState == AttachState.Hand) 
                return;
            
            Detach();
        }

        public void Detach()
        {
            // switch (AttachState)
            // {
            //     case AttachState.Hand:
            //         
            //         break;
            //     case AttachState.Socket:
            //         
            //         break;
            // }
            //
            // AttachState = AttachState.None;
            //
            // if (IsKinematicWhenAttached)
            // {
            //     transform.parent = transform.root;
            //     _rigidbody.isKinematic = false;
            //     _rigidbody.useGravity = true;
            // }

            transform.parent = RetainTransformParent ? originalSceneParent : null;

            _rigidbody.isKinematic = false;
            _rigidbody.useGravity = true;

            AttachState = AttachState.None;
        }

        public void RaiseTrySelectEvent(WebXRDirectInteractor interactor)
        {
            m_OnTrySelect?.Invoke(interactor);
        }

        /// <summary>
        /// This method is called when the directInteractor sends an activation event down to an interactable.
        /// </summary>
        /// <param name="interactor">Interactor that is sending the activation event.</param>
        protected internal virtual void OnActivate(WebXRDirectInteractor interactor)
        {
            m_OnActivate?.Invoke(interactor);
        }
        
        /// <summary>
        /// This method is called when the directInteractor sends an deactivation event down to an interactable.
        /// </summary>
        /// <param name="interactor">Interactor that is sending the deactivation event.</param>
        protected internal virtual void OnDeactivate(WebXRDirectInteractor interactor)
        {
            m_OnDeactivate?.Invoke(interactor);
        }

        // private void SetTransformToInteractorAttachPoint(WebXRInteractor directInteractor)
        // {
        //     var attachTransform = directInteractor.Hand == WebXRControllerHand.RIGHT
        //         ? AttachTransformRight
        //         : AttachTransformLeft;
        //     var attachPosition = attachTransform.position;
        //
        //     var attachOffset = _rigidbody.worldCenterOfMass - attachPosition;
        //     var localAttachOffset = attachTransform.InverseTransformDirection(attachOffset);
        //
        //     var inverseLocalScale = directInteractor.AttachTransform.lossyScale;
        //     inverseLocalScale =
        //         new Vector3(1f / inverseLocalScale.x, 1f / inverseLocalScale.y, 1f / inverseLocalScale.z);
        //     localAttachOffset.Scale(inverseLocalScale);
        //
        //     var interactorRotation = directInteractor.AttachTransform.rotation;
        //     transform.position = directInteractor.AttachTransform.position + interactorRotation * localAttachOffset;
        //     transform.rotation = interactorRotation *
        //                          Quaternion.Inverse(Quaternion.Inverse(_rigidbody.rotation) * attachTransform.rotation);
        // }
    }
}