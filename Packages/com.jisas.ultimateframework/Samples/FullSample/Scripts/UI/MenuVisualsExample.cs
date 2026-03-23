using System.Collections.Generic;
using UltimateFramework.UISystem;
using UltimateFramework.Inputs;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine;
using System;

public class MenuVisualsExample : RuntimeVisualsBase
{
    #region Serialized Fields
    [Header("Buttons")]
    public Color focusIn;
    public Color focusOut;
    #endregion

    #region Porperties
    #endregion

    #region Actions
    public static Action OpenInventory;
    public static Action OpenEquipment;
    #endregion

    #region Private Fields
    private static MenuVisualsExample instance;
    private Dictionary<string, Action> menuButtonsDictionary;
    private bool isOpen;
    #endregion

    #region Sigleton
    private MenuVisualsExample() { }
    public static MenuVisualsExample Instance
    {
        get
        {
            if (instance == null) instance = new();
            return instance;
        }
    }
    #endregion

    #region Mono
    private void OnEnable()
    {
        InputsManager.Player.OpenMenu.performed += InputHandler;

        UIDoc.enabled = true;
        root = UIDoc.rootVisualElement;
        OnHide();

        menuButtonsDictionary = new Dictionary<string, Action>()
        {
            {"inventory", OnInventory},
            {"equipment", OnEquipment}
        };

        SetUpButtons();
    }
    private void OnDisable()
    {
        InputsManager.Player.OpenMenu.performed -= InputHandler;
    }
    #endregion

    #region Callbacks
    private void InputHandler(InputAction.CallbackContext input)
    {
        if (!input.performed) return;
        if (!isOpen) OnShow();
        else OnHide();
        isOpen = !isOpen;
    }
    #endregion

    #region Internal
    private void SetUpButtons()
    {
        foreach (KeyValuePair<string, Action> entry in menuButtonsDictionary)
        {
            var button = FindElementInRoot<Button>(root, entry.Key);
            button.clickable.clicked += entry.Value;
        }
    }
    private void OnInventory()
    {
        OnHide();
        OpenInventory?.Invoke();
    }
    private void OnEquipment()
    {
        OnHide();
        OpenEquipment?.Invoke();
    }
    #endregion
}