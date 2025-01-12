using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResourceText : MonoBehaviour
{
    [System.Serializable]
    public struct ResourceWriteable
    {
        public ResourceType type;
        public TextMeshProUGUI textField;
    }

    public List<ResourceWriteable> texts;

    public void UpdateType(ResourceType type, double amount, double perTurn, double maxAmount)
    {
        foreach(ResourceWriteable text in texts)
        {
            if(text.type == type)
            {
                string prefix = perTurn >= 0 ? "+" : "";
                text.textField.text = $"{amount}/{maxAmount} ({prefix}{perTurn})";
            }
        }
    }
}
