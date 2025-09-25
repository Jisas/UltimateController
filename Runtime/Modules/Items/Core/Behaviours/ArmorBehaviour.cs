using UltimateFramework.Utils;

namespace UltimateFramework.ItemSystem
{
    public class ArmorBehaviour : ItemBehaviour
    {
        public string setName;
        public ItemType armaturePartType;

        private Item _item;

        #region Properties
        public override Item Item
        {
            get { return _item; }
            protected set { _item = value; }
        }
        #endregion

        #region Mono
        public override void Awake()
        {
            base.Awake();
            Item = ItemDB.FindArmaturePart(setName, armaturePartType);
            if (Item.Stats.Count > 0) Item.SetAllValuesToBase();
        }
        public override void Start()
        {
            SetUpTransitions();
            CurrentState = InternalStateList[0];
            CurrentState.StateStart(this);
        }
        public override void Update()
        {
            CurrentState.StateUpdate(this);
        }
        #endregion

        #region Internal
        protected override void SwitchState(Transition transition)
        {
            // Transition action is executed
            transition.Action();

            // The current state is changed to the target state of the transition.
            CurrentState = transition.TargetState as ArmorState;

            // The logic of the new current state is executed
            CurrentState.StateStart(this);
        }
        #endregion
    }
}