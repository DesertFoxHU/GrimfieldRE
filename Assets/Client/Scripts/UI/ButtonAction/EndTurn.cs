using ClientSide;
using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class EndTurn : MonoBehaviour
{
    private Button button;
    private float timer = 0f;

    private void Start()
    {
        this.button = this.GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void Update()
    {
        if (timer <= 0f) return;
        timer -= Time.deltaTime;
    }

    public void OnClick()
    {
        //Against packet spamming
        if (timer > 0f) return;

        timer = .5f;
        NetworkManager.Instance.Client.Send(Message.Create(MessageSendMode.unreliable, ClientToServerPacket.NextTurn));
    }
}
