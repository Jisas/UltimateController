using UltimateFramework.StatisticsSystem;
using UltimateFramework.ItemSystem;
using UnityEngine;

public class WeaponBehaviour : ItemBehaviour
{    
    private Item _item;

    #region Poperties
    /// <summary xml:lang="es">
    /// Referencia a la cantidad de mejoras.
    /// </summary>
    /// /// <summary xml:lang="en">
    /// Reference to upgrades amount.
    /// </summary>
    public int UpgradesAmount { get; private set; }
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
        Item = ItemDB.FindItem(itemName);
        if(Item.Stats.Count > 0) Item.SetAllValuesToBase();
        UpgradesAmount = 0;
    }
    public override void Start()
    {
        base.Start();
        CurrentState = InternalStateList[UpgradesAmount];
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
        transition.Action();
        CurrentState = transition.TargetState as WeaponState;
        CurrentState.StateStart(this);
    }
    #endregion

    #region Testing Methods
    [ContextMenu("Make Upgrade")]
    public override void MakeUpgrade()
    {
        ItemUpgrade currentUpgrade = Item.FindUpgrade(UpgradesAmount);
        StatisticsComponent characterStats = owner.GetComponent<StatisticsComponent>();
        if (currentUpgrade == null) return;

        if (currentUpgrade.useStatUpgrade)
        {
            ItemUpgradeDecorator statUpgrade = new ItemStatUpgradeDecorator(Item);     
            statUpgrade.UpgradeItem(currentUpgrade);            
        }

        if (currentUpgrade.useScaleUpgrade)
        {
            ItemUpgradeDecorator scaleUpgrade = new ItemScaleUpgradeDecorator(Item);            
            scaleUpgrade.UpgradeItem(currentUpgrade, characterStats);
        }

        if (currentUpgrade.useAttModUpgrade)
        {
            ItemUpgradeDecorator attModUpgrade = new ItemAttModUpgradeDecorator(Item);
            attModUpgrade.UpgradeItem(currentUpgrade, characterStats);
        }

        if (currentUpgrade.useStatModUpgrade)
        {
            
        }

        UpgradesAmount++;
    }
    #endregion
}