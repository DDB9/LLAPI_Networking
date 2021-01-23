using UnityEngine;
using Unity.Collections;
using UnityEngine.Assertions;
using Unity.Networking.Transport;

public enum DataCodes
{
    READY_PLAYER_ONE = 11,
    READY_PLAYER_TWO = 12,

    BLOCKER_DEFAULT = 100,
    BLOCKER_OBSTACLE = 101,
    BLOCKER_ENEMY_GHOST = 102,
    BLOCKER_ENEMY_GRUNT = 103,

    RUNNER_DEFAULT = 110,
    RUNNER_JUMP = 111,
    RUNNER_DODGE = 112,
    RUNNER_ATTACK = 113,

    OK = 200,
    START_GAME = 300,
    PASS_TURN = 310,
    END_GAME = 320,
}

public class ServerBehaviour : MonoBehaviour
{
    #region Network Variables
    public NetworkDriver Driver;
    private NativeList<NetworkConnection> connections;
    #endregion

    #region Generic Variables
    public DataCodes ServerAction, ClientAction;

    private bool pOneReady, pTwoReady;
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
                    byte dataCode = stream.ReadByte();
                    switch (dataCode)
                    {
                        case (byte)DataCodes.READY_PLAYER_ONE:
                            pOneReady = true;
                            PlayersReady();
                            break;

                        case (byte)DataCodes.READY_PLAYER_TWO:
                            pTwoReady = true;
                            PlayersReady();
                            break;

                        case (byte)DataCodes.RUNNER_JUMP:
                            Debug.Log("Runner has jumped!");
                            ClientAction = DataCodes.RUNNER_JUMP;
                            break;

                        case (byte)DataCodes.RUNNER_DODGE:
                            Debug.Log("Runner has dodged!");
                            ClientAction = DataCodes.RUNNER_DODGE;
                            break;

                        case (byte)DataCodes.RUNNER_ATTACK:
                            Debug.Log("Runner has attacked!");
                            ClientAction = DataCodes.RUNNER_ATTACK;
                            break;

                        case (byte)DataCodes.BLOCKER_OBSTACLE:
                            Debug.Log("Blocker has placed an obstacle!");
                            ClientAction = DataCodes.BLOCKER_OBSTACLE;
                            break;

                        case (byte)DataCodes.BLOCKER_ENEMY_GHOST:
                            Debug.Log("Blocker has sent a ghost!");
                            ClientAction = DataCodes.BLOCKER_ENEMY_GHOST;
                            break;

                        case (byte)DataCodes.BLOCKER_ENEMY_GRUNT:
                            Debug.Log("Client has sent a grunt!");
                            ClientAction = DataCodes.BLOCKER_ENEMY_GRUNT;
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
