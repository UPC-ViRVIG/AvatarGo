using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;

public class NetHUD : MonoBehaviour
{
    private static bool IsClient;

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
        }
        GUILayout.EndArea();
    }

    private static string IP = "127.0.0.1";
    static void StartButtons()
    {
        if (!IsClient)
        {
            if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
            if (GUILayout.Button("Client"))
            {
                IsClient = true;
            }
        }
        else
        {
            if (GUILayout.Button("Back"))
            {
                IsClient = false;
            }
            if (GUILayout.Button("Connect"))
            {
                NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = IP;
                NetworkManager.Singleton.StartClient();
            }
            GUILayout.Label("Host IP: ");
            IP = GUILayout.TextField(IP);
        }
    }

    static void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ?
            "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Mode: " + mode);
    }
}
