using haw.pd20.tasksystem;
using TMPro;
using UnityEngine;

public class DebugCanvas : MonoBehaviour
{
    public TextMeshProUGUI StepLabel;
    public TextMeshProUGUI ActivityLabel;
    public TextMeshProUGUI ClusterLabel;
    public TextMeshProUGUI TaskCompletedLabel;
    public TextMeshProUGUI ErrorLabel;
    public TextMeshProUGUI HelpLabel;
    public TextMeshProUGUI ResultLabel;

    [SerializeField] private ModuleGraph graph;


    private int totalErrors = 0;
    private int totalHelpCount = 0;

    private void Awake()
    {
        TaskCompletedLabel.gameObject.SetActive(false);
        HelpLabel.gameObject.SetActive(false);
        ErrorLabel.gameObject.SetActive(false);
        ResultLabel.gameObject.SetActive(false);

        // UPDATED
        graph.OnStepUpdated += OnStepUpdated;
        graph.OnActivityUpdated += OnActivityUpdated;
        graph.OnClusterUpdated += OnClusterUpdated;

        graph.OnTaskCompleted += OnTaskCompleted;

        // ERROR
        graph.OnErrorsOccured += OnErrorsOccured;

        // HELP
        graph.OnHelpActivated += OnHelpActivated;
        graph.OnHelpDeactivated += OnHelpDeactivated;

        // End
        graph.OnGraphFinished += OnGraphFinished;
    }

    private void Start()
    {
        StepLabel.text = "Schritt: " + graph.Step.Name;
        ActivityLabel.text = "Aktivität: " + graph.Step.Activity.Name;
        ClusterLabel.text = "Cluster: " + graph.Step.Activity.TaskCluster.Name;
    }


    private void OnStepUpdated(StepNode step)
    {
        StepLabel.text = "Schritt: " + step.Name;
        ActivityLabel.text = "Aktivität: " + graph.Step.Activity.Name;
        ClusterLabel.text = "Cluster: " + graph.Step.Activity.TaskCluster.Name;

        TaskCompletedLabel.gameObject.SetActive(false);
        HelpLabel.gameObject.SetActive(false);
        ErrorLabel.gameObject.SetActive(false);
    }

    private void OnActivityUpdated(ActivityNode activity)
    {
        ActivityLabel.text = "Aktivität: " + activity.Name;
        ClusterLabel.text = "Cluster: " + graph.Step.Activity.TaskCluster.Name;

        TaskCompletedLabel.gameObject.SetActive(false);
        HelpLabel.gameObject.SetActive(false);
        ErrorLabel.gameObject.SetActive(false);
    }

    private void OnClusterUpdated(TaskClusterNode cluster)
    {
        ClusterLabel.text = "Cluster: " + cluster.Name;

        TaskCompletedLabel.gameObject.SetActive(false);
        HelpLabel.gameObject.SetActive(false);
        ErrorLabel.gameObject.SetActive(false);
    }

    private void OnTaskCompleted(TaskNode task)
    {
        TaskCompletedLabel.gameObject.SetActive(true);
        ErrorLabel.gameObject.SetActive(false);
        TaskCompletedLabel.text = "Task completed: " + task.Name;
    }

    private void OnErrorsOccured(params ErrorData[] errorData)
    {
        TaskCompletedLabel.gameObject.SetActive(false);
        ErrorLabel.gameObject.SetActive(true);

        var errorStr = "Fehler:\n";
        foreach (var error in errorData)
        {
            var taskName = error.Task ? error.Task.Name : "null";

            errorStr += "ERROR [ Type: " + error.Type + " | Task: " + taskName + " | Date: " + error.DateTime +
                        " | Message: " + error.Message + "]\n";
        }

        ErrorLabel.text = errorStr;
    }

    private void OnHelpActivated(HelpNode helpNode)
    {
        if (!helpNode)
            return;

        HelpLabel.gameObject.SetActive(true);
        HelpLabel.text = helpNode.Message;
    }

    private void OnHelpDeactivated()
    {
        HelpLabel.gameObject.SetActive(true);
        HelpLabel.text = "";
    }

    private void OnGraphFinished()
    {
        var dataStr = "Ergebnis:\n\n";
        var stepCount = 1;

        foreach (var step in graph.StepsInOrder)
        {
            dataStr += "Schritt: " + stepCount + " " + step.Name + " | Zeit: " + step.Duration.ToString(@"mm\:ss") +
                       "\n";

            var activities = step.ActivitiesInOrder;

            if (activities.Count > 1)
            {
                foreach (var activity in activities)
                {
                    dataStr += "Aktivität: " + activity.Name + " | " + GetTimeErrorHelpDataStr(activities[0]) + "\n";
                }
            }
            else
            {
                dataStr += GetTimeErrorHelpDataStr(activities[0]) + "\n";
            }

            stepCount++;
        }

        dataStr += "Gesamt - Zeit: " + graph.Duration.ToString(@"mm\:ss") + " | Fehler: " + totalErrors +
                   " | Hilfen: " + totalHelpCount;


        StepLabel.gameObject.SetActive(false);
        ActivityLabel.gameObject.SetActive(false);
        ClusterLabel.gameObject.SetActive(false);
        HelpLabel.gameObject.SetActive(false);
        ErrorLabel.gameObject.SetActive(false);
        TaskCompletedLabel.gameObject.SetActive(false);
        ResultLabel.gameObject.SetActive(true);
        ResultLabel.text = dataStr;
    }

    private string GetTimeErrorHelpDataStr(ActivityNode activityNode)
    {
        var str = "";
        var errorCount = 0;
        var helpCount = 0;
        foreach (var cluster in activityNode.TaskClustersInOrder)
        {
            errorCount += cluster.Errors.Count;
            helpCount += cluster.HelpCount;
        }

        totalErrors += errorCount;
        totalHelpCount += helpCount;

        str += "Zeit: " + activityNode.Duration.ToString(@"mm\:ss") + " | Fehler: " + errorCount + " | Hilfen: " +
               helpCount +
               "\n";

        return str;
    }
}