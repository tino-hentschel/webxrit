using haw.pd20.webxr;
using UnityEngine;
using WebXR;

public class GameplayHand : BaseHand
{
    [SerializeField] private WebXRDirectInteractor targetInteractor;
    [SerializeField] private Animator animator;

    private PoseContainer currentPoseContainer;
    
    private void OnEnable()
    {
        // Subscribe to selected events
        targetInteractor.onSelectEntered.AddListener(TryApplyObjectPoseContainer);
        targetInteractor.onSelectExited.AddListener(TryApplyDefaultPose);
    }

    private void OnDisable()
    {
        // Unsubscribe to selected events
        targetInteractor.onSelectEntered.RemoveListener(TryApplyObjectPoseContainer);
        targetInteractor.onSelectExited.RemoveListener(TryApplyDefaultPose);
    }

    private void Update()
    {
        UpdateHandAnimation();
    }
    
    private void UpdateHandAnimation()
    {
        if(!animator || !animator.enabled)
            return;
        
        // We could also use an int cast from the enum ButtonTypes but since this is the update method a direct input as integers might save a bit of performance.
        // Trigger = 0,
        // Grip = 1,

        animator.SetFloat("Trigger", targetInteractor.Controller.GetButtonIndexValue(0));
        animator.SetFloat("Grip", targetInteractor.Controller.GetButtonIndexValue(1));
        
        // handAnimator.SetFloat("Trigger",
        //     targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue) ? triggerValue : 0.0f);
        //
        // handAnimator.SetFloat("Grip",
        //     targetDevice.TryGetFeatureValue(CommonUsages.grip, out float gripValue) ? gripValue : 0.0f);
    }

    private void TryApplyObjectPoseContainer(WebXRInteractable interactable)
    {
        // disable the animator to let the grabPose container control the joints of the hand
        if (interactable.GrabType == GrabType.KinematicPose)
            animator.enabled = false;

        // Try and get grabPose container, and apply
        if (!interactable.TryGetComponent(out PoseContainer poseContainer)) 
            return;
        
        currentPoseContainer = poseContainer;
        ApplyPose(currentPoseContainer.GrabPose);
        interactable.onActivate.AddListener(TryApplyTriggerPose);
        interactable.onDeactivate.AddListener(TryApplyGrabPose);
    }

    private void TryApplyDefaultPose(WebXRInteractable interactable)
    {
        if (interactable.GrabType == GrabType.KinematicPose)
            animator.enabled = true;

        // Try and get grabPose container, and apply
        if (!interactable.TryGetComponent(out PoseContainer poseContainer)) 
            return;

        ApplyDefaultPose();
        currentPoseContainer = null;
        interactable.onActivate.RemoveListener(TryApplyTriggerPose);
        interactable.onDeactivate.RemoveListener(TryApplyGrabPose);
    }

    private void TryApplyGrabPose(WebXRDirectInteractor directInteractor)
    {
        if(currentPoseContainer && currentPoseContainer.GrabPose)
            ApplyPose(currentPoseContainer.GrabPose);
    }
    
    private void TryApplyTriggerPose(WebXRDirectInteractor directInteractor)
    {
        if(currentPoseContainer && currentPoseContainer.TriggerPose)
            ApplyPose(currentPoseContainer.TriggerPose);
    }

    public override void ApplyPose(Pose pose)
    {
        //Get the proper info using hand's type
        HandInfo handInfo = pose.GetHandInfo(handType);

        // Apply rotations 
        ApplyFingerRotations(handInfo.fingerRotations);

        var scaledAttachPosition = handInfo.attachPosition;
        var inverseLocalScale = handInfo.handLocalScale;
        inverseLocalScale = new Vector3(1f / inverseLocalScale.x, 1f / inverseLocalScale.y, 1f / inverseLocalScale.z);
        scaledAttachPosition.Scale(inverseLocalScale);

        // Position, and rotate, this differs on the type of hand
        ApplyOffset(scaledAttachPosition, handInfo.attachRotation);
    }

    protected override void ApplyOffset(Vector3 position, Quaternion rotation)
    {
        // Invert since the we're moving the attach point instead of the hand
        Vector3 finalPosition = position * -1.0f;
        Quaternion finalRotation = Quaternion.Inverse(rotation);

        // Since it's a local position, we can just rotate around zero
        finalPosition = finalPosition.RotatePointAroundPivot(Vector3.zero, finalRotation.eulerAngles);

        // Set the position and rotach of attach
        targetInteractor.AttachTransform.localPosition = finalPosition;
        targetInteractor.AttachTransform.localRotation = finalRotation;
    }

    private void OnValidate()
    {
        // Let's have this done automatically, but not hide the requirement
        if (!targetInteractor)
            targetInteractor = GetComponentInParent<WebXRDirectInteractor>();
        
        if (!animator)
            animator = GetComponentInChildren<Animator>();
    }
}