using System.Collections.Generic;
using UltimateFramework.Utils;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using System;

namespace UltimateFramework.ItemSystem
{
    /// <summary xml:lang="es">
    /// Clase base de un estado
    /// </summary>
    /// /// <summary xml:lang="en">
    /// Base class of a state
    /// </summary>
    public abstract class ItemStateSO : ScriptableObject
    {
        public Image icon;
        public GameObject prefab;

        public ItemStateSO Clone() => Instantiate(this);

        /// <summary xml:lang="es">
        /// Evento que debe ser llamado en la funcion ChangeVisual().
        /// </summary>
        /// /// <summary xml:lang="en">
        /// Event that must be called in the ChangeVisual() function.
        /// </summary>
        [Space(3)] public Action OnChangeVisual;

        /// <summary xml:lang="es">
        /// Nombre del estado.
        /// </summary>
        /// /// <summary xml:lang="en">
        /// State name.
        /// </summary>
        public string StateName { get; private set; }

        /// <summary xml:lang="es">
        /// Flujo del estado.
        /// </summary>
        /// /// <summary xml:lang="en">
        /// State flow.
        /// </summary>
        protected StateFlow stateFlow;

        /// <summary xml:lang="es">
        /// Lista de transiciones posibles desde este estado.
        /// </summary>
        /// /// <summary xml:lang="en">
        /// List of possible transitions from this state.
        /// </summary>
        public List<Transition> Transitions { get; set; }

        /// <summary xml:lang="es">
        /// Método para añadir una transición a la lista.
        /// </summary>
        /// /// <summary xml:lang="en">
        /// Method to add a transition to the list.
        /// </summary>
        public void AddTransition(Transition transition)
        {
            Transitions.Add(transition);
        }

        /// <summary xml:lang="es">
        /// Método para definir el nombre del estado.
        /// </summary>
        /// /// <summary xml:lang="en">
        /// Method for defining the state name
        /// </summary>
        public void SetStateName(string name)
        {
            StateName = name;
        }

        /// <summary xml:lang="es">
        /// Método para añadir la logica al entrar en un estado. Se debe ejecutar una sola vez.
        /// </summary>
        /// /// <summary xml:lang="en">
        /// Method to add logic when entering a state. To be executed only once.
        /// </summary>
        public virtual void StateStart(ItemBehaviour machine) { }
        public virtual void StateStart(ArmorBehaviour machine) { }
        public virtual void StateStart(WeaponBehaviour machine) { }

        /// <summary xml:lang="es">
        /// Método para añadir la logica al actualizar un estado. Se debe ejecutar una vez por cada frame.
        /// </summary>
        /// /// <summary xml:lang="en">
        /// Method to add logic when updating a state. It must be executed once for each frame.
        /// </summary>
        public virtual void StateUpdate(ItemBehaviour machine) { }
        public virtual void StateUpdate(ArmorBehaviour machine) { }
        public virtual void StateUpdate(WeaponBehaviour machine) { }
    }
}
