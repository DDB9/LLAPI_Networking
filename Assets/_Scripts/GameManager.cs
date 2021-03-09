using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public ClientBehaviour player;

    [HideInInspector]
    public bool GameReady = true;

    public bool Turn = false;
    public float TurnTimer = 5f;
    public float StartGameTimer = 5f;
    public GameObject TurnText;

    [Header("Arrays")]
    public Button[] Buttons;
    public TextMeshProUGUI[] Timers;
    public GameObject[] UIs;

    private TextMeshProUGUI StartTimerText;
    private TextMeshProUGUI TimerText;
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
        /*TODO: Turn off functionality at game start. Start countdown. Either send message 
         to server to send to clients to start game, or just start countdown. That should also work. 
        Then game starts. Give blocker side about 3 seconds of descicion time, 
        then the turn ends and runner has to react within abt. 1.5f seconds.*/
        
        foreach(GameObject ui in UIs) { ui.SetActive(false); }
        StartTimerText = Timers[0];
        StartTimerText.gameObject.SetActive(true);
        TurnText.SetActive(false);

        // If the GameManager is instantiated (and therefore the Start method is called),
        // this means that both players have joined and the game is ready to start.
        GameReady = false;
        player = FindObjectOfType<ClientBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameReady) GameSetup();
        else
        {
            if (Turn)
            {
                foreach (Button button in Buttons) button.enabled = true;
                TurnText.GetComponent<TextMeshProUGUI>().SetText("YOUR TURN");

                // ! TIMER FUNCTIONALITY.
                //TurnTimer -= Time.deltaTime;
                //TimerText.SetText(TurnTimer.ToString("F0"));
                //if (TurnTimer <= 0)
                //{
                //    // do something.
                //    player.SendActionToServer((uint)DataCodes.PASS_TURN);
                //    Turn = false;
                //    if (player.PlayerNum == 1) TurnTimer = 5f;
                //    else TurnTimer = 2f;
                //}
            }
            else
            {
                foreach (Button button in Buttons) button.enabled = false;
                TurnText.GetComponent<TextMeshProUGUI>().SetText("OTHER PLAYER IS DECIDING...");
            }
        }
    }

    private void GameSetup()
    {
        StartGameTimer -= Time.deltaTime;
        StartTimerText.SetText("GAME START IN: " + StartGameTimer.ToString("F0"));
        if (StartGameTimer < 0)
        {
            StartTimerText.gameObject.SetActive(false);

            if (player.PlayerNum == 1)
            {
                TimerText = Timers[1];
                playerUI = UIs[0];
                playerUI.SetActive(true);
            }
            else if (player.PlayerNum == 2)
            {
                TimerText = Timers[2];
                playerUI = UIs[1];
                playerUI.SetActive(true);
            }

            TurnText.SetActive(true);
            GameReady = true;
        }
    }

    #region Button Functions
    // Runner Actions
    public void RunnerJump() 
    { 
        player.SendActionToServer((uint)DataCodes.RUNNER_JUMP); 
        player.SendActionToServer((uint)DataCodes.PASS_TURN);  
    }
    public void RunnerDodge() 
    {
        player.SendActionToServer((uint)DataCodes.RUNNER_DODGE);
        player.SendActionToServer((uint)DataCodes.PASS_TURN);  
    }
    public void RunnerAttack() 
    {
        player.SendActionToServer((uint)DataCodes.RUNNER_ATTACK);
        player.SendActionToServer((uint)DataCodes.PASS_TURN);  
    }

    // Blocker Actions
    public void BlockerObstacle() 
    {
        player.SendActionToServer((uint)DataCodes.BLOCKER_OBSTACLE);
        player.SendActionToServer((uint)DataCodes.PASS_TURN);  
    }
    public void BlockerGhost() 
    {
        player.SendActionToServer((uint)DataCodes.BLOCKER_ENEMY_GHOST);
        player.SendActionToServer((uint)DataCodes.PASS_TURN);  
    }
    public void BlockerGrunt() 
    {
        player.SendActionToServer((uint)DataCodes.BLOCKER_ENEMY_GRUNT);
        player.SendActionToServer((uint)DataCodes.PASS_TURN);  
    }
    #endregion
}
