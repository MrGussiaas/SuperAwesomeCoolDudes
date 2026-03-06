using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using Telepathy;

public class ConnectionHandler : MonoBehaviour
{
    private TMP_InputField ipField;
    private TMP_InputField portField; 

    private NetworkManager manager;
    private TelepathyTransport transport;

    private const string IP_TAG = "IpAddress";
    private const string PORT_TAG = "Port";
    void Start()
    {
        manager = NetworkManager.singleton;
        transport = Transport.active as TelepathyTransport;
        for(int i = 0, n = transform.childCount; i < n; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.CompareTag(IP_TAG))
            {
                ipField = child.GetComponent<TMP_InputField>();
            }
            if (child.CompareTag(PORT_TAG))
            {
                portField = child.GetComponent<TMP_InputField>();
            }
        }
    }

    public void ConnectHostClient()
    {
        ApplySettings();
        manager.StartHost();
        gameObject.SetActive(false);
    }

    public void ConnectClientOnly()
    {
        ApplySettings();
        manager.StartClient();
        gameObject.SetActive(false);
    }

    public void StartServer()
    {
        
    }

    private void ApplySettings()
    {
        manager.networkAddress = ipField.text;

        if (transport != null && ushort.TryParse(portField.text, out ushort port))
        {
            transport.port = port;
        }
    }
}
