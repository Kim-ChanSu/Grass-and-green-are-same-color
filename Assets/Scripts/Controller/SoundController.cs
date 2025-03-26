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

        /* �̰� ���̺� �ε� �ƴϴ��� �־���� ��������
        ��Ŀ�� ����â �������� �����̴� �ʱ�ȭ �ȵǰ� �Ϸ��� 
        ��� �����ߴµ� �����̴� ���Ŵ� ����ǳ� �� �����ѵ� �������� ���鶧 ���� �ɵ�

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
