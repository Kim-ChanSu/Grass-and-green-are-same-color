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
            Debug.LogWarning("씬에 게임매니저가 2개이상 존재합니다.");
            Destroy(gameObject);
        }

        CheckCSVNum = 0;
        Debug.Log("CheckCSVNum = " + CheckCSVNum);

        CSVName = StartCSVName; // 처음 시작하는 csv파일이름
        KeyCheck = false;

        CSVFolder = Application.dataPath + "/StreamingAssets/CSV/"; // 실험용

        GameLog.TalkerNameLog = new string[LogLength];
        GameLog.TalkLog = new string[LogLength];
        for(int i = 0; i < GameLog.TalkLog.Length; i++)
        { 
            GameLog.TalkLog[i] = "";
            GameLog.TalkerNameLog[i] = "";
            
        }

        LogNum = -1; // 구조상 ++를 먼저 불러오게할꺼라 -1로 시작함

        isMain = true;
    }

    void Start()
    { 
        Debug.Log("등록된 캐릭터의 갯수는 " + CharacterDatabaseManager.instance.DBLength());
    }

    [HideInInspector]
    public string CSVFolder;
    public int LogLength = 30;
    public string StartCSVName;

    // 꼭 저장 해야하는 값
    private int CheckCSVNum; // 현재 CSV넘버(CSVNum)

    private string ChoiceKey = "0"; // 선택지로 변경되는 키 값이다. KeyCheck가 true이면 여기에 해당하는 것만 출력된다.
    private bool KeyCheck; // 키를 사용해야하는지 아닌지를 판단한다.
    
    private string CSVName;

    private string FirstChoiceKey;
    private string SecondChoiceKey;
    private string ThirdChoiceKey;
    
    [HideInInspector]
    public Character[] Talker = new Character[3]; //왼쪽 중앙 오른쪽 순으로 0 1 2

    private int TalkerNum = -1;

    public string Background;
    public string BGM;
    private bool IsBGMOn = true;
    public GameVariable[] Var;
    public GameSwitch[] Switch;

    //---------------- csv다른거 불러오게 할때 저장할 값들 (TalkManager에 ChangeCSV말고 ReturnCSV만들때 쓰자.)
    private int SaveCSVNum; // 저장된 CSV넘버(앤 세이브로드 구현됨)
    
    private string SaveCSVName; // csv파일(걍 CSVName 넣어다 저장하면 됨)
    private string SaveBackgroundName; // 저장할 배경
    private string SaveBGMName; // BGM
    private string SaveChoiceKey; // 키값
    private string[] SaveTalker = new string[3];// 화자
    private bool SaveIsBGMOn; // BGM의 재생여부
    private int SaveTalkerNum; // 말하고 있는 화자
    private bool SaveKeyCheck; // 키가 사용중인지여부
    //------------------
    //로그용
    public Log GameLog;
    [HideInInspector]
    public int LogNum;
    [HideInInspector]
    public string SceneNameForLoad; // 로드할때만 사용하도록 할 예정

    [HideInInspector]
    public bool IsFadeOut; 
    [HideInInspector]
    public string FadeColor;
    [HideInInspector]
    public bool SaveIsFadeOut;

    //--------------- 이 밑으로 미구현
 
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
        Debug.Log("CSVNum이 변경 되었습니다. CSVNum = " + CheckCSVNum );
    }

    public void ChangeCSVNum(int i)
    { 
        CheckCSVNum = i;
        Debug.Log("CSVNum이 변경 되었습니다. CSVNum = " + CheckCSVNum );
    }

    public int GetSaveCSVNum()
    { 
        return SaveCSVNum;
    }

    public void ChangeSaveCSVNum(int i)
    { 
        SaveCSVNum = i;
        Debug.Log("SaveCSVNum이 변경 되었습니다. SaveCSVNum = " + SaveCSVNum );
    }

    public string GetChoiceKey()
    { 
        return ChoiceKey;
    }

    public void ChangeChoiceKey(string i)
    { 
        ChoiceKey = i;
        Debug.Log("ChoiceKey가 변경 되었습니다. ChoiceKey = " + ChoiceKey);
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
            Debug.LogWarning("GameManager GetSaveTalker에 잘못된 int 값이 들어왔습니다.");
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
            Debug.LogWarning("GameManager ChangeSaveTalker에 잘못된 int 값이 들어왔습니다.");
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
        Debug.Log("SceneNameForLoad를 업데이트 합니다 현재 값: " + SceneNameForLoad);
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

    //BGM 관련 (브금관련)

    public void ChangeBGM(AudioClip Music)
    { 
        Debug.Log("바꾸는 브금이름은 " + Music.ToString());
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
    
    public bool CantTalkCheck() // 다음 대화로 넘어갈 수 있는지 체크하는 곳
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

    public bool CheckTalk() //대화 선택지 넘어가는 키 체크
    {
        if(isChoice == false && isMain == false)
        { 
            if(Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeySetting.Key[KeyAction.Talk])||TextButtonCheck == true) //||Input.GetKeyDown(KeyCode.Mouse0) <- 메뉴버튼 눌러도 발생하는 덕에 분리
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



    public bool OpenMenuCheck() // 매뉴 키 체크
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


    // 테스트 모음집

    /*테스트
    LeftTalker = CharacterDatabaseManager.instance.CharacterDB(0);
    for (int i = 0; i <  CharacterDatabaseManager.instance.DBLength(); i++)
    { 
        if(LeftTalker.Name == CharacterDatabaseManager.instance.CharacterDB(i).Name)
        { 
            Debug.Log("검사완료");
        }
        else
        { 
            Debug.Log("불일치");
        }
    }
    */
