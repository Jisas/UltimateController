using System;

/// <summary xml:lang="es">
/// Clase que representa una transicion.
/// </summary>
/// /// <summary xml:lang="en">
/// Class representing a transition.
/// </summary>
namespace UltimateFramework.ItemSystem
{
    public class Transition
    {
        /// <summary xml:lang="es">
        /// Condición que debe cumplirse para activar la transición.
        /// </summary>
        /// /// <summary xml:lang="en">
        /// Condition to be met to trigger the transition.
        /// </summary>
        public Func<bool> Condition { get; private set; }

        /// <summary xml:lang="es">
        /// Acción que se ejecuta al activar la transición.
        /// </summary>
        /// /// <summary xml:lang="en">
        /// Action to be performed when the transition is activated.
        /// </summary>
        public Action Action { get; private set; }

        /// <summary xml:lang="es">
        /// Estado al que se llega al activar la transición.
        /// </summary>
        /// /// <summary xml:lang="en">
        ///  State reached when the transition is activated.
        /// </summary>
        public ItemStateSO TargetState { get; private set; }

        /// <summary xml:lang="es">
        /// Constructor que recibe la condición, la acción y el estado destino.
        /// </summary>
        /// /// <summary xml:lang="en">
        /// Constructor that receives the condition, action and target state.
        /// </summary>
        public Transition(Func<bool> condition, Action action, ItemStateSO targetState)
        {
            Condition = condition;
            Action = action;
            TargetState = targetState;
        }
    }
}