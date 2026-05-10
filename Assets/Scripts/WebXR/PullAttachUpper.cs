using UnityEngine;

namespace haw.pd20.webxr
{
    [RequireComponent(typeof(PullInteractor))]
    public class PullAttachUpper : MonoBehaviour
    {
        [SerializeField] private WebXRInteractable InteractablePrefab;
        [SerializeField] private bool isActive = true;
        
        private PullInteractor pullInteractor;

        private void Awake()
        {
            pullInteractor = GetComponent<PullInteractor>();
            pullInteractor.OnUpperLimitReached.AddListener(TryAttach);
        }

        private void TryAttach()
        {
            if(!isActive)
                return;
            
            var interactable = Instantiate(InteractablePrefab, pullInteractor.transform.position, Quaternion.identity);
            pullInteractor.FollowTransform.GetComponent<WebXRDirectInteractor>().ExternalAttach(interactable);
        }

        public void SetActive(bool active)
        {
            isActive = active;
        }
    }
}