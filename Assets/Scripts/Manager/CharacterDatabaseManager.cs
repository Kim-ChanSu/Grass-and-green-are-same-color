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
            Debug.LogWarning("씬에 캐릭터 데이터 베이스가 2개이상 존재합니다.");
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
