using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundController : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider audioSlider;

    public void BGMValueControl()
    { 
        float value = audioSlider.value;

        if(value <= -39.99f)
        { 
            audioMixer.SetFloat("BGMSound", -80);
        } 
        else
        { 
            audioMixer.SetFloat("BGMSound", value);
        }

        /* 이거 세이브 로드 아니더라도 넣어야함 ㅇㅇㅋㅋ
        비커즈 설정창 열때마다 슬라이더 초기화 안되게 하려면 
        라고 생각했는데 슬라이더 저거는 저장되네 걍 껏다켜도 설정유지 만들때 쓰면 될듯

        float test;
        audioMixer.GetFloat("BGMSound", out test);
        Debug.Log(test);
        */
    }

    public void SEValueControl()
    { 
        float value = audioSlider.value;

        if(value <= -39.99f)
        { 
            audioMixer.SetFloat("SESound", -80);
        } 
        else
        { 
            audioMixer.SetFloat("SESound", value); 
        }              
    }
}
