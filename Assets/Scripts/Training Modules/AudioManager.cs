using haw.pd20.tasksystem;
using UnityEngine;

namespace haw.pd20.training_modules
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private ModuleGraph graph;

        [SerializeField] private AudioClip successClip;
        [SerializeField] private AudioClip errorClip;

        private AudioSource audioSrc;

        private void Awake()
        {
            audioSrc = GetComponent<AudioSource>();

            graph.OnErrorsOccured += OnErrorsOccured;
            graph.OnTaskCompleted += OnTaskCompleted;
            graph.OnClusterCompleted += OnClusterCompleted;
        }

        private void OnDisable()
        {
            graph.OnErrorsOccured -= OnErrorsOccured;
            graph.OnTaskCompleted -= OnTaskCompleted;
            graph.OnClusterCompleted -= OnClusterCompleted;
        }

        private void OnErrorsOccured(params ErrorData[] errors)
        {
            audioSrc.PlayOneShot(errorClip);
        }

        private void OnTaskCompleted(TaskNode task)
        {
            audioSrc.PlayOneShot(successClip);
        }

        private void OnClusterCompleted(TaskClusterNode cluster)
        {
            audioSrc.PlayOneShot(successClip);
        }
    }
}