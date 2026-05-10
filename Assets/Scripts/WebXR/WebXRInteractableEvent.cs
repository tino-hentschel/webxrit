using System;
using UnityEngine.Events;

namespace haw.pd20.webxr
{
    // TODO: Check why we are not using WebXRBaseInteractor as type?
    [Serializable]
    public class WebXRInteractableEvent : UnityEvent<WebXRDirectInteractor>
    {
    }    
}