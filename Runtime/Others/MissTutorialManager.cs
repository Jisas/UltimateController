using UltimateFramework.Inputs;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine;

public class MissTutorialManager : MonoBehaviour
{
    #region Public And Serialized Fields
    [Header("References")]
    [SerializeField] private EntityActionInputs entityInputs;
    [SerializeField] private PlayerInput playerInputs;

    [Header("Dialogue")]
    public DialogueLines[] dialogueLines;
    #endregion

    #region Private Fields
    private DialogueManager dialogueManager;
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
    public void StartMissTutorialHandler() => dialogueManager.StartDialogue(dialogueLines);
    #endregion
}
