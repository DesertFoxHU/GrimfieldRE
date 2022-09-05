using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace InfoPanel
{

    /// <summary>
    /// ContentPanel is inside of InfoPanel and controls filling data and processing them
    /// </summary>
    public class ContentPanel : MonoBehaviour
    {
        public InfoWindow.ContentType type;

        public void Load(object obj)
        {
            ClearChildren();
            float Y = 0;
            if(type == InfoWindow.ContentType.Building)
            {
                AbstractBuilding building = (AbstractBuilding) obj;

                //These are just prefabs
                List<ContentSegment> prefabs = FindObjectOfType<ContentSegmentHolder>().GetPrefabs(building);
                prefabs = prefabs.OrderBy(x => x.order).ToList();

                foreach(ContentSegment segment in prefabs)
                {
                    GameObject newInstance = CreateNewInstance(segment.gameObject, Y);

                    if(segment.ID == "CAPITAL")
                    {
                        string input = ((Village)building).IsCapital ? "Yes" : "No";
                        newInstance.GetComponentInChildren<TextMeshProUGUI>().text = $"Capital: {input}";
                    }
                    else if(segment.ID == "STAT")
                    {
                        newInstance.GetComponentInChildren<TextMeshProUGUI>().text = $"Level: {building.Level}";
                    }
                    else if(segment.ID == "RES_STORAGE")
                    {
                        GameObject storageHolder = newInstance.GetChildrenByName("Storage");
                        foreach(ResourceStorage resStorage in ((IResourceStorage)building).Storage)
                        {
                            storageHolder.GetChildrenByName(resStorage.Type.ToString()).GetComponentInChildren<TextMeshProUGUI>().text = "" + resStorage.Amount;
                        }
                    }

                    Y -= segment.height;
                }
            }
            else if(type == InfoWindow.ContentType.Entity)
            {
                Entity entity = (Entity)obj;


            }
        }

        public void ClearChildren()
        {
            foreach(Transform children in this.transform)
            {
                if (children.gameObject.activeSelf)
                {
                    Destroy(children.gameObject);
                }
            }
        }

        public GameObject CreateNewInstance(GameObject prefab, float Y)
        {
            GameObject newInstance = Instantiate(prefab, new Vector3(0, Y, 0), Quaternion.identity);
            newInstance.transform.SetParent(this.transform, false);
            newInstance.SetActive(true);
            return newInstance;
        }
    }
}