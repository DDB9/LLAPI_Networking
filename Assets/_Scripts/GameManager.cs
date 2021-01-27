using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public ClientBehaviour player;

    [HideInInspector]
    public bool GameReady = false;

    public bool Turn = false;
    public float TurnTimer = 2f;
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
        GameReady = true;
        player = GameObject.Find("client").GetComponent<ClientBehaviour>();
        player.SendActionToServer((uint)DataCodes.LEVEL_LOADED);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameReady) GameSetup();
        else
        {
            if (Turn)
            {
                // TODO: PASS TURN DOESNT WORK YET.
                if (player.PlayerNum == 1)
                {
                    TurnText.GetComponent<TextMeshProUGUI>().SetText("CURRENT PLAYER TURN: BLOCKER");
                    TurnTimer -= Time.deltaTime;
                    TimerText.SetText(TurnTimer.ToString("F0"));
                    if (TurnTimer <= 0)
                    {
                        // do something.
                        Turn = false;
                        player.SendActionToServer((uint)DataCodes.PLAYER_TWO_TURN);
                        TurnTimer = 2f;
                    }
                } 
                else if (player.PlayerNum == 2)
                {
                    TurnText.GetComponent<TextMeshProUGUI>().SetText("CURRENT PLAYER TURN: RUNNER");
                    TurnTimer -= Time.deltaTime;
                    TimerText.SetText(TurnTimer.ToString("F0"));
                    if (TurnTimer <= 0)
                    {
                        // do something.
                        Turn = false;
                        player.SendActionToServer((uint)DataCodes.PLAYER_ONE_TURN);
                        TurnTimer = 5f;
                    }
                }
            }
            else
            {
                if (player.PlayerNum == 1) TurnText.GetComponent<TextMeshProUGUI>().SetText("CURRENT PLAYER TURN: RUNNER");
                else if (player.PlayerNum == 2) TurnText.GetComponent<TextMeshProUGUI>().SetText("CURRENT PLAYER TURN: BLOCKER");
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
            GameReady = false;
        }
    }

    #region Button Functions
    // Runner Actions
    public void RunnerJump() { player.SendActionToServer((uint)DataCodes.RUNNER_JUMP); }
    public void RunnerDodge() { player.SendActionToServer((uint)DataCodes.RUNNER_DODGE); }
    public void RunnerAttack() { player.SendActionToServer((uint)DataCodes.RUNNER_ATTACK); }

    // Blocker Actions
    public void BlockerObstacle() { player.SendActionToServer((uint)DataCodes.BLOCKER_OBSTACLE); }
    public void BlockerGhost() { player.SendActionToServer((uint)DataCodes.BLOCKER_ENEMY_GHOST); }
    public void BlockerGrunt() { player.SendActionToServer((uint)DataCodes.BLOCKER_ENEMY_GRUNT); }
    #endregion
}
