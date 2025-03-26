using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDatabaseManager : MonoBehaviour
{
    public static CharacterDatabaseManager instance;

    void Awake() 
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("���� ĳ���� ������ ���̽��� 2���̻� �����մϴ�.");
            Destroy(gameObject);
        }
    }

    [SerializeField]
    private Character[] Talker;

    public Character CharacterDB(int i)
    {       
        return Talker[i];
    }

    public int DBLength()
    {         
        return Talker.Length;
    }

}
