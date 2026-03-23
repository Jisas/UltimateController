
namespace UltimateFramework.ItemSystem
{
    public abstract class WeaponState : ItemStateSO
    {
        public abstract override void StateStart(WeaponBehaviour machine);
        public abstract override void StateUpdate(WeaponBehaviour machine);
    }
}