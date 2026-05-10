using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using XNode;

namespace haw.pd20.tasksystem
{
    [CreateAssetMenu]
    public class ModuleGraph : NodeGraph
    {
        #region Delegates

        public delegate void TaskCompletedDelegate(TaskNode task);

        public delegate void ClusterUpdatedDelegate(TaskClusterNode cluster);

        public delegate void ClusterCompletedDelegate(TaskClusterNode cluster);

        public delegate void ActivityUpdatedDelegate(ActivityNode activity);

        public delegate void ActivityCompletedDelegate(ActivityNode activity);

        public delegate void StepUpdatedDelegate(StepNode step);

        public delegate void StepCompletedDelegate(StepNode step);

        public delegate void TaskSetRemainingDelegate(IEnumerable<TaskNode> openTasks);

        public delegate void ErrorsOccuredDelegate(params ErrorData[] errors);

        public delegate void HelpActivatedDelegate(HelpNode help);

        public delegate void HelpDeactivatedDelegate();

        public delegate void GraphFinishedDelegate();

        public delegate void GraphInitDelegate();

        #endregion

        #region Events

        public event TaskCompletedDelegate OnTaskCompleted;
        public event ClusterUpdatedDelegate OnClusterUpdated;
        public event ClusterCompletedDelegate OnClusterCompleted;
        public event ActivityUpdatedDelegate OnActivityUpdated;
        public event ActivityCompletedDelegate OnActivityCompleted;
        public event StepUpdatedDelegate OnStepUpdated;
        public event StepCompletedDelegate OnStepCompleted;
        public event TaskSetRemainingDelegate OnTaskSetRemaining;
        public event ErrorsOccuredDelegate OnErrorsOccured;
        public event HelpActivatedDelegate OnHelpActivated;
        public event HelpDeactivatedDelegate OnHelpDeactivated;
        public event GraphFinishedDelegate OnGraphFinished;
        public event GraphInitDelegate OnGraphInitialized;

        #endregion

        public StepNode Step { get; private set; }

        [NonSerialized] private readonly List<StepNode> stepsInOrder = new List<StepNode>();
        public ReadOnlyCollection<StepNode> StepsInOrder => stepsInOrder.AsReadOnly();

        // TODO We use this fields to make the data easier to access for evaluation. Could also be stored in the nodes (steps) themselves.
        public DateTime StartDateTime { get; private set; }

        public DateTime EndDateTime { get; private set; }

        // TODO a Duration Interface might be useful
        public TimeSpan Duration => EndDateTime.Subtract(StartDateTime);

        public void Init()
        {
            foreach (var node in nodes)
            {
                var startNode = node as StartNode;
                if (startNode)
                {
                    var startPort = startNode.GetPort("Out");
                    if (startPort.IsConnected)
                    {
                        var tempStep = Step = (StepNode) startPort.Connection.node;
                        Step.Activity.TaskCluster.Reset();
                        Step.Activity.TaskCluster.StartDateTime = StartDateTime = DateTime.Now;
                        RegisterTaskExecutionListener();
                        OnStepUpdated?.Invoke(Step);

                        // TODO This is the same that happens in the init() of the StepNode. We can do better.
                        stepsInOrder.Add(tempStep);
                        while (tempStep.HasNextStep())
                        {
                            tempStep = tempStep.GetNextStep();
                            stepsInOrder.Add(tempStep);
                        }

                        OnGraphInitialized?.Invoke();

                        break;
                    }
                }
            }
        }

        public void RaiseDefaultError(string errorMessage)
        {
            var defaultErrorData = new ErrorData(errorMessage);

            OnErrorsOccured?.Invoke(defaultErrorData);

            if (Step.Activity.TaskCluster is SequenceTaskClusterNode seqCluster)
            {
                seqCluster.CurrentTask.RaiseErrorEvent();
            }

            Step.RaiseErrorEvent();
        }

        private void OnTaskExecutionError(TaskNode task, string errorMessage)
        {
            var executionErrorData = new ErrorData(ErrorType.Execution, task, errorMessage);

            if (Step.Activity.TaskCluster.Tasks.Contains(task))
            {
                OnErrorsOccured?.Invoke(executionErrorData);
                Step.Activity.TaskCluster.AddErrorData(executionErrorData);
            }
            else
            {
                var wrongClusterErrorData = new ErrorData(ErrorType.WrongCluster, task);
                OnErrorsOccured?.Invoke(executionErrorData, wrongClusterErrorData);
                Step.Activity.TaskCluster.AddErrorData(executionErrorData);
            }

            task.RaiseErrorEvent();
            Step.RaiseErrorEvent();
        }

