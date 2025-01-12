using Riptide;
using Riptide.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager instance;
    public static NetworkManager Instance
    {
        get => instance;
        set
        {
            if(instance == null)
            {
                instance = value;
            }
            else
            {
                Debug.LogError("NetworkManager instance is already exist!");
                Destroy(value);
            }
        }
    }

    public string ip;
    public ushort port = 6112;

    public Client Client { get; private set; }
    public string Name { get; set; }
    public ClientPlayer ClientPlayer { get; set; }
    public List<ClientPlayer> Players { get; set; }
    public bool IsYourTurn { get; set; } = false;

    public List<ClientPlayer> GetAllPlayer()
    {
        List<ClientPlayer> players = new List<ClientPlayer>(Players);
        players.Add(ClientPlayer);
        return players;
    }

    private void Awake() 
    { 
        Instance = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, true);

        Client = new Client();
        Client.TimeoutTime = 15000;
        Client.Connected += DidConnect;
        Client.ConnectionFailed += FailedConnect;
        Client.Disconnected += Disconnected;

        Message.MaxPayloadSize = 10000;

        Debug.Log("Created new RiptideClient!");
    }

    private void FailedConnect(object sender, EventArgs e)
    {
        FindAnyObjectByType<MessageDisplayer>().SetMessage("Connection Failed");
    }

    private void FixedUpdate()
    {
        Client.Tick();
    }

    private void OnApplicationQuit()
    {
        Client.Disconnect();
    }

    public void Connect()
    {
        Client.Connect($"{ip}:{port}");
        Debug.Log("Join request sent!");
    }

    public void Disconnected(object sender, DisconnectedEventArgs e)
    {
        SceneManager.UnloadSceneAsync("LobbyScene");
        SceneManager.UnloadSceneAsync("MainGame");
        SceneManager.LoadScene("Menu", LoadSceneMode.Additive);
    }

    public void DidConnect(object sender, EventArgs e)
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerPacket.JoinLobby);
        message.Add(Name);
        Client.Send(message);
    }

    public void SendMessageToServer(string text)
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerPacket.SendMessage);
        message.Add(text);
        Client.Send(message);
    }
}
