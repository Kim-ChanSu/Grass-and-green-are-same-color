using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BGMDatabaseManager : MonoBehaviour
{
    public static BGMDatabaseManager instance;

    void Awake() 
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("���� BGMDatabaseManager�� 2���̻� �����մϴ�.");
            Destroy(gameObject);
        }
    }

    [System.Serializable]
    private class GameBGM
    {
        public string BGMName;
        public AudioClip BGM;
    }

    [SerializeField]
    private GameBGM[] BGM;    

    public AudioClip GetBGM(string name)
    { 
        int check = 0;
        for(check = 0; check <= BGM.Length; check++)
        {
            if(check < BGM.Length)
            { 
                if(BGM[check].BGMName  == name)
                {                 
                    break;
                }
            }
        }
        if(check < BGM.Length)
        {
            GameManager.instance.BGM = BGM[check].BGMName;
            return BGM[check].BGM;
        }
        else
        { 
            Debug.LogWarning("��ġ�ϴ� ����� �����ϴ�!");
            return GameManager.instance.GetDefaultBGM();
        }
    }


    public AudioClip BGMDB(int i)
    {
        if(i < BGM.Length)
        {
            GameManager.instance.BGM = BGM[i].BGMName;
            return BGM[i].BGM;
        }
        else
        {
            Debug.LogWarning("BGMDatabaseManager�� BGMDB�Լ��� �߸��� ���� ���Խ��ϴ�.");
            return GameManager.instance.GetDefaultBGM();
        }
    }

    public int DBLength()
    {         
        return BGM.Length;
    }

}
