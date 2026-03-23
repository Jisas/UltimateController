using UltimateFramework.CollisionsAndDamageSystem;
using UltimateFramework.ItemSystem;
using UnityEngine;

public class WeaponComponent : MonoBehaviour
{
    public Item Item { get; private set; }
    public Transform BodySocket { get; private set; }
    public Transform HandSocket { get; private set; }
    public WeaponBehaviour WeaponBehaviour { get; private set; }
    public DefenceComponent DefenceComponent { get; private set; }

    private void Start()
    {
        DefenceComponent = GetComponent<DefenceComponent>();
        WeaponBehaviour = GetComponent<WeaponBehaviour>();
        Item = WeaponBehaviour.Item;
    }

    public void SetBodySocket(Transform socket) { BodySocket = socket; }
    public void SetHandSocket(Transform socket) { HandSocket = socket; }
}

