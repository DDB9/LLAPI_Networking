using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public ClientBehaviour player;
    public bool Turn = false;
    public float TurnTimer = 2f;

    [Header("Arrays")]
    public Button[] Buttons;
    public TextMeshProUGUI[] Timers;
    public GameObject[] UIs;

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
        //foreach(Button button in Buttons) { button.interactable = false; }
        foreach(GameObject ui in UIs) { ui.SetActive(false); }

        player = GameObject.Find("client").GetComponent<ClientBehaviour>();
        if (player.PlayerNum == 1)
        {
            timerText = Timers[0];
            playerUI = UIs[0];
            playerUI.SetActive(true);
        }
        else if (player.PlayerNum == 2)
        {
            timerText = Timers[1];
            playerUI = UIs[1];
            playerUI.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        var timeLeft = TurnTimer -= Time.deltaTime;
        timerText.SetText(timeLeft.ToString());
        if (timeLeft < 0)
        {
            // do something.
            Turn = false;
            ClientBehaviour.Instance.SendActionToServer((uint)DataCodes.PASS_TURN);
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
