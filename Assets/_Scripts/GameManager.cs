using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public ClientBehaviour Client;
    public int Score = 0;
    public TextMeshProUGUI ScoreText, ResultText;

    [HideInInspector]
    public bool GameReady = true;

    public bool Turn = false;
    public float GameTimer = 5f;
    public TextMeshProUGUI GameTimerText;
    public GameObject TurnText, GameOverScreen;

    [Header("Arrays")]
    public Button[] Buttons;
    public GameObject[] UIs;

    private float gameTime = 60f;
    private bool runOnce;
    private TextMeshProUGUI timerText;
    private GameObject playerUI;

    #region Singleton
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {  
        foreach(GameObject ui in UIs) { ui.SetActive(false); }
        ScoreText.enabled = false;
        TurnText.SetActive(false);
        GameTimer = 5f;

        // If the GameManager is instantiated (and therefore the Start method is called),
        // this means that both players have joined and the game is ready to start.
        GameReady = false;
        Client = FindObjectOfType<ClientBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameReady) GameSetup();
        else
        {
            GameTimer -= Time.deltaTime;
            GameTimerText.SetText("Time left: " + GameTimer.ToString("F0"));
            if (GameTimer <= 0)
            {
                if (!runOnce) GameOver();
            }

            ScoreText.SetText(Score.ToString());
            if (Turn)
            {
                foreach (Button button in Buttons) button.interactable = true;
                TurnText.GetComponent<TextMeshProUGUI>().SetText("YOUR TURN");
            }
            else
            {
                foreach (Button button in Buttons) button.interactable = false;
                TurnText.GetComponent<TextMeshProUGUI>().SetText("OTHER PLAYER IS DECIDING...");
            }
        }
    }

    private void GameSetup()
    {
        GameOverScreen.SetActive(false);

        GameTimer -= Time.deltaTime;
        GameTimerText.SetText("GAME START IN: " + GameTimer.ToString("F0"));
        if (GameTimer < 0)
        {
            if (Client.PlayerNum == 1)
            {
                playerUI = UIs[0];
                playerUI.SetActive(true);
                Turn = true;
            }
            else if (Client.PlayerNum == 2)
            {
                playerUI = UIs[1];
                playerUI.SetActive(true);
                Turn = false;
            }

            GameTimer = gameTime;
            ScoreText.enabled = true;
            TurnText.SetActive(true);
            GameReady = true;
        }
    }

    public void AssignScore(DataCodes pResultCode)
    {
        switch (pResultCode)
        {
            case DataCodes.P1_ROUND_WON:
                if (Client.PlayerNum == 1)
                {
                    Score += 1;
                    ResultText.SetText("YOU WIN");
                }
                else ResultText.SetText("YOU LOSE");
                break;

            case DataCodes.P1_ROUND_LOST:
                if (Client.PlayerNum == 2)
                {
                    Score += 1;
                    ResultText.SetText("YOU WIN");
                }
                else ResultText.SetText("YOU LOSE");
                break;

            case DataCodes.ROUND_TIE:
                break;
        }
        ResultText.gameObject.SetActive(true);
    }


    private void GameOver()
    {
        GameOverScreen.transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetText("Score: " + Score);
        GameOverScreen.SetActive(true);
        Client.SendActionToServer((uint)DataCodes.END_GAME);
        
        // turn off the client component.
        Client.enabled = false;

        runOnce = true;
    }

    #region Button Functions
    // Runner Actions
    public void P1_STEEN() 
    { 
        Client.SendActionToServer((uint)DataCodes.P1_STEEN); 
        Turn = false;
    }
    public void P1_PAPIER() 
    {
        Client.SendActionToServer((uint)DataCodes.P1_PAPIER);
        Turn = false;
    }
    public void P1_SCHAAR() 
    {
        Client.SendActionToServer((uint)DataCodes.P1_SCHAAR);
        Turn = false;
    }

    // Blocker Actions
    public void P2_STEEN() 
    {
        Client.SendActionToServer((uint)DataCodes.P2_STEEN);
        Turn = false;
    }
    public void P2_PAPIER() 
    {
        Client.SendActionToServer((uint)DataCodes.P2_PAPIER);
        Turn = false;
    }
    public void P2_SCHAAR() 
    {
        Client.SendActionToServer((uint)DataCodes.P2_SCHAAR);
        Turn = false;
    }

    // Misc.
    public void QuitGame()
    {
        Application.Quit();
    }
    #endregion
}
