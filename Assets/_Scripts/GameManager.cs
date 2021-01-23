using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public ClientBehaviour player;
    public Button[] Buttons;

    public bool Turn = false;
    public TextMeshProUGUI TimerText;
    public float TurnTimer = 2f;

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

        // Send ready message to server depending on player number so the game can start.
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            ClientBehaviour.Instance.PlayerNum = 1;
            ClientBehaviour.Instance.SendActionToServer((uint)DataCodes.READY_PLAYER_ONE);
        }
        else if (SceneManager.GetActiveScene().buildIndex == 2)
        {
            ClientBehaviour.Instance.PlayerNum = 2;
            ClientBehaviour.Instance.SendActionToServer((uint)DataCodes.READY_PLAYER_TWO);
        }
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        foreach(Button button in Buttons)
        {
            button.interactable = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        var timeLeft = TurnTimer -= Time.deltaTime;
        TimerText.SetText(timeLeft.ToString());
        if (timeLeft < 0)
        {
            // do something.
            Turn = false;
            ClientBehaviour.Instance.SendActionToServer((uint)DataCodes.PASS_TURN);
        }
    }
}
