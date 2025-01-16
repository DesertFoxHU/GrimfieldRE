using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BuildPanel : MonoBehaviour
{
    public static BuildPanel Instance;

    public enum Category
    {
        Village,
        Tower,
        Factory
    }

    public bool isOpen;
    public GameObject categoryButtonHolder;
    [HideInInspector] public Category category = Category.Village;
    public GameObject villageCategory;
    public GameObject towerCategory;
    public GameObject factoryCategory;
    public GameObject segmentPrefab;

    [Space]
    public GameObject upwardButton;
    public GameObject downwardButton;

    [HideInInspector] public List<BuildMenuSegment> segments = new(); //Segments which are active in scene
    [HideInInspector] public Dictionary<BuildingType, int> BuildingBought = new();

    [HideInInspector] public Dictionary<Category, int> pages = new();
    [HideInInspector] public Dictionary<Category, List<BuildMenuElement>> processedSegments = new();

    public void Start()
    {
        Instance = this;
        foreach(Category category in Enum.GetValues(typeof(Category)))
        {
            pages.Add(category, 0);
            processedSegments.Add(category, new List<BuildMenuElement>());
        }

        foreach(BuildMenuElement element in BuildMenuElementRegistry.elements)
        {
            processedSegments[element.Category].Add(element);
        }

        Load(Category.Village);
    }

    public void PageUpwards()
    {
        int current = pages[category];
        current--;

        if (current < 0) current = 0;
        else if (current >= processedSegments[category].Count)
        {
            current++;
        }

        pages[category] = current;
        Load(category);
    }

    public void PageDownwards()
    {
        int current = pages[category];
        current++;

        if (current < 0) current = 0;
        else if(current >= processedSegments[category].Count)
        {
            current--;
        }

        pages[category] = current;
        Load(category);
    }

    public BuildMenuSegment GetSegment(BuildingType type)
    {
        return segments.Find(x => x.Type == type);
    }

    public void Load(Category category)
    {
        foreach(BuildMenuSegment segment in segments)
        {
            Destroy(segment.gameObject);
        }
        segments.Clear();

        this.category = category;
        villageCategory.SetActive(false);
        towerCategory.SetActive(false);
        factoryCategory.SetActive(false);

        int pageIndex = pages[category];
        GameObject parentHolder = GetCategorysObject(category);

        int count = 0;
        for(int index = pageIndex; index < pageIndex + 4; index++)
        {
            if (processedSegments[category].Count <= index)
            {
                break;
            }

            BuildMenuElement element = processedSegments[category][index];
            float Y = 0 + (-180 * count);

            GameObject newSegment = Instantiate(segmentPrefab, new Vector3(0f, Y, 0f), Quaternion.identity);
            newSegment.transform.SetParent(GetCategorysObject(element.Category).transform, false);
            newSegment.GetComponent<BuildMenuSegment>().Load(element, BuildingBought.GetValueOrDefault(element.BuildingType, 0));
            segments.Add(newSegment.GetComponent<BuildMenuSegment>());

            count++;
        }

        parentHolder.SetActive(true);
    }

    public void LoadCategory(string categoryRaw)
    {
        Load((Category)Enum.Parse(typeof(Category), categoryRaw));
    }

    public GameObject GetCategorysObject(Category category)
    {
        if (category == Category.Village) return villageCategory;
        else if (category == Category.Tower) return towerCategory;
        else return factoryCategory;
    }

    #region Close/Open related functions
    public void Trigger()
    {
        isOpen = !isOpen;
        GetComponent<Animator>().SetTrigger("Toggle");
    }

    public void HideCategoryButtons()
    {
        categoryButtonHolder.SetActive(false);
    }

    public void ShowCategoryButtons()
    {
        categoryButtonHolder.SetActive(true);
    }
    #endregion
}
