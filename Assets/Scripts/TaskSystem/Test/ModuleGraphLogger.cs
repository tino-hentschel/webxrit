using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace haw.pd20.tasksystem
{
    public class ModuleGraphLogger : MonoBehaviour
    {
        [SerializeField] private ModuleGraph graph;

        private void Start()
        {
            Debug.Log("Graph Log started: " + graph.StartDateTime);

            // COMPLETED
            graph.OnStepCompleted += OnStepCompleted;
            graph.OnActivityCompleted += OnActivityCompleted;
            graph.OnClusterCompleted += OnClusterCompleted;
            graph.OnTaskCompleted += OnTaskCompleted;

            // UPDATED
            graph.OnStepUpdated += OnStepUpdated;
            graph.OnActivityUpdated += OnActivityUpdated;
            graph.OnClusterUpdated += OnClusterUpdated;

            // ERROR
            graph.OnErrorsOccured += OnErrorsOccured;

            // OTHER
            graph.OnTaskSetRemaining += OnTaskSetRemaining;
            graph.OnGraphFinished += OnGraphFinished;
        }

        #region Completed Callbacks

        private void OnStepCompleted(StepNode step)
        {
            Debug.Log("Step [ " + step.Name + " ] completed | Duration: " + step.Duration.ToString(@"mm\:ss"));
        }

        private void OnActivityCompleted(ActivityNode activity)
        {
            Debug.Log("Activity [ " + activity.Name + " ] completed | Duration: " + activity.Duration.ToString(@"mm\:ss"));
        }

        private void OnClusterCompleted(TaskClusterNode cluster)
        {
            Debug.Log("Cluster [ " + cluster.Name + " ] completed | Duration: " + cluster.Duration.ToString(@"mm\:ss"));
        }

        private void OnTaskCompleted(TaskNode task)
        {
            Debug.Log("Task [ " + task.Name + " ] completed");
        }

        #endregion


        #region Updated Callbacks

        private void OnStepUpdated(StepNode step)
        {
            Debug.Log("Step [ " + step.Name + " ] updated");
        }

        private void OnActivityUpdated(ActivityNode activity)
        {
            Debug.Log("Activity [ " + activity.Name + " ] updated");
        }

        private void OnClusterUpdated(TaskClusterNode cluster)
        {
            Debug.Log("Cluster [ " + cluster.Name + " ] updated");
        }

        #endregion


        #region Error Callbacks

        private void OnErrorsOccured(params ErrorData[] errorData)
        {
            foreach (var error in errorData)
            {
                var taskName = error.Task ? error.Task.Name : "null";
                Debug.Log("ERROR [ Type: " + error.Type + " | Task: " + taskName + " | Date: " + error.DateTime +
                          " | Message: " + error.Message + "]");
            }
        }

        #endregion

        #region Other Callbacks

        private void OnTaskSetRemaining(IEnumerable<TaskNode> remainingTasks)
        {
            var remainingTasksStr = remainingTasks.Aggregate("", (current, task) => current + (task.Name + ", "));

            Debug.Log("SET - Tasks Remaining [ " + remainingTasksStr + "]");
        }

        private void OnGraphFinished()
        {
            Debug.Log("Graph Log finished: " + graph.EndDateTime + " | Duration: " + graph.Duration.ToString(@"mm\:ss"));
        }

        #endregion
    }
}