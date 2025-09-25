using UnityEngine.Rendering.Universal;
using UltimateFramework.Inputs;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using System.Collections;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    #region Public And Serialized Fields
    [Header("References")]
    [SerializeField] private EntityActionInputs entityInputs;
    [SerializeField] private Animator cinemachineAnimator;
    [SerializeField] private PlayerInput playerInputs;
    [SerializeField] private Volume volume;

    [Header("Dialogue")]
    public DialogueLines[] firstSectionLines;
    public DialogueLines[] secondSectionLines;
    public DialogueLines[] tirdSectionLines;
    public DialogueLines[] fourthSectionLines;
    public DialogueLines[] fifthSectionLines;
    public DialogueLines[] sixthSectionLines;
    public DialogueLines[] seventhSectionLines;
    #endregion

    #region Private Fields
    private DialogueManager dialogueManager;
    private DepthOfField depthOfField;
    #endregion

    #region Properties
    private bool IsCurrentDeviceMouse
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return InputsManager.GetIsCurrentDiviceMouse(playerInputs);
#else
            return false;
#endif
        }
    }
    #endregion

    #region Mono
    private void Start() => dialogueManager = DialogueManager.Instance;
    #endregion

    #region Public Methods
    public void EnableDephtOfFieldHandler()
    {
        if (volume.profile.TryGet<DepthOfField>(out depthOfField))
            depthOfField.active = true;
    }
    public void DisableDephtOfFieldHandler()
    {
        if (volume.profile.TryGet<DepthOfField>(out depthOfField))
            depthOfField.active = false;
    }
    public void SetTutorialCameraEvent(bool value)
    {
        if (value) cinemachineAnimator.Play("TutorialPractice");
        else cinemachineAnimator.Play("FollowCamera");
    }
    public void StartTutorialHandler() => StartCoroutine(StartFirstSectionTutorial());
    public void StartSeconSectionTutorialHandler() => dialogueManager.StartDialogue(secondSectionLines);
    public void StartTirdSectionTutorialHandler() => dialogueManager.StartDialogue(tirdSectionLines);
    public void StartFourthSectionTutorialHandler() => dialogueManager.StartDialogue(fourthSectionLines);
    public void StartFifthSectionTutorialHandler() => dialogueManager.StartDialogue(fifthSectionLines);
    public void StartSixthSectionTutorialHandler() => dialogueManager.StartDialogue(sixthSectionLines);
    public void StartSeventhSectionTutorialHandler() => dialogueManager.StartDialogue(seventhSectionLines);
    #endregion

    #region Internal
    private IEnumerator StartFirstSectionTutorial()
    {
        yield return new WaitForSeconds(.5f);
        dialogueManager.StartDialogue(firstSectionLines);
    }
    #endregion
}
