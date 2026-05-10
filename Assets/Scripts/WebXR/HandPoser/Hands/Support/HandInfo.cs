using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HandInfo
{
    public Vector3 attachPosition = Vector3.zero;
    public Quaternion attachRotation = Quaternion.identity;
    public Vector3 handLocalScale = Vector3.one;
    public List<Quaternion> fingerRotations = new List<Quaternion>();

    public static HandInfo Empty => new HandInfo();

    public void Save(PreviewHand hand)
    {
        // Save position and rotation
        attachPosition = hand.transform.localPosition;
        attachRotation = hand.transform.localRotation;
        
        // save local scale (in case the interactable has been scaled which also scales the preview hand and therefore affects the attach position of the game hand) 
        handLocalScale = hand.transform.localScale;

        // Save rotations from the hand's current joints
        fingerRotations = hand.GetJointRotations();
    }
}