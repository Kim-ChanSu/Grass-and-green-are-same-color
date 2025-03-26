using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class BackgroundDatabaseManager : MonoBehaviour
{
    public static BackgroundDatabaseManager instance;

    void Awake() 
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("씬에 백그라운드 데이터 베이스가 2개이상 존재합니다.");
            Destroy(gameObject);
        }
    }

    [System.Serializable]
    private class GameBackground
    {
        public string BackgroundName;
        public Sprite BackgroundImage;
    }

    [SerializeField]
    private GameBackground[] Background;

    public Sprite GetBackGround(string name)
    { 
        int check = 0;
        for(check = 0; check <= Background.Length; check++)
        {
            if(check < Background.Length)
            { 
                if(Background[check].BackgroundName  == name)
                {                 
                    break;
                }
            }
        }
        if(check < Background.Length)
        {
            GameManager.instance.Background = Background[check].BackgroundName;
            return Background[check].BackgroundImage;
        }
        else
        { 
            Debug.LogWarning("일치하는 배경이 없습니다!");
            return GameManager.instance.GetBlank();
        }
    }


    public Sprite BackGroundDB(int i)
    {
        if(i < Background.Length)
        {
            GameManager.instance.Background = Background[i].BackgroundName;
            return Background[i].BackgroundImage;
        }
        else
        {
            Debug.LogWarning("BackgroundDatabaseManager안 BackGroundDB함수에 잘못된 값이 들어왔습니다.");
            return GameManager.instance.GetBlank();
        }
    }

    public int DBLength()
    {         
        return Background.Length;
    }

}
