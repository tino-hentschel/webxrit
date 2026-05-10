using haw.pd20.tasksystem;
using haw.pd20.webxr;
using UnityEngine;

namespace haw.pd20.training_modules
{
    /// <summary>
    /// Handles the button input of the controllers (e.g. Help Button) and provides a base to display UI components (e.g. Hand Interaction Canvas) at the player hand.
    /// </summary>
    public class HandInterface : MonoBehaviour
    {
        [SerializeField] private WebXRDirectInteractor directInteractor;
        [SerializeField] private ModuleGraph graph;
        [SerializeField] private HandInterface otherHandInterface;
        [SerializeField] private HandInteractionCanvas handInteractionCanvas;

        public bool HelpActive { get; private set; }

        private void Start()
        {
            handInteractionCanvas.Hide();
        }

        private void OnEnable()
        {
            directInteractor.OnButtonADown += OnHelpButtonDown;
            directInteractor.OnButtonAUp += OnHelpButtonUp;
            // TODO MuC Code
            graph.OnGraphFinished += OnGraphFinished;
        }

        private void OnDisable()
        {
            directInteractor.OnButtonADown -= OnHelpButtonDown;
            directInteractor.OnButtonAUp -= OnHelpButtonUp;
            // TODO MuC Code
            graph.OnGraphFinished -= OnGraphFinished;
        }

        private void Update()
        {
            if (directInteractor.HasInteractable)
            {
                if(!handInteractionCanvas.IsHidden)
                    handInteractionCanvas.Hide();
                
                return;
            }

            switch (directInteractor.CurrentInReach)
            {
                case WebXRDirectInteractor.InReach.Interactable when directInteractor.NearestInteractable.DisplayInteractionText:
                    handInteractionCanvas.Show(directInteractor.NearestInteractable.InteractionText);
                    break;
                case WebXRDirectInteractor.InReach.PullInteractor:
                    handInteractionCanvas.Show(directInteractor.NearestPullInteractor.InteractionText);
                    break;
                
                default:
                    handInteractionCanvas.Hide();
                    break;
            }
        }

        private void OnHelpButtonDown()
        {
            if (otherHandInterface.HelpActive)
                return;

            HelpActive = true;
            graph.ActivateHelp();
        }

        private void OnHelpButtonUp()
        {
            graph.DeactivateHelp();
            HelpActive = false;
        }
        
        // TODO MuC Code
        private void OnGraphFinished()
        {
            directInteractor.OnButtonADown -= OnHelpButtonDown;
        }
    }
}