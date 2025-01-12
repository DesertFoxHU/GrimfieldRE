using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_Text))]
public class WikiHyperLink : MonoBehaviour, IPointerClickHandler
{
	private TMP_Text text;
	public string[] Ids;

    private void Start()
    {
		text = this.GetComponent<TMP_Text>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
		var linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);
		var linkId = text.textInfo.linkInfo[linkIndex].GetLinkID();
        int index;
        int.TryParse(linkId, out index);
        string id = Ids[index];
        FindAnyObjectByType<WikiCanvas>().LoadById(id);
	}
}
