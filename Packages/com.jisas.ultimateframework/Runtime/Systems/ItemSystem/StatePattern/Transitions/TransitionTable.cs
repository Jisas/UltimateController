using System.Collections.Generic;
using UnityEngine;

namespace UltimateFramework.ItemSystem
{
    /// <summary xml:lang="es">
    /// Clase que representa una tabla de transición.
    /// </summary>
    /// /// <summary xml:lang="en">
    /// Class representing a transition table.
    /// </summary>
    public class TransitionTable
    {
        /// <summary xml:lang="es">
        /// Diccionario que asocia cada estado con sus transiciones.
        /// </summary>
        /// /// <summary xml:lang="en">
        /// Dictionary that associates each state with its transitions.
        /// </summary>
        public Dictionary<ItemStateSO, List<Transition>> Table { get; private set; }

        /// <summary xml:lang="es">
        /// Constructor que recibe una lista de estados.
        /// </summary>
        /// /// <summary xml:lang="en">
        /// Constructor that receives a list of states.
        /// </summary>
        public TransitionTable(List<ItemStateSO> states)
        {
            Table = new Dictionary<ItemStateSO, List<Transition>>();

            // For each state, its list of transitions is added to the dictionary.
            foreach (ItemStateSO state in states)
            {
                Table.Add(state, state.Transitions);
            }
        }

        /// <summary xml:lang="es">
        /// Método para obtener la transición válida según el estado actual y el evento recibido.
        /// </summary>
        /// /// <summary xml:lang="en">
        /// Method to obtain the valid transition according to the current state and the event received.
        /// </summary>
        public Transition GetTransition(ItemStateSO currentState)
        {
            // The list of transitions of the current state is obtained.
            List<Transition> transitions = Table[currentState];

            // The list is traversed looking for the first transition whose condition is met by the received event.
            foreach (Transition transition in transitions)
            {
                if (transition.Condition())
                {
                    return transition;
                }
            }
            // If no valid transition is found, null is returned.
            return null;
        }
    }
}