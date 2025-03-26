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
            Debug.LogWarning("���� ��׶��� ������ ���̽��� 2���̻� �����մϴ�.");
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
            Debug.LogWarning("��ġ�ϴ� ����� �����ϴ�!");
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
            Debug.LogWarning("BackgroundDatabaseManager�� BackGroundDB�Լ��� �߸��� ���� ���Խ��ϴ�.");
            return GameManager.instance.GetBlank();
        }
    }

    public int DBLength()
    {         
        return Background.Length;
    }

}
