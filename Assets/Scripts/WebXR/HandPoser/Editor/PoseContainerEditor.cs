#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PoseContainer))]
public class PoseContainerEditor : Editor
{
    private PoseContainer poseContainer = null;

    private SerializedProperty grabPoseProperty;
    private SerializedProperty triggerPoseProperty;
    
    private void OnEnable()
    {
        poseContainer = (PoseContainer)target;
        
        grabPoseProperty = serializedObject.FindProperty("grabPose");
        triggerPoseProperty = serializedObject.FindProperty("triggerPose");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        GUILayout.BeginHorizontal("box");

        EditorGUILayout.PropertyField(grabPoseProperty);
        
        if (GUILayout.Button("Open Pose Editor"))
            PoseWindow.Open(poseContainer.GrabPose);

        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal("box");

        EditorGUILayout.PropertyField(triggerPoseProperty);
        
        if (GUILayout.Button("Open Pose Editor"))
            PoseWindow.Open(poseContainer.TriggerPose);

        GUILayout.EndHorizontal();
        
        serializedObject.ApplyModifiedProperties();
    }
}

#endif
