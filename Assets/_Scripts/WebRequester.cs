using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine;
using DebugStuff;
using TMPro;

public class WebRequester : MonoBehaviour
{
    public TextMeshProUGUI ServerMessagesText;
    public GameObject WelcomeScreen;

    readonly string POST_URL = "https://studenthome.hku.nl/~daan.debruijn/userloginWebrequest.php?";
    readonly string POST_URL_SERVER = "https://studenthome.hku.nl/~daan.debruijn/ServerloginWebrequest.php?";
    readonly string POST_URL_REGISTER = "https://studenthome.hku.nl/~daan.debruijn/register.php?";
    readonly string POST_URL_SCORE = "https://studenthome.hku.nl/~daan.debruijn/score_insertWebrequest.php?";
    readonly string POST_URL_MESSAGE = "https://studenthome.hku.nl/~daan.debruijn/messageWebrequest.php?";
    readonly string POST_URL_USERINFO = "https://studenthome.hku.nl/~daan.debruijn/getuserinfoWebrequest.php?";

    private void Start()
    {
        ServerMessagesText.SetText("");
    }

    /// <summary>
    /// Sends a webrequest to the server to log player in with specified username and password.
    /// </summary>
    /// <param name="pUsername"></param>
    /// <param name="pPassword"></param>
    /// <returns></returns>
    public IEnumerator Login(string pUsername, string pPassword)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", pUsername);
        form.AddField("password", pPassword);

        using (UnityWebRequest www = UnityWebRequest.Post(POST_URL, form))
        {
            yield return www.SendWebRequest();
            string _result = www.downloadHandler.text.Remove(0, 1);
            Debug.Log(_result);

            if (www.isNetworkError || www.isHttpError)
            {
                ServerMessagesText.SetText(www.error);
            }
            else
            {
                if (!_result.Contains("(err)"))
                {
                    Main.Instance.CurrentUser = new DataStructs.User()
                    {
                        // downloadhandler should return UserID on succesful login.
                        UserID = int.Parse(_result),
                        Score = 0
                    };
                    Debug.Log("User " + _result + " has succesfully logged in");

                    Main.Instance.PlayerObject = new GameObject("client");
                    Main.Instance.PlayerObject.AddComponent<ClientBehaviour>();

                    WelcomeScreen.SetActive(true);
                    ServerMessagesText.SetText("");
                }
                else HandleError(_result);
            }
        }
    }

    public IEnumerator ServerLogin(string pID, string pPassword)
    {
        WWWForm form = new WWWForm();
        form.AddField("id", pID);
        form.AddField("password", pPassword);

        using (UnityWebRequest www = UnityWebRequest.Post(POST_URL_SERVER, form))
        {
            yield return www.SendWebRequest();
            string _result = www.downloadHandler.text.Remove(0, 1);

            if (www.isNetworkError || www.isHttpError)
            {
                ServerMessagesText.SetText(www.error);
            }
            else
            {
                if (!_result.Contains("(err)"))
                {
                    Debug.Log("Logged in as server " + _result);
                    Main.Instance.CurrentServer = new DataStructs.Server()
                    {
                        ServerID = _result
                    };
                    SceneManager.LoadScene("Server");
                }
                else HandleError(_result);
            }
        }

    }

    /// <summary>
    /// Send a webrequest to the server to register a new user.
    /// </summary>
    /// <param name="pUsername">Chosen username.</param>
    /// <param name="pPassword">Chosen password.</param>
    /// <returns></returns>
    public IEnumerator Register(string pUsername, string pPassword)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", pUsername);
        form.AddField("password", pPassword);

        using (UnityWebRequest www = UnityWebRequest.Post(POST_URL_REGISTER, form))
        {
            yield return www.SendWebRequest();
            string _result = www.downloadHandler.text.Remove(0, 1);

            if (www.isNetworkError || www.isHttpError)
            {
                ServerMessagesText.SetText(www.error);
            }
            else
            {
                if (!_result.Contains("(err)")) Debug.Log(_result);
                else HandleError(_result);
            }
        }
    }

    /// <summary>
    /// POST function that sends a form to the online server with the neccessairy details to post a new score.
    /// </summary>
    /// <param name="pServerID">Game server ID.</param>
    /// <param name="pServerPassword">Game server password.</param>
    /// <param name="pUserID">Username of the user that scored.</param>
    /// <param name="pScore">User's score.</param>
    /// <returns></returns>
    public IEnumerator PostScore(int pUserID, int pScore)
    {
        if (!Main.Instance.ServerBuild)
        {
            Debug.LogError("Not allowed to POST scores as a Non-Server instance.");
            yield return null;
        }

        WWWForm form = new WWWForm();
        form.AddField("user", pUserID);
        form.AddField("score", pScore);

        using (UnityWebRequest www = UnityWebRequest.Post(POST_URL_SCORE, form))
        {
            yield return www.SendWebRequest();
            string _result = www.downloadHandler.text.Remove(0, 1);

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                if (!_result.Contains("(err)")) Debug.Log(_result);
                else HandleError(_result);
            }
        }
    }

    public IEnumerator PostMessage(string pMessage)
    {
        WWWForm form = new WWWForm();
        form.AddField("json", pMessage);

        using (UnityWebRequest www = UnityWebRequest.Post(POST_URL_MESSAGE, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
            }
        }
    }

    public IEnumerator GetUserInfo(string pUsername)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", pUsername);

        using (UnityWebRequest www = UnityWebRequest.Post(POST_URL_USERINFO, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                JsonUtility.FromJson<DataStructs.User>(www.downloadHandler.text);
            }
        }
    }

    private void HandleError(string pErrorCode)
    {
        switch (pErrorCode)
        {
            case "(err)0":
                ServerMessagesText.SetText("Username or password is incorrect!");
                break;

            case "(err)1":
                ServerMessagesText.SetText("Username does not exist!");
                break;

            case "(err)2":
                    ServerMessagesText.SetText("Username already exists");
                break;
        }
    }
}


