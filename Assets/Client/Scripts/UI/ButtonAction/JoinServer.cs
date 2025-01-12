using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class JoinServer : MonoBehaviour
{
    public TMP_InputField ipAddressField;
    public TMP_InputField nameField;

    void Start()
    {
        if (PlayerPrefs.HasKey("IpAddress"))
        {
            ipAddressField.text = PlayerPrefs.GetString("IpAddress");
        }
        if (PlayerPrefs.HasKey("Name"))
        {
            nameField.text = PlayerPrefs.GetString("Name");
        }
        this.GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        if (ipAddressField.text == "" || ipAddressField.text == null)
        {
            FindAnyObjectByType<MessageDisplayer>().SetMessage("IpAddress field cannot be empty");
            return;
        }
        if (nameField.text == "" || nameField.text == null)
        {
            FindAnyObjectByType<MessageDisplayer>().SetMessage("Name field cannot be empty");
            return;
        }

        if(nameField.text.Length > 16)
        {
            FindAnyObjectByType<MessageDisplayer>().SetMessage("Name cannot be longer than 16 characters!");
            return;
        }

        NetworkManager.Instance.Name = nameField.text.Trim();

        string ipAddress = ipAddressField.text.Trim();
        if (ipAddress.Contains(':'))
        {
            NetworkManager.Instance.port = ushort.Parse(ipAddress.Split(':')[1].Trim(), System.Globalization.NumberStyles.Any);
            NetworkManager.Instance.ip = ipAddress.Split(':')[0];
        }
        else
        {
            NetworkManager.Instance.ip = ipAddress;
        }

        PlayerPrefs.SetString("IpAddress", ipAddress);
        PlayerPrefs.SetString("Name", NetworkManager.Instance.Name);
        PlayerPrefs.Save();

        FindAnyObjectByType<NetworkManager>().Connect();
    }
}
