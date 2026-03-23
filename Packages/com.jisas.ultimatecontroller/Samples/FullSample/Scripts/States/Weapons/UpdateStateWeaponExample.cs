using UltimateFramework.ItemSystem;
using UltimateFramework.Utils;
using UnityEngine;

[CreateAssetMenu(menuName = "Ultimate Framework/Systems/Items/State Asset/Weapon State/UpdateState")]
public class UpdateStateWeaponExample : WeaponState, IVisualChangeState
{
    public override void StateStart(WeaponBehaviour machine)
    {
        Debug.Log("State Upgraded");

        this.stateFlow = StateFlow.Start;
        OnChangeVisual += VisualChange;
    }

    public override void StateUpdate(WeaponBehaviour machine)
    {
        this.stateFlow = StateFlow.Running;

        if (this.Transitions.Count > 0)
            machine.ProcessEvent(this);
    }

    public void VisualChange()
    {
        Debug.Log("Visuals Changed");
    }

    #region Conditions
    private bool GenericCondition()
    {
        return false;
    }
    #endregion

    #region Actions
    private void GenericTransitionAction(WeaponBehaviour machine)
    {
        this.stateFlow = StateFlow.Finished;
        machine.SetLastState(this);
    }
    #endregion
}