using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject WaitingForPlayers;
    public GameObject LookingForGame;

    private bool joiningGame = false;

    public void HostGame()
    {
        WaitingForPlayers.SetActive(true);
        SceneManager.LoadScene(1);
    }

    public void JoinGame()
    {
        LookingForGame.SetActive(true);
        joiningGame = true;
    }

    private void Update()
    {
        if (joiningGame) SearchGame();
    }

    private void SearchGame()
    {
        var client = new GameObject("client");
        client.AddComponent<ClientBehaviour>();

        if (client.GetComponent<ClientBehaviour>().Connected == true)
            SceneManager.LoadScene(2);
    }
}
