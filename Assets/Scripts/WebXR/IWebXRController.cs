using UnityEngine;

namespace haw.pd20.webxr
{
    public enum ControllerHand {
        Left,
        Right
    }
    
    /// <summary>
    /// Interface that provides access to controller input (buttons, axis) and output (vibration) for interactables.
    /// </summary>
    public interface IWebXRController
    {
        public ControllerHand GetHand();
        
        public Vector2 ThumbstickAxis();

        public void Vibrate(float intensity, float durationMilliseconds);
    }
}