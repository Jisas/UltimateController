using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollManager : MonoBehaviour
{
    public LayerMask excludeLayer;
    private Rigidbody[] rbs;

    void Start()
    {
        rbs = GetComponentsInChildren<Rigidbody>();
        DesactiveRagDoll();
    }

    public void DesactiveRagDoll()
    {
        foreach (var rb in rbs)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.None;
            rb.excludeLayers = excludeLayer;
        }
    }
    public void ActiveRagdoll()
    {
        foreach (var rb in rbs)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.None;
            rb.excludeLayers = excludeLayer;
        }
    }
}
