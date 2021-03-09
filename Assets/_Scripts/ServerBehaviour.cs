using TMPro;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;
using System.Collections.Generic;

public enum DataCodes
{
    DEBUG_MESSAGE = 0,
    PING = 1,
    PASS_TURN = 2,

    READY_PLAYER_ONE = 10,
    READY_PLAYER_TWO = 11,
    
    START_GAME = 20,
    END_GAME = 21,

    BLOCKER_DEFAULT = 100,
    BLOCKER_OBSTACLE = 101,
    BLOCKER_ENEMY_GHOST = 102,
    BLOCKER_ENEMY_GRUNT = 103,

    RUNNER_DEFAULT = 110,
    RUNNER_JUMP = 111,
    RUNNER_DODGE = 112,
    RUNNER_ATTACK = 113,
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
    public DataCodes BlockerAction, RunnerAction;

    private bool pOneReady, pTwoReady;

    [SerializeField]
    private List<NativeString64> players = new List<NativeString64>();
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

                    switch (dataCode)
                    {
                        case (uint)DataCodes.PING:
                            SendActionToClient(connections[i], (uint)DataCodes.PING);
                            break;

                        case (uint)DataCodes.DEBUG_MESSAGE:
                            SendActionToClient(connections[i], (uint)DataCodes.DEBUG_MESSAGE);
                            break;

                        case (uint)DataCodes.PASS_TURN:
                            SendActionToOther(connections[i], (uint)DataCodes.PASS_TURN);
                            break;

                        case (uint)DataCodes.READY_PLAYER_ONE:
                            pOneReady = true;
                            PlayersReady();
                            break;

                        case (uint)DataCodes.READY_PLAYER_TWO:
                            pTwoReady = true;
                            PlayersReady();
                            break;

                        case (uint)DataCodes.RUNNER_JUMP:
                            // if (BlockerAction != Obstacle) subtract lives?
                            // else continue game, maybe display success message.
                            RunnerAction = DataCodes.RUNNER_JUMP;
                            Debug.Log("Runner has jumped!");
                            break;

                        case (uint)DataCodes.RUNNER_DODGE:
                            RunnerAction = DataCodes.RUNNER_DODGE;
                            Debug.Log("Runner has dodged!");
                            break;

                        case (uint)DataCodes.RUNNER_ATTACK:
                            RunnerAction = DataCodes.RUNNER_ATTACK;
                            Debug.Log("Runner has attacked!");
                            break; 

                        case (uint)DataCodes.BLOCKER_OBSTACLE:
                            BlockerAction = DataCodes.BLOCKER_OBSTACLE;
                            Debug.Log("Blocker has placed an obstacle!");
                            break;

                        case (uint)DataCodes.BLOCKER_ENEMY_GHOST:
                            BlockerAction = DataCodes.BLOCKER_ENEMY_GHOST;
                            Debug.Log("Blocker has sent a ghost!");
                            break;

                        case (uint)DataCodes.BLOCKER_ENEMY_GRUNT:
                            BlockerAction = DataCodes.BLOCKER_ENEMY_GRUNT;
                            Debug.Log("Client has sent a grunt!");
                            break;

                        default:
                            break;
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
        Debug.LogWarning("Preparing to send message to client...");
        for (int i = 0; i < connections.Length; i++)
        {
            // Skip a connection if it is stale.
            if (!connections[i].IsCreated) continue;

            if (connections[i] != pClient)
            {
                Debug.Log("Sending message to " + connections[i].ToString());
                var writer = Driver.BeginSend(connections[i]);
                writer.WriteUInt(pAction);
                Driver.EndSend(writer);
            }
        }
    }

    private void DetermineTurnWinner()
    {
        // Check client and server actions.

        // Big if-statement or otherwise switch here comparing the two and handeling accordingly.
    }

    private void OnDestroy()
    {
        // Dispose of unmanaged memory
        Driver.Dispose();
        connections.Dispose();
    }
}
