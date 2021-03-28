using TMPro;
using UnityEngine;
using Unity.Collections;
using System.Collections;
using Unity.Networking.Transport;
using System.Collections.Generic;

public enum DataCodes
{
    DEBUG_MESSAGE = 0,
    PING = 1,
    PASS_TURN = 2,
    USERINFO = 3,
    LOGIN_ERROR = 4,

    ASSIGN_P1 = 10,
    ASSIGN_P2 = 11,
    P1_READY = 12,
    P2_READY = 13,
    
    START_GAME = 20,
    END_GAME = 21,
    P1_ROUND_WON = 22,
    P1_ROUND_LOST = 23,
    ROUND_TIE = 26,

    P1_DEFAULT = 100,
    P1_STEEN = 101,
    P1_PAPIER = 102,
    P1_SCHAAR = 103,

    P2_DEFAULT = 110,
    P2_STEEN = 111,
    P2_PAPIER = 112,
    P2_SCHAAR = 113,

    // Incoming Score to post = 150;
}

// TODO HANDLE DISCONNECTS AND TIMEOUTS
public class ServerBehaviour : MonoBehaviour
{
    #region Network Variables
    public NetworkDriver Driver;
    private NativeList<NetworkConnection> connections;
    #endregion

    #region Generic Variables
    public TextMeshProUGUI ServerNumber, ServerConnectionsNumber;
    public DataCodes P1_ACTION, P2_ACTION;
    [HideInInspector]
    public string serverPassword;

    public List<DataStructs.User> Players = new List<DataStructs.User>();
    private bool pOneReady, pTwoReady;
    private bool duplicatePlayer;
    #endregion

    #region Singleton
    private static ServerBehaviour _instance;
    public static ServerBehaviour Instance { get { return _instance; } }
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
        // Create the NetworkDriver.
        Driver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = 9000;
        if (Driver.Bind(endpoint) != 0) Debug.LogWarning("failed to bind to port 9000");
        else Driver.Listen();

        connections = new NativeList<NetworkConnection>(2, Allocator.Persistent);

