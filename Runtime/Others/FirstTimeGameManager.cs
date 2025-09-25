using UltimateFramework.LocomotionSystem;
using UltimateFramework.InventorySystem;
using UltimateFramework.Inputs;
using System.Collections;
using UnityEngine;
using UltimateFramework.SerializationSystem;
using UnityEngine.Events;

public class FirstTimeGameManager : MonoBehaviour
{
    #region Serialized Fields
    [Header("Config")]
    [SerializeField] private float startDelay;

    [Header("References")]
    [SerializeField] private GameObject player;

    [Header("Dialogue")]
    public DialogueLines[] dialogueLines;

    [Header("Events")]
    public UnityEvent OnFirstTime;
    public UnityEvent OnDataSaved;
    #endregion

    #region Private Fields
    private InventoryAndEquipmentComponent inventoryAndEquipment;
    private BaseLocomotionComponent locomotionComponent;
    private EntityActionInputs playerInputs;
    private DialogueManager dialogueManager;
    private Animator playerAnimator;
    #endregion

    #region Mono
    void Start()
    {
        inventoryAndEquipment = player.GetComponent<InventoryAndEquipmentComponent>();
        locomotionComponent = player.GetComponent<BaseLocomotionComponent>();
        playerInputs = player.GetComponent<EntityActionInputs>();
        playerAnimator = player.GetComponent<Animator>();
        dialogueManager = DialogueManager.Instance;

        if(!DataGameManager.IsDataSaved())
        {
            playerAnimator.SetBool("GameInit", true);
            locomotionComponent.CanMove = false;
            InputsManager.EnablePlayerMap(false);
            OnFirstTime?.Invoke();
            StartCoroutine(InitGame());
        }
        else
        {
            playerInputs.SwitchToPlayer();
            InputsManager.EnablePlayerMap(true);
            OnDataSaved?.Invoke();
        }
    }
    #endregion

    #region Internal
    private IEnumerator InitGame()
    {
        yield return new WaitForSeconds(startDelay);

        dialogueManager.StartDialogue(dialogueLines);
        while (dialogueManager.IsDialogueActive)
        {
            yield return null;
        }

        playerAnimator.SetTrigger("Getup");
        var animLength = playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;

        yield return new WaitForSeconds(1.5f);

        var inputAction = playerInputs.FindInputAction("Equip");
        if (inputAction == null)
        {
            Debug.LogError("There is no input action with the name: Equip");
            yield break;
        }

        var tag = inputAction.PrimaryAction.actionTag.tag;
        var priority = inputAction.PrimaryAction.priority;
        var isBaseAction = inputAction.PrimaryAction.isBaseAction;

        inputAction.ExecuteAction(tag, priority, isBaseAction);
        animLength = playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        yield return new WaitForSeconds(animLength);

        InputsManager.EnablePlayerMap(true);
        locomotionComponent.CanMove = true;
    }
    #endregion
}
