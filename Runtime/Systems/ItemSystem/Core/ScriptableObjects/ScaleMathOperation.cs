using UltimateFramework.StatisticsSystem;
using System.Collections.Generic;
using UltimateFramework.Utils;
using UnityEngine;

[System.Serializable, CreateAssetMenu(fileName = "NewScaleMathOPData", menuName = "Ultimate Framework/Systems/Math/Scale Operation Data")]
public class ScaleMathOperation : ScriptableObject
{
    [SerializeField]
    private ScalingType scalingType = ScalingType.Linear;
    [SerializeField, Range(0, 10)]
    private float A_ScaleFactor = 1.0f;
    [SerializeField, Range(0, 10)]
    private float B_ScaleFactor = 0.8f;
    [SerializeField, Range(0, 10)]
    private float C_ScaleFactor = 0.6f;
    [SerializeField, Range(0, 10)]
    private float D_ScaleFactor = 0.4f;
    [SerializeField, Range(0, 10)]
    private float F_ScaleFactor = 0.2f;

    private readonly Dictionary<string, float> attributeValues = new();

    public void SubscribeToAttributes(StatisticsComponent statsComponent)
    {
        if (statsComponent != null)
        {
            if (statsComponent.primaryAttributes.Count > 0)
            {
                foreach (UltimateFramework.StatisticsSystem.Attribute primaryAttribute in statsComponent.primaryAttributes)
                {
                    if (!attributeValues.ContainsKey(primaryAttribute.attributeType.tag))
                        attributeValues.Add(primaryAttribute.attributeType.tag, primaryAttribute.CurrentValue);

                    primaryAttribute.OnValueChange += OnPrimaryAttributeValueChange;
                }
            }
            if (statsComponent.attributes.Count > 0)
            {
                foreach (UltimateFramework.StatisticsSystem.Attribute attribute in statsComponent.attributes)
                {
                    if (!attributeValues.ContainsKey(attribute.attributeType.tag))
                        attributeValues.Add(attribute.attributeType.tag, attribute.CurrentValue);

                    attribute.OnValueChange += OnAttributeValueChange;
                }
            }
        }
    }
    private void OnPrimaryAttributeValueChange(Attribute primaryAttribute, float value)
    {
        attributeValues[primaryAttribute.attributeType.tag] = value;
    }

    private void OnAttributeValueChange(Attribute attribute, float value)
    {
        attributeValues[attribute.attributeType.tag] = value;
    }
    private float GetAttributeValue(string attributeTag)
    {
        if (attributeValues.TryGetValue(attributeTag, out float value))
        {
            return value;
        }
        throw new System.NullReferenceException("Attribute not found");
    }
    public float GetScaleFactor(ScalingLevel scaled)
    {
        float scaleFactor = 0;

        switch (scaled)
        {
            case ScalingLevel.A:
                scaleFactor = A_ScaleFactor;
                break;
            case ScalingLevel.B:
                scaleFactor = B_ScaleFactor;
                break;
            case ScalingLevel.C:
                scaleFactor = C_ScaleFactor;
                break;
            case ScalingLevel.D:
                scaleFactor = D_ScaleFactor;
                break;
            case ScalingLevel.F:
                scaleFactor = F_ScaleFactor;
                break;
        }

        return scaleFactor;
    }
    public float CalculateScale(float currentItemStatValue, string attributeTag, ScalingLevel scaled, bool isSubstraction)
    {
        float attributeValue = GetAttributeValue(attributeTag);
        float scaleFactor = GetScaleFactor(scaled);

        switch (scalingType)
        {
            case ScalingType.Linear:
                return LinearScaling(currentItemStatValue, attributeValue, scaleFactor, isSubstraction);

            case ScalingType.Exponential:
                return ExponentialScaling(currentItemStatValue, attributeValue, scaleFactor, isSubstraction);

            case ScalingType.Logarithmic:
                return LogarithmicScaling(currentItemStatValue, attributeValue, scaleFactor, isSubstraction);

            default:
                throw new System.ArgumentException("Invalid scaling type");
        }
    }

    private float LinearScaling(float currentItemStatValue, float operationValue, float scaleFactor, bool isSubstraction)
    {
        if (isSubstraction) return currentItemStatValue -= operationValue * scaleFactor;
        else return currentItemStatValue + (operationValue * scaleFactor);
    }
    private float ExponentialScaling(float currentValue, float operationValue, float scaleFactor, bool isSubstraction)
    {
        if (isSubstraction) return currentValue -= Mathf.Pow(operationValue, scaleFactor);
        else return currentValue + Mathf.Pow(operationValue, scaleFactor);
    }
    private float LogarithmicScaling(float currentValue, float operationValue, float scaleFactor, bool isSubstraction)
    {
        if (isSubstraction) return currentValue -= Mathf.Log(operationValue) * scaleFactor;
        else return currentValue + (Mathf.Log(operationValue) * scaleFactor);
    }
}
