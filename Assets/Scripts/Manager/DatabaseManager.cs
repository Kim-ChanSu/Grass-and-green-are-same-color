using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

[Serializable]
public class GameSaveData
{
    [HideInInspector]
    public int CheckCSVNum;
    [HideInInspector]
    public string ChoiceKey;
    [HideInInspector]
    public bool KeyCheck;
    [HideInInspector]
    public string CSVName;
    [HideInInspector]
    public string FirstChoiceKey;
    [HideInInspector]
    public string SecondChoiceKey;
    [HideInInspector]
    public string ThirdChoiceKey;
    [HideInInspector]
    public string[] Talker;
    [HideInInspector]
    public int TalkerNum;
    [HideInInspector]
    public string Background;  
    [HideInInspector]
    public GameVariable[] Var;
    [HideInInspector]
    public GameSwitch[] Switch;
    [HideInInspector]
    public string BGM;
    [HideInInspector]
    public bool IsBGMOn;

    //---------------------- 인게임 CSV 세이브 로드용
    [HideInInspector]
    public int SaveCSVNum;
    [HideInInspector]
    public string SaveCSVName;
    [HideInInspector]
    public string SaveBackgroundName; 
    [HideInInspector]
    public string SaveBGMName;
    [HideInInspector]
    public string SaveChoiceKey; 
    [HideInInspector]
    public string[] SaveTalker;
    [HideInInspector]
    public bool SaveIsBGMOn; 
    [HideInInspector]
    public int SaveTalkerNum; 
    [HideInInspector]
    public bool SaveKeyCheck;
    //--------------------------------------
    [HideInInspector]
    public Log GameLog;
    [HideInInspector]
    public int LogNum;  

    //----------------
    [HideInInspector]
    public string SceneNameForLoad; // 얘는 세이브할때 업데이트 해줌 ㅋㅋ;

    [HideInInspector]
    public bool IsFadeOut;
    [HideInInspector]
    public bool SaveIsFadeOut;
    [HideInInspector]
    public string FadeColor;

   //--------------- 이 밑으로 미구현

}

