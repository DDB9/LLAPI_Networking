using UnityEngine;
using Unity.Collections;
using System.Collections;
using Unity.Networking.Transport;
using UnityEngine.SceneManagement;

public class ClientBehaviour : MonoBehaviour
{
    #region Network Variables
    public NetworkDriver Driver;
    public NetworkConnection Connection;
    public bool Connected;
    public bool Done;
    public float PingRate = 1f;
    #endregion

    #region Generic Variables
    public int PlayerNum;
    public bool yourTurn = false;
    public bool GameReady;
    #endregion

    #region Singleton
    private static ClientBehaviour _instance;
    public static ClientBehaviour Instance { get { return _instance; } }
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

    void Start()
    {
        // Create Network Driver for the client, and an adress for the server.
        Driver = NetworkDriver.Create();
        Connection = default;

        // Connect to server.
        var endpoint = NetworkEndPoint.LoopbackIpv4;
        endpoint.Port = 9000;
        Connection = Driver.Connect(endpoint);
        
        InvokeRepeating(nameof(PingServer), 0f, 1f);
    }

    void Update()
    {
        // Update Network Driver.
        Driver.ScheduleUpdate().Complete();

        // Check connection to server.
        if (!Connection.IsCreated)
        {
            if (!Done)
            {
                Debug.LogWarning("Something went wrong trying to connect to the server.");
            }
            return;
        }

        // Pop event for this connection.
        DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = Connection.PopEvent(Driver, out stream)) != NetworkEvent.Type.Empty)
        {

            // On a connect event, write and send number to the sever.
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("Succesfully connected to the server!");
                SendActionToServer(Main.Instance.ConcatInt((int)DataCodes.USERINFO, Main.Instance.CurrentUser.UserID));

                Connected = true;
            }

            // On a data event, display recieved number form server.
            else if (cmd == NetworkEvent.Type.Data)
            {
                uint dataCode = stream.ReadUInt();
                switch (dataCode)
                {
                    case (uint)DataCodes.LOGIN_ERROR:
                        Driver.Disconnect(Connection);
                        Main.Instance.Web.ServerMessagesText.SetText("Something went wrong trying to log in. Please try again.");
                        SceneManager.LoadScene("Login");
                        break;

                    case (uint)DataCodes.DEBUG_MESSAGE:
                        Debug.Log("Debug message recieved!");
                        break;
                    
                    case (uint)DataCodes.PING:
                        break;

                    case (uint)DataCodes.ASSIGN_P1:
                        PlayerNum = 1;
                        break;
                    
                    case (uint)DataCodes.ASSIGN_P2:
                        PlayerNum = 2;
                        break;

                    case (uint)DataCodes.PASS_TURN:
                        GameManager.Instance.Turn = true;
                        break;

                    case (uint)DataCodes.START_GAME:
                        Debug.Log("Players ready! Starting game...");
                        GameReady = true;
                        break;

                    case (uint)DataCodes.P1_ROUND_WON:
                        GameManager.Instance.AssignScore(DataCodes.P1_ROUND_WON);
                        break;

                    case (uint)DataCodes.P1_ROUND_LOST:
                        GameManager.Instance.AssignScore(DataCodes.P1_ROUND_LOST);
                        break;

                    case (uint)DataCodes.ROUND_TIE:
                        GameManager.Instance.AssignScore(DataCodes.ROUND_TIE);
                        break;
                }
            }

            // On a disconnect event, display disconnect message and reset connection.
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from the server.");
                Connection = default;
                Connected = false;
                CancelInvoke();
                SceneManager.LoadScene("Login");

                // ! Send a message to the server that there was a disconnect so it can shut down the game.
                //Disconnect client.
                //Done = true;
                //Connection.Disconnect(Driver);
                //Connection = default;
                //Connected = false;
            }
        }
    }

    public void ReadyUp()
    {
        if (PlayerNum == 1) SendActionToServer((uint)DataCodes.P1_READY);
        else if (PlayerNum == 2) SendActionToServer((uint)DataCodes.P2_READY);
    }

    public NativeString64 CreateUser()
    {
        Main.Instance.CurrentUser.PlayerNum = PlayerNum;
        Main.Instance.CurrentUser.Connection = Connection;

        StartCoroutine(Main.Instance.Web.PostMessage(JsonUtility.ToJson(Main.Instance.CurrentUser)));
        return JsonUtility.ToJson(Main.Instance.CurrentUser);
    }

    public void SendActionToServer(uint pAction)
    {
        var writer = Driver.BeginSend(Connection);
        writer.WriteUInt(pAction);
        Driver.EndSend(writer);
    }      
    public void SendActionToServer(int pAction)
    {
        var writer = Driver.BeginSend(Connection);
        writer.WriteInt(pAction);
        Driver.EndSend(writer);
    }  
    public void SendActionToServer(byte pAction)
    {
        var writer = Driver.BeginSend(Connection);
        writer.WriteByte(pAction);
        Driver.EndSend(writer);
    }
    public void SendActionToServer(ulong pAction)
    {
        var writer = Driver.BeginSend(Connection);
        writer.WriteULong(pAction);
        Driver.EndSend(writer);
    }
    public void SendActionToServer(NativeString64 pAction)
    {
        var writer = Driver.BeginSend(Connection);
        writer.WriteString(pAction);
        Driver.EndSend(writer);
    }
    
    public void PingServer()
    {
        if (Connected)
        {
            var writer = Driver.BeginSend(Connection);
            writer.WriteUInt((uint)DataCodes.PING);
            Driver.EndSend(writer);
        }
    }

    public void DC()
    {
        Driver.Dispose();
        Connected = false;
    }
    private void OnDestroy()
    {
        Driver.Dispose();
        Connected = false;
    }
}
