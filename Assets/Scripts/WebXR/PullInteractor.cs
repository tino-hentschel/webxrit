using UnityEngine;
using UnityEngine.Events;

namespace haw.pd20.webxr
{
    public class PullInteractor : MonoBehaviour
    {
        private enum Axis
        {
            X,
            Y,
            Z
        }

        [SerializeField] private Axis pullAxis;
        [SerializeField] private float lowerLimit;

        public float LowerLimit
        {
            get
            {
                return pullAxis switch
                {
                    Axis.X => transform.localPosition.x - lowerLimit,
                    Axis.Y => transform.localPosition.y - lowerLimit,
                    Axis.Z => transform.localPosition.z - lowerLimit,
                    _ => 0.0f
                };
            }
        }

        [SerializeField] private float upperLimit;

        public float UpperLimit
        {
            get
            {
                return pullAxis switch
                {
                    Axis.X => transform.localPosition.x + upperLimit,
                    Axis.Y => transform.localPosition.y + upperLimit,
                    Axis.Z => transform.localPosition.z + upperLimit,
                    _ => 0.0f
                };
            }
        }

        public float PullAxisValue
        {
            get
            {
                return pullAxis switch
                {
                    Axis.X => transform.localPosition.x,
                    Axis.Y => transform.localPosition.y,
                    Axis.Z => transform.localPosition.z,
                    _ => 0.0f
                };
            }
        }

        [Tooltip(
            "Resets the PullInteractor to its initial position when it is released by a WebXRDirectInteractor (Hand).")]
        [SerializeField]
        private bool ResetPositionOnRelease;

        [Tooltip(
            "Snaps to PullInteractor to the position of the WebXRDirectInteractor (Hand) that grabs it, otherwise an offset is used.")]
        [SerializeField]
        private bool SnapOnGrab;

        public GameObject PullIndicator;
        public string InteractionText;

        
        [SerializeField] private bool upperLimitCallbackEnabled = true;
        public void EnableUpperLimitCallback(bool isEnabled) => upperLimitCallbackEnabled = isEnabled;
        
        [SerializeField] private bool lowerLimitCallbackEnabled = true;
        public void EnableLowerLimitCallback(bool isEnabled) => lowerLimitCallbackEnabled = isEnabled;
        
        [SerializeField] private UnityEvent onUpperLimitReached;

        public UnityEvent OnUpperLimitReached
        {
            get => onUpperLimitReached;
            set => onUpperLimitReached = value;
        }

        [SerializeField] private UnityEvent onLowerLimitReached;

        public UnityEvent OnLowerLimitReached
        {
            get => onLowerLimitReached;
            set => onLowerLimitReached = value;
        }

        private Vector3 initialPosition;
        private float grabOffset;
        private bool upperLimitReachedEventRaised;
        private bool lowerLimitReachedEventRaised;

        public Transform FollowTransform { get; protected set; }

#if UNITY_EDITOR
        private MeshRenderer _meshRenderer;
#endif
        private void Start()
        {
            // initialPosition = transform.position;
            initialPosition = transform.localPosition;
#if UNITY_EDITOR
            _meshRenderer = GetComponent<MeshRenderer>();
#endif
            DisplayPullIndicator(false);
        }

        private void Update()
        {
            if (!FollowTransform)
            {
#if UNITY_EDITOR
                _meshRenderer.material.color = Color.gray;
#endif
                return;
            }
#if UNITY_EDITOR
            _meshRenderer.material.color = Color.magenta;
#endif

            var followPosLocal = transform.parent.InverseTransformPoint(FollowTransform.position);

            switch (pullAxis)
            {
                case Axis.X:

                    // if (PullLimitReached(FollowTransform.position.x + grabOffset, initialPosition.x))
                    //     return;

                    if (PullLimitReached(followPosLocal.x + grabOffset, initialPosition.x))
                        return;

                    upperLimitReachedEventRaised = false;
                    lowerLimitReachedEventRaised = false;
                    
                    // transform.position = new Vector3(FollowTransform.position.x + grabOffset, transform.position.y,
                    //     transform.position.z);

                    transform.localPosition = new Vector3(followPosLocal.x + grabOffset, transform.localPosition.y,
                        transform.localPosition.z);

                    break;
                case Axis.Y:
                    // if (PullLimitReached(FollowTransform.position.y + grabOffset, initialPosition.y))
                    //     return;
                    //
                    // transform.position = new Vector3(transform.position.x, FollowTransform.position.y + grabOffset,
                    //     transform.position.z);

                    if (PullLimitReached(followPosLocal.y + grabOffset, initialPosition.y))
                        return;
                    
                    upperLimitReachedEventRaised = false;
                    lowerLimitReachedEventRaised = false;

                    transform.localPosition = new Vector3(transform.localPosition.x, followPosLocal.y + grabOffset,
                        transform.localPosition.z);
                    break;
                case Axis.Z:
                    if (PullLimitReached(followPosLocal.z + grabOffset, initialPosition.z))
                        return;
                    
                    upperLimitReachedEventRaised = false;
                    lowerLimitReachedEventRaised = false;

                    transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y,
                        followPosLocal.z + grabOffset);
                    break;
            }
        }

        private bool PullLimitReached(float followPosAxisValue, float initialPosAxisValue)
        {
            if (followPosAxisValue > initialPosAxisValue + upperLimit)
            {
                if (upperLimitReachedEventRaised)
                    return true;

                if (upperLimitCallbackEnabled)
                    onUpperLimitReached?.Invoke();

                upperLimitReachedEventRaised = true;

                return true;
            }

            if (followPosAxisValue < initialPosAxisValue - lowerLimit)
            {
                if (lowerLimitReachedEventRaised)
                    return true;

                if (lowerLimitCallbackEnabled)
                    onLowerLimitReached?.Invoke();

                lowerLimitReachedEventRaised = true;

                return true;
            }

            return false;
        }

        public void Attach(Transform nextFollowTransform)
        {
            if (FollowTransform)
                return;

            FollowTransform = nextFollowTransform;
            DisplayPullIndicator(true);

            if (SnapOnGrab)
                return;

            // var followPosition = FollowTransform.position;

            var followPosLocal = transform.parent.InverseTransformPoint(FollowTransform.position);

            switch (pullAxis)
            {
                case Axis.X:
                    grabOffset = transform.localPosition.x - followPosLocal.x;
                    break;
                case Axis.Y:
                    grabOffset = transform.localPosition.y - followPosLocal.y;
                    break;
                case Axis.Z:
                    grabOffset = transform.localPosition.z - followPosLocal.z;
                    break;
            }
        }

        public void Detach()
        {
            if (!FollowTransform)
                return;

            FollowTransform = null;
            DisplayPullIndicator(false);

            upperLimitReachedEventRaised = false;
            lowerLimitReachedEventRaised = false;

            if (ResetPositionOnRelease)
            {
                ResetPositionToInitial();
            }
        }

        public void ResetPositionToInitial() => transform.localPosition = initialPosition;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            if (!Application.isPlaying)
            {
                initialPosition = transform.localPosition;
            }

            var transformParent = transform.parent;
            if (transformParent == null)
            {
                switch (pullAxis)
                {
                    case Axis.X:
                        Gizmos.DrawLine(initialPosition, initialPosition + new Vector3(upperLimit, 0f, 0f));
                        Gizmos.DrawLine(initialPosition, initialPosition - new Vector3(lowerLimit, 0f, 0f));
                        break;
                    case Axis.Y:
                        Gizmos.DrawLine(initialPosition, initialPosition + new Vector3(0f, upperLimit, 0f));
                        Gizmos.DrawLine(initialPosition, initialPosition - new Vector3(0f, lowerLimit, 0f));
                        break;
                    case Axis.Z:
                        Gizmos.DrawLine(initialPosition, initialPosition + new Vector3(0f, 0f, upperLimit));
                        Gizmos.DrawLine(initialPosition, initialPosition - new Vector3(0f, 0f, lowerLimit));
                        break;
                }
            }
            else
            {
                switch (pullAxis)
                {
                    case Axis.X:
                        Gizmos.DrawLine(transformParent.TransformPoint(initialPosition),
                            transformParent.TransformPoint(initialPosition + new Vector3(upperLimit, 0f, 0f)));
                        Gizmos.DrawLine(transformParent.TransformPoint(initialPosition),
                            transformParent.TransformPoint(initialPosition - new Vector3(lowerLimit, 0f, 0f)));

                        break;
                    case Axis.Y:
                        Gizmos.DrawLine(transform.parent.TransformPoint(initialPosition),
                            transformParent.TransformPoint(initialPosition + new Vector3(0f, upperLimit, 0f)));
                        Gizmos.DrawLine(transformParent.TransformPoint(initialPosition),
                            transformParent.TransformPoint(initialPosition - new Vector3(0f, lowerLimit, 0f)));
                        break;
                    case Axis.Z:
                        Gizmos.DrawLine(transformParent.TransformPoint(initialPosition),
                            transformParent.TransformPoint(initialPosition + new Vector3(0f, 0f, upperLimit)));
                        Gizmos.DrawLine(transformParent.TransformPoint(initialPosition),
                            transformParent.TransformPoint(initialPosition - new Vector3(0f, 0f, lowerLimit)));
                        break;
                }
            }
        }

        private void DisplayPullIndicator(bool display)
        {
            if (PullIndicator)
                PullIndicator.SetActive(display);
        }
    }
}