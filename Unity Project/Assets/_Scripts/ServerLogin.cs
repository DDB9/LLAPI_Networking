using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ServerLogin : MonoBehaviour
{
    public InputField IDField;
    public InputField PasswordField;
    public Button SubmitButton;

    // Start is called before the first frame update
    void Start()
    {
        // ON CLICK call login function with username and password field values.
        SubmitButton.onClick.AddListener(() =>
        {
            // First make sure all fields have been filled.
            if (string.IsNullOrWhiteSpace(IDField.text) || string.IsNullOrWhiteSpace(PasswordField.text))
            {
                Main.Instance.Web.ServerMessagesText.SetText("Not all fields were filled!");
            }
            else
            {
                StartCoroutine(Main.Instance.Web.ServerLogin(IDField.text, PasswordField.text));
                Main.Instance.Web.ServerMessagesText.SetText("Loading...");
            }
        });
    }
}
