using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerController : MonoBehaviour
{
    public static TimerController instance;

    void Awake() 
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("씬에 타이머 컨트롤러가 2개이상 존재합니다.");
            Destroy(gameObject);
        }
    }

    private float SettingTime = 0.0f;
    private float TimerTime = 0.0f;

    void Update()
    {
        if(GameManager.instance.isTimer == true)
        { 
            TimerTime += Time.deltaTime;
            if(TimerTime >= SettingTime)
            { 
                TimerTime = 0.0f;
                //TalkManager.instance.OnTalkBackground();
                GameManager.instance.isTimer = false;
                this.gameObject.SetActive(false);
               
            }
        }
    }

    public void SetTimer(float Time)
    {
        SettingTime = Time;
        TimerTime = 0.0f;
        GameManager.instance.isTimer = true;
    }
}
