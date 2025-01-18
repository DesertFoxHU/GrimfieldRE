using ClientSide;
using Riptide;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecruitPanel : MonoBehaviour
{
    #region Local properties
    public Vector3Int LastPosition { get; private set; }
    public BuildingDefinition LastBuilding { get; private set; }
    public EntityDefinition LastEntity { get; private set; }
    #endregion

    #region Property
    public GameObject mainPanel;
    public GameObject entityList;
    public GameObject entityElementPrefab;

    public Image icon;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    public TextMeshProUGUI healthValue;
    public TextMeshProUGUI damageValue;
    public TextMeshProUGUI speedValue;
    public GameObject costHolder;
    public GameObject costPrefab;
    #endregion

    private void Start()
    {
        Close();
    }

    public void LoadPanel(Vector3Int position, BuildingDefinition building)
    {
        if (building.canRecruit.Count == 0) return;

        LastPosition = position;
        LastBuilding = building;

        foreach(Transform child in entityList.transform)
        {
            Destroy(child);
        }

        float Y = 0;
        foreach (EntityDefinition entity in building.GetRecruitable())
        {
            GameObject go = Instantiate(entityElementPrefab, new Vector3(0, Y, 0), Quaternion.identity);
            go.transform.SetParent(entityList.transform, false);
            go.SetActive(true);

            go.GetComponent<RecruitEntityElement>().Load(entity);

            Y -= 99;
        }

        LoadDetailedEntityPanel(building.GetRecruitable()[0]);

        Show();
    }

    public void LoadDetailedEntityPanel(EntityDefinition definition)
    {
        LastEntity = definition;
        icon.sprite = definition.RecruitIcon;
        title.text = definition.Name;
        description.text = definition.Description;
        healthValue.text = definition.Health[0].ToString();
        damageValue.text = definition.Damage[0].ToString();
        speedValue.text = definition.Speed[0].ToString();

        foreach(Transform child in costHolder.transform)
        {
            Destroy(child.gameObject);
        }

        ResourceIconRegistry iconReg = FindAnyObjectByType<ResourceIconRegistry>();
        int index = 0;
        foreach (ResourceHolder resource in definition.RecruitCost)
        {
            GameObject go = Instantiate(costPrefab, new Vector3(80 * index, 0, 0), Quaternion.identity);
            go.transform.SetParent(costHolder.transform, false);
            go.GetComponent<Image>().sprite = iconReg.Find(resource.type);
            go.GetComponentInChildren<TextMeshProUGUI>().text = "" + resource.Value;

            index++;
        }
    }

    public void BuyEntity()
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerPacket.BuyEntity);
        message.Add(LastEntity.Type.ToString());
        message.Add(LastPosition);
        NetworkManager.Instance.Client.Send(message);

        Close();
    }

    public void Close()
    {
        mainPanel.SetActive(false);
    }

    public void Show()
    {
        mainPanel.SetActive(true);
    }
}