        private void OnTaskExecuted(TaskNode task)
        {
            var cluster = Step.Activity.TaskCluster;

            if (!cluster.Tasks.Contains(task))
            {
                var wrongClusterErrorData = new ErrorData(ErrorType.WrongCluster, task);
                OnErrorsOccured?.Invoke(wrongClusterErrorData);
                task.RaiseErrorEvent();
                Step.RaiseErrorEvent();
                // TODO This works but it is not elegant. The error propagation itself is too error prone.
                Step.Activity.TaskCluster.AddErrorData(wrongClusterErrorData);
                return;
            }

            switch (cluster)
            {
                case SequenceTaskClusterNode seqCluster:
                    ProcessSequenceCluster(seqCluster, task);
                    break;
                case SetTaskClusterNode setCluster:
                    ProcessSetCluster(setCluster, task);
                    break;
            }
        }

        private bool OnCheckIsCurrentTask(TaskNode task)
        {
            var cluster = Step.Activity.TaskCluster;

            if (!cluster.Tasks.Contains(task))
                return false;

            switch (cluster)
            {
                case SequenceTaskClusterNode seqCluster:
                    if (task == seqCluster.CurrentTask)
                        return true;
                    break;
                case SetTaskClusterNode setCluster:
                    if (task.Status == TaskStatus.Open)
                        return true;
                    break;
            }

            return false;
        }

        private void ProcessSequenceCluster(SequenceTaskClusterNode seqCluster, TaskNode task)
        {
            var requiredTask = seqCluster.CurrentTask;

            if (requiredTask == task)
            {
                task.Status = TaskStatus.Completed;
                // TODO TASK COMPLETED
                OnTaskCompleted?.Invoke(task);
                DeactivateHelp();
                seqCluster.CurrentTask.RaiseCompletedEvent();

                if (seqCluster.HasOpenTasks())
                {
                    seqCluster.UpdateTask();
                }
                else
                {
                    // TODO CLUSTER COMPLETED
                    CompleteCluster(seqCluster);
                }
            }
            else
            {
                // TODO TASK SEQUENCE ERROR
                // requiredTask.RaiseErrorEvent();

                task.RaiseErrorEvent();
                // TODO Executed Task in Message
                var errorData = new ErrorData(ErrorType.Sequence, task, requiredTask.Name);// "Required Task: " + requiredTask.Name);
                OnErrorsOccured?.Invoke(errorData);
                Step.RaiseErrorEvent();
                Step.Activity.TaskCluster.AddErrorData(errorData);
                // OnTaskSequenceError?.Invoke(errorData, task);
            }
        }

        private void ProcessSetCluster(SetTaskClusterNode setCluster, TaskNode task)
        {
            if (task.Status == TaskStatus.Completed)
            {
                OnTaskCompleted?.Invoke(task);
                DeactivateHelp();
                task.RaiseCompletedEvent();
            }

            if (setCluster.HasOpenTasks())
            {
                // TODO TASKS IN SET REMAINING
                OnTaskSetRemaining?.Invoke(setCluster.Tasks.Where(t => t.Status == TaskStatus.Open));
            }
            else if (setCluster.Tasks.All(t => t.Status == TaskStatus.Completed))
            {
                // TODO CLUSTER COMPLETED
                CompleteCluster(setCluster);
            }
            else if (setCluster.Tasks.Any(t => t.Status == TaskStatus.Failed))
            {
                var failedTasks = setCluster.Tasks.Where(t => t.Status == TaskStatus.Failed).ToList();

                var errorData = new ErrorData[failedTasks.Count];
                for (int i = 0; i < failedTasks.Count; i++)
                {
                    var failedTask = failedTasks[i];
                    failedTask.RaiseErrorEvent();
                    errorData[i] = new ErrorData(ErrorType.Set, failedTask);
                }

                // TODO TASK SET ERROR
                OnErrorsOccured?.Invoke(errorData);
                Step.RaiseErrorEvent();
                Step.Activity.TaskCluster.AddErrorData(errorData);
            }
            else
            {
                foreach (var defaultStatusTasK in setCluster.Tasks.Where(t => t.Status == TaskStatus.Default))
                {
                    // log which tasks have the default state
                    Debug.LogError("Task [" + defaultStatusTasK.Name + "] in Cluster [" + setCluster.Name +
                                   "] has a Default Status.");
                }
            }
        }

