using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecruitEntityElement : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI title;

    public EntityDefinition LastLoaded { get; private set; }

    public void Load(EntityDefinition definition)
    {
        LastLoaded = definition;
        icon.sprite = definition.RecruitIcon;
        title.text = definition.Name;
    }

    public void ForceLoadEntityDetail()
    {
        FindAnyObjectByType<RecruitPanel>().LoadDetailedEntityPanel(LastLoaded);
    }
}
