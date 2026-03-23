
namespace UltimateFramework.ItemSystem
{
    public abstract class ArmorState : ItemStateSO
    {
        public abstract override void StateStart(ArmorBehaviour machine);
        public abstract override void StateUpdate(ArmorBehaviour machine);      
    }
}