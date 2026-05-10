using UnityEngine;
using UnityEngine.Serialization;

public class PoseContainer : MonoBehaviour
{
    [SerializeField] [FormerlySerializedAs("pose")]
    private Pose grabPose = null;

    [SerializeField] private Pose triggerPose = null;

    public enum PoseType
    {
        Grab,
        Trigger
    }
    
    private PoseType editorPoseType;
    public PoseType EditorPoseType => editorPoseType;

    public Pose ActiveEditorPose
    {
        get
        {
            return EditorPoseType switch
            {
                PoseType.Grab => grabPose,
                PoseType.Trigger => triggerPose,
                _ => null
            };
        }

        set
        {
            switch (EditorPoseType)
            {
                case PoseType.Grab:
                    grabPose = value;
                    break;
                
                case PoseType.Trigger:
                    triggerPose = value;
                    break;
            }
        }
    }
    
    public Pose GrabPose
    {
        get
        {
            editorPoseType = PoseType.Grab;
            return grabPose;
        }
    }

    public Pose TriggerPose
    {
        get
        {
            editorPoseType = PoseType.Trigger;
            return triggerPose;
        }
    }
}