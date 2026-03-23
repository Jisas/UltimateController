
namespace UltimateFramework.ItemSystem
{
    public abstract class ItemState : ItemStateSO
    {
        public abstract override void StateStart(ItemBehaviour machine);
        public abstract override void StateUpdate(ItemBehaviour machine);
    }
}