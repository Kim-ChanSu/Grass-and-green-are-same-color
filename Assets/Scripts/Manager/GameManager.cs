using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


[System.Serializable]
public class GameVariable
{ 
    public string VarName;
    public int Var;
}

[System.Serializable]
public class GameSwitch
{ 
    public string SwitchName;
    public bool Switch;
}

[System.Serializable]
public class Log
{ 
    public string[] TalkerNameLog;
    public string[] TalkLog;
}     

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    void Awake() 
    {
        if (instance == null)
        {
            DontDestroyOnLoad(this.gameObject);
            instance = this;
        }
        else
        {
            Debug.LogWarning("���� ���ӸŴ����� 2���̻� �����մϴ�.");
            Destroy(gameObject);
        }

        CheckCSVNum = 0;
        Debug.Log("CheckCSVNum = " + CheckCSVNum);

        CSVName = StartCSVName; // ó�� �����ϴ� csv�����̸�
        KeyCheck = false;

        CSVFolder = Application.dataPath + "/StreamingAssets/CSV/"; // �����

        GameLog.TalkerNameLog = new string[LogLength];
        GameLog.TalkLog = new string[LogLength];
        for(int i = 0; i < GameLog.TalkLog.Length; i++)
        { 
            GameLog.TalkLog[i] = "";
            GameLog.TalkerNameLog[i] = "";
            
        }

        LogNum = -1; // ������ ++�� ���� �ҷ������Ҳ��� -1�� ������

        isMain = true;
    }

    void Start()
    { 
        Debug.Log("��ϵ� ĳ������ ������ " + CharacterDatabaseManager.instance.DBLength());
    }

    [HideInInspector]
    public string CSVFolder;
    public int LogLength = 30;
    public string StartCSVName;

    // �� ���� �ؾ��ϴ� ��
    private int CheckCSVNum; // ���� CSV�ѹ�(CSVNum)

    private string ChoiceKey = "0"; // �������� ����Ǵ� Ű ���̴�. KeyCheck�� true�̸� ���⿡ �ش��ϴ� �͸� ��µȴ�.
    private bool KeyCheck; // Ű�� ����ؾ��ϴ��� �ƴ����� �Ǵ��Ѵ�.
    
    private string CSVName;

    private string FirstChoiceKey;
    private string SecondChoiceKey;
    private string ThirdChoiceKey;
    
    [HideInInspector]
    public Character[] Talker = new Character[3]; //���� �߾� ������ ������ 0 1 2

    private int TalkerNum = -1;

    public string Background;
    public string BGM;
    private bool IsBGMOn = true;
    public GameVariable[] Var;
    public GameSwitch[] Switch;

    //---------------- csv�ٸ��� �ҷ����� �Ҷ� ������ ���� (TalkManager�� ChangeCSV���� ReturnCSV���鶧 ����.)
    private int SaveCSVNum; // ����� CSV�ѹ�(�� ���̺�ε� ������)
    
    private string SaveCSVName; // csv����(�� CSVName �־�� �����ϸ� ��)
    private string SaveBackgroundName; // ������ ���
    private string SaveBGMName; // BGM
    private string SaveChoiceKey; // Ű��
    private string[] SaveTalker = new string[3];// ȭ��
    private bool SaveIsBGMOn; // BGM�� �������
    private int SaveTalkerNum; // ���ϰ� �ִ� ȭ��
    private bool SaveKeyCheck; // Ű�� �������������
    //------------------
    //�α׿�
    public Log GameLog;
    [HideInInspector]
    public int LogNum;
    [HideInInspector]
    public string SceneNameForLoad; // �ε��Ҷ��� ����ϵ��� �� ����

    [HideInInspector]
    public bool IsFadeOut; 
    [HideInInspector]
    public string FadeColor;
    [HideInInspector]
    public bool SaveIsFadeOut;

    //--------------- �� ������ �̱���
 
    //------------------



    private float CenterTalkerPositionX = 0.0f;
    private float CenterTalkerPositionY = 0.0f;

    private float LeftTalkerPositionX = -425.0f;
    private float LeftTalkerPositionY = 0.0f;

    private float RightTalkerPositionX = 425.0f;
    private float RightTalkerPositionY = 0.0f;

    [HideInInspector]
    public bool isMain;
    [HideInInspector]
    public bool isMenu;
    [HideInInspector]
    public bool isTalk;
    [HideInInspector]
    public bool TextReading;
    [HideInInspector]
    public bool isChoice;
    [HideInInspector]
    public bool TextButtonCheck = false;
    [HideInInspector]
    public bool isLog;
    [HideInInspector]
    public bool isTimer;
    [HideInInspector]
    public bool isEffect;
    [HideInInspector]
    public bool CantUseMenu  = true;
    [SerializeField]
    private Sprite Blank;
    [SerializeField]
    private Sprite White;
    [SerializeField]
    private AudioClip DefaultBGM;
    [SerializeField]
    private AudioClip DefaultSE;

    [SerializeField]
    private AudioSource GameBGM;

    [SerializeField]
    private AudioSource GameSE;




    public int GetCSVNum()
    { 
        return CheckCSVNum;
    }

    public void PlusCSVNum(int i)
    { 
        CheckCSVNum = CheckCSVNum + i;
        Debug.Log("CSVNum�� ���� �Ǿ����ϴ�. CSVNum = " + CheckCSVNum );
    }

    public void ChangeCSVNum(int i)
    { 
        CheckCSVNum = i;
        Debug.Log("CSVNum�� ���� �Ǿ����ϴ�. CSVNum = " + CheckCSVNum );
    }

    public int GetSaveCSVNum()
    { 
        return SaveCSVNum;
    }

    public void ChangeSaveCSVNum(int i)
    { 
        SaveCSVNum = i;
        Debug.Log("SaveCSVNum�� ���� �Ǿ����ϴ�. SaveCSVNum = " + SaveCSVNum );
    }

    public string GetChoiceKey()
    { 
        return ChoiceKey;
    }

    public void ChangeChoiceKey(string i)
    { 
        ChoiceKey = i;
        Debug.Log("ChoiceKey�� ���� �Ǿ����ϴ�. ChoiceKey = " + ChoiceKey);
    }

    public bool GetKeyCheck()
    { 
        return KeyCheck;
    }

    public void ChangeKeyCheck(bool i)
    { 
        KeyCheck = i;
    }    

    public string GetFirstChoiceKey()
    { 
        return FirstChoiceKey;
    }

    public void ChangeFirstChoiceKey(string i)
    { 
        FirstChoiceKey = i;
    }

    public string GetSecondChoiceKey()
    { 
        return SecondChoiceKey;
    }

    public void ChangeSecondChoiceKey(string i)
    { 
        SecondChoiceKey = i;
    }

    public string GetThirdChoiceKey()
    { 
        return ThirdChoiceKey;
    }

    public void ChangeThirdChoiceKey(string i)
    { 
        ThirdChoiceKey = i;
    }


    public string GetCSVName()
    { 
        return CSVName;     
    }

    public void ChangeCSVName(string name)
    { 
        CSVName = name;     
    }

    public int GetTalkerNum()
    { 
        return TalkerNum;
    }

    public void ChangeTalkerNum(int Number)
    { 
        TalkerNum = Number;
    }

    public Sprite GetBlank()
    { 
        return Blank;
    }

    public Sprite GetWhite()
    { 
        return White;
    }

    public AudioClip GetDefaultBGM()
    { 
        return DefaultBGM;
    }

    public AudioClip GetDefaultSE()
    { 
        return DefaultSE;
    }

    public bool GetIsBGMOn()
    { 
        return IsBGMOn;    
    }

    public void ChangeIsBGMOn(bool Check)
    { 
        IsBGMOn = Check;
    }

    public string GetSaveCSVName()
    { 
        return SaveCSVName;
    }

    public void ChangeSaveCSVName(string Name)
    { 
        SaveCSVName = Name;
    }

    public string GetSaveBackgroundName()
    { 
        return SaveBackgroundName;
    }

    public void ChangeSaveBackgroundName(string Name)
    { 
        SaveBackgroundName = Name;
    }

    public string GetSaveBGMName()
    { 
        return SaveBGMName;
    }

    public void ChangeSaveBGMName(string Name)
    { 
        SaveBGMName = Name;
    }

    public string GetSaveChoiceKey()
    { 
        return SaveChoiceKey;
    }

    public void ChangeSaveChoiceKey(string Name)
    { 
        SaveChoiceKey = Name;
    }

    public string GetSaveTalker(int Num)
    {
        if(Num < SaveTalker.Length)
        { 
            return SaveTalker[Num];
        }
        else
        { 
            Debug.LogWarning("GameManager GetSaveTalker�� �߸��� int ���� ���Խ��ϴ�.");
            return SaveTalker[0];
        }
    }

    public void ChangeSaveTalker(string Name, int Num)
    {
        if(Num < SaveTalker.Length)
        { 
            SaveTalker[Num]  = Name;
        }
        else
        { 
            Debug.LogWarning("GameManager ChangeSaveTalker�� �߸��� int ���� ���Խ��ϴ�.");
        }        
    }

    public bool GetSaveIsBGMOn()
    { 
        return SaveIsBGMOn;    
    }

    public void ChangeSaveIsBGMOn(bool Check)
    { 
        SaveIsBGMOn = Check;
    }

    public int GetSaveTalkerNum()
    { 
        return SaveTalkerNum;    
    }

    public void ChangeSaveTalkerNum(int Num)
    { 
        SaveTalkerNum = Num;
    }

    public bool GetSaveKeyCheck()
    { 
        return SaveKeyCheck;    
    }

    public void ChangeSaveKeyCheck(bool Check)
    { 
        SaveKeyCheck = Check;
    }

    public void UpdateSceneNameForLoad()
    { 
        SceneNameForLoad = SceneManager.GetActiveScene().name;
        Debug.Log("SceneNameForLoad�� ������Ʈ �մϴ� ���� ��: " + SceneNameForLoad);
    }    

    public float GetCenterTalkerPositionX()
    { 
        return CenterTalkerPositionX;
    }

    public float GetCenterTalkerPositionY()
    { 
        return CenterTalkerPositionY;
    }

    public float GetLeftTalkerPositionX()
    { 
        return LeftTalkerPositionX;
    }

    public float GetLeftTalkerPositionY()
    { 
        return LeftTalkerPositionY;
    }

    public float GetRightTalkerPositionX()
    { 
        return RightTalkerPositionX;
    }

    public float GetRightTalkerPositionY()
    { 
        return RightTalkerPositionY;
    }

    public void ReturnTitle()
    { 
        Destroy(GameManager.instance.gameObject);
        Destroy(TalkManager.instance.gameObject);
        Destroy(MenuManager.instance.gameObject);
        Destroy(EffectManager.instance.gameObject);
        GameManager.instance.CantUseMenu = true;
        isMain = true;
        SceneManager.LoadScene("MainScene");        
    }

    public void StartGame()
    { 
        SceneManager.LoadScene("GameScene");  
        TalkManager.instance.StartGame();       
    }

    //BGM ���� (��ݰ���)

    public void ChangeBGM(AudioClip Music)
    { 
        Debug.Log("�ٲٴ� ����̸��� " + Music.ToString());
        GameBGM.clip = Music;
        if(IsBGMOn == true)
        {     
            GameBGM.Play();
        }
    }

    public void StartBGM()
    {
        IsBGMOn = true;
        GameBGM.Play();
    }    

    public void StopBGM()
    {
        IsBGMOn = false;
        GameBGM.Stop();
    }

    public void PlaySE(AudioClip Music)
    { 
        GameSE.clip = Music;
        GameSE.Play();
    } 
    
    public bool CantTalkCheck() // ���� ��ȭ�� �Ѿ �� �ִ��� üũ�ϴ� ��
    { 
        if(isMenu == true || isTalk == true || TextReading == true || isChoice == true || isLog == true || isTimer == true || isEffect == true)
        { 
            return true;
        }
        else
        { 
            return false;
        }
    }

    public bool CantMenuCheck()
    { 
        if(isLog == true || CantUseMenu == true || isTimer == true || isEffect == true)
        { 
            /*
            if(isLog == true)
            { 
               Debug.LogWarning("isLog"); 
            }
            if(CantUseMenu== true)
            { 
               Debug.LogWarning("CantUseMenu"); 
            }            
            if(isTimer == true)
            { 
               Debug.LogWarning("isTimer"); 
            }            
            if(isEffect == true)
            { 
               Debug.LogWarning("isEffect"); 
            }
            */
            return true;
        }
        else
        { 
            return false;
        }        
    }

    public bool CantLogCheck()
    { 
        if(isMenu == true || isTimer == true || isEffect == true)
        { 
            return true;
        }
        else
        { 
            return false;
        }        
    }

    private float SkipCount = 0.0f;

    [SerializeField]
    private float SkipSpeed;

    void Update()
    { 
        SkipCount += Time.deltaTime;
    }

    public bool SkipCheck()
    {
        if(isChoice == false && isMain == false)
        { 
            if(Input.GetKey(KeySetting.Key[KeyAction.Skip]) && SkipCount > SkipSpeed)
            { 
                SkipCount = 0;
                return true;
            }
            else
            { 
                return false; 
            }
        }
        else
        { 
            return false; 
        }
    }

    public float GetSkipSpeed()
    { 
        return SkipSpeed;
    }

    public void ChangeSkipSpeed(float i)
    { 
        SkipSpeed = i;
    }

    public bool CheckTalk() //��ȭ ������ �Ѿ�� Ű üũ
    {
        if(isChoice == false && isMain == false)
        { 
            if(Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeySetting.Key[KeyAction.Talk])||TextButtonCheck == true) //||Input.GetKeyDown(KeyCode.Mouse0) <- �޴���ư ������ �߻��ϴ� ���� �и�
            {
                TextButtonCheck = false;
                return true;    
            }
            else
            { 
                return false;    
            }
        }
        else
        { 
            return false; 
        }
    }



    public bool OpenMenuCheck() // �Ŵ� Ű üũ
    { 
        if(Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeySetting.Key[KeyAction.Menu]))
        { 
            return true;
        }
        else
        { 
            return false;
        }        
    }

}


    // �׽�Ʈ ������

    /*�׽�Ʈ
    LeftTalker = CharacterDatabaseManager.instance.CharacterDB(0);
    for (int i = 0; i <  CharacterDatabaseManager.instance.DBLength(); i++)
    { 
        if(LeftTalker.Name == CharacterDatabaseManager.instance.CharacterDB(i).Name)
        { 
            Debug.Log("�˻�Ϸ�");
        }
        else
        { 
            Debug.Log("����ġ");
        }
    }
    */
