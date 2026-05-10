using haw.pd20.webxr;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteInEditMode]
public class SelectionHandler : MonoBehaviour
{
    public WebXRInteractable CurrentInteractable { get; private set; } = null;

    public bool CheckForNewInteractable()
    {
        // First see if we have an interactable to use
        WebXRInteractable  newInteractable = GetInteractable();

        // Update if different
        bool isDifferent = IsDifferentInteractable(CurrentInteractable, newInteractable);
        CurrentInteractable = isDifferent ? newInteractable : CurrentInteractable;

        return isDifferent;
    }

    private WebXRInteractable  GetInteractable()
    {
        // Set up the stuff now
        WebXRInteractable  newInteractable = null;
        GameObject selectedObject = null;

        #if UNITY_EDITOR
        selectedObject = Selection.activeGameObject;
        #endif

        // If we have an object selected
        if (selectedObject)
        {
            // Does it have an interactable component
            if (selectedObject.TryGetComponent(out WebXRInteractable interactable))
                newInteractable = interactable;
        }

        return newInteractable;
    }

    private bool IsDifferentInteractable(WebXRInteractable  currentInteractable, WebXRInteractable  newInteractable)
    {
        // Assume it's the same
        var isDifferent = !currentInteractable;
        
        // bool isDifferent = false;
        //
        // // If we're selecting on object for the first time, it's true
        // if (!CurrentInteractable)
        //     isDifferent = true;

        // If we're selecting on object for the first time, it's true

        // If we have a stored object, and we select a new one
        if (currentInteractable && newInteractable)
            isDifferent = currentInteractable != newInteractable;

        return isDifferent;
    }

    public GameObject SetObjectPose(Pose pose)
    {
        GameObject selectedObject = null;

        #if UNITY_EDITOR
        selectedObject = Selection.activeGameObject;
        #endif

        if (selectedObject)
        {
            // Check if the object has a container to put a grabPose into
            if (selectedObject.TryGetComponent(out PoseContainer poseContainer))
            {
                // Set the grabPose, mark as dirty to save
                poseContainer.ActiveEditorPose = pose;

                // Mark scene for saving
                MarkActiveSceneAsDirty();
            }
        }

        return selectedObject;
    }

    public Pose TryGetPose(GameObject targetObject)
    {
        Pose pose = null;

        if (targetObject)
        {
            // Check if the object has a container to take a grabPose from
            if (targetObject.TryGetComponent(out PoseContainer poseContainer))
                pose = poseContainer.ActiveEditorPose;
        }

        return pose;
    }

    private void MarkActiveSceneAsDirty()
    {
        #if UNITY_EDITOR
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        #endif
    }
}
