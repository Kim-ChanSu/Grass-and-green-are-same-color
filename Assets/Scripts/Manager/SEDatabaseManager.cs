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
            Debug.LogWarning("���� SEDatabaseManager�� 2���̻� �����մϴ�.");
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
            Debug.LogWarning("��ġ�ϴ� SE�� �����ϴ�!");
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
            Debug.LogWarning("SEDatabaseManager�� SEDB�Լ��� �߸��� ���� ���Խ��ϴ�.");
            return GameManager.instance.GetDefaultSE();
        }
    }

    public int DBLength()
    {         
        return SE.Length;
    }

}
