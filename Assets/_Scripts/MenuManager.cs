using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public TextMeshProUGUI HeaderText;
    public GameObject LoadingScreen;

    private GameObject client;
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

    public void PlayBlocker()
    {
        LoadingScreen.SetActive(true);

        // Create a new client object for this client.
        client = new GameObject("client");
        client.AddComponent<ClientBehaviour>();
        client.GetComponent<ClientBehaviour>().PlayerNum = 1;

        enterGame = true;
    }    

    public void PlayRunner()
    {
        LoadingScreen.SetActive(true);

        // Create a new client object for this client.
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
            SceneManager.LoadScene("Game");
    }
}
