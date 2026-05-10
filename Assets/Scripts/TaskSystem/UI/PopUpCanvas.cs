using System.Collections;
using UnityEngine;

namespace haw.pd20.tasksystem.ui
{
    public class PopUpCanvas : MonoBehaviour
    {
        [SerializeField] private ModuleGraph graph;
        [SerializeField] private MsgPopUp msgPopUp;
        // TODO MuC Code
        [SerializeField] private MsgPopUp msgPopUpEnd;
        
        [SerializeField] private string helpMsgTitle;
        [SerializeField] private string errorMsgTitle;
        [SerializeField] private string taskCompletedMsgTitle;
        
        [SerializeField] private Color helpColor;
        [SerializeField] private Color errorColor;
        [SerializeField] private Color taskCompletedColor;

        [SerializeField] private float taskCompletedDisplayTime;
        [SerializeField] private float errorDisplayTime;

        private bool popUpDisplayTimerActive;
        
        // TODO helper variables for MuC Demo. Stats should be handled in a dedicated class.
        private int totalErrorCount;
        private int totalHelpCount;
        
        private void OnEnable()
        {
            // UPDATED
            // graph.OnStepUpdated += OnStepUpdated;
            // graph.OnActivityUpdated += OnActivityUpdated;
            // graph.OnClusterUpdated += OnClusterUpdated;
            graph.OnTaskCompleted += OnTaskCompleted;

            // ERROR
            graph.OnErrorsOccured += OnErrorsOccured;

            // HELP
            graph.OnHelpActivated += OnHelpActivated;
            graph.OnHelpDeactivated += OnHelpDeactivated;

            // End
            graph.OnGraphFinished += OnGraphFinished;
        }

        private void OnDisable()
        {
            graph.OnTaskCompleted -= OnTaskCompleted;
            graph.OnErrorsOccured -= OnErrorsOccured;
            graph.OnHelpActivated -= OnHelpActivated;
            graph.OnHelpDeactivated -= OnHelpDeactivated;
            graph.OnGraphFinished -= OnGraphFinished;
        }

        private void Start()
        {
            msgPopUp.SetTitle("");
            msgPopUp.SetMessage("");
        }

        public void SetPositionAndRotation(Transform targetTransform)
        {
            transform.position = targetTransform.position;
            transform.rotation = targetTransform.rotation;
        }
        
        private void ClosePopUpSetValues(string title, string msg, Color color)
        {
            if (msgPopUp.IsOpen)
            {
                StopAllCoroutines();
                msgPopUp.Close();
                popUpDisplayTimerActive = false;
            }
            
            msgPopUp.SetTitle(title);
            msgPopUp.SetMessage(msg);
            msgPopUp.SetHeaderColor(color);
        }
        
        private IEnumerator DisplayPopUpForTime(float duration)
        {
            msgPopUp.OpenAnim();
            popUpDisplayTimerActive = true;
            yield return new WaitForSeconds(duration);
            msgPopUp.CloseAnim();
            popUpDisplayTimerActive = false;
        }

        private void OnTaskCompleted(TaskNode task)
        {
            ClosePopUpSetValues(taskCompletedMsgTitle, task.Name, taskCompletedColor);
            StartCoroutine(DisplayPopUpForTime(taskCompletedDisplayTime));
        }

        private void OnErrorsOccured(params ErrorData[] errorData)
        {
            var errorMsg = "";
            foreach (var error in errorData)
            {
                totalErrorCount++;
                
                switch (error.Type)
                {
                    case ErrorType.Default:
                        errorMsg += error.Message + "\n";
                        break;
                    // case ErrorType.Execution:
                    //     break;
                    case ErrorType.Sequence:
                        var taskName = error.Task ? error.Task.Name : "";
                        errorMsg += "Der Arbeitsschritt \"" + taskName + "\" ist nicht an der Reihe.";
                        break;
                    case ErrorType.Set:
                        errorMsg += "Falsch ausgeführt: " + error.Task.Name + "\n";
                        break;
                    // case ErrorType.WrongCluster:
                    //     break;
                    default:
                        errorMsg += error.Message + "\n";
                        break;
                }
            }

            ClosePopUpSetValues(errorMsgTitle, errorMsg, errorColor);
            StartCoroutine(DisplayPopUpForTime(errorDisplayTime));
        }
        
        private void OnHelpActivated(HelpNode helpNode)
        {
            if (!helpNode)
                return;
            
            ClosePopUpSetValues(helpMsgTitle, helpNode.Message, helpColor);
            msgPopUp.OpenAnim();
            totalHelpCount++;
        }

        private void OnHelpDeactivated()
        {
            if(!msgPopUp.IsOpen)
                return;
            
            if(popUpDisplayTimerActive)
                return;
            
            msgPopUp.CloseAnim();
        }

        private void OnGraphFinished()
        {
            // Todo MuC Demo Code
            // graph.OnTaskCompleted -= OnTaskCompleted;
            graph.OnErrorsOccured -= OnErrorsOccured;
            graph.OnHelpActivated -= OnHelpActivated;
            
            // ClosePopUpSetValues("Training abgeschlossen", msg, taskCompletedColor);
            
            StopAllCoroutines();
            msgPopUp.Close();
            popUpDisplayTimerActive = true;
            
            var msg = "Vielen Dank für Ihre Teilnahme.\n\nZeit: " + graph.Duration.ToString(@"mm\:ss") + "\nFehler: " +
                      totalErrorCount + "\nHilfen: " + totalHelpCount;
            
            msgPopUpEnd.SetTitle("Training abgeschlossen");
            msgPopUpEnd.SetMessage(msg);
            msgPopUpEnd.SetHeaderColor(taskCompletedColor);
            msgPopUpEnd.OpenAnim();
        }
    }
}