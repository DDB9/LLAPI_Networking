using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Networking.Transport;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public bool ServerBuild;

    private static Main _instance;
    public static Main Instance { get { return _instance; } }
    
    public WebRequester Web; 
    public DataStructs.User CurrentUser;
    public DataStructs.Server CurrentServer;
    public GameObject PlayerObject;

    [HideInInspector]
    public NetworkConfigParameter NetworkConfig = new NetworkConfigParameter()
    {
        disconnectTimeoutMS = 30000,
    };

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Web = GetComponent<WebRequester>();
    }

    public int ConcatInt(int a, int b)
    {
        string _stringA = a.ToString();
        string _stringB = b.ToString();

        string _concatString = _stringA + _stringB;
        int _concatInt = int.Parse(_concatString);

        return _concatInt;
    }    
    public uint ConcatInt(uint a, uint b)
    {
        string _stringA = a.ToString();
        string _stringB = b.ToString();

        string _concatString = _stringA + _stringB;
        uint _concatInt = uint.Parse(_concatString);

        return _concatInt;
    }
}
