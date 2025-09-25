using UltimateFramework.Inputs;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Events;
using UnityEngine;
using TMPro;

[System.Serializable]
public struct DialogueLines
{
    [TextArea(5,10)] public string line;
    public UnityEvent onChangeLine;
}

public class DialogueManager : MonoBehaviour
{
    #region Public Fields
    public PlayerInput playerInput;
    public EntityActionInputs playerEntityInputs;
    public TextMeshProUGUI dialogueText;
    public GameObject dialoguePanel;
    [Space] public float typingSpeed = 0.05f;
    #endregion

    #region Private Fields
    private DialogueLines[] dialogueLines;
    private int currentLineIndex = 0;
    private bool isDialogueActive;
    #endregion

    #region Properties
    public static DialogueManager Instance { get; private set; }
    public bool IsDialogueActive => isDialogueActive;
    protected bool IsCurrentDeviceMouse
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return InputsManager.GetIsCurrentDiviceMouse(playerInput);
#else
            return false;
#endif
        }
    }
    #endregion

    #region Mono
    private void Awake() => Instance = this;
    void Start() => dialoguePanel.SetActive(false);
    void Update()
    {
        if (dialoguePanel.activeSelf && InputsManager.UI.Submit.WasPressedThisFrame())
        {
            if (dialogueText.text == dialogueLines[currentLineIndex].line)
            {
                dialogueLines[currentLineIndex].onChangeLine.Invoke();
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                dialogueText.text = dialogueLines[currentLineIndex].line;
            }
        }
    }
    #endregion

    #region Internal
    IEnumerator TypeLine()
    {
        dialogueText.text = "";
        foreach (char letter in dialogueLines[currentLineIndex].line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
    void NextLine()
    {
        if (currentLineIndex < dialogueLines.Length - 1)
        {
            currentLineIndex++;
            StartCoroutine(TypeLine());
        }
        else
        {
            InputsManager.SwitchToPlayer(playerEntityInputs, IsCurrentDeviceMouse);
            dialoguePanel.SetActive(false);
            isDialogueActive = false;
        }
    }
    #endregion

    #region Public Methods
    public void StartDialogue(DialogueLines[] lines)
    {
        isDialogueActive = true;
        InputsManager.EnablePlayerMap(false);
        InputsManager.SwitchToUI(playerEntityInputs, IsCurrentDeviceMouse);

        dialogueLines = lines;
        currentLineIndex = 0;
        dialoguePanel.SetActive(true);
        StartCoroutine(TypeLine());
    }
    #endregion
}
