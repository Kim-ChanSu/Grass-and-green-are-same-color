using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using UnityEngine.Audio;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    void Awake() 
    {
        if (instance == null)
        {
            DontDestroyOnLoad(this.gameObject);
            instance = this;
        }

        else
        {
            Debug.LogWarning("씬에 메뉴 매니저가 2개이상 존재합니다.");
            Destroy(gameObject);
        }
    }

    [SerializeField]
    private GameObject MenuCanvas;

    [SerializeField]
    private GameObject MenuButtons;
    [SerializeField]
    private GameObject ExitButtons;

    [SerializeField]
    private GameObject FirstMenuButton;

    [SerializeField]
    private GameObject FirstExitButton;

    [SerializeField]
    private GameObject MenuBackground;

    [SerializeField]
    private GameObject SaveMenu;

    [SerializeField]
    private GameObject FirstSaveButton;

    [SerializeField]
    private GameObject[] SaveBox;

    [SerializeField]
    private GameObject SaveCheckWindow;

    [SerializeField]
    private GameObject SaveText;

    [HideInInspector]
    public bool isSave; // true면 세이브 false면 로드

    [HideInInspector]
    public bool[] ThereIsSaveFile;

    [HideInInspector]
    public int SaveFileNum;

     //설정용
    [SerializeField]
    private GameObject SettingMenu;

    public Text[] KeySettingText;

    public GameObject ExitSettingButton;

    public Slider BGMSlider;
    public Slider SESlider;


    void Start()
    { 
        ThereIsSaveFile = new bool[SaveBox.Length];
        for(int i = 0; i < ThereIsSaveFile.Length; i++)
        { 
            ThereIsSaveFile[i] = false;
        }

        /*
        Debug.Log(BGMSlider.value);
        BGMSlider.value = -80f;
        Debug.Log(BGMSlider.value);
        */
    }

    void Update()
    {

        if(GameManager.instance.OpenMenuCheck() == true && KeyManager.instance.GetSetKeyCheck() == false)
        {
            //Debug.Log("메뉴 체크"); 키설정할때도 로그 찍히는거 거슬려서 주석처리
            if(GameManager.instance.isMenu != true)
            { 
                OpenMenu();
            }
            else
            { 
                CloseMenu();
            }
        }
        
    }

    private void OpenMenu()
    {
        if(GameManager.instance.CantMenuCheck() == false)
        { 
            GameManager.instance.PlaySE(SEDatabaseManager.instance.GetSE("Click"));
            Debug.Log("메뉴 오픈");
            GameManager.instance.isMenu = true;
            MenuCanvas.SetActive(true);
            MenuButtons.SetActive(true);
        
            TalkManager.instance.ChoiceButtonOff();

            var eventSystem = EventSystem.current;
            eventSystem.SetSelectedGameObject(FirstMenuButton, new BaseEventData(eventSystem));

            TalkManager.instance.SetLogButton(false);
        }
    }

    private void CloseMenu()
    {
        GameManager.instance.PlaySE(SEDatabaseManager.instance.GetSE("Click"));
        Debug.Log("메뉴 닫음");
        ExitButtons.SetActive(false);
        MenuButtons.SetActive(true);
        MenuCanvas.SetActive(false);
        MenuBackground.SetActive(true);
        CloseSaveMenu();
        CloseSettingMenu();
        GameManager.instance.isMenu = false;

        TalkManager.instance.ChoiceButtonOn();

        TalkManager.instance.SetLogButton(true);
    }

    public void CallCloseMenu()
    { 
        CloseMenu();
    }

    public void MenuButton()
    {
        
        if(GameManager.instance.isMenu != true)
        {
            // 버튼 이미지 바꾸는거 추가
            OpenMenu();
        }
        else
        {
            // 버튼 이미지 바꾸는거 추가
            CloseMenu();
        }
    }

    public void OpenExitMenu()
    {
        MenuButtons.SetActive(false);
        ExitButtons.SetActive(true);     
        var eventSystem = EventSystem.current;
        eventSystem.SetSelectedGameObject(FirstExitButton, new BaseEventData(eventSystem));
    }

    public void OpenSaveMenu()
    {
        isSave = true;
        SaveText.GetComponent<Text>().text = "세이브";
        MenuCanvas.SetActive(true);
        MenuBackground.SetActive(false);        
        UpdateSaveData();
        SaveMenu.SetActive(true);
        var eventSystem = EventSystem.current;
        eventSystem.SetSelectedGameObject(FirstSaveButton, new BaseEventData(eventSystem));
    }

    public void OpenLoadMenu()
    {
        isSave = false;
        SaveText.GetComponent<Text>().text = "로드";
        MenuCanvas.SetActive(true);
        MenuBackground.SetActive(false);        
        UpdateSaveData();
        SaveMenu.SetActive(true);
        var eventSystem = EventSystem.current;
        eventSystem.SetSelectedGameObject(FirstSaveButton, new BaseEventData(eventSystem));
    }
  

    public void CloseSaveMenu()
    { 
        SaveMenu.SetActive(false);
        SaveCheckWindow.SetActive(false);
        for(int i = 0; i < ThereIsSaveFile.Length; i++)
        {
            SaveBox[i].GetComponent<Button>().interactable = true;
            SaveBox[i].transform.GetChild(1).gameObject.SetActive(true);
            SaveBox[i].transform.GetChild(2).gameObject.SetActive(true);
            SaveBox[i].transform.GetChild(3).gameObject.SetActive(false);
            SaveBox[i].transform.GetChild(1).GetComponent<Text>().text = "";
            SaveBox[i].transform.GetChild(2).GetComponent<Text>().text = "";
        }
    }

    public void SaveGameData(int ButtonNum)
    {
        DatabaseManager.instance.SaveData(ButtonNum);
        UpdateSaveData();
    }

    public void LoadGameData()
    { 
        if(File.Exists(DatabaseManager.instance.SaveFile + SaveFileNum))
        { 
            TalkManager.instance.StopAllCoroutinesForLoad();
            TalkManager.instance.TurnOffChoiceButton();
            GameManager.instance.isChoice = false;
            GameManager.instance.isTalk = false;
            GameManager.instance.TextReading = false;
            CallCloseMenu();
            DatabaseManager.instance.LoadData(SaveFileNum);
            SceneManager.LoadScene(GameManager.instance.SceneNameForLoad);                   
            TalkManager.instance.LoadGame();            
        }
    }

    public void SaveGameDataInWindow()
    { 
        DatabaseManager.instance.SaveData(SaveFileNum);
        CloseSaveCheckWindow();
    }

    public void OpenSaveCheckWindow(int ButtonNum)
    {
        if(isSave == true)
        { 
            SaveCheckWindow.transform.GetChild(0).GetComponent<Text>().text = "이미 저장된 파일이 있습니다.\n기존 파일을 덮어쓰시겠습니까?";
        }
        else
        { 
            SaveCheckWindow.transform.GetChild(0).GetComponent<Text>().text = ButtonNum + 1 + "번 파일을 불러오시겠습니까?";
        }
        for(int i = 0; i < ThereIsSaveFile.Length; i++)
        {
            SaveBox[i].GetComponent<Button>().interactable = false;
        }
        SaveCheckWindow.SetActive(true);
        SaveFileNum = ButtonNum;
    }


    public void CloseSaveCheckWindow()
    {
        SaveCheckWindow.SetActive(false);
        for(int i = 0; i < ThereIsSaveFile.Length; i++)
        {
            SaveBox[i].GetComponent<Button>().interactable = true;
        }
        UpdateSaveData();
    }

    public void UpdateSaveData()
    { 
        for(int i = 0; i < ThereIsSaveFile.Length; i++)
        {
            SaveBox[i].transform.GetChild(1).gameObject.SetActive(true);
            SaveBox[i].transform.GetChild(2).gameObject.SetActive(true);
            SaveBox[i].transform.GetChild(3).gameObject.SetActive(false);
            SaveBox[i].transform.GetChild(1).GetComponent<Text>().text = "";
            SaveBox[i].transform.GetChild(2).GetComponent<Text>().text = "";

            if(File.Exists(DatabaseManager.instance.SaveFile + i))
            { 
                ThereIsSaveFile[i] = true;
                if(DatabaseManager.instance.GetTalkerName(i) != "")
                { 
                    SaveBox[i].transform.GetChild(1).GetComponent<Text>().text = DatabaseManager.instance.GetTalkerName(i) + " : ";
                }
                char[] SaveBoxText = DatabaseManager.instance.GetTalkData(i).ToCharArray();
                for(int j = 0; j < SaveBoxText.Length; j++)
                { 
                    if(SaveBoxText[j] != TalkManager.instance.LineBreakForTalk)
                    { 
                        SaveBox[i].transform.GetChild(2).GetComponent<Text>().text += SaveBoxText[j];
                    }
                }
                 
            }
            else
            { 
                ThereIsSaveFile[i] = false;
                SaveBox[i].transform.GetChild(1).gameObject.SetActive(false);
                SaveBox[i].transform.GetChild(2).gameObject.SetActive(false);
                SaveBox[i].transform.GetChild(3).gameObject.SetActive(true);
                if(isSave == false)
                { 
                    SaveBox[i].GetComponent<Button>().interactable = false;
                }
            }
        }
    }

    // 설정

    public void OpenSettingMenu()
    { 
        MenuCanvas.SetActive(true);
        MenuBackground.SetActive(false);        
        UpdateKeyData();
        SettingMenu.SetActive(true);
        var eventSystem = EventSystem.current;
        eventSystem.SetSelectedGameObject(ExitSettingButton, new BaseEventData(eventSystem));        
    }

    public void CloseSettingMenu()
    { 
        SettingMenu.SetActive(false);
        DatabaseManager.instance.SaveSettingDataOutside();
    }

    //이거 세이브 창이나 설정창에서 나오는 확인 버튼 누르면 CloseMenu호출되서 CloseMenu에서 CloseSettingMenu같은거 호출해주면됨
    

    public void UpdateKeyData()
    { 
        for(int i = 0; i < KeySettingText.Length; i++)
        { 
            KeySettingText[i].text = KeySetting.Key[(KeyAction)i].ToString(); 
        }    
    }



}
