using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject WaitingForPlayers;
    public GameObject LookingForGame;

    private GameObject client;
    private GameObject server;
    private bool enterGame;

    public void HostGame()
    {
        WaitingForPlayers.SetActive(true);

        server = new GameObject("server");
        server.AddComponent<ServerBehaviour>(); 

        client = new GameObject("client");
        client.AddComponent<ClientBehaviour>();
        client.GetComponent<ClientBehaviour>().PlayerNum = 1;

        enterGame = true;
    }

    public void JoinGame()
    {
        LookingForGame.SetActive(true);

        client = new GameObject("client");
        client.AddComponent<ClientBehaviour>();
        client.GetComponent<ClientBehaviour>().PlayerNum = 2;

        enterGame = true;
    }

    private void Update()
    {
        if (enterGame) EnterGame(); 
    }

    private void EnterGame()
    {
        if (client.GetComponent<ClientBehaviour>().GameReady)
            SceneManager.LoadScene(1);
    }
}
