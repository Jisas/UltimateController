using UltimateFramework.StatisticsSystem;
using UltimateFramework.ItemSystem;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class ItemBehaviour : MonoBehaviour
{
    #region Publics
    //Base
    public string itemName;

    // States
    public List<ItemStatesInfo> statesList;
    #endregion

    #region Protected
    protected List<ItemStateSO> InternalStateList = new();
    protected GameObject owner;
    #endregion

    #region Properties
    public GameObject Owner 
    {  
        get { return owner; } 
        set { owner = value; }
    }

    /// <summary xml:lang="es">
    /// Referencia al item actual.
    /// </summary>
    /// /// <summary xml:lang="en">
    /// Reference to current item.
    /// </summary>
    public abstract Item Item { get; protected set; }

    /// <summary xml:lang="es">
    /// Referencia a la base de datos de items.
    /// </summary>
    /// /// <summary xml:lang="en">
    /// Reference to item database.
    /// </summary>
    protected ItemDatabase ItemDB { get; set; }


    /// <summary xml:lang="es">
    /// Referencia al estado actual.
    /// </summary>
    /// /// <summary xml:lang="en">
    /// Reference to current status.
    /// </summary>
    public ItemStateSO CurrentState { get; protected set; }

    /// <summary xml:lang="es">
    /// Referencia al estado pasado.
    /// </summary>
    /// /// <summary xml:lang="en">
    /// Reference to last status.
    /// </summary>
    public ItemStateSO LastState { get; protected set; }

    /// <summary xml:lang="es">
    /// Referencia a la tabla de transición.
    /// </summary>
    /// /// <summary xml:lang="en">
    /// Reference to the transition table.
    /// </summary>
    public TransitionTable TransitionTable { get; protected set; }
    #endregion

    // --------- METHODS --------- //

    #region Upgrades
    public virtual void MakeUpgrade() { }
    #endregion

    #region Scaling
    public void SetUpScaling(StatisticsComponent statisticsComponent)
    {
        if (Item.Scaled.Count > 0 && statisticsComponent != null)
        {
            foreach (var scaled in Item.Scaled)
            {
                scaled.operation.SubscribeToAttributes(statisticsComponent);
            }
        }
    }
    #endregion

    #region States
    /// <summary xml:lang="es">
    /// Metodo para encontrar un estado en la lista de estados.
    /// </summary>
    /// /// <summary xml:lang="en">
    /// Method to find a state in the list of states.
    /// </summary>
    public ItemStateSO FindState(string name)
    {
        foreach (var state in InternalStateList)
            if (state.StateName == name) 
                return state;

        return null;
    }
    public ItemStateSO FindState(int index)
    {
        foreach (var state in InternalStateList)
            if (InternalStateList.IndexOf(state) == index)
                return state;

        return null;
    }

    /// <summary xml:lang="es">
    /// Método para procesar un evento y ver si hay que cambiar de estado.
    /// </summary>
    /// /// <summary xml:lang="en">
    /// Method to process an event and see if the status needs to be changed.
    /// </summary>
    public void ProcessEvent(ItemStateSO currentState)
    {
        // The valid transition is obtained according to the current state and the event received.
        Transition transition = TransitionTable.GetTransition(currentState);

        // If there is a valid transition, the state is changed
        if (transition != null) SwitchState(transition);
    }

    /// <summary xml:lang="es">
    /// Método para cambiar estado deacuerdo a la transicion.
    /// </summary>
    /// /// <summary xml:lang="en">
    /// Method to change state according to a transition.
    /// </summary>
    protected abstract void SwitchState(Transition transition);

    /// <summary xml:lang="es">
    /// Metodo que asigna un valor a el unitmo estado en el que estubo la maquina de estados
    /// </summary>
    /// /// <summary xml:lang="en">
    /// Method that assigns a value to the last state the state machine was in.
    /// </summary>
    public void SetLastState(ItemStateSO state) => LastState = state;

    // Method that assigns the necessary initial values to a state.
    private ItemStateSO SetUpState(ItemStateSO state)
    {
        string stateName = state.name.Replace("(Clone)", "").Trim();
        state.SetStateName(stateName);
        state.Transitions = new();
        return state;
    }

    // Method that assigns the states assigned in the public list to an internal list of the system.
    private void SetUpStatesList()
    {
        foreach (var stateSO in statesList)
        {
            var state = SetUpState(stateSO.stateAsset.Clone());
            InternalStateList.Add(state);
        }
    }

    protected void SetUpTransitions()
    {
        if (InternalStateList != null)
            TransitionTable = new(InternalStateList);
    }
    #endregion

    #region Unity Flow
    public virtual void Awake()
    {
        ItemDB = Resources.Load<ItemDatabase>("Data/ItemDB/ItemsDatabase");
        SetUpStatesList();        
    }

    public virtual void Start() => SetUpTransitions();

    public abstract void Update();
    #endregion
}

/// <summary xml:lang="es">
/// Clase que contiene la informacion de los estados.
/// </summary>
/// /// <summary xml:lang="en">
/// Class containing the information of the states.
/// </summary>
[Serializable]
public class ItemStatesInfo
{
    public bool useAnimatorStateName = false;
    public ItemStateSO stateAsset;
    public string animatorStateName;
}