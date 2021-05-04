using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOverTime : MonoBehaviour
{
    public float DisableAfter = 2f;
    private float timer;

    public void Update() 
    {
        timer -= Time.deltaTime;
        if (timer <= 0) gameObject.SetActive(false);
    }

    public void OnDisable()
    {
        timer = DisableAfter;
    }
}
