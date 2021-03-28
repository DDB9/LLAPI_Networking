using Unity.Networking.Transport;
using UnityEngine;
using System;

public class DataStructs : MonoBehaviour
{
    [Serializable]
    public class ScoreData
    {
        public int MessageCode;
        public string Username;
        public float ScoreToSend;
    }

    [Serializable]
    public class User
    {
        public int UserID;
        public int PlayerNum;
        public int Score;
        public NetworkConnection Connection;
    }

    [Serializable]
    public class Server
    {
        public string ServerID;
    }
}
