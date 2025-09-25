using UltimateFramework.SoundSystem;
//using EasyTransition;
using UnityEngine;

public class SceneManagerController : MonoBehaviour
{
    [Header("Transition")]
    //public TransitionSettings transitionSettings;
    public Animator menuCanvasAnimator;
    public float startDelay;

    [Header("Sound")]
    [SerializeField] private float musicFadeDuration;
    [SerializeField] private SoundsFadeManager backgroundMusic;

    public void LoadScene(int sceneIndex)
    {
        menuCanvasAnimator.SetBool("Exit", true);
        StartCoroutine(backgroundMusic.FadeOut(backgroundMusic.AudioSource, musicFadeDuration, 0, true));
        //TransitionManager.Instance.Transition(sceneIndex, transitionSettings, startDelay);
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}