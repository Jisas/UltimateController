using UnityEngine.Events;
using UnityEngine;

public class Triggerer : MonoBehaviour
{
    [SerializeField] private string colliderTag;
    [Space, SerializeField] private UnityEvent onTriggerEnter;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(colliderTag))
            onTriggerEnter.Invoke();
    }
}
