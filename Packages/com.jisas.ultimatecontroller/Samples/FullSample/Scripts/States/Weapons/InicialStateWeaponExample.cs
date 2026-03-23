using UltimateFramework.ItemSystem;
using UltimateFramework.Utils;
using UnityEngine;

[CreateAssetMenu(menuName = "Ultimate Framework/Systems/Items/State Asset/Weapon State/InicialState")]
public class InicialStateWeaponExample : WeaponState
{
    public override void StateStart(WeaponBehaviour machine)
    {
        stateFlow = StateFlow.Start;

        AddTransition(new Transition(
            () => GenericCondition(machine),
            () => GenericTransition(machine),
            machine.FindState("UpdateStateWeapon")));
    }

    public override void StateUpdate(WeaponBehaviour machine)
    {
        stateFlow = StateFlow.Running;
        if (Transitions.Count > 0)
            machine.ProcessEvent(this);
    }

    #region Conditions
    private bool GenericCondition(WeaponBehaviour machine)
    {
        if (machine.UpgradesAmount % 2 == 0 && machine.UpgradesAmount > 0)
             return true;
        else return false;
    }
    #endregion

    #region Actions
    private void GenericTransition(WeaponBehaviour machine)
    {
        stateFlow = StateFlow.Finished;
        machine.SetLastState(this);
    }
    #endregion
}
