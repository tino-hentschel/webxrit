using UnityEngine;

namespace haw.pd20.webxr
{
    /// <summary>
    /// An interactor that casts a <c>Ray</c> outwards from the users eye position that interacts with (OnEnter / OnExit) <see cref="GazeInteractable"/>s. Enabling the user to Select / Deselect <see cref="GazeInteractable"/>s by looking / not looking at them.
    /// </summary>
    public class GazeInteractor : MonoBehaviour
    {
        [SerializeField] private Transform WebGlGazeInteractorParent;
        [SerializeField] private Transform EditorGazeInteractorParent;
        
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private float maxDetectionDistance;
        
        private GazeInteractable gazeInteractable;

        private void Start()
        {
            transform.position = EditorGazeInteractorParent.position;
            transform.rotation = EditorGazeInteractorParent.rotation;
            transform.parent = EditorGazeInteractorParent;

            if (Application.platform != RuntimePlatform.WebGLPlayer) 
                return;
            
            transform.position = WebGlGazeInteractorParent.position;
            transform.rotation = WebGlGazeInteractorParent.rotation;
            transform.parent = WebGlGazeInteractorParent;
        }

        private void Update() => UpdateGazeInteractor();

        private void UpdateGazeInteractor()
        {
            var ray = new Ray(transform.position, transform.forward);

            if (Physics.Raycast(ray, out var hit, maxDetectionDistance, layerMask))
            {
#if UNITY_EDITOR
                Debug.DrawLine(ray.origin, hit.point, Color.green);
#endif
                if (!gazeInteractable)
                {
                    // CASE 1: Gaze Raycast goes from No GameObject to a GameObject on the 'Gaze' LayerMask.
                    GazeEnter(hit.collider.gameObject);
                }
                else if (!gazeInteractable.gameObject.Equals(hit.transform.gameObject))
                {
                    // CASE 2: Gaze Raycast goes from a 'Gaze' GameObject to a different 'Gaze' GameObject.
                    GazeExit();
                }
            }
            else if (gazeInteractable)
            {
                // CASE 3: Gaze Raycast goes from a 'Gaze' GameObject to No GameObject.
                GazeExit();
            }
#if UNITY_EDITOR
            else
            {
                Debug.DrawLine(ray.origin, ray.direction * maxDetectionDistance, Color.red);
            }
#endif
        }

        private void GazeEnter(GameObject gazeGameObject)
        {
            var currentGazeInteractable = gazeGameObject.GetComponent<GazeInteractable>();
            if (!currentGazeInteractable)
                return;

            gazeInteractable = currentGazeInteractable;
            gazeInteractable.GazeEnter();
        }

        private void GazeExit()
        {
            gazeInteractable.GazeExit();
            gazeInteractable = null;
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if(Application.isPlaying)
                return;
            
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, transform.forward * maxDetectionDistance);
        }
#endif
    }
}