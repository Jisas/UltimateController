using UnityEngine.Events;
using UnityEngine;

public class CustomAnimationEvent : UnityEvent<string> { }

public class AnimationEvents : MonoBehaviour
{
    public CustomAnimationEvent animationEvent = new();

    public void OnAnimationEvent(string eventName)
    {
        animationEvent.Invoke(eventName);
    }
}
