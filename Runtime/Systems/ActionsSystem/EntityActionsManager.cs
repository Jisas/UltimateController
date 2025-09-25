using UltimateFramework.StatisticsSystem;
using System.Collections.Generic;
using UltimateFramework.Utils;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using System;
using System.Threading;

namespace UltimateFramework.ActionsSystem
{
    public class EntityActionsManager
    {
        public List<BaseAction> AllActions { get; private set; } = new List<BaseAction>();
        public ActionStructure CurrentActionStructure { get; set; }
        public BaseAction LastAction { get; set; }
        public BaseAction CurrentAction { get; set; }

        public Action<ActionsPriority> OnActionInterrupted;

        private readonly Queue<BaseAction> highestPriority = new();
        private readonly Queue<BaseAction> highPriority = new();
        private readonly Queue<BaseAction> middlePriority = new();
        private readonly Queue<BaseAction> lowPriority = new();
        private readonly Queue<BaseAction> lowestPriority = new();

        public void OnEnable() => OnActionInterrupted += InterruptLowerPriorityActions;
        public void OnDisable() => OnActionInterrupted -= InterruptLowerPriorityActions;

        public void AddAction(BaseAction action)
        {           
            AllActions.Add(action);            
        }

        public void EnqueueAction(BaseAction action, ActionsPriority priority)
        {
            switch (priority)
            {
                case ActionsPriority.Highest:
                    highestPriority.Enqueue(action);
                    break;
                case ActionsPriority.High:
                    highPriority.Enqueue(action);
                    break;
                case ActionsPriority.Middle:
                    middlePriority.Enqueue(action);
                    break;
                case ActionsPriority.Low:
                    lowPriority.Enqueue(action);
                    break;
                case ActionsPriority.Lowest:
                    lowestPriority.Enqueue(action);
                    break;
            }
        }

        public void TriggerIterruptEventAction(BaseAction action)
        {
            OnActionInterrupted.Invoke(action.Priority);
        }

        public void InterruptLowerPriorityActions(ActionsPriority priority)
        {
            if (LastAction != null && !LastAction.cantBeInterrupted && LastAction.Priority > priority && LastAction.IsExecuting)
            {
                LastAction.InterruptAction();
                LastAction.InterruptAction(CurrentActionStructure);
            }
        }

        public bool IsHigherOrEqualPriorityActionExecuting(BaseAction currentAction)
        {
            return AllActions.Any(action =>
            {
                return action != null && action != currentAction && action.Priority <= currentAction.Priority && action.IsExecuting;
            });
        }

        public bool IsCantBeInterruptedActionExecuting(BaseAction currentAction)
        {
            return AllActions.Any(action =>
            {
                return action != null && action != currentAction && action.cantBeInterrupted && action.IsExecuting;
            });
        }

        public bool MeetsActionCost(StatisticsComponent characterStats)
        {
            var cost = CurrentActionStructure.actionCost;
            var costStat = cost.statType.tag;

            if (!String.IsNullOrEmpty(costStat) && cost.value > 0)
            {
                return characterStats.FindStatistic(costStat).CurrentValue >= cost.value;
            }
            else return true;
        }

        public async Task ProcessActions(Animator animator, CancellationToken ct)
        {
            await ProcessQueue(highestPriority, animator, ct);
            await ProcessQueue(highPriority, animator, ct);
            await ProcessQueue(middlePriority, animator, ct);
            await ProcessQueue(lowPriority, animator, ct);
            await ProcessQueue(lowestPriority, animator, ct);
        }

        private async Task ProcessQueue(Queue<BaseAction> queue, Animator animator, CancellationToken ct)
        {
            while (queue.Count > 0)
            {
                // Comprobar si hay acciones de mayor prioridad en las colas
                if (HasHigherPriorityActions(queue))
                    return;

                var action = queue.Dequeue();
                await HandleExceptions(action.Execute(this, CurrentActionStructure, animator, ct));
            }

            return;
        }

        private bool HasHigherPriorityActions(Queue<BaseAction> currentQueue)
        {
            if (currentQueue == highestPriority) return false;
            else if (currentQueue == highPriority) return highestPriority.Count > 0;
            else if (currentQueue == middlePriority) return highestPriority.Count > 0 || highPriority.Count > 0;
            else if (currentQueue == lowPriority) return highestPriority.Count > 0 || highPriority.Count > 0 || middlePriority.Count > 0;
            else return highestPriority.Count > 0 || highPriority.Count > 0 || middlePriority.Count > 0 || lowPriority.Count > 0;
        }

        async Task HandleExceptions(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
