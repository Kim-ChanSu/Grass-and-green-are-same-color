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
            Debug.LogWarning("씬에 BGMDatabaseManager가 2개이상 존재합니다.");
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
            Debug.LogWarning("일치하는 브금이 없습니다!");
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
            Debug.LogWarning("BGMDatabaseManager안 BGMDB함수에 잘못된 값이 들어왔습니다.");
            return GameManager.instance.GetDefaultBGM();
        }
    }

    public int DBLength()
    {         
        return BGM.Length;
    }

}
