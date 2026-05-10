using System.Collections;
using System.Collections.Generic;
using haw.pd20.webxr;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace haw.pd20.training_modules
{
    public class HoverInteractorHand : MonoBehaviour
    {
        private enum ActivationType
        {
            OneHand,
            BothHands
        }

        public Canvas canvas;
        public Image loadingImage;

        [SerializeField] private float activationTime = 1.0f;
        [SerializeField] private float delayTime = 0.0f;

        [SerializeField] private ActivationType activationType;
        // [SerializeField] private HandAppearanceState requiredHandAppearanceState;

        [SerializeField] private List<HandState> allowedHandStates;

        [SerializeField] private bool activateWithInteractableInHand;
        [SerializeField] private UnityEvent OnHoverCompleted;

        private bool hovering;
        private string handTag = "Hand";
        private List<GameObject> hands = new List<GameObject>();

        void Awake()
        {
            canvas.gameObject.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag(handTag))
                return;

            if (!allowedHandStates.Contains(other.gameObject.GetComponentInChildren<PlayerHandModel>().HandState))
                return;
            // if (requiredHandAppearanceState !=
            //     other.gameObject.GetComponentInChildren<PlayerHandModel>().HandAppearanceState)
            //     return;
            if (!activateWithInteractableInHand &&
                other.gameObject.GetComponent<WebXRDirectInteractor>().HasInteractable)
                return;

            hands.Add(other.gameObject);

            switch (activationType)
            {
                case ActivationType.OneHand when hands.Count == 1:
                    StartCoroutine(DoHover());
                    break;

                case ActivationType.BothHands when hands.Count == 2:
                    StartCoroutine(DoHover());
                    break;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.gameObject.CompareTag(handTag))
                return;

            // if(!allowedHandStates.Contains(other.gameObject.GetComponentInChildren<PlayerHandModel>().HandState))
            //     return;

            // if (requiredHandAppearanceState !=
            //     other.gameObject.GetComponentInChildren<PlayerHandModel>().HandAppearanceState)
            //     return;

            hands.Remove(other.gameObject);

            if (!hovering)
                return;

            switch (activationType)
            {
                case ActivationType.OneHand when hands.Count == 0:
                    StopHover();
                    break;

                case ActivationType.BothHands:
                    StopHover();
                    break;
            }
        }

        private IEnumerator DoHover()
        {
            hovering = true;

            yield return new WaitForSeconds(delayTime);

            canvas.gameObject.SetActive(true);
            var currentActivationTime = loadingImage.fillAmount = 0.0f;

            while (currentActivationTime < activationTime)
            {
                currentActivationTime += Time.deltaTime;
                loadingImage.fillAmount = currentActivationTime / activationTime;
                yield return null;
            }

            OnHoverCompleted?.Invoke();
            canvas.gameObject.SetActive(false);

            hovering = false;
        }

        private void StopHover()
        {
            StopAllCoroutines();
            hovering = false;
            canvas.gameObject.SetActive(false);
        }
    }
}