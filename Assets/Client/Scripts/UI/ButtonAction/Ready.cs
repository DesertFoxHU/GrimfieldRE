using ClientSide;
using Riptide;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class Ready : MonoBehaviour
{
    private bool IsReady = false;
    public TextMeshProUGUI text;

    void Start()
    {
        this.GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void UpdateText()
    {
        text.text = IsReady ? "Unready" : "Ready";
    }

    public void OnClick()
    {
        IsReady = !IsReady;
        UpdateText();

        NetworkManager.Instance.Client.Send(Message.Create(MessageSendMode.reliable, ClientToServerPacket.ChangeReadyStatus).AddBool(IsReady));
    }
}
