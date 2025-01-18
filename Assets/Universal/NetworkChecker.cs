using UnityEngine;

public class NetworkChecker : MonoBehaviour
{
    public static NetworkChecker Instance { get; private set; }

    public void Start()
    {
        Instance = this;
    }

    public static bool IsServer()
    {
        return FindAnyObjectByType<ClientSide.NetworkManager>() == null;
    }

    public static bool IsClient()
    {
        return FindAnyObjectByType<ClientSide.NetworkManager>() != null;
    }
}