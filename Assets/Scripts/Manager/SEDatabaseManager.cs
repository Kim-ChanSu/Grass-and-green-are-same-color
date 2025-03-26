using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SEDatabaseManager : MonoBehaviour
{
    public static SEDatabaseManager instance;

    void Awake() 
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("씬에 SEDatabaseManager가 2개이상 존재합니다.");
            Destroy(gameObject);
        }
    }

    [System.Serializable]
    private class GameSE
    {
        public string SEName;
        public AudioClip SE;
    }

    [SerializeField]
    private GameSE[] SE;    

    public AudioClip GetSE(string name)
    { 
        int check = 0;
        for(check = 0; check <= SE.Length; check++)
        {
            if(check < SE.Length)
            { 
                if(SE[check].SEName  == name)
                {                 
                    break;
                }
            }
        }
        if(check < SE.Length)
        {
            return SE[check].SE;
        }
        else
        { 
            Debug.LogWarning("일치하는 SE가 없습니다!");
            return GameManager.instance.GetDefaultSE();
        }
    }


    public AudioClip SEDB(int i)
    {
        if(i < SE.Length)
        {
            return SE[i].SE;
        }
        else
        {
            Debug.LogWarning("SEDatabaseManager안 SEDB함수에 잘못된 값이 들어왔습니다.");
            return GameManager.instance.GetDefaultSE();
        }
    }

    public int DBLength()
    {         
        return SE.Length;
    }

}