        private void CompleteCluster(TaskClusterNode cluster)
        {
            cluster.EndDateTime = DateTime.Now;
            OnClusterCompleted?.Invoke(cluster);
            cluster.RaiseCompletedEvent();
            UpdateGraph();
        }

        private void UpdateGraph()
        {
            UnregisterTaskExecutionListener();

            if (Step.Activity.HasNextCluster())
            {
                Step.Activity.UpdateCluster();

                // TODO CLUSTER UPDATED
                OnClusterUpdated?.Invoke(Step.Activity.TaskCluster);
            }
            else
            {
                // TODO ACTIVITY COMPLETED
                OnActivityCompleted?.Invoke(Step.Activity);
                Step.Activity.RaiseCompletedEvent();

                if (Step.Activity.HasNextActivity())
                {
                    Step.UpdateActivity();
                    // TODO ACTIVITY UPDATED
                    OnActivityUpdated?.Invoke(Step.Activity);
                }
                else
                {
                    // TODO STEP COMPLETED
                    OnStepCompleted?.Invoke(Step);
                    Step.RaiseCompletedEvent();

                    if (Step.HasNextStep())
                    {
                        Step = Step.GetNextStep();
                        Step.Activity.TaskCluster.StartDateTime = DateTime.Now;
                        // TODO STEP UPDATED
                        OnStepUpdated?.Invoke(Step);
                    }
                    else
                    {
                        // TODO -> END OF GRAPH | MODULE COMPLETED
                        UnregisterTaskExecutionListener();
                        EndDateTime = DateTime.Now;
                        OnGraphFinished?.Invoke();
                    }
                }
            }

            RegisterTaskExecutionListener();
        }

        public void ActivateHelp()
        {
            // TODO 
            // Currently we only show the information from the help node that is connected with the current action node deepest in the module graph.
            // This event and the GetHelpNode methods could be adjusted to return all Help nodes relevant for the current state of the graph (using the params keyword). 
            OnHelpActivated?.Invoke(Step.GetHelpNode());
            Step.RaiseHelpActivatedEvent();
            Step.Activity.TaskCluster.HelpCount++;
        }

        public void DeactivateHelp()
        {
            OnHelpDeactivated?.Invoke();
            Step.RaiseHelpDeactivatedEvent();
        }

        private HashSet<TaskNode> CreateTaskNodeSetFromCurrentActivity()
        {
            var taskNodeSet = new HashSet<TaskNode>();

            if (Step.Activity.RegisterTaskListenersInAllClusters)
            {
                foreach (var taskCluster in Step.Activity.OpenTaskClustersInOrder)
                {
                    foreach (var taskNode in taskCluster.Tasks)
                    {
                        taskNodeSet.Add(taskNode);
                    }
                }
            }
            else
            {
                taskNodeSet = new HashSet<TaskNode>(Step.Activity.TaskCluster.Tasks);
            }

            return taskNodeSet;
        }

        private void RegisterTaskExecutionListener() =>
            RegisterTaskExecutionListener(CreateTaskNodeSetFromCurrentActivity());

        private void RegisterTaskExecutionListener(HashSet<TaskNode> taskNodeSet)
        {
            foreach (var task in new HashSet<TaskNode>(taskNodeSet))
            {
                task.OnNotifyExecutionListeners += OnTaskExecuted;
                task.OnNotifyExecutionErrorListeners += OnTaskExecutionError;
                task.OnCheckIsCurrentTaskListeners += OnCheckIsCurrentTask;
            }
        }

        private void UnregisterTaskExecutionListener() =>
            UnregisterTaskExecutionListener(CreateTaskNodeSetFromCurrentActivity());

        private void UnregisterTaskExecutionListener(HashSet<TaskNode> taskNodeSet)
        {
            foreach (var task in new HashSet<TaskNode>(taskNodeSet))
            {
                task.OnNotifyExecutionListeners -= OnTaskExecuted;
                task.OnNotifyExecutionErrorListeners -= OnTaskExecutionError;
                task.OnCheckIsCurrentTaskListeners -= OnCheckIsCurrentTask;
            }
        }
    }
}