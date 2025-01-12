using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using static UnityEditor.PlayerSettings;

/// <summary>
/// Like a BuildMenuElement but it's attached to one of them to make logic
/// </summary>
public class BuildMenuSegment : MonoBehaviour
{
    public Image Icon;
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Description;
    public GameObject CostHolder;
    public GameObject CostPrefab;
    public GameObject UpkeepHolder;

    public BuildMenuElement LastLoaded { get; private set; }
    public BuildingType Type
    {
        get => LastLoaded.BuildingType;
    }

    public void Load(BuildMenuElement element)
    {
        LastLoaded = element;
        Title.text = LastLoaded.Title;
        Description.text = DefinitionRegistry.Instance.Find(element.BuildingType).description;
        Icon.sprite = DefinitionRegistry.Instance.Find(element.BuildingType).GetSpriteByLevel(1);
        RenderCost(element, 0);
        RenderUpkeep(element);
    }

    public void UpdateCostColor()
    {
        foreach (Transform children in CostHolder.transform)
        {
            ResourceType resourceType = (ResourceType) System.Enum.Parse(typeof(ResourceType), children.name);
            TextMeshProUGUI valueText = children.GetComponentInChildren<TextMeshProUGUI>();
            double cost = double.Parse(valueText.text.Trim());
            if (!PlayerInfo.Resources.ContainsKey(resourceType))
            {
                valueText.color = Color.red;
            }
            else
            {
                if (PlayerInfo.Resources[resourceType] >= cost)
                {
                    valueText.color = Color.black;
                }
                else valueText.color = Color.red;
            }
        }
    }

    public void RenderCost(int boughtCount)
    {
        RenderCost(LastLoaded, boughtCount);
    }

    public void RenderCost(BuildMenuElement element, int boughtCount)
    {
        BuildingDefinition definition = DefinitionRegistry.Instance.Find(element.BuildingType);
        foreach (Transform children in CostHolder.transform) Destroy(children.gameObject);

        Dictionary<ResourceType, double> cost = definition.GetBuildingCost(boughtCount);

        cost.RemoveAll(val => val <= 0);
        int count = 0;
        foreach (ResourceType type in cost.Keys)
        {
            Vector3 pos = new Vector3(3, 24, 0);
            pos.y = pos.y - (23 * count);
            if(count >= 3)
            {
                pos.y = 24 - (23 * (count - 3));
                pos.x = 103.2f;
            }

            GameObject costObject = Instantiate(CostPrefab, pos, Quaternion.identity);
            costObject.transform.SetParent(CostHolder.transform, false);
            costObject.name = type.ToString();
            costObject.GetComponent<Image>().sprite = FindAnyObjectByType<ResourceIconRegistry>().Find(type);
            costObject.GetComponentInChildren<TextMeshProUGUI>().text = "" + cost[type];

            count++;
        }
    }

    public void RenderUpkeep(BuildMenuElement element)
    {
        BuildingDefinition definition = DefinitionRegistry.Instance.Find(element.BuildingType);
        foreach (Transform children in UpkeepHolder.transform) Destroy(children.gameObject);

        int count = 0;
        foreach(ResourceHolder holder in definition.Upkeep)
        {
            Vector3 pos = new(3, 24, 0);
            pos.y = pos.y - (23 * count);
            if (count >= 3)
            {
                pos.y = 24 - (23 * (count-3));
                pos.x = 103.2f;
            }

            GameObject costObject = Instantiate(CostPrefab, pos, Quaternion.identity);
            costObject.transform.SetParent(UpkeepHolder.transform, false);
            costObject.name = holder.type.ToString();
            costObject.GetComponent<Image>().sprite = FindAnyObjectByType<ResourceIconRegistry>().Find(holder.type);
            costObject.GetComponentInChildren<TextMeshProUGUI>().text = "" + holder.Value;
            count++;
        }
    }

    public void StartBlueprintMode()
    {
        FindAnyObjectByType<Blueprinting>().ChangeBlueprint(DefinitionRegistry.Instance.Find(LastLoaded.BuildingType));
    }
}
