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
    public User CurrentUser;
    public Server CurrentServer;

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
}