// 설정 저장용
[Serializable]
public class SettingData
{ 
    //[HideInInspector]
    public float SaveBGMSoundValue;
    //[HideInInspector]
    public float SaveSESoundValue;
    //[HideInInspector]
    public KeyCode[] SaveKey;

}

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager instance;

    [HideInInspector]
    public string SaveFile;
    private string SaveFolder;

    public GameSaveData GameSave = new GameSaveData();

    public GameSaveData GameSaveForData = new GameSaveData(); // 혹시 몰라서 분리해둠

    //설정 저장용
    public SettingData SaveSettingData = new SettingData();
    [SerializeField]
    private SettingData DefaultSettingData = new SettingData();
    private string SettingSaveFile;
    //---------------------------------

    void Awake() 
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("씬에 데이터베이스 매니저가 2개이상 존재합니다.");
            Destroy(gameObject);
        }

        SaveFolder = Application.dataPath + "/save/";
        SaveFile = SaveFolder + "savefile";  
        
        //설정 저장용
        SettingSaveFile = SaveFolder + "SettingSave";

    }

    void Start()
    { 
        GameSave.Talker = new string[GameManager.instance.Talker.Length];       
        GameSave.SaveTalker = new string[GameManager.instance.Talker.Length]; 
        //설정 저장용
        SaveSettingData.SaveKey = new KeyCode[(int)KeyAction.KeyCount];
        ResetSaveSettingData();       
        GetSettingDataOutside();
        //----------------------
    }

    public void SaveData(int FileNum)
    {
        Debug.Log("세이브 체크");
        GameManager.instance.UpdateSceneNameForLoad();
        if(Directory.Exists(SaveFolder) == false)
        {
            Debug.Log("세이브 폴더가 없으므로 폴더를 생성합니다.");
            Directory.CreateDirectory(SaveFolder);
        }
        if(File.Exists(SaveFile + FileNum) == true)
        { 
            Debug.Log("기존의 세이브 파일이 있으므로 삭제 합니다."); 
            File.Delete(SaveFile + FileNum); // 안하니까 버그남 <- 원인은 Talker 세이브할때 null일때 예외처리 안해놔서 기존에 들어갔던게 안지워져서였음 그래도 걍 냅두자
        }
        Save(FileNum);
        //Log();
    }

    /*
    private void Log()
    { 
        string LogText = "";
        int LogCheck = GameManager.instance.LogNum;

        for (int i = 0; i < GameManager.instance.GameLog.TalkLog.Length; i++)
        {
            if(GameManager.instance.GameLog.TalkLog[LogCheck] != "")
            {
                LogText += i + " : ";
                if(GameManager.instance.GameLog.TalkerNameLog[LogCheck] != "")
                { 
                    LogText += GameManager.instance.GameLog.TalkerNameLog[LogCheck] + " : ";
                }

                LogText += GameManager.instance.GameLog.TalkLog[LogCheck];

                Debug.Log(LogText);
                LogText = "";                            
            }
            
            
            LogCheck--;
            if(LogCheck < 0)
            { 
                LogCheck = GameManager.instance.GameLog.TalkLog.Length -1;
            }                     
        }    
    }
    */

    private void Save(int FileNum)
    {
        string SaveFileNum = SaveFile + FileNum;

        GameSave.CheckCSVNum = GameManager.instance.GetCSVNum();        

        GameSave.ChoiceKey = GameManager.instance.GetChoiceKey();
        GameSave.KeyCheck = GameManager.instance.GetKeyCheck();
        GameSave.CSVName = GameManager.instance.GetCSVName();
        GameSave.FirstChoiceKey = GameManager.instance.GetFirstChoiceKey();
        GameSave.SecondChoiceKey = GameManager.instance.GetSecondChoiceKey();
        GameSave.ThirdChoiceKey = GameManager.instance.GetThirdChoiceKey();
        for(int i = 0; i < GameManager.instance.Talker.Length; i++)
        { 
            if(GameManager.instance.Talker[i] != null)
            { 
                GameSave.Talker[i] = GameManager.instance.Talker[i].Name;
            }
            else
            { 
                GameSave.Talker[i] = "";
            }
        }
        GameSave.TalkerNum = GameManager.instance.GetTalkerNum();
        GameSave.Background = GameManager.instance.Background; 

        GameSave.Var = GameManager.instance.Var;
        GameSave.Switch = GameManager.instance.Switch;

        GameSave.BGM = GameManager.instance.BGM;
        GameSave.IsBGMOn = GameManager.instance.GetIsBGMOn();

        //-------Save 시리즈---------------
        GameSave.SaveCSVNum = GameManager.instance.GetSaveCSVNum();
        GameSave.SaveCSVName = GameManager.instance.GetSaveCSVName();
        GameSave.SaveBackgroundName = GameManager.instance.GetSaveBackgroundName();
        GameSave.SaveBGMName = GameManager.instance.GetSaveBGMName();
        GameSave.SaveChoiceKey = GameManager.instance.GetSaveChoiceKey();
        GameSave.SaveIsBGMOn= GameManager.instance.GetSaveIsBGMOn();
        GameSave.SaveTalkerNum = GameManager.instance.GetSaveTalkerNum();
        GameSave.SaveKeyCheck = GameManager.instance.GetSaveKeyCheck();
        for(int i = 0; i < GameSave.SaveTalker.Length; i++)
        { 
            GameSave.SaveTalker[i] = GameManager.instance.GetSaveTalker(i);
        }
        GameSave.SaveIsFadeOut = GameManager.instance.SaveIsFadeOut;
        //-------------------------------------
        GameSave.GameLog = GameManager.instance.GameLog;
        GameSave.LogNum = GameManager.instance.LogNum;
        GameSave.SceneNameForLoad = GameManager.instance.SceneNameForLoad;
        GameSave.IsFadeOut = GameManager.instance.IsFadeOut;        
        GameSave.FadeColor = GameManager.instance.FadeColor;        
        
        string data = JsonUtility.ToJson(GameSave);
        File.WriteAllText(SaveFileNum.ToString(), data);
        Debug.Log("세이브 완료");
    }

    public void LoadData(int FileNum)
    {
        string SaveFileNum = SaveFile + FileNum;

        string data = File.ReadAllText(SaveFileNum.ToString());
        GameSave = JsonUtility.FromJson<GameSaveData>(data);

        GameManager.instance.ChangeCSVNum(GameSave.CheckCSVNum);       
        GameManager.instance.ChangeChoiceKey(GameSave.ChoiceKey);
        GameManager.instance.ChangeKeyCheck(GameSave.KeyCheck);
        GameManager.instance.ChangeCSVName(GameSave.CSVName);
        GameManager.instance.ChangeFirstChoiceKey(GameSave.FirstChoiceKey);
        GameManager.instance.ChangeSecondChoiceKey(GameSave.SecondChoiceKey);
        GameManager.instance.ChangeThirdChoiceKey(GameSave.ThirdChoiceKey);

        GameManager.instance.Background =  GameSave.Background;

        for(int j = 0; j < GameSave.Talker.Length; j++)
        {
            if(GameSave.Talker[j] != "")
            { 
                for(int i = 0; i < CharacterDatabaseManager.instance.DBLength(); i++)
                { 
                    if(CharacterDatabaseManager.instance.CharacterDB(i).Name == GameSave.Talker[j])
                    { 
                        GameManager.instance.Talker[j] = CharacterDatabaseManager.instance.CharacterDB(i);                    
                        break;
                    }
                }
            }
            else
            { 
                GameManager.instance.Talker[j] = null;
            }
        }

        GameManager.instance.Var = GameSave.Var;
        GameManager.instance.Switch = GameSave.Switch;

        GameManager.instance.ChangeTalkerNum(GameSave.TalkerNum);

        GameManager.instance.BGM = GameSave.BGM;
        GameManager.instance.ChangeIsBGMOn(GameSave.IsBGMOn);

        //-------Save 시리즈---------------
        GameManager.instance.ChangeSaveCSVNum(GameSave.SaveCSVNum);
        GameManager.instance.ChangeSaveCSVName(GameSave.SaveCSVName);
        GameManager.instance.ChangeSaveBackgroundName(GameSave.SaveBackgroundName);
        GameManager.instance.ChangeSaveBGMName(GameSave.SaveBGMName);
        GameManager.instance.ChangeSaveChoiceKey(GameSave.SaveChoiceKey);
        GameManager.instance.ChangeSaveIsBGMOn(GameSave.SaveIsBGMOn);
        GameManager.instance.ChangeSaveTalkerNum(GameSave.SaveTalkerNum);
        GameManager.instance.ChangeSaveKeyCheck(GameSave.SaveKeyCheck);
        for(int i = 0; i < GameSave.SaveTalker.Length; i++)
        { 
            GameManager.instance.ChangeSaveTalker(GameSave.SaveTalker[i], i);
        }
        GameManager.instance.SaveIsFadeOut= GameSave.SaveIsFadeOut;
        //-------------------------------------

        GameManager.instance.GameLog = GameSave.GameLog;
        GameManager.instance.LogNum = GameSave.LogNum; // ChoiceEnd가 아닐때 TalkManager에서 - 1 해줌
        GameManager.instance.SceneNameForLoad = GameSave.SceneNameForLoad;

        GameManager.instance.IsFadeOut = GameSave.IsFadeOut;       
        GameManager.instance.FadeColor= GameSave.FadeColor;       
    }

    public string GetTalkerName(int i)
    { 
        string SaveFileNum = SaveFile + i;
        if(File.Exists(SaveFileNum))
        { 
            string data = File.ReadAllText(SaveFileNum.ToString());
            GameSaveForData = JsonUtility.FromJson<GameSaveData>(data); 
            return GameSaveForData.GameLog.TalkerNameLog[GameSaveForData.LogNum];
        }
        else
        {
            Debug.LogWarning("DatabaseManager의 GetTalkerName에 잘못된 값이 들어왔습니다!");
            return "";    
        }
    }

    public string GetTalkData(int i)
    { 
        string SaveFileNum = SaveFile + i;
        if(File.Exists(SaveFileNum))
        { 
            string data = File.ReadAllText(SaveFileNum.ToString());
            GameSaveForData = JsonUtility.FromJson<GameSaveData>(data); 
            return GameSaveForData.GameLog.TalkLog[GameSaveForData.LogNum];
        }
        else
        {
            Debug.LogWarning("DatabaseManager의 GetTalkData에 잘못된 값이 들어왔습니다!");
            return "";    
        }        
    }

    //설정저장용
    public void ResetSaveSettingData()
    { 
        GetSettingDataInside();

        //초기값 다 받아오고 저걸 디폴트에 넣어두기
        DefaultSettingData = SaveSettingData;
        for(int i = 0; i < DefaultSettingData.SaveKey.Length; i++) // 젠 인게임에서 타이틀로 돌아가도 초기화 안되니까 이걸로 초기화
        { 
             DefaultSettingData.SaveKey[i] = KeyManager.instance.DefaultKeys[i];
        }
       
    }    

    //설정 저장용
    public void LoadDefaultSettingData()//11
    { 
        /* 이 코드쓰면 두개가 일심동체가 되서 움직이네 ㅋㅋ 뭐지
        SaveSettingData = DefaultSettingData;     
        SaveSettingData = SaveSettingData;
        LoadSettingData();
        */
        MenuManager.instance.BGMSlider.value = DefaultSettingData.SaveBGMSoundValue;
        MenuManager.instance.SESlider.value = DefaultSettingData.SaveSESoundValue;

        for(int i = 0; i < DefaultSettingData.SaveKey.Length; i++)
        { 
            KeySetting.Key[(KeyAction)i] = DefaultSettingData.SaveKey[i];
        }        
    }

    public void SaveSettingDataOutside()//1
    { 
        GetSettingDataInside();

        //Debug.Log("설정 세이브 파일이 없으므로 폴더를 생성합니다.");
        string data = JsonUtility.ToJson(SaveSettingData);
        File.WriteAllText(SettingSaveFile.ToString(), data);            
    }

    public void LoadSettingData()//22
    { 
        MenuManager.instance.BGMSlider.value = SaveSettingData.SaveBGMSoundValue;
        MenuManager.instance.SESlider.value = SaveSettingData.SaveSESoundValue;

        for(int i = 0; i < SaveSettingData.SaveKey.Length; i++)
        { 
            KeySetting.Key[(KeyAction)i] = SaveSettingData.SaveKey[i];
        }        
    }

    public void GetSettingDataOutside()
    { 
        if(File.Exists(SettingSaveFile) == true)
        { 
            string data = File.ReadAllText(SettingSaveFile.ToString());
            SaveSettingData = JsonUtility.FromJson<SettingData>(data);  
            LoadSettingData();
        }    
    }

    public void GetSettingDataInside()//2
    {
        SaveSettingData.SaveBGMSoundValue = MenuManager.instance.BGMSlider.value;
        SaveSettingData.SaveSESoundValue = MenuManager.instance.SESlider.value;

        for(int i = 0; i < SaveSettingData.SaveKey.Length; i++)
        { 
            SaveSettingData.SaveKey[i] = KeySetting.Key[(KeyAction)i];
        }     
        
        if(Directory.Exists(SaveFolder) == false)
        {
            Debug.Log("세이브 폴더가 없으므로 폴더를 생성합니다.");
            Directory.CreateDirectory(SaveFolder);
        }        
    }

}
