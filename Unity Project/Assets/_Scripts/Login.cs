using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Login : MonoBehaviour
{
    public InputField UsernameField;
    public InputField PasswordField;
    public Button SubmitButton;
    public Button PlayButton;

    // Start is called before the first frame update
    void Start()
    {
        // ON CLICK call login function with username and password field values.
        SubmitButton.onClick.AddListener(() =>
        {
            // First make sure all fields have been filled.
            if (string.IsNullOrWhiteSpace(UsernameField.text) || string.IsNullOrWhiteSpace(PasswordField.text))
            {
                Main.Instance.Web.ServerMessagesText.SetText("Not all fields were filled!");
            }
            else
            {
                StartCoroutine(Main.Instance.Web.Login(UsernameField.text, PasswordField.text));
                Main.Instance.Web.ServerMessagesText.SetText("Loading...");
            }
        });
    }
}
