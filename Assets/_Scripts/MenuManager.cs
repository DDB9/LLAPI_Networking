using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public TextMeshProUGUI HeaderText;
    public GameObject WaitingForPlayers;
    public GameObject LookingForGame;

    private GameObject client;
    private GameObject server;
    private bool enterGame;

    private void Awake()
    {
        if (Main.Instance.CurrentUser == null)
        {
            Debug.LogError("No user detected! Please log in.");
            SceneManager.LoadScene("Login");
        }
    }

    private void Start()
    {
        HeaderText.SetText("Welcome " + Main.Instance.CurrentUser.Username + "!");
    }

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
