using UnityEngine;
using UnityEngine.Events;

namespace haw.pd20.webxr
{
    /// <summary>
    /// Interactable that can be selected / deselected by the raycast of a <see cref="GazeInteractor"/>.
    /// </summary>
    public class GazeInteractable : MonoBehaviour
    {
        [SerializeField] private UnityEvent onGazeEnter;
        [SerializeField] private UnityEvent onGazeExit;

        // Select - User looks at the GazeInteractable
        public void GazeEnter()
        {
            onGazeEnter?.Invoke();
        }

        // Deselect - User looks away from the GazeInteractable
        public void GazeExit()
        {
            onGazeExit?.Invoke();
        }
    }
}