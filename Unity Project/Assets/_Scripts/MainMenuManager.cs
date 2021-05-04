using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
    public GameObject MainObject, Login, ServerLogin, Register;
    private EventSystem eventSystem;

    public void Start()
    {
        eventSystem = EventSystem.current;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Selectable next = eventSystem.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
            if (next != null)
            {
                InputField field = next.GetComponent<InputField>();
                if (field != null) field.OnPointerClick(new PointerEventData(eventSystem));

                eventSystem.SetSelectedGameObject(next.gameObject, new BaseEventData(eventSystem));
            }
            else Debug.Log("Next navigation element not found!");
        }
    }

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