        ServerNumber.SetText("Server number: " + Main.Instance.CurrentServer.ServerID);
        Main.Instance.ServerBuild = true;
    }

    void Update()
    {
        // Update NetworkDriver
        Driver.ScheduleUpdate().Complete();

        #region Update Connection List
        // Remove stale connections
        for (int i = 0; i < connections.Length; i++)
        {
            if (!connections[i].IsCreated)
            {
                connections.RemoveAtSwapBack(i);
                --i;
            }
        }

        // Accept new incoming connections
        NetworkConnection nc;
        while ((nc = Driver.Accept()) != default)
        {
            connections.Add(nc);
            Debug.Log("Accepted incoming connection");
        }

        ServerConnectionsNumber.SetText("Open Connections: " + connections.Length);
        #endregion

        // Create a DataStreamReader and loop through all connections.
        DataStreamReader stream;
        for (int i = 0; i < connections.Length; i++)
        {
            // Skip a connection if it is stale.
            if (!connections[i].IsCreated) continue;

            // Pop event for current connection. Run while there is an event that needs to be processed.
            NetworkEvent.Type cmd;
            while ((cmd = Driver.PopEventForConnection(connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    #region uint data
                    uint dataCode = stream.ReadByte();
                    try
                    {
                        switch (dataCode)
                        {
                            case (uint)DataCodes.PING:
                                SendActionToClient(connections[i], (uint)DataCodes.PING);
                                break;

                            case (uint)DataCodes.DEBUG_MESSAGE:
                                SendActionToClient(connections[i], (uint)DataCodes.DEBUG_MESSAGE);
                                break;

                            case (uint)DataCodes.P1_READY:
                                pOneReady = true;
                                PlayersReady();
                                break;

                            case (uint)DataCodes.P2_READY:
                                pTwoReady = true;
                                PlayersReady();
                                break;

                            case (uint)DataCodes.PASS_TURN:
                                SendActionToOther(connections[i], (uint)DataCodes.PASS_TURN);
                                break;

                            case (uint)DataCodes.P1_STEEN:
                                P1_ACTION = DataCodes.P1_STEEN;
                                Debug.Log("Runner has jumped!");
                                SendActionToOther(connections[i], (uint)DataCodes.PASS_TURN);
                                break;

                            case (uint)DataCodes.P1_PAPIER:
                                P1_ACTION = DataCodes.P1_PAPIER;
                                Debug.Log("Runner has dodged!");
                                SendActionToOther(connections[i], (uint)DataCodes.PASS_TURN);
                                break;

                            case (uint)DataCodes.P1_SCHAAR:
                                P1_ACTION = DataCodes.P1_SCHAAR;
                                Debug.Log("Runner has attacked!");
                                SendActionToOther(connections[i], (uint)DataCodes.PASS_TURN);
                                break;

                            case (uint)DataCodes.P2_STEEN:
                                P2_ACTION = DataCodes.P2_STEEN;
                                Debug.Log("Blocker has placed an obstacle!");
                                DetermineTurnWinner();
                                break;

                            case (uint)DataCodes.P2_PAPIER:
                                P2_ACTION = DataCodes.P2_PAPIER;
                                Debug.Log("Blocker has sent a ghost!");
                                DetermineTurnWinner();
                                break;

                            case (uint)DataCodes.P2_SCHAAR:
                                P2_ACTION = DataCodes.P2_SCHAAR;
                                Debug.Log("Client has sent a grunt!");
                                DetermineTurnWinner();
                                break;

                            case (uint)DataCodes.END_GAME:
                                EndGame();
                                break;

                            default:
                                break;
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(e.Message);
                    }

                    if (dataCode.ToString().StartsWith("3"))
                    {
                        int _data = int.Parse(dataCode.ToString().Split('3')[1]);
                        if (Players.Count <= 0)
                        {
                            Players.Add(new DataStructs.User()
                            {
                                UserID = _data,
                                PlayerNum = 1,
                                Score = 0,
                                Connection = connections[i]
                            });
                            SendActionToClient(connections[i], (uint)DataCodes.ASSIGN_P1);
                        }
                        else
                        {
                            foreach (DataStructs.User user in Players)
                            {
                                // Check if the player has already logged in.
                                if (user.UserID == _data)
                                {
                                    SendActionToClient(connections[i], (uint)DataCodes.LOGIN_ERROR);
                                    duplicatePlayer = true;
                                }
                            }

                            if (!duplicatePlayer)
                            {
                                Players.Add(new DataStructs.User()
                                {
                                    UserID = _data,
                                    PlayerNum = 2,
                                    Score = 0,
                                    Connection = connections[i]
                                });
                                SendActionToClient(connections[i], (uint)DataCodes.ASSIGN_P2);
                            }
                        }
                    }
                    #endregion
                }

                // On a disconnect event, reset connection to default values, making it a stale connection.
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("A user disconnected from the server.");
                    connections[i] = default;
                }
            }
        }
    }

    public bool PlayersReady()
    {
        if (pOneReady && pTwoReady)
        {
            SendActionToClients((uint)DataCodes.START_GAME);
            return true;
        }
        return false;
    }

    public void EndGame()
    {
        // First, send the scores to the server.
        foreach (DataStructs.User player in Players)
        {
            // Post the scores to the database...
            StartCoroutine(Main.Instance.Web.PostScore(player.UserID, player.Score));
        }

        // Shutdown server.

    }

    public void SendActionToClients(uint pAction)
    {
        for (int i = 0; i < connections.Length; i++)
        {
            // Skip a connection if it is stale.
            if (!connections[i].IsCreated) continue;

            var writer = Driver.BeginSend(connections[i]);
            writer.WriteUInt(pAction);
            Driver.EndSend(writer);
        }
    }   

    public void SendActionToClient(NetworkConnection pClient, NativeString64 pAction)
    {
        if (!pClient.IsCreated)
        {
            Debug.LogWarning("You are trying to send a message to a stale connection");
            return;
        }

        var writer = Driver.BeginSend(pClient);
        writer.WriteString(pAction);
        Driver.EndSend(writer);
    }    
    public void SendActionToClient(NetworkConnection pClient, uint pAction)
    {
        if (!pClient.IsCreated)
        {
            Debug.LogWarning("You are trying to send a message to a stale connection");
            return;
        }

        var writer = Driver.BeginSend(pClient);
        writer.WriteUInt(pAction);
        Driver.EndSend(writer);
    }
    /// <summary>
    /// Sends an action to the other player from the one that the server has recieved a message from.
    /// </summary>
    /// <param name="pClient">Client that sent previous action to the server.</param>
    /// <param name="pAction"></param>
    public void SendActionToOther(NetworkConnection pClient, uint pAction)
    {
        for (int i = 0; i < connections.Length; i++)
        {
            // Skip a connection if it is stale.
            if (!connections[i].IsCreated) continue;

            if (connections[i] != pClient)
            {
                var writer = Driver.BeginSend(connections[i]);
                writer.WriteUInt(pAction);
                Driver.EndSend(writer);
            }
        }
    }

    private void DetermineTurnWinner()
    {
        switch (P1_ACTION)
        {
            case DataCodes.P1_STEEN:
                if (P2_ACTION == DataCodes.P2_PAPIER)
                {
                    foreach (DataStructs.User player in Players)
                    {
                        if (player.PlayerNum == 2) player.Score += 1;
                    }
                    SendActionToClients((uint)DataCodes.P1_ROUND_LOST);
                }
                else if (P2_ACTION == DataCodes.P2_STEEN) SendActionToClients((uint)DataCodes.ROUND_TIE);
                else
                {
                    foreach (DataStructs.User player in Players)
                    {
                        if (player.PlayerNum == 1) player.Score += 1;
                    }
                    SendActionToClients((uint)DataCodes.P1_ROUND_WON);
                }
                break;


            case DataCodes.P1_PAPIER:
                if (P2_ACTION == DataCodes.P2_SCHAAR)
                {
                    foreach (DataStructs.User player in Players)
                    {
                        if (player.PlayerNum == 2) player.Score += 1;
                    }
                    SendActionToClients((uint)DataCodes.P1_ROUND_LOST);
                }
                else if (P2_ACTION == DataCodes.P2_PAPIER) SendActionToClients((uint)DataCodes.ROUND_TIE);
                else
                {
                    foreach (DataStructs.User player in Players)
                    {
                        if (player.PlayerNum == 1) player.Score += 1;
                    }
                    SendActionToClients((uint)DataCodes.P1_ROUND_WON);
                }
                break;


            case DataCodes.P1_SCHAAR:
                if (P2_ACTION == DataCodes.P2_STEEN)
                {
                    foreach (DataStructs.User player in Players)
                    {
                        if (player.PlayerNum == 2) player.Score += 1;
                    }
                    SendActionToClients((uint)DataCodes.P1_ROUND_LOST);
                }
                else if (P2_ACTION == DataCodes.P2_SCHAAR) SendActionToClients((uint)DataCodes.ROUND_TIE);
                else
                {
                    foreach (DataStructs.User player in Players)
                    {
                        if (player.PlayerNum == 1) player.Score += 1;
                    }
                    SendActionToClients((uint)DataCodes.P1_ROUND_WON);
                }
                break;

            default:
                Debug.LogError("No value to compare!");
                break;
        }

        // TODO DON'T FORGET TO "RESET" ROUND FOR AS FAR AS NEEDED AND ASSIGN POINTS.
    }

    private void OnDestroy()
    {
        // Dispose of unmanaged memory
        Driver.Dispose();
        connections.Dispose();
    }
}
