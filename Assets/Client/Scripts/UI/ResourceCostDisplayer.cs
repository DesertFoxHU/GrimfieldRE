using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// This script can used to visualize list of resources, for example to display cost in a list-way
/// </summary>
[Tooltip("This script can visualize list of resources")]
public class ResourceCostDisplayer : MonoBehaviour
{
    [HideInInspector] public List<GameObject> initializedPrefab = new List<GameObject>();
    public GameObject prefab;
    public Vector3 start;
    [Tooltip("The distance between resource displayers")] public Vector2 step;

    #region Settings
    [Space]
    public bool renderOnStart;
    public List<ResourceHolder> resources;
    #endregion

    public void Start()
    {
        if (renderOnStart)
        {
            Render(resources.ToDictionary(x => x.type, y => y.Value));
        }
    }

    public void Clear()
    {
        foreach(GameObject go in initializedPrefab)
        {
            Destroy(go);
        }
        initializedPrefab.Clear();
    }

    public void Render(Dictionary<ResourceType, double> resources)
    {
        Clear();

        int count = 0;
        foreach (ResourceType type in resources.Keys)
        {
            /*Vector3 pos = new Vector3(0, 0, 0);
            if (count % 2 == 0) pos.y = -12;
            else pos.y = 12;

            pos.x = 120 * ((int)((count - 1) / 2));

            GameObject costObject = Instantiate(CostPrefab, pos, Quaternion.identity);
            costObject.transform.SetParent(CostHolder.transform, false);
            costObject.name = type.ToString();
            costObject.GetComponent<Image>().sprite = FindObjectOfType<ResourceIconRegistry>().Find(type);
            costObject.GetComponentInChildren<TextMeshProUGUI>().text = "" + cost[type];*/

            Vector3 pos = new Vector3(start.x + (step.x*count), start.y + (step.y*count), start.z);

            GameObject costObject = Instantiate(prefab, pos, Quaternion.identity);
            costObject.transform.SetParent(this.transform, false);
            costObject.name = type.ToString();
            costObject.GetComponent<Image>().sprite = FindAnyObjectByType<ResourceIconRegistry>().Find(type);
            costObject.GetComponentInChildren<TextMeshProUGUI>().text = "" + resources[type];

            initializedPrefab.Add(costObject);

            count++;
        }
    }
}
