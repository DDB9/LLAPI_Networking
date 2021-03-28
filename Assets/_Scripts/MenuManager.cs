using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public TextMeshProUGUI HeaderText;
    public GameObject LoadingScreen;

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
        HeaderText.SetText("Welcome user No." + Main.Instance.CurrentUser.UserID + "!");
    }    

    // This works because you cannot push this button unless you're logged in.
    public void PlayRunner()
    {
        LoadingScreen.SetActive(true);
        enterGame = true;
        Main.Instance.PlayerObject.GetComponent<ClientBehaviour>().ReadyUp();
    }

    private void Update()
    {
        if (enterGame) EnterGame(); 
    }

    private void EnterGame()
    {
        if (Main.Instance.PlayerObject.GetComponent<ClientBehaviour>().GameReady)
            SceneManager.LoadScene("Game");
    }
}
