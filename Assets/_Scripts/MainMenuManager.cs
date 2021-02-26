using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public GameObject MainObject, Login, ServerLogin, Register;

    public void Return()
    {
        Login.SetActive(false);
        ServerLogin.SetActive(false);
        Register.SetActive(false);

        Main.Instance.Web.WelcomeScreen.SetActive(false);
        Main.Instance.Web.ServerMessagesText.SetText("");

        MainObject.SetActive(true);
    }

    public void OpenLogin()
    {
        Login.SetActive(true);
        ServerLogin.SetActive(false);
        Register.SetActive(false);
        MainObject.SetActive(false);
    }

    public void OpenServerLogin()
    {
        Login.SetActive(false);
        ServerLogin.SetActive(true);
        Register.SetActive(false);
        MainObject.SetActive(false);
    }

    public void OpenRegister()
    {
        Login.SetActive(false);
        ServerLogin.SetActive(false);
        Register.SetActive(true);
        MainObject.SetActive(false);
    }
}
