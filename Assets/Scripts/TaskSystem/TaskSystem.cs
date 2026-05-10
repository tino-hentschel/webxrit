using haw.pd20.events;
using UnityEngine;
using UnityEngine.Events;

namespace haw.pd20.tasksystem
{
    public class TaskSystem : MonoBehaviour
    {
        [SerializeField] private ModuleGraph moduleGraph;

        [SerializeField] private UnityEvent onGraphInitialized;
        [SerializeField] private UnityEvent onApplicationStarted;
        [SerializeField] private UnityEvent onGraphFinished;

        private void Awake()
        {
            moduleGraph.Init();

            moduleGraph.OnGraphInitialized += OnGraphInitialized;
            moduleGraph.OnGraphFinished += OnGraphFinished;

            EventManager.Instance.AddListener<DefaultErrorEvent>(OnDefaultError);
            
            
        }

        private void Start() => onApplicationStarted?.Invoke();

        private void OnGraphInitialized() => onGraphInitialized?.Invoke();
        private void OnGraphFinished() => onGraphFinished?.Invoke();

        private void OnDefaultError(DefaultErrorEvent e)
        {
            moduleGraph.RaiseDefaultError(e.ErrorMessage);
        }
    }
}