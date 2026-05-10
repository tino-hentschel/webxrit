using UnityEngine;
using UnityEngine.Events;

namespace haw.pd20.webxr
{
    // TODO: We need an WebXRBaseInteractable Class as base for interactable, pull interactor and this hand button. All Events need to be defined in this base class.

    public class HandButton : MonoBehaviour
    {
        [SerializeField] private UnityEvent onPress;
        [SerializeField] private float inRangeThreshold = 0.01f;

        private float yMin = 0.0f;
        private float yMax = 0.0f;
        private bool previousPress = false;
        
        private float previousHandHeight = 0.0f;
        private WebXRDirectInteractor hoverInteractor = null;


        // TODO: Everything below this line goes to the base class.

        [SerializeField] [Tooltip("Called every time when an interactor begins hovering over this interactable.")]
        WebXRInteractableEvent m_OnHoverEntered = new WebXRInteractableEvent();

        /// <summary>
        /// Called when an Interactor activates this selected Interactable.
        /// </summary>
        public WebXRInteractableEvent onHoverEntered
        {
            get => m_OnHoverEntered;
            set => m_OnHoverEntered = value;
        }

        [SerializeField] [Tooltip("Called every time when an interactor stops hovering over this interactable.")]
        WebXRInteractableEvent m_OnHoverExited = new WebXRInteractableEvent();

        /// <summary>
        /// Called when an Interactor activates this selected Interactable.
        /// </summary>
        public WebXRInteractableEvent onOnHoverExited
        {
            get => m_OnHoverExited;
            set => m_OnHoverExited = value;
        }

        private void OnTriggerEnter(Collider other)
        {
            var interactor = other.gameObject.GetComponent<WebXRDirectInteractor>();
            if (interactor == null || interactor.HasInteractable)
                return;

            m_OnHoverEntered?.Invoke(interactor);
        }

        private void OnTriggerExit(Collider other)
        {
            var interactor = other.gameObject.GetComponent<WebXRDirectInteractor>();
            if (interactor == null || interactor.HasInteractable)
                return;

            m_OnHoverExited?.Invoke(interactor);
        }

        // TODO: Everything above this line goes to the base class.


        private void Awake()
        {
            onHoverEntered.AddListener(StartPress);
            onOnHoverExited.AddListener(EndPress);
        }

        private void Start()
        {
            SetMinMax();
        }

        private void Update()
        {
            if (hoverInteractor)
            {
                // float newHandHeight = GetLocalYPosition(hoverInteractor.transform.position);
                float newHandHeight = GetLocalYPosition(hoverInteractor.transform.position);
                float handDifference = previousHandHeight - newHandHeight;
                previousHandHeight = newHandHeight;

                // TODO: Axis need to be configured in the Editor
                // float newButtonYPosition = transform.localPosition.y - handDifference;
                float newButtonYPosition = transform.localPosition.z - handDifference;
                // SetYPosition(newButtonYPosition);
                SetZPosition(newButtonYPosition);
                
                CheckPress();
            }
        }

        private void StartPress(WebXRDirectInteractor interactor)
        {
            hoverInteractor = interactor;
            // TODO: Axis need to be configured in the Editor
            // previousHandHeight = GetLocalYPosition(hoverInteractor.transform.position);
            previousHandHeight = GetLocalYPosition(hoverInteractor.transform.position);

        }

        private void EndPress(WebXRDirectInteractor interactor)
        {
            hoverInteractor = null;
            previousHandHeight = 0.0f;

            previousPress = false;
            // TODO: Axis need to be configured in the Editor
            // SetYPosition(yMax);
            SetZPosition(yMax);
        }

        private void SetMinMax()
        {
            Collider coll = GetComponent<Collider>();

            // local pos is used so the button will work with any global rotation (e.g. if it is placed on a wall)
            
            // TODO: Axis need to be configured in the Editor
            // yMin = transform.localPosition.y - coll.bounds.size.y * 0.5f;
            // yMax = transform.localPosition.y;
            yMin = transform.localPosition.z - coll.bounds.size.z * 0.5f;
            yMax = transform.localPosition.z;
        }

        private float GetLocalYPosition(Vector3 position)
        {
            Vector3 localPosition = transform.root.InverseTransformPoint(position);
            return localPosition.y;
        }
        
        // TODO: Axis need to be configured in the Editor
        private float GetLocalZPosition(Vector3 position)
        {
            Vector3 localPosition = transform.root.InverseTransformPoint(position);
            return localPosition.z;
        }

        private void SetYPosition(float yPosition)
        {
            Vector3 newPosition = transform.localPosition;
            newPosition.y = Mathf.Clamp(yPosition, yMin, yMax);
            transform.localPosition = newPosition;
        }
        
        // TODO: Axis need to be configured in the Editor
        private void SetZPosition(float yPosition)
        {
            Vector3 newPosition = transform.localPosition;
            newPosition.z = Mathf.Clamp(yPosition, yMin, yMax);
            transform.localPosition = newPosition;
        }

        private void CheckPress()
        {
            bool inPosition = InPosition();

            if (inPosition && inPosition != previousPress) // TODO: replace "inPosition != previousPress" with "!previousPress"
            {
                onPress.Invoke();
            }

            previousPress = inPosition;
        }

        private bool InPosition()
        {
            // TODO: Axis need to be configured in the Editor
            // float inRange = Mathf.Clamp(transform.localPosition.y, yMin, yMin + inRangeThreshold);
            float inRange = Mathf.Clamp(transform.localPosition.z, yMin, yMin + inRangeThreshold);

            // if transform.localPosition.y is different from inRange it means that the inRange value was clamped and the button is not in the minPos within the given threshold.

            // return transform.localPosition.y == inRange;
            return transform.localPosition.z == inRange;

        }
    }
}