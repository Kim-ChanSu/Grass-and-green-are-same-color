using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TalkManager : MonoBehaviour
{
    public static TalkManager instance;

    void Awake() 
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        else
        {
            Debug.LogWarning("씬에 토크 매니저가 2개이상 존재합니다.");
            Destroy(gameObject);
        }
    }

    [SerializeField]
    private GameObject[] TalkerImage; // 왼쪽 중앙 오른쪽 순으로 0 1 2
    [SerializeField]
    private GameObject TalkerImageBase;

    [SerializeField]
    private GameObject BackgroundCanvas;

    [SerializeField]
    private GameObject TalkCanvas;
    [SerializeField]
    private GameObject TalkBackground;
    [SerializeField]
    private GameObject TalkerNameText;
    [SerializeField]
    private GameObject TalkText;

    [SerializeField]
    private GameObject Choices;
    [SerializeField]
    private GameObject[] ChoiceButton;

    [SerializeField]
    private GameObject LogBackground;
    [SerializeField]
    private GameObject LogScrollviewContent;
    [SerializeField]
    private GameObject OpenLogButton;
    [SerializeField]
    private GameObject OpenMenuButton;
    [SerializeField]
    private GameObject Timer;
    [SerializeField]
    private GameObject LogTextPrefab;

    List<Dictionary<string, object>> TalkData;
    
    private int ChoiceCount = 0;

    private float TypingSpeed = 0.1f;
    [SerializeField]
    private float DefaultTalkSpeed = 0.1f;

    public char LineBreakForTalk = '#';

    /*
    void Start()
    {
        TalkText.GetComponent<Text>().text = "";
        TalkData = CSVReader.Read(GameManager.instance.GetCSVName()); 
        Debug.Log("대본의 길이는 " + TalkData.Count);
        NextTalk();
    }
    */


    private bool CheckNextTalk; // 순서꼬이는거 방지용으로 심어둠
    /*
     저거 구조 설명 스킵넣으면 코루틴이 꼬여버림 그래서 어짜피 대사 넘기는 거 나오기 전까지 
    막히는거 없이 루틴대로 흘러가니까 대화 이벤트가 감지되면 false로 바뀌도록 하면 됨

    */

    void Update()
    { 
        if(GameManager.instance.CheckTalk() == true || GameManager.instance.SkipCheck() == true)
        { 
            if(GameManager.instance.TextReading == true)
            { 
                GameManager.instance.TextReading = false;
            }
            else
            { 
                //GameManager.instance.isTalk= false; //임시
                if(GameManager.instance.CantTalkCheck() == false && CheckNextTalk == false)
                { 
                    CheckNextTalk = true;
                    PrepareNextTalk();
                }    
            }          
        }        
    }

    /* 이벤트 목록
        Choice <- 선택지를 의미 한다. 선택지가 여러 개이면 다음 행에도 적어야 한다. 선택지는 Key에 고유 값이 있어야하며 ResetKey 이벤트가 실행되기 전까지 Branch와 선택지 고유의 값이 같은 행만 실행된다.
        ChoiceEnd <- 선택지 설정이 끝났음을 의미한다.
        ResetKey <- 선택지로 인해 설정된 키 값을 초기화시킨다.
        ChangeKey <- Key값을 바꾼다. CSV파일 Key에 바꿀 Key값을 입력하면 된다. 이걸입력하면 키를 다시 비교하게 된다.
        If <- 조건을 분기시킨다.(자세한건 기획서 참고)

        ChangeCSV(미구현) csv파일을 바꾼다

        AddTalker <- 화자(Talker)를 추가한다. Talker에 추가될 인물의 이름을 적고 Content에는 화자가 들어갈 번호를 입력하면 된다. 왼 중 오 순으로 0 1 2
        RemoveTalker <- 화자(Talker)를 제거한다. Content에 제거될 번호를 입력하면 된다. 왼 중 오 순으로 0 1 2
        TalkerBack <- 모든 화자를 말하지 않는 상태로 변경한다.(이미지만 변경)
        
        Bookmark <- Content에 내용을 넣어 주석이나 위치 확인용으로 쓴다. 
        FindBookmark <- Bookmark를 찾는다 Talker에 찾기 시작할 순서를 Content에 찾을 내용을 넣는다.(일치해야함)
        IncreaseVar <- 변수값을 증가 시킨다. Talker에 바꿀 변수 번호 Content에 더할값을 입력한다.
        ChangeSwitch <- 스위치를 변경한다. Talker에 바꿀 스위치 번호 Content에 true인지 false인지 입력한다.

    */

    /*          
        이게 지금 구조적으로 생각해보니까
        코루틴으로 불러오긴 해도 문제 구조적으로 문제 되는 부분이 대사 이벤트쪽 하나 아닌가...? 
        게임시작 -> "TalkData"변수에 csv파일의 내용이 리스트에 들어감 
        이벤트를 읽어와서 이벤트에 맞는 내용을 실행시킨 후 코루틴으로 다음 행에 있는 이벤트를 실행시킴
        이때 대사나 연출 부분등 시간이 필요한 부분에서는 bool값을 비교해서 해당 하는 동작이 완료 될때까지 다음 행에 적힌
        코루틴을 반복 발생시키다가 동작이 완료된게 확인(bool)값이 바뀌면 호출을 멈추고 위의 과정을 반복하게 됨
        이때 Choice이벤트 쪽은 그런거 없고 버튼눌러야 다음 이벤트가 실행 됨
        어짜피 일반 이벤트쪽들은 바로 바로 넘어갈꺼가니 큰 차이 없을꺼고(bool값으로 안막힘) 
        연출부분에서 막으려면 연출 끝난거 확인한다음 그쪽에서 다음을 불러오면 되긴하는데 그러면 저쪽에서 끝난거 확인되면 불러올 수 있게 public 선언 하든가
        지금 방식대로 bool값으로 하든가이고
        대화쪽은 걍 키 입력 체크할때 불러오면 되고... 나머지는 걍 냅둬도 반복호출 안 가고 물흐르듯 진행될테니 상관이 없네? 
        코루틴이 더 메모리 먹는거 아니면 void로 고칠 필요는 없을듯
    */

    public void PrepareNextTalk()
    { 
        GameManager.instance.PlusCSVNum(1);
        NextTalk();        
    }


    private void Talking(int i) // Talk에 들어가는 대화 코드가 들어가는곳
    { 
        TalkText.GetComponent<Text>().text = "";
        GameManager.instance.isTalk = true;

        TalkerNameText.GetComponent<Text>().text = "";
        OnTalkBackground();

        if(TalkData[i]["Talker"].ToString() == "0")
        { 
            if(GameManager.instance.Talker[0] != null)
            {
                Debug.Log("등장인물이 체크 되었습니다.");
                ChangeTalker(0);
                TalkerNameText.GetComponent<Text>().text = GameManager.instance.Talker[0].KoreanName;   
                TypingSpeed = GameManager.instance.Talker[0].TalkSpeed;
            }
            else
            { 
                TalkerNameText.GetComponent<Text>().text = TalkData[i]["Talker"].ToString();
                Debug.Log("동일한 인물이 없으므로 이름만 출력합니다.");                
            }
        }
        else if(TalkData[i]["Talker"].ToString() == "1")
        { 
            if(GameManager.instance.Talker[1] != null)
            {
                Debug.Log("등장인물이 체크 되었습니다.");
                ChangeTalker(1);
                TalkerNameText.GetComponent<Text>().text = GameManager.instance.Talker[1].KoreanName;  
                TypingSpeed = GameManager.instance.Talker[1].TalkSpeed;
            }
            else
            { 
                TalkerNameText.GetComponent<Text>().text = TalkData[i]["Talker"].ToString();
                Debug.Log("동일한 인물이 없으므로 이름만 출력합니다.");                
            }
        }
        else if(TalkData[i]["Talker"].ToString() == "2")
        { 
            if(GameManager.instance.Talker[2] != null)
            { 
                Debug.Log("등장인물이 체크 되었습니다.");
                ChangeTalker(2);
                TalkerNameText.GetComponent<Text>().text = GameManager.instance.Talker[2].KoreanName; 
                TypingSpeed = GameManager.instance.Talker[2].TalkSpeed;
            }
            else
            { 
                TalkerNameText.GetComponent<Text>().text = TalkData[i]["Talker"].ToString();
                Debug.Log("동일한 인물이 없으므로 이름만 출력합니다.");                
            }
        }
        else
        {
            int TalkerCheck = 0;
            for(int j = 0; j < GameManager.instance.Talker.Length; j++)
            {
                if(GameManager.instance.Talker[j] != null)
                { 
                    if(GameManager.instance.Talker[j].Name == TalkData[i]["Talker"].ToString())
                    {
                        Debug.Log("등장인물이 체크 되었습니다.");
                        //나중에 말하는사람 바뀌는거 추가
                        // 구조는 TalkData[i]["Talker"].ToString()이 ""인가 아닌가 구별해서 적용하면 빈칸일때 화자 안바뀌도록 냅둘수 있는데
                        // 그럼 공백을 따로 넣어줘야함
                        ChangeTalker(j);
                        TalkerNameText.GetComponent<Text>().text = GameManager.instance.Talker[j].KoreanName;   
                        TypingSpeed = GameManager.instance.Talker[j].TalkSpeed;
                        break;
                    }
                } 
                TalkerCheck ++;
            }

            if(TalkerCheck >= GameManager.instance.Talker.Length)
            {
                TalkerNameText.GetComponent<Text>().text = TalkData[i]["Talker"].ToString();
                TypingSpeed = DefaultTalkSpeed;
                Debug.Log("동일한 인물이 없으므로 이름만 출력합니다.");
            }
            TalkerCheck = 0;
        }      

        TalkLog(TalkData[i]["Content"].ToString()); // 로그

        //말하는 속도 조정할꺼면 저기서 0.1f 바꾸자.
        StartCoroutine(Typing(TalkData[i]["Content"].ToString(), TypingSpeed));
           
    }
    
    // 기본
   IEnumerator Talk(int i)
   { 
        if(GameManager.instance.CantTalkCheck() == false)
        {
            CheckNextTalk = false;
            Talking(i);
        }
        else
        { 
            yield return null;
            StartCoroutine(Talk(i));   
        }
   }

    IEnumerator Typing(string text, float talkspeed)
    {
        if(GameManager.instance.isTalk == true)
        { 
            GameManager.instance.TextReading = true;
            
            char[] letter =  text.ToCharArray();
            for(int i = 0; i < letter.Length; i++)
            {
                if(letter[i] == LineBreakForTalk)
                { 
                    TalkText.GetComponent<Text>().text += "\n";
                }
                else
                { 
                    TalkText.GetComponent<Text>().text += letter[i];
                }
                yield return new WaitForSeconds(talkspeed);

                if(GameManager.instance.TextReading == false)
                {                    
                    TalkText.GetComponent<Text>().text = "";
                    for(int j = 0; j < letter.Length; j++)
                    {
                        if(letter[j] == LineBreakForTalk)
                        { 
                            TalkText.GetComponent<Text>().text += "\n";
                        }
                        else
                        { 
                            TalkText.GetComponent<Text>().text += letter[j];
                        }
                    }
                    GameManager.instance.isTalk= false;//임시
                    Debug.Log("말하기 스킵");
                    break;                   
                }                
             }
            

            /*
            foreach (char letter in text.ToCharArray())
            {
                TalkText.GetComponent<Text>().text += letter;
                yield return new WaitForSeconds(talkspeed);

                if(GameManager.instance.TextReading == false)
                {                    
                    TalkText.GetComponent<Text>().text = text;
                    Debug.Log("말하기 스킵");
                    break;                   
                }
            }
            */
            GameManager.instance.TextReading = false;
            GameManager.instance.isTalk= false;//임시
        }
    }

    private void TalkLog(string Talk)
    {
        GameManager.instance.LogNum++;
        if(GameManager.instance.LogNum >= GameManager.instance.GameLog.TalkLog.Length)
        { 
           GameManager.instance.LogNum = 0; 
        }
        GameManager.instance.GameLog.TalkerNameLog[GameManager.instance.LogNum] = TalkerNameText.GetComponent<Text>().text;
        GameManager.instance.GameLog.TalkLog[GameManager.instance.LogNum] = Talk;

    }
    
    private void NextTalkCheck()
    {         
        if(TalkData[GameManager.instance.GetCSVNum()]["Event"].ToString() != "")
        { 
            if(EventCheck(GameManager.instance.GetCSVNum()) == false)
            { 
                StartCoroutine(Talk(GameManager.instance.GetCSVNum()));
            }
        }
        else
        { 
            StartCoroutine(Talk(GameManager.instance.GetCSVNum()));
        }                  
    }

    // 기본
    private void NextTalk()
    {
        if(GameManager.instance.GetCSVNum() < TalkData.Count)
        {  
            if(TalkData[GameManager.instance.GetCSVNum()]["Event"].ToString() == "ResetKey")
            { 
                StartCoroutine(ResetKey());
            }
            else
            { 
                if(GameManager.instance.GetKeyCheck() == true)
                {
                    Debug.Log("KeyCheck조건이 걸려있습니다.");

                    if(TalkData[GameManager.instance.GetCSVNum()]["Branch"].ToString() == GameManager.instance.GetChoiceKey())
                    {
                        Debug.Log("Branch = ChoiceKey임으로 내용을 출력합니다.");
                        NextTalkCheck();
                    }
                    else
                    {
                        Debug.Log("Branch != ChoiceKey임으로 내용을 스킵합니다.");
                        PrepareNextTalk();
                    }
                
                }
                else
                { 
                    NextTalkCheck();
                }
            }
        }
    }

    private bool EventCheck(int i)
    { 
        if(TalkData[i]["Event"].ToString() == "Choice")
        { 
            Debug.Log("ChoiceCheck");
            StartCoroutine(Choice(i));
            return true;            
        }
        else if(TalkData[i]["Event"].ToString() == "ChoiceEnd")
        { 
            Debug.Log("ChoiceEndCheck");
            StartCoroutine(ChoiceEnd(i));
            return true;                 
        }
        else if(TalkData[i]["Event"].ToString() == "ChangeKey")
        { 
            Debug.Log("ChangeKeyCheck");
            StartCoroutine(ChangeKey(i));
            return true;                 
        }       
        else if(TalkData[i]["Event"].ToString() == "AddTalker")
        { 
            Debug.Log("AddTalkerCheck");
            StartCoroutine(AddTalker(i));
            return true;                      
        }
        else if(TalkData[i]["Event"].ToString() == "RemoveTalker")
        { 
            Debug.Log("RemoveTalkerCheck");
            StartCoroutine(RemoveTalker(i));
            return true;                
        }
        else if(TalkData[i]["Event"].ToString() == "SelectTalker")
        {
            Debug.Log("SelectTalkerCheck");
            StartCoroutine(SelectTalker(i));
            return true;  
        }  
        else if(TalkData[i]["Event"].ToString() == "TalkerBack")
        {
            Debug.Log("TalkerBackCheck");
            StartCoroutine(TalkerBack());
            return true;  
        }
        else if(TalkData[i]["Event"].ToString() == "If")
        {
            Debug.Log("IfCheck");
            StartCoroutine(If(i));
            return true;  
        } 
        else if(TalkData[i]["Event"].ToString() == "IfByName")
        {
            Debug.Log("IfByNameCheck");
            StartCoroutine(IfByName(i));
            return true;             
        }
        else if(TalkData[i]["Event"].ToString() == "IfByVar")
        {
            Debug.Log("IfByVarCheck");
            StartCoroutine(IfByVar(i));
            return true;             
        }
        else if(TalkData[i]["Event"].ToString() == "IfByVarByName")
        {
            Debug.Log("IfByVarByNameCheck");
            StartCoroutine(IfByVarByName(i));
            return true;             
        }
        else if(TalkData[i]["Event"].ToString() == "Bookmark")
        {
            Debug.Log("BookmarkCheck");
            StartCoroutine(Bookmark());
            return true;  
        }   
        else if(TalkData[i]["Event"].ToString() == "FindBookmark")
        {
            Debug.Log("FindBookmarkCheck");
            StartCoroutine(FindBookmark(i));
            return true;  
        }
        else if(TalkData[i]["Event"].ToString() == "IncreaseVar")
        {
            Debug.Log("IncreaseVarCheck");
            StartCoroutine(IncreaseVar(i));
            return true;  
        }
        else if(TalkData[i]["Event"].ToString() == "IncreaseVarByName")
        {
            Debug.Log("IncreaseVarByNameCheck");
            StartCoroutine(IncreaseVarByName(i));
            return true;  
        }
        else if(TalkData[i]["Event"].ToString() == "IncreaseVarByVar")
        {
            Debug.Log("IncreaseVarByVarCheck");
            StartCoroutine(IncreaseVarByVar(i));
            return true;  
        }
        else if(TalkData[i]["Event"].ToString() == "IncreaseVarByVarByName")
        {
            Debug.Log("IncreaseVarByVarByNameCheck");
            StartCoroutine(IncreaseVarByVarByName(i));
            return true;  
        }
        else if(TalkData[i]["Event"].ToString() == "IncreaseVarByRand")
        {
            Debug.Log("IncreaseVarByRandCheck");
            StartCoroutine(IncreaseVarByRand(i));
            return true;  
        }
        else if(TalkData[i]["Event"].ToString() == "IncreaseVarByRandByName")
        {
            Debug.Log("IncreaseVarByRandByNameCheck");
            StartCoroutine(IncreaseVarByRandByName(i));
            return true;  
        }
        else if(TalkData[i]["Event"].ToString() == "ChangeVar")
        {
            Debug.Log("ChangeVarCheck");
            StartCoroutine(ChangeVar(i));
            return true;  
        }
        else if(TalkData[i]["Event"].ToString() == "ChangeVarByName")
        {
            Debug.Log("ChangeVarByNameCheck");
            StartCoroutine(ChangeVarByName(i));
            return true;  
        }
        else if(TalkData[i]["Event"].ToString() == "ChangeVarByVar")
        {
            Debug.Log("ChangeVarByVarCheck");
            StartCoroutine(ChangeVarByVar(i));
            return true;  
        }
        else if(TalkData[i]["Event"].ToString() == "ChangeVarByVarByName")
        {
            Debug.Log("ChangeVarByVarByNameCheck");
            StartCoroutine(ChangeVarByVarByName(i));
            return true;  
        }
        else if(TalkData[i]["Event"].ToString() == "ChangeVarByRand")
        {
            Debug.Log("ChangeVarByRandCheck");
            StartCoroutine(ChangeVarByRand(i));
            return true;  
        }
        else if(TalkData[i]["Event"].ToString() == "ChangeVarByRandByName")
        {
            Debug.Log("ChangeVarByRandByNameCheck");
            StartCoroutine(ChangeVarByRandByName(i));
            return true;  
        }
        else if(TalkData[i]["Event"].ToString() == "ChangeSwitch")
        {
            Debug.Log("ChangeSwitchCheck");
            StartCoroutine(ChangeSwitch(i));
            return true;  
        }
        else if(TalkData[i]["Event"].ToString() == "ChangeSwitchByName")
        {
            Debug.Log("ChangeSwitchByNameCheck");
            StartCoroutine(ChangeSwitchByName(i));
            return true;  
        }
        else if(TalkData[i]["Event"].ToString() == "ChangeSwitchByVar")
        {
            Debug.Log("ChangeSwitchByVarCheck");
            StartCoroutine(ChangeSwitchByVar(i));
            return true;  
        }
        else if(TalkData[i]["Event"].ToString() == "ChangeSwitchByVarByName")
        {
            Debug.Log("ChangeSwitchByVarByNameCheck");
            StartCoroutine(ChangeSwitchByVarByName(i));
            return true;  
        }
        else if(TalkData[i]["Event"].ToString() == "ChangeBackground")
        {
            Debug.Log("ChangeBackgroundCheck");
            StartCoroutine(ChangeBackground(i));
            return true;  
        } 
        else if(TalkData[i]["Event"].ToString() == "ChangeBGM")
        {
            Debug.Log("ChangeBGMCheck");
            StartCoroutine(ChangeBGM(i));
            return true;  
        }  
        else if(TalkData[i]["Event"].ToString() == "StartBGM")
        {
            Debug.Log("StartBGMCheck");
            StartCoroutine(StartBGM());
            return true;  
        }  
        else if(TalkData[i]["Event"].ToString() == "StopBGM")
        {
            Debug.Log("StopBGMCheck");
            StartCoroutine(StopBGM());
            return true;  
        }   
        else if(TalkData[i]["Event"].ToString() == "PlaySE")
        {
            Debug.Log("PlaySECheck");
            StartCoroutine(PlaySE(i));
            return true;  
        }  
        else if(TalkData[i]["Event"].ToString() == "SetTimer")
        {
            Debug.Log("SetTimerCheck");
            StartCoroutine(SetTimer(i));
            return true;  
        } 
        else if(TalkData[i]["Event"].ToString() == "FadeOut")
        {
            Debug.Log("FadeOutCheck");
            StartCoroutine(FadeOut(i));
            return true;  
        }  
        else if(TalkData[i]["Event"].ToString() == "FadeIn")
        {
            Debug.Log("FadeInCheck");
            StartCoroutine(FadeIn(i));
            return true;  
        } 
        else if(TalkData[i]["Event"].ToString() == "Flash")
        {
            Debug.Log("FlashCheck");
            StartCoroutine(Flash(i));
            return true;  
        } 
        else if(TalkData[i]["Event"].ToString() == "ChangeBackgroundByMove")
        {
            Debug.Log("ChangeBackgroundByMoveCheck");
            StartCoroutine(ChangeBackgroundByMove(i));
            return true;  
        } 
        else if(TalkData[i]["Event"].ToString() == "ReturnTitle")
        {
            Debug.Log("ReturnTitleCheck");
            StartCoroutine(ReturnTitle(i));
            return true;  
        } 
        else if(TalkData[i]["Event"].ToString() == "SaveCSV")
        {
            Debug.Log("SaveCSVCheck");
            StartCoroutine(SaveCSV());
            return true;  
        }
        else if(TalkData[i]["Event"].ToString() == "LoadCSV")
        {
            Debug.Log("LoadCSVCheck");
            StartCoroutine(LoadCSV(i));
            return true;  
        }    
        else if(TalkData[i]["Event"].ToString() == "ChangeCSV")
        {
            Debug.Log("ChangeCSVCheck");
            StartCoroutine(ChangeCSV(i));
            return true;  
        }  
        else if(TalkData[i]["Event"].ToString() == "EndingCredit")
        {
            Debug.Log("EndingCreditCheck");
            StartCoroutine(EndingCredit(i));
            return true;  
        } 
        else if(TalkData[i]["Event"].ToString() == "RemoveEndingCredit")
        {
            Debug.Log("RemoveEndingCreditCheck");
            StartCoroutine(RemoveEndingCredit(i));
            return true;  
        } 
        else
        {
            Debug.LogWarning("Event에 잘못된 값이 들어왔습니다 현재 CSVNum의 값은 " + i);
            return false; 
        }
    }

    IEnumerator ChangeCSV(int CSVNum)
    { 
        if(GameManager.instance.CantTalkCheck() == false)
        {
            int NewCSVNum = 0;

            if(TalkData[CSVNum]["Talker"].ToString() == "")
            { 
                NewCSVNum = 0;
            }
            else
            { 
                NewCSVNum = int.Parse(TalkData[CSVNum]["Talker"].ToString());
            }


            if(TalkData[CSVNum]["Key"].ToString() == "true" || TalkData[CSVNum]["Key"].ToString() == "ResetKey")
            { 
                ChangeCSVFile(TalkData[CSVNum]["Content"].ToString(), NewCSVNum, true);
            }
            else
            { 
                ChangeCSVFile(TalkData[CSVNum]["Content"].ToString(), NewCSVNum, false);
            }
        }
        else
        { 
            yield return null;
            StartCoroutine(ChangeCSV(CSVNum));              
        }
    }      
    
    
   IEnumerator Choice(int i)
   { 
        if(GameManager.instance.CantTalkCheck() == false)
        {
            // 이거 이렇게 짜면 나중에 불러올때 전 데이터들도 읽어야 할듯
            // 일반 대사인건 Event ""로 체크해야 할듯
            if(ChoiceCount < 3)
            {
                Debug.Log(ChoiceCount + " 번 선택지 들어옴");                       
                
                ChoiceButton[ChoiceCount].transform.GetChild(0).GetComponent<Text>().text = TalkData[i]["Content"].ToString();
                if(ChoiceCount == 0)
                { 
                    GameManager.instance.ChangeFirstChoiceKey(TalkData[i]["Key"].ToString());
                    Debug.Log("0번 선택지의 키 값은 " + GameManager.instance.GetFirstChoiceKey());
                }
                if(ChoiceCount == 1)
                { 
                    GameManager.instance.ChangeSecondChoiceKey(TalkData[i]["Key"].ToString());
                    Debug.Log("1번 선택지의 키 값은 " + GameManager.instance.GetSecondChoiceKey());
                }
                if(ChoiceCount == 2)
                { 
                    GameManager.instance.ChangeThirdChoiceKey(TalkData[i]["Key"].ToString());
                    Debug.Log("2번 선택지의 키 값은 " + GameManager.instance.GetThirdChoiceKey());
                }
                ChoiceButton[ChoiceCount].SetActive(true);
                ChoiceCount++;
            }
            else
            { 
                Debug.Log("선택지가 3개가 넘어갑니다.");               
            }            
            PrepareNextTalk();   

            /*
            GameManager.instance.ChangeChoiceNum(0);
            string TextBeta = TalkData[i]["Content"].ToString();
            char[] ChoiceText =  TextBeta.ToCharArray();

            int ButtonNumber = 0;
            ChoiceButton[ButtonNumber].SetActive(true);

            ChoiceButton[ButtonNumber].transform.GetChild(0).GetComponent<Text>().text = "";
            for(int t = 0; t < ChoiceText.Length; t++)
            { 
                if(ChoiceText[t] == '#')
                { 
                    ButtonNumber++;
                    if(ButtonNumber >= ChoiceButton.Length)
                    {
                        Debug.LogWarning("선택지가 " + ChoiceButton.Length +"개 이상입니다");
                        ButtonNumber = ChoiceButton.Length - 1;
                        break;    
                    }
                    else
                    {
                        ChoiceButton[ButtonNumber].SetActive(true);
                        ChoiceButton[ButtonNumber].transform.GetChild(0).GetComponent<Text>().text = "";
                    }
                }
                else
                { 
                    ChoiceButton[ButtonNumber].transform.GetChild(0).GetComponent<Text>().text += ChoiceText[t];
                }
            }
            */
        }
        else
        { 
            yield return null;
            StartCoroutine(Choice(i));   
        }
    }

    IEnumerator ChoiceEnd(int i)
    {
       if(GameManager.instance.CantTalkCheck() == false)
       {
            GameManager.instance.ChangeKeyCheck(true);
            Debug.Log("KeyCheck가 변경 되었습니다. 현재 값은 " + GameManager.instance.GetKeyCheck());

            GameManager.instance.isChoice = true;
            ChoiceCount = 0;
            Debug.Log("선택지 설정이 끝남을 체크");

            var eventSystem = EventSystem.current;
            eventSystem.SetSelectedGameObject(ChoiceButton[0], new BaseEventData(eventSystem));
            
            /* 여기서는 선택지를 눌러야 넘어가기 때문에 이걸 적용하지 않는다.
                PrepareNextTalk();
            */
        }
       else
       { 
            yield return null;
            StartCoroutine(ChoiceEnd(i));            
       }
    }

    public void TurnOffChoiceButton() // 선택지 눌렀을때 버튼들 사라지게 하기용
    {
        for(int i = 0; i < ChoiceButton.Length; i++)
        {
            ChoiceButton[i].transform.GetChild(0).GetComponent<Text>().text = "";
            ChoiceButton[i].SetActive(false);
        }
        GameManager.instance.ChangeFirstChoiceKey("1");
        GameManager.instance.ChangeSecondChoiceKey("2");
        GameManager.instance.ChangeThirdChoiceKey("3");
    }

    public void ChoiceButtonOff()
    {
        for(int i = 0; i < ChoiceButton.Length; i++)
        { 
            ChoiceButton[i].GetComponent<Button>().interactable = false;   
        }      
    }

    public void ChoiceButtonOn()
    {
        for(int i = 0; i < ChoiceButton.Length; i++)
        { 
            ChoiceButton[i].GetComponent<Button>().interactable = true;   
        }
        if(GameManager.instance.isChoice == true)
        { 
            var eventSystem = EventSystem.current;
            eventSystem.SetSelectedGameObject(ChoiceButton[0], new BaseEventData(eventSystem));            
        }
    }

    IEnumerator ResetKey()
    { 
       if(GameManager.instance.CantTalkCheck() == false)
       {
            ResetGameKey();
            PrepareNextTalk();
       }
       else
       { 
            yield return null;
            StartCoroutine(ResetKey());            
       }
    }

    private void ResetGameKey()
    { 
        GameManager.instance.ChangeKeyCheck(false);
        GameManager.instance.ChangeChoiceKey("0");
        GameManager.instance.ChangeFirstChoiceKey("0");
        GameManager.instance.ChangeSecondChoiceKey("0");
        GameManager.instance.ChangeThirdChoiceKey("0");

        Debug.Log("키(선택지)설정이 초기화 되었습니다");        
    }


    IEnumerator ChangeKey(int CSVNum)
    { 
       if(GameManager.instance.CantTalkCheck() == false)
       {
            ChangeGameKey(CSVNum);
            PrepareNextTalk();
       }
       else
       { 
            yield return null;
            StartCoroutine(ChangeKey(CSVNum));            
       }
    }

    private void ChangeGameKey(int CSVNum)
    { 
        GameManager.instance.ChangeKeyCheck(true);            
        GameManager.instance.ChangeChoiceKey(TalkData[CSVNum]["Key"].ToString());       
    }

    private void ChangeTalker(int i) // -1 일시 전부 
    {
        GameManager.instance.ChangeTalkerNum(i); 
        Debug.Log("화자 변경 : " + i);
        if(i == 0)
        { 
            TalkerImage[0].GetComponent<Image>().color = new Color(255/255f, 255/255f, 255/255f, 255/255f);
            TalkerImage[0].transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            TalkerImage[1].GetComponent<Image>().color = new Color(123/255f, 123/255f, 123/255f, 200/255f);
            TalkerImage[1].transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            TalkerImage[2].GetComponent<Image>().color = new Color(123/255f, 123/255f, 123/255f, 200/255f);
            TalkerImage[2].transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);            
        }
        else if(i == 1)
        { 
            TalkerImage[1].GetComponent<Image>().color = new Color(255/255f, 255/255f, 255/255f, 255/255f);
            TalkerImage[1].transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            TalkerImage[0].GetComponent<Image>().color = new Color(123/255f, 123/255f, 123/255f, 200/255f);
            TalkerImage[0].transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            TalkerImage[2].GetComponent<Image>().color = new Color(123/255f, 123/255f, 123/255f, 200/255f);
            TalkerImage[2].transform.localScale = new Vector3(0.9f, 0.9f, 0.9f); 
        }
        else if(i == 2)
        { 
            TalkerImage[2].GetComponent<Image>().color = new Color(255/255f, 255/255f, 255/255f, 255/255f);
            TalkerImage[2].transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            TalkerImage[1].GetComponent<Image>().color = new Color(123/255f, 123/255f, 123/255f, 200/255f);
            TalkerImage[1].transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            TalkerImage[0].GetComponent<Image>().color = new Color(123/255f, 123/255f, 123/255f, 200/255f);
            TalkerImage[0].transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
        }
        else if(i == -1)
        { 
            TalkerImage[0].GetComponent<Image>().color = new Color(123/255f, 123/255f, 123/255f, 200/255f);
            TalkerImage[0].transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            TalkerImage[1].GetComponent<Image>().color = new Color(123/255f, 123/255f, 123/255f, 200/255f);
            TalkerImage[1].transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            TalkerImage[2].GetComponent<Image>().color = new Color(123/255f, 123/255f, 123/255f, 200/255f);
            TalkerImage[2].transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);            
        }
        else
        { 
            Debug.LogWarning("TalkManager의 ChangeTalker(int i)에 잘못된 값이 들어왔습니다.");   
        }
    }

    IEnumerator SelectTalker(int CSVNum)
    { 
       if(GameManager.instance.CantTalkCheck() == false)
       {
            ChangeTalker(int.Parse(TalkData[CSVNum]["Content"].ToString()));
            PrepareNextTalk();
       }
       else
       { 
            yield return null;
            StartCoroutine(SelectTalker(CSVNum));            
       }
    }

    private void TalkerPlus(int Position, string name)
    { 
        for(int i = 0; i < CharacterDatabaseManager.instance.DBLength(); i++)
        { 
            if(CharacterDatabaseManager.instance.CharacterDB(i).Name == name)
            { 
                GameManager.instance.Talker[Position] = CharacterDatabaseManager.instance.CharacterDB(i);
                TalkerImage[Position].GetComponent<Image>().sprite = GameManager.instance.Talker[Position].Face;

                TalkerImage[Position].GetComponent<Image>().color = new Color(123/255f, 123/255f, 123/255f, 200/255f);
                TalkerImage[Position].transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                break;
            }
        }
    }

    private void TalkerMinus(int Position)
    { 
        GameManager.instance.Talker[Position] = null;
        TalkerImage[Position].GetComponent<Image>().sprite = GameManager.instance.GetBlank();       
    }

    IEnumerator AddTalker(int CSVNum)
    {
       if(GameManager.instance.CantTalkCheck() == false)
       {
            TalkerPlus(int.Parse(TalkData[CSVNum]["Content"].ToString()), TalkData[CSVNum]["Talker"].ToString());           
            PrepareNextTalk();            
        }
       else
       { 
            yield return null;
            StartCoroutine(AddTalker(CSVNum));            
       }
    }

    IEnumerator RemoveTalker(int CSVNum)
    {
       if(GameManager.instance.CantTalkCheck() == false)
       {
            TalkerMinus(int.Parse(TalkData[CSVNum]["Content"].ToString()));
            PrepareNextTalk();            
       }
       else
       { 
            yield return null;
            StartCoroutine(RemoveTalker(CSVNum));            
       }
    }

    IEnumerator TalkerBack()
    {
       if(GameManager.instance.CantTalkCheck() == false)
       {
            ChangeTalker(-1);           
            PrepareNextTalk();            
       }
       else
       { 
            yield return null;
            StartCoroutine(TalkerBack());            
       }
    }

    IEnumerator If(int CSVNum)
    { 
        if(GameManager.instance.CantTalkCheck() == false)
        {
            IfCheck(CSVNum);       
            PrepareNextTalk();            
        }
        else
        { 
            yield return null;
            StartCoroutine(If(CSVNum));            
        }       
    }

    IEnumerator IfByName(int CSVNum)
    { 
        if(GameManager.instance.CantTalkCheck() == false)
        {
            IfCheckByName(CSVNum);       
            PrepareNextTalk();            
        }
        else
        { 
            yield return null;
            StartCoroutine(IfByName(CSVNum));            
        }       
    }

    IEnumerator IfByVar(int CSVNum)
    { 
        if(GameManager.instance.CantTalkCheck() == false)
        {
            IfCheckByVar(CSVNum);       
            PrepareNextTalk();            
        }
        else
        { 
            yield return null;
            StartCoroutine(IfByVar(CSVNum));            
        }       
    }

    IEnumerator IfByVarByName(int CSVNum)
    { 
        if(GameManager.instance.CantTalkCheck() == false)
        {
            IfCheckByVarByName(CSVNum);       
            PrepareNextTalk();            
        }
        else
        { 
            yield return null;
            StartCoroutine(IfByVarByName(CSVNum));            
        }       
    }

    IEnumerator Bookmark()
    { 
        if(GameManager.instance.CantTalkCheck() == false)
        {     
            PrepareNextTalk();            
        }
        else
        { 
            yield return null;
            StartCoroutine(Bookmark());            
        }       
    }

    IEnumerator FindBookmark(int CSVNum)
    { 
        if(GameManager.instance.CantTalkCheck() == false)
        {
            int alpha = 0;

            if(TalkData[CSVNum]["Talker"].ToString() == "")
            { 
                alpha = 0;
            }
            else
            { 
               alpha = int.Parse(TalkData[CSVNum]["Talker"].ToString());
            }           
            for(int i = alpha; i < TalkData.Count; i++)
            { 
                if(TalkData[i]["Event"].ToString() == "Bookmark")
                { 
                    if(TalkData[CSVNum]["Content"].ToString() == TalkData[i]["Content"].ToString())
                    {
                        Debug.Log("일치하는 Bookmark를 찾았습니다. CSVnum = " + i);
                        GameManager.instance.ChangeCSVNum(i);
                        StopAllCoroutines();

                        if(TalkData[CSVNum]["Key"].ToString() == "ResetKey")
                        { 
                            ResetGameKey();                           
                        }
                        else if(TalkData[CSVNum]["Key"].ToString() == "ChangeKey")
                        { 
                            ChangeGameKey(i);                           
                        }                       
                        break;                       
                    }
                }
            }
            PrepareNextTalk();            
        }
        else
        { 
            yield return null;
            StartCoroutine(FindBookmark(CSVNum));            
        }         
    }

    IEnumerator IncreaseVar(int CSVNum)
    {
        if(GameManager.instance.CantTalkCheck() == false)
        {
            int ChangeNum = int.Parse(TalkData[CSVNum]["Talker"].ToString());
            int IncreaseNum = int.Parse(TalkData[CSVNum]["Content"].ToString());

            if(ChangeNum < GameManager.instance.Var.Length)
            { 
                GameManager.instance.Var[ChangeNum].Var += IncreaseNum;
                Debug.Log(ChangeNum + "번 변수(" + GameManager.instance.Var[ChangeNum].VarName + ")가 " +  IncreaseNum + "만큼 증가하였습니다. 현재 값 = " + GameManager.instance.Var[ChangeNum].Var);
                
            }
            else
            { 
                Debug.LogWarning("IncreaseVar 이벤트 Talker에 잘못된 값이 들어왔습니다. 현재 CSVNum은 " + CSVNum);
            }
            PrepareNextTalk();
        }
        else
        { 
            yield return null;
            StartCoroutine(IncreaseVar(CSVNum));            
        }  
    }

    IEnumerator IncreaseVarByName(int CSVNum)
    {
        if(GameManager.instance.CantTalkCheck() == false)
        {
            int IncreaseNum = int.Parse(TalkData[CSVNum]["Content"].ToString());
            int Check = 0;

            for(int i = 0; i < GameManager.instance.Var.Length; i++)
            { 
                if(GameManager.instance.Var[i].VarName == TalkData[CSVNum]["Talker"].ToString())
                { 
                    GameManager.instance.Var[i].Var += IncreaseNum;
                    Debug.Log(i + "번 변수(" + GameManager.instance.Var[i].VarName + ")가 " +  IncreaseNum + "만큼 증가하였습니다. 현재 값 = " + GameManager.instance.Var[i].Var);
                    break;
                }
                Check++;
            }

            if(Check >= GameManager.instance.Var.Length)
            { 
                Debug.LogWarning("IncreaseVarByName 이벤트 Talker에 잘못된 값이 들어왔습니다. 현재 CSVNum은 " + CSVNum);
            }
            Check = 0;
            PrepareNextTalk();
        }
        else
        { 
            yield return null;
            StartCoroutine(IncreaseVarByName(CSVNum));            
        }  
    }

    IEnumerator IncreaseVarByVar(int CSVNum)
    {
        if(GameManager.instance.CantTalkCheck() == false)
        {
            int ChangeNum = int.Parse(TalkData[CSVNum]["Talker"].ToString());
            int IncreaseNum = int.Parse(TalkData[CSVNum]["Content"].ToString());

            if(ChangeNum < GameManager.instance.Var.Length && IncreaseNum < GameManager.instance.Var.Length)
            { 
                GameManager.instance.Var[ChangeNum].Var += GameManager.instance.Var[IncreaseNum].Var;
                Debug.Log(ChangeNum + "번 변수(" + GameManager.instance.Var[ChangeNum].VarName + ")가 " +  GameManager.instance.Var[IncreaseNum].Var + "만큼 증가하였습니다. 현재 값 = " + GameManager.instance.Var[ChangeNum].Var);                
            }
            else
            { 
                Debug.LogWarning("IncreaseVarByVar 이벤트 Talker나 Content 잘못된 값이 들어왔습니다. 현재 CSVNum은 " + CSVNum);
            }
            PrepareNextTalk();
        }
        else
        { 
            yield return null;
            StartCoroutine(IncreaseVarByVar(CSVNum));            
        }  
    }

    IEnumerator IncreaseVarByVarByName(int CSVNum)
    {
        if(GameManager.instance.CantTalkCheck() == false)
        {
            int ChangeNum = 0;
            int IncreaseNum = 0;
            int Check = 0;

            for(int i = 0; i < GameManager.instance.Var.Length; i++)
            { 
                if(GameManager.instance.Var[i].VarName == TalkData[CSVNum]["Talker"].ToString())
                { 
                    ChangeNum = i;                    
                    break;
                }
                Check++;
            }

            if(Check >= GameManager.instance.Var.Length)
            { 
                Debug.LogWarning("IncreaseVarByVarByName 이벤트 Talker에 잘못된 값이 들어왔습니다. 현재 CSVNum은 " + CSVNum);
            }
            Check = 0;

            for(int i = 0; i < GameManager.instance.Var.Length; i++)
            { 
                if(GameManager.instance.Var[i].VarName == TalkData[CSVNum]["Content"].ToString())
                { 
                    IncreaseNum = i;                    
                    break;
                }
                Check++;
            }

            if(Check >= GameManager.instance.Var.Length)
            { 
                Debug.LogWarning("IncreaseVarByVarByName 이벤트 Content에 잘못된 값이 들어왔습니다. 현재 CSVNum은 " + CSVNum);
            }
            Check = 0;

            GameManager.instance.Var[ChangeNum].Var += GameManager.instance.Var[IncreaseNum].Var;
            Debug.Log(ChangeNum + "번 변수(" + GameManager.instance.Var[ChangeNum].VarName + ")가 " +  GameManager.instance.Var[IncreaseNum].Var + "만큼 증가하였습니다. 현재 값 = " + GameManager.instance.Var[ChangeNum].Var);
            
            PrepareNextTalk();
        }
        else
        { 
            yield return null;
            StartCoroutine(IncreaseVarByVarByName(CSVNum));            
        }  
    }

    IEnumerator ChangeVar(int CSVNum)
    {
        if(GameManager.instance.CantTalkCheck() == false)
        {
            int VarNum = int.Parse(TalkData[CSVNum]["Talker"].ToString());
            int ChangeNum = int.Parse(TalkData[CSVNum]["Content"].ToString());

            if(VarNum  < GameManager.instance.Var.Length)
            { 
                GameManager.instance.Var[VarNum].Var = ChangeNum;
                Debug.Log(VarNum  + "번 변수(" + GameManager.instance.Var[VarNum].VarName + ")가 " +  ChangeNum+ "로 변경되었습니다. 현재 값 = " + GameManager.instance.Var[VarNum].Var);
                
            }
            else
            { 
                Debug.LogWarning("ChangeVar 이벤트 Talker에 잘못된 값이 들어왔습니다. 현재 CSVNum은 " + CSVNum);
            }
            PrepareNextTalk();
        }
        else
        { 
            yield return null;
            StartCoroutine(ChangeVar(CSVNum));            
        }  
    }

    IEnumerator ChangeVarByVar(int CSVNum)
    {
        if(GameManager.instance.CantTalkCheck() == false)
        {
            int VarNum = int.Parse(TalkData[CSVNum]["Talker"].ToString());
            int ChangeNum = int.Parse(TalkData[CSVNum]["Content"].ToString());

            if(VarNum  < GameManager.instance.Var.Length)
            {
                if(ChangeNum  < GameManager.instance.Var.Length)
                { 
                    GameManager.instance.Var[VarNum].Var = GameManager.instance.Var[ChangeNum].Var;
                    Debug.Log(VarNum  + "번 변수(" + GameManager.instance.Var[VarNum].VarName + ")가 " +  GameManager.instance.Var[ChangeNum].Var + "로 변경되었습니다. 현재 값 = " + GameManager.instance.Var[VarNum].Var);
                }
                else
                { 
                    Debug.LogWarning("ChangeVar 이벤트 Content에 잘못된 값이 들어왔습니다. 현재 CSVNum은 " + CSVNum);
                }
            }
            else
            { 
                Debug.LogWarning("ChangeVar 이벤트 Talker에 잘못된 값이 들어왔습니다. 현재 CSVNum은 " + CSVNum);
            }
            PrepareNextTalk();
        }
        else
        { 
            yield return null;
            StartCoroutine(ChangeVarByVar(CSVNum));            
        }  
    }

    IEnumerator ChangeVarByName(int CSVNum)
    {
        if(GameManager.instance.CantTalkCheck() == false)
        {
            int ChangeNum = int.Parse(TalkData[CSVNum]["Content"].ToString());
            int Check = 0;

            for(int i = 0; i < GameManager.instance.Var.Length; i++)
            { 
                if(GameManager.instance.Var[i].VarName == TalkData[CSVNum]["Talker"].ToString())
                { 
                    GameManager.instance.Var[i].Var = ChangeNum;
                    Debug.Log(i + "번 변수(" + GameManager.instance.Var[i].VarName + ")가 " +  ChangeNum + "으로 변경되었습니다. 현재 값 = " + GameManager.instance.Var[i].Var);
                    break;
                }
                Check++;
            }

            if(Check >= GameManager.instance.Var.Length)
            { 
                Debug.LogWarning("IncreaseVarByName 이벤트 Talker에 잘못된 값이 들어왔습니다. 현재 CSVNum은 " + CSVNum);
            }
            Check = 0;
            PrepareNextTalk();
        }
        else
        { 
            yield return null;
            StartCoroutine(ChangeVarByName(CSVNum));            
        }  
    }

    IEnumerator ChangeVarByVarByName(int CSVNum)
    {
        if(GameManager.instance.CantTalkCheck() == false)
        {
            int VarNum = 0;
            int ChangeNum = 0;
            int Check = 0;

            for(int i = 0; i < GameManager.instance.Var.Length; i++)
            { 
                if(GameManager.instance.Var[i].VarName == TalkData[CSVNum]["Talker"].ToString())
                { 
                    VarNum = i;                  
                    break;
                }
                Check++;
            }

            if(Check >= GameManager.instance.Var.Length)
            { 
                Debug.LogWarning("ChangeVarByVarByName 이벤트 Talker에 잘못된 값이 들어왔습니다. 현재 CSVNum은 " + CSVNum);
            }
            Check = 0;

            for(int i = 0; i < GameManager.instance.Var.Length; i++)
            { 
                if(GameManager.instance.Var[i].VarName == TalkData[CSVNum]["Content"].ToString())
                { 
                    ChangeNum = i;                  
                    break;
                }
                Check++;
            }

            if(Check >= GameManager.instance.Var.Length)
            { 
                Debug.LogWarning("ChangeVarByVarByName 이벤트 Content에 잘못된 값이 들어왔습니다. 현재 CSVNum은 " + CSVNum);
            }
            Check = 0;

            GameManager.instance.Var[VarNum].Var = GameManager.instance.Var[ChangeNum].Var;
            Debug.Log(VarNum + "번 변수(" + GameManager.instance.Var[VarNum].VarName + ")가 " +  GameManager.instance.Var[ChangeNum].Var + "으로 변경되었습니다. 현재 값 = " + GameManager.instance.Var[VarNum].Var);

            PrepareNextTalk();
        }
        else
        { 
            yield return null;
            StartCoroutine(ChangeVarByVarByName(CSVNum));            
        }  
    }

    IEnumerator IncreaseVarByRand(int CSVNum)
    {
        if(GameManager.instance.CantTalkCheck() == false)
        {
            int VarNum = int.Parse(TalkData[CSVNum]["Talker"].ToString());
            int FirstRandNum = int.Parse(TalkData[CSVNum]["Content"].ToString());
            int SecondRandNum = int.Parse(TalkData[CSVNum]["Key"].ToString());

            if(FirstRandNum > SecondRandNum)
            {
                GameManager.instance.Var[VarNum].Var += Random.Range(SecondRandNum,FirstRandNum);
                Debug.Log(VarNum + "번 변수(" + GameManager.instance.Var[VarNum].VarName + ")가 랜덤 값만큼 증가하였습니다. 현재 값 = " + GameManager.instance.Var[VarNum].Var);                
            }
            else if(SecondRandNum > FirstRandNum)
            { 
                GameManager.instance.Var[VarNum].Var += Random.Range(FirstRandNum, SecondRandNum);
                Debug.Log(VarNum + "번 변수(" + GameManager.instance.Var[VarNum].VarName + ")가 랜덤 값만큼 증가하였습니다. 현재 값 = " + GameManager.instance.Var[VarNum].Var);                
            }
            else
            { 
                Debug.LogWarning("IncreaseVarByRand 이벤트에 잘못된 값이 들어왔습니다. 현재 CSVNum은 " + CSVNum);
            }

            PrepareNextTalk();
        }
        else
        { 
            yield return null;
            StartCoroutine(IncreaseVarByRand(CSVNum));            
        }  
    }

    IEnumerator IncreaseVarByRandByName(int CSVNum)
    {
        if(GameManager.instance.CantTalkCheck() == false)
        {
            int VarNum = 0;
            int FirstRandNum = int.Parse(TalkData[CSVNum]["Content"].ToString());
            int SecondRandNum = int.Parse(TalkData[CSVNum]["Key"].ToString());
            int Check = 0;


            for(int i = 0; i < GameManager.instance.Var.Length; i++)
            { 
                if(GameManager.instance.Var[i].VarName == TalkData[CSVNum]["Talker"].ToString())
                { 
                    VarNum = i;                  
                    break;
                }
                Check++;
            }

            if(Check >= GameManager.instance.Var.Length)
            { 
                Debug.LogWarning("IncreaseVarByRandByName 이벤트 Talker에 잘못된 값이 들어왔습니다. 현재 CSVNum은 " + CSVNum);
            }
            Check = 0;

            if(FirstRandNum > SecondRandNum)
            {
                GameManager.instance.Var[VarNum].Var += Random.Range(SecondRandNum,FirstRandNum);
                Debug.Log(VarNum + "번 변수(" + GameManager.instance.Var[VarNum].VarName + ")가 랜덤 값만큼 증가하였습니다. 현재 값 = " + GameManager.instance.Var[VarNum].Var);                
            }
            else if(SecondRandNum > FirstRandNum)
            { 
                GameManager.instance.Var[VarNum].Var += Random.Range(FirstRandNum, SecondRandNum);
                Debug.Log(VarNum + "번 변수(" + GameManager.instance.Var[VarNum].VarName + ")가 랜덤 값만큼 증가하였습니다. 현재 값 = " + GameManager.instance.Var[VarNum].Var);                
            }
            else
            { 
                Debug.LogWarning("IncreaseVarByRandByName 이벤트에 잘못된 값이 들어왔습니다. 현재 CSVNum은 " + CSVNum);
            }

            PrepareNextTalk();
        }
        else
        { 
            yield return null;
            StartCoroutine(IncreaseVarByRandByName(CSVNum));            
        }  
    }

    IEnumerator ChangeVarByRand(int CSVNum)
    {
        if(GameManager.instance.CantTalkCheck() == false)
        {
            int VarNum = int.Parse(TalkData[CSVNum]["Talker"].ToString());
            int FirstRandNum = int.Parse(TalkData[CSVNum]["Content"].ToString());
            int SecondRandNum = int.Parse(TalkData[CSVNum]["Key"].ToString());

            if(FirstRandNum > SecondRandNum)
            {
                GameManager.instance.Var[VarNum].Var = Random.Range(SecondRandNum,FirstRandNum);
                Debug.Log(VarNum + "번 변수(" + GameManager.instance.Var[VarNum].VarName + ")가 랜덤 값으로 변경되었습니다. 현재 값 = " + GameManager.instance.Var[VarNum].Var);                
            }
            else if(SecondRandNum > FirstRandNum)
            { 
                GameManager.instance.Var[VarNum].Var = Random.Range(FirstRandNum, SecondRandNum);
                Debug.Log(VarNum + "번 변수(" + GameManager.instance.Var[VarNum].VarName + ")가 랜덤 값으로 변경되었습니다. 현재 값 = " + GameManager.instance.Var[VarNum].Var);                
            }
            else
            { 
                Debug.LogWarning("ChangeVarByRand 이벤트에 잘못된 값이 들어왔습니다. 현재 CSVNum은 " + CSVNum);
            }

            PrepareNextTalk();
        }
        else
        { 
            yield return null;
            StartCoroutine(ChangeVarByRand(CSVNum));            
        }  
    }

    IEnumerator ChangeVarByRandByName(int CSVNum)
    {
        if(GameManager.instance.CantTalkCheck() == false)
        {
            int VarNum = 0;
            int FirstRandNum = int.Parse(TalkData[CSVNum]["Content"].ToString());
            int SecondRandNum = int.Parse(TalkData[CSVNum]["Key"].ToString());
            int Check = 0;

            for(int i = 0; i < GameManager.instance.Var.Length; i++)
            { 
                if(GameManager.instance.Var[i].VarName == TalkData[CSVNum]["Talker"].ToString())
                { 
                    VarNum = i;                  
                    break;
                }
                Check++;
            }

            if(Check >= GameManager.instance.Var.Length)
            { 
                Debug.LogWarning("ChangeVarByRandByName 이벤트 Talker에 잘못된 값이 들어왔습니다. 현재 CSVNum은 " + CSVNum);
            }
            Check = 0;

            if(FirstRandNum > SecondRandNum)
            {
                GameManager.instance.Var[VarNum].Var = Random.Range(SecondRandNum,FirstRandNum);
                Debug.Log(VarNum + "번 변수(" + GameManager.instance.Var[VarNum].VarName + ")가 랜덤 값으로 변경되었습니다. 현재 값 = " + GameManager.instance.Var[VarNum].Var);                
            }
            else if(SecondRandNum > FirstRandNum)
            { 
                GameManager.instance.Var[VarNum].Var = Random.Range(FirstRandNum, SecondRandNum);
                Debug.Log(VarNum + "번 변수(" + GameManager.instance.Var[VarNum].VarName + ")가 랜덤 값으로 변경되었습니다. 현재 값 = " + GameManager.instance.Var[VarNum].Var);                
            }
            else
            { 
                Debug.LogWarning("ChangeVarByRandByName 이벤트에 잘못된 값이 들어왔습니다. 현재 CSVNum은 " + CSVNum);
            }

            PrepareNextTalk();
        }
        else
        { 
            yield return null;
            StartCoroutine(ChangeVarByRandByName(CSVNum));            
        }  
    }

    IEnumerator ChangeSwitch(int CSVNum)
    {
        if(GameManager.instance.CantTalkCheck() == false)
        {
            int ChangeNum = int.Parse(TalkData[CSVNum]["Talker"].ToString());
            if(ChangeNum < GameManager.instance.Switch.Length)
            {             
                if(TalkData[CSVNum]["Content"].ToString() == "true")
                { 
                    GameManager.instance.Switch[ChangeNum].Switch = true;
                    Debug.Log(ChangeNum + "번째 스위치(" + GameManager.instance.Switch[ChangeNum].SwitchName + ")가 true로 변경되었습니다.");
                }
                else if(TalkData[CSVNum]["Content"].ToString() == "false")
                { 
                    GameManager.instance.Switch[ChangeNum].Switch = false;
                    Debug.Log(ChangeNum + "번째 스위치(" + GameManager.instance.Switch[ChangeNum].SwitchName + ")가 false로 변경되었습니다.");
                }
                else
                { 
                    Debug.LogWarning("ChangeSwitch 이벤트 Content에 잘못된 값이 들어왔습니다. 현재 CSVNum은 " + CSVNum);
                }
            }
            else
            { 
                Debug.LogWarning("ChangeSwitch 이벤트 Talker에 잘못된 값이 들어왔습니다. 현재 CSVNum은 " + CSVNum);
            }
            PrepareNextTalk();            
        }
        else
        { 
            yield return null;
            StartCoroutine(ChangeSwitch(CSVNum));            
        }  
    }

    IEnumerator ChangeSwitchByName(int CSVNum)
    {
        if(GameManager.instance.CantTalkCheck() == false)
        {
            int Check = 0;

            for(int i = 0; i < GameManager.instance.Switch.Length; i++)
            { 
                if(GameManager.instance.Switch[i].SwitchName == TalkData[CSVNum]["Talker"].ToString())
                {             
                    if(TalkData[CSVNum]["Content"].ToString() == "true")
                    { 
                        GameManager.instance.Switch[i].Switch = true;
                        Debug.Log(i + "번째 스위치(" + GameManager.instance.Switch[i].SwitchName + ")가 true로 변경되었습니다.");
                    }
                    else if(TalkData[CSVNum]["Content"].ToString() == "false")
                    { 
                        GameManager.instance.Switch[i].Switch = false;
                        Debug.Log(i + "번째 스위치(" + GameManager.instance.Switch[i].SwitchName + ")가 false로 변경되었습니다.");
                    }
                    else
                    { 
                        Debug.LogWarning("ChangeSwitchByName 이벤트 Content에 잘못된 값이 들어왔습니다. 현재 CSVNum은 " + CSVNum);
                    }
                    break;
                }
                Check ++;
            }


            if(Check >= GameManager.instance.Switch.Length)
            { 
                Debug.LogWarning("ChangeSwitchByName 이벤트 Talker에 잘못된 값이 들어왔습니다. 현재 CSVNum은 " + CSVNum);
            }
            Check = 0;

            PrepareNextTalk();            
        }
        else
        { 
            yield return null;
            StartCoroutine(ChangeSwitchByName(CSVNum));            
        }  
    }

    IEnumerator ChangeSwitchByVar(int CSVNum)
    {
        if(GameManager.instance.CantTalkCheck() == false)
        {
            int ChangeNum = int.Parse(TalkData[CSVNum]["Talker"].ToString());
            int IncreaseNum = int.Parse(TalkData[CSVNum]["Content"].ToString());
            if(ChangeNum < GameManager.instance.Switch.Length && IncreaseNum < GameManager.instance.Switch.Length)
            {             
                GameManager.instance.Switch[ChangeNum].Switch = GameManager.instance.Switch[IncreaseNum].Switch;
                Debug.Log(ChangeNum + "번째 스위치(" + GameManager.instance.Switch[ChangeNum].SwitchName + ")가 "+ GameManager.instance.Switch[IncreaseNum].SwitchName + "로 변경되었습니다.");
            }
            else
            { 
                Debug.LogWarning("ChangeSwitch 이벤트 Talker나 Content에 잘못된 값이 들어왔습니다. 현재 CSVNum은 " + CSVNum);
            }
            PrepareNextTalk();            
        }
        else
        { 
            yield return null;
            StartCoroutine(ChangeSwitchByVar(CSVNum));            
        }  
    }

    IEnumerator ChangeSwitchByVarByName(int CSVNum)
    {
        if(GameManager.instance.CantTalkCheck() == false)
        {
            int ChangeNum = 0;
            int IncreaseNum = 0;
            int Check = 0;

            for(int i = 0; i < GameManager.instance.Switch.Length; i++)
            { 
                if(GameManager.instance.Switch[i].SwitchName == TalkData[CSVNum]["Talker"].ToString())
                {
                    ChangeNum = i;
                    break;
                }
                Check ++;
            }
            if(Check >= GameManager.instance.Switch.Length)
            { 
                Debug.LogWarning("ChangeSwitchByVarByName 이벤트 Talker에 잘못된 값이 들어왔습니다. 현재 CSVNum은 " + CSVNum);
            }
            Check = 0;

            for(int i = 0; i < GameManager.instance.Switch.Length; i++)
            { 
                if(GameManager.instance.Switch[i].SwitchName == TalkData[CSVNum]["Content"].ToString())
                {
                    IncreaseNum = i;
                    break;
                }
                Check ++;
            }
            if(Check >= GameManager.instance.Switch.Length)
            { 
                Debug.LogWarning("ChangeSwitchByVarByName 이벤트 Content에 잘못된 값이 들어왔습니다. 현재 CSVNum은 " + CSVNum);
            }
            Check = 0;

            GameManager.instance.Switch[ChangeNum].Switch = GameManager.instance.Switch[IncreaseNum].Switch;
            Debug.Log(ChangeNum + "번째 스위치(" + GameManager.instance.Switch[ChangeNum].SwitchName + ")가 "+ GameManager.instance.Switch[IncreaseNum].SwitchName + "로 변경되었습니다.");

            PrepareNextTalk();            
        }
        else
        { 
            yield return null;
            StartCoroutine(ChangeSwitchByVarByName(CSVNum));            
        }  
    }

    IEnumerator ChangeBackground(int CSVNum)
    { 
        if(GameManager.instance.CantTalkCheck() == false)
        {
            BackgroundCanvas.transform.GetChild(0).GetComponent<Image>().sprite = BackgroundDatabaseManager.instance.GetBackGround(TalkData[CSVNum]["Content"].ToString());
            PrepareNextTalk();            
        }
        else
        { 
            yield return null;
            StartCoroutine(ChangeBackground(CSVNum));            
        }       
    }

    IEnumerator ChangeBGM(int CSVNum)
    { 
        if(GameManager.instance.CantTalkCheck() == false)
        {
            GameManager.instance.ChangeBGM(BGMDatabaseManager.instance.GetBGM(TalkData[CSVNum]["Content"].ToString()));
            PrepareNextTalk();            
        }
        else
        { 
            yield return null;
            StartCoroutine(ChangeBGM(CSVNum));            
        }       
    }

    IEnumerator StartBGM()
    { 
        if(GameManager.instance.CantTalkCheck() == false)
        {
            GameManager.instance.StartBGM();
            PrepareNextTalk();            
        }
        else
        { 
            yield return null;
            StartCoroutine(StartBGM());            
        }       
    }

    IEnumerator StopBGM()
    { 
        if(GameManager.instance.CantTalkCheck() == false)
        {
            GameManager.instance.StopBGM();
            PrepareNextTalk();            
        }
        else
        { 
            yield return null;
            StartCoroutine(StopBGM());            
        }       
    }

    IEnumerator PlaySE(int CSVNum)
    { 
        if(GameManager.instance.CantTalkCheck() == false)
        {
            GameManager.instance.PlaySE(SEDatabaseManager.instance.GetSE(TalkData[CSVNum]["Content"].ToString()));
            PrepareNextTalk();            
        }
        else
        { 
            yield return null;
            StartCoroutine(PlaySE(CSVNum));            
        }       
    }

    IEnumerator SetTimer(int CSVNum)
    { 
        if(GameManager.instance.CantTalkCheck() == false)
        {
            float Time = float.Parse(TalkData[CSVNum]["Content"].ToString());
            Timer.gameObject.SetActive(true);
            TimerController.instance.SetTimer(Time);
            TalkBackground.gameObject.SetActive(false);
            PrepareNextTalk();            
        }
        else
        { 
            yield return null;
            StartCoroutine(SetTimer(CSVNum));            
        }       
    }

    IEnumerator FadeOut(int CSVNum)
    { 
        if(GameManager.instance.CantTalkCheck() == false)
        {
            if(TalkData[CSVNum]["Talker"].ToString() != "")
            { 
                EffectManager.instance.ChangeEffectImageColor(TalkData[CSVNum]["Talker"].ToString());    
            }
            else
            { 
                EffectManager.instance.ChangeEffectImageColor("Black");
            }
            EffectManager.instance.FadeOut(float.Parse(TalkData[CSVNum]["Content"].ToString()));
            PrepareNextTalk();            
        }
        else
        { 
            yield return null;
            StartCoroutine(FadeOut(CSVNum));            
        }       
    }

    IEnumerator FadeIn(int CSVNum)
    { 
        if(GameManager.instance.CantTalkCheck() == false)
        {
            EffectManager.instance.FadeIn(float.Parse(TalkData[CSVNum]["Content"].ToString()));
            PrepareNextTalk();            
        }
        else
        { 
            yield return null;
            StartCoroutine(FadeIn(CSVNum));            
        }       
    }

    IEnumerator Flash(int CSVNum)
    { 
        if(GameManager.instance.CantTalkCheck() == false)
        {
            if(TalkData[CSVNum]["Talker"].ToString() != "")
            { 
                EffectManager.instance.ChangeFlashImageColor(TalkData[CSVNum]["Talker"].ToString());    
            }
            else
            { 
                EffectManager.instance.ChangeFlashImageColor("Yellow");
            }
            EffectManager.instance.Flash(float.Parse(TalkData[CSVNum]["Content"].ToString()));
            PrepareNextTalk();            
        }
        else
        { 
            yield return null;
            StartCoroutine(Flash(CSVNum));            
        }       
    }

    IEnumerator ChangeBackgroundByMove(int CSVNum)
    { 
        if(GameManager.instance.CantTalkCheck() == false)
        {
            TalkBackground.gameObject.SetActive(false);
            EffectManager.instance.BackgroundMoveLeft(BackgroundCanvas.transform.GetChild(0).GetComponent<Image>().sprite);
            BackgroundCanvas.transform.GetChild(0).GetComponent<Image>().sprite = BackgroundDatabaseManager.instance.GetBackGround(TalkData[CSVNum]["Content"].ToString());
            PrepareNextTalk();            
        }
        else
        { 
            yield return null;
            StartCoroutine(ChangeBackgroundByMove(CSVNum));            
        }       
    }

    IEnumerator EndingCredit(int CSVNum)
    { 
        if(GameManager.instance.CantTalkCheck() == false)
        {
            EffectManager.instance.EndingCredit();
            PrepareNextTalk();            
        }
        else
        { 
            yield return null;
            StartCoroutine(EndingCredit(CSVNum));            
        }       
    }

    IEnumerator RemoveEndingCredit(int CSVNum)
    { 
        if(GameManager.instance.CantTalkCheck() == false)
        {
            EffectManager.instance.RemoveEndingCredit();
            PrepareNextTalk();            
        }
        else
        { 
            yield return null;
            StartCoroutine(RemoveEndingCredit(CSVNum));            
        }       
    }

    IEnumerator ReturnTitle(int CSVNum)
    { 
        if(GameManager.instance.CantTalkCheck() == false)
        {
            GameManager.instance.ReturnTitle();
            PrepareNextTalk();            
        }
        else
        { 
            yield return null;
            StartCoroutine(ReturnTitle(CSVNum));            
        }       
    }

    IEnumerator SaveCSV()
    { 
        if(GameManager.instance.CantTalkCheck() == false)
        {
            SaveCSVFile();
            PrepareNextTalk();            
        }
        else
        { 
            yield return null;
            StartCoroutine(SaveCSV());            
        }       
    }

    IEnumerator LoadCSV(int CSVNum)
    { 
        if(GameManager.instance.CantTalkCheck() == false)
        {
            int PlusNum = 2;
            if(TalkData[CSVNum]["Content"].ToString() == "")
            { 
                LoadCSVFile(PlusNum);  
            }
            else
            {                
                PlusNum = int.Parse(TalkData[CSVNum]["Content"].ToString());
                LoadCSVFile(PlusNum); 
            }
        }
        else
        { 
            yield return null;
            StartCoroutine(LoadCSV(CSVNum));            
        }       
    }

    public void OnTalkBackground()
    { 
        TalkBackground.gameObject.SetActive(true);    
    }

    public void OffTalkBackground()
    { 
        TalkBackground.gameObject.SetActive(false);    
    }

    public void OnTalkerImageBase()
    { 
        TalkerImageBase.gameObject.SetActive(true);    
    }

    public void OffTalkerImageBase()
    { 
        TalkerImageBase.gameObject.SetActive(false);    
    }

    private void ChangeCSVFile(string Name, int Num, bool IsReset)
    {
        StopAllCoroutines();
        if(IsReset == true)
        { 
            ResetGame();   
        }
        IsReset = false;
        GameManager.instance.ChangeCSVName(Name);
        TalkData = CSVReader.Read(GameManager.instance.GetCSVName());            
        GameManager.instance.ChangeCSVNum(Num);
        ReLoadGame();        
    }

    private void SaveCSVFile()
    { 
        GameManager.instance.ChangeSaveCSVNum(GameManager.instance.GetCSVNum());
        GameManager.instance.ChangeSaveCSVName(GameManager.instance.GetCSVName());
        GameManager.instance.ChangeSaveBackgroundName(GameManager.instance.Background);
        GameManager.instance.ChangeSaveBGMName(GameManager.instance.BGM);
        GameManager.instance.ChangeSaveChoiceKey(GameManager.instance.GetChoiceKey());
        GameManager.instance.ChangeSaveTalkerNum(GameManager.instance.GetTalkerNum());

        for(int i = 0; i < GameManager.instance.Talker.Length; i++)
        { 
            if(GameManager.instance.Talker[i] != null)
            { 
                GameManager.instance.ChangeSaveTalker(GameManager.instance.Talker[i].Name, i);
            }
            else
            { 
                GameManager.instance.ChangeSaveTalker("",i);
            }
        }
        GameManager.instance.ChangeSaveIsBGMOn(GameManager.instance.GetIsBGMOn());
        GameManager.instance.ChangeSaveKeyCheck(GameManager.instance.GetKeyCheck());
        GameManager.instance.SaveIsFadeOut = GameManager.instance.IsFadeOut;
    }

    private void LoadCSVFile(int PlusNum)
    { 
        Debug.Log("CSV파일을 바꿉니다.");
        GameManager.instance.ChangeCSVNum(GameManager.instance.GetSaveCSVNum());
        GameManager.instance.ChangeCSVName(GameManager.instance.GetSaveCSVName());
        GameManager.instance.Background = GameManager.instance.GetSaveBackgroundName();
        GameManager.instance.BGM = GameManager.instance.GetSaveBGMName();
        GameManager.instance.ChangeChoiceKey(GameManager.instance.GetSaveChoiceKey());
        GameManager.instance.ChangeIsBGMOn(GameManager.instance.GetSaveIsBGMOn());
        GameManager.instance.ChangeTalkerNum(GameManager.instance.GetSaveTalkerNum());
        GameManager.instance.ChangeKeyCheck(GameManager.instance.GetSaveKeyCheck());

        for(int k = 0; k < GameManager.instance.Talker.Length; k++)
        { 
            GameManager.instance.Talker[k] = null;
            TalkerImage[k].GetComponent<Image>().sprite = GameManager.instance.GetBlank();
        }

        for(int j = 0; j < GameManager.instance.Talker.Length; j++)
        {
            for(int i = 0; i < CharacterDatabaseManager.instance.DBLength(); i++)
            { 
                if(CharacterDatabaseManager.instance.CharacterDB(i).Name == GameManager.instance.GetSaveTalker(j))
                {
                    GameManager.instance.Talker[j] = CharacterDatabaseManager.instance.CharacterDB(i);
                    break;
                }
            }
        }
        GameManager.instance.IsFadeOut = GameManager.instance.SaveIsFadeOut;
        GameManager.instance.PlusCSVNum(PlusNum);
        ReLoadGame();
    }

    private void IfCheck(int CSVNum) //switch 부분은 문제없음
    { 
        string IfTrue = ""; // 조건을 충족할때 키값
        string IfFalse = ""; // 조건을 충족하지 않을 때 키값
        int GameManagerNum = 0; // 비교할 스위치나 변수의 값
        int FirstCompare = 0; // 첫번째 비교값
        int SecondCompare = 0; // 두번째 비교값

        GameManager.instance.ChangeKeyCheck(true);

        if(TalkData[CSVNum]["Talker"].ToString() == "Switch")
        { 
            if(int.Parse(TalkData[CSVNum]["Content"].ToString()) < GameManager.instance.Switch.Length)
            { 
                GameManagerNum = int.Parse(TalkData[CSVNum]["Content"].ToString());

                GameManager.instance.PlusCSVNum(1);
                CSVNum++;

                IfTrue = TalkData[CSVNum]["Key"].ToString();

                if(TalkData[CSVNum]["Content"].ToString() == "true")
                {
                    for(int i = CSVNum; i < TalkData.Count; i++)
                    { 
                        GameManager.instance.PlusCSVNum(1);
                        CSVNum++;
                        if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                        { 
                            IfFalse = TalkData[CSVNum]["Key"].ToString();
                            break;
                        }
                    } 

                    if(GameManager.instance.Switch[GameManagerNum].Switch == true)
                    { 
                        
                        GameManager.instance.ChangeChoiceKey(IfTrue);
                    }
                    else
                    { 
                        
                        GameManager.instance.ChangeChoiceKey(IfFalse);                        
                    }

                }
                else if(TalkData[CSVNum]["Content"].ToString() == "false")
                { 
                    for(int i = CSVNum; i < TalkData.Count; i++)
                    { 
                        GameManager.instance.PlusCSVNum(1);
                        CSVNum++;
                        if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                        { 
                            IfFalse = TalkData[CSVNum]["Key"].ToString();
                            break;
                        }
                    }

                    if(GameManager.instance.Switch[GameManagerNum].Switch != true)
                    { 
                        
                        GameManager.instance.ChangeChoiceKey(IfTrue);
                    }
                    else
                    { 
                        
                        GameManager.instance.ChangeChoiceKey(IfFalse);                        
                    }
                    
                }
                else
                { 
                    Debug.LogWarning("if 이벤트 두번째 줄 Content부분에 잘못된 값이 들어왔습니다. CSVNum 값 " + CSVNum);
                }

            }
            else
            { 
                Debug.LogWarning("if 이벤트 Content부분에 잘못된 값이 들어왔습니다 CSVNum 값 " + CSVNum);
            }
        }
        else if(TalkData[CSVNum]["Talker"].ToString() == "Var") // 변수는 여기서 부터 이게 최종보스 ㅋㅋ;;;;
        { 
            if(int.Parse(TalkData[CSVNum]["Content"].ToString()) < GameManager.instance.Var.Length)
            { 
                GameManagerNum = int.Parse(TalkData[CSVNum]["Content"].ToString());

                GameManager.instance.PlusCSVNum(1);
                CSVNum++;

                IfTrue = TalkData[CSVNum]["Key"].ToString();
                FirstCompare = int.Parse(TalkData[CSVNum]["Content"].ToString());

                if(TalkData[CSVNum]["Event"].ToString() == "Range") //범위 지정 체크
                { 
                    GameManager.instance.PlusCSVNum(1);
                    CSVNum++;        
                    SecondCompare = int.Parse(TalkData[CSVNum]["Content"].ToString());

                    for(int i = CSVNum; i < TalkData.Count; i++)
                    { 
                        GameManager.instance.PlusCSVNum(1);
                        CSVNum++;
                        if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                        { 
                            IfFalse = TalkData[CSVNum]["Key"].ToString();
                            break;
                        }
                    }       
                    
                    if(FirstCompare <= SecondCompare)
                    { 
                        if(FirstCompare <= GameManager.instance.Var[GameManagerNum].Var && GameManager.instance.Var[GameManagerNum].Var <= SecondCompare)
                        { 
                            GameManager.instance.ChangeChoiceKey(IfTrue);
                        }
                        else
                        { 
                            GameManager.instance.ChangeChoiceKey(IfFalse);
                        }
                    }
                    else
                    { 
                        if(FirstCompare >= GameManager.instance.Var[GameManagerNum].Var && GameManager.instance.Var[GameManagerNum].Var >= SecondCompare)
                        { 
                            GameManager.instance.ChangeChoiceKey(IfTrue);
                        }
                        else
                        { 
                            GameManager.instance.ChangeChoiceKey(IfFalse);
                        }                                
                    }
                    
                }
                else if(TalkData[CSVNum]["Talker"].ToString() == "<") // < 의 경우
                {
                    for(int i = CSVNum; i < TalkData.Count; i++)
                    { 
                        GameManager.instance.PlusCSVNum(1);
                        CSVNum++;
                        if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                        { 
                            IfFalse = TalkData[CSVNum]["Key"].ToString();
                            break;
                        }
                    } 
                    
                    if(FirstCompare > GameManager.instance.Var[GameManagerNum].Var) // 변경완료
                    { 
                        GameManager.instance.ChangeChoiceKey(IfTrue);
                    }
                    else
                    { 
                        GameManager.instance.ChangeChoiceKey(IfFalse);
                    }
                }
                else if(TalkData[CSVNum]["Talker"].ToString() == "<=") 
                { 
                    for(int i = CSVNum; i < TalkData.Count; i++)
                    { 
                        GameManager.instance.PlusCSVNum(1);
                        CSVNum++;
                        if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                        { 
                            IfFalse = TalkData[CSVNum]["Key"].ToString();
                            break;
                        }
                    } 
                    
                    if(FirstCompare >= GameManager.instance.Var[GameManagerNum].Var) // 변경완료
                    { 
                        GameManager.instance.ChangeChoiceKey(IfTrue);
                    }
                    else
                    { 
                        GameManager.instance.ChangeChoiceKey(IfFalse);
                    }                
                }
                else if(TalkData[CSVNum]["Talker"].ToString() == "=" || TalkData[CSVNum]["Talker"].ToString() == "==")
                { 
                     for(int i = CSVNum; i < TalkData.Count; i++)
                    { 
                        GameManager.instance.PlusCSVNum(1);
                        CSVNum++;
                        if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                        { 
                            IfFalse = TalkData[CSVNum]["Key"].ToString();
                            break;
                        }
                    } 
                    
                    if(FirstCompare == GameManager.instance.Var[GameManagerNum].Var)
                    { 
                        GameManager.instance.ChangeChoiceKey(IfTrue);
                    }
                    else
                    { 
                        GameManager.instance.ChangeChoiceKey(IfFalse);
                    }                 
                }
                else if(TalkData[CSVNum]["Talker"].ToString() == ">")
                { 
                     for(int i = CSVNum; i < TalkData.Count; i++)
                    { 
                        GameManager.instance.PlusCSVNum(1);
                        CSVNum++;
                        if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                        { 
                            IfFalse = TalkData[CSVNum]["Key"].ToString();
                            break;
                        }
                    } 
                    
                    if(FirstCompare < GameManager.instance.Var[GameManagerNum].Var) // 변경완료
                    { 
                        GameManager.instance.ChangeChoiceKey(IfTrue);
                    }
                    else
                    { 
                        GameManager.instance.ChangeChoiceKey(IfFalse);
                    }                 
                }
                else if(TalkData[CSVNum]["Talker"].ToString() == ">=")
                { 
                     for(int i = CSVNum; i < TalkData.Count; i++)
                    { 
                        GameManager.instance.PlusCSVNum(1);
                        CSVNum++;
                        if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                        { 
                            IfFalse = TalkData[CSVNum]["Key"].ToString();
                            break;
                        }
                    } 
                    
                    if(FirstCompare <= GameManager.instance.Var[GameManagerNum].Var) // 변경완료
                    { 
                        GameManager.instance.ChangeChoiceKey(IfTrue);
                    }
                    else
                    { 
                        GameManager.instance.ChangeChoiceKey(IfFalse);
                    }                 
                }
                else
                { 
                    Debug.LogWarning("if 이벤트 두번째 줄 Talker부분에 잘못된 값이 들어왔습니다. CSVNum 값 " + CSVNum);
                }

            }
            else
            { 
                Debug.LogWarning("if 이벤트 Content부분에 잘못된 값이 들어왔습니다. CSVNum 값 " + CSVNum);
            }            
        }
        else
        { 
            Debug.LogWarning("if 이벤트 talker부분에 잘못된 값이 들어왔습니다. CSVNum 값 " + CSVNum);    
        }
    }   

   
    private void IfCheckByName(int CSVNum) 
    { 
        string IfTrue = ""; // 조건을 충족할때 키값
        string IfFalse = ""; // 조건을 충족하지 않을 때 키값
        int FirstCompare = 0; // 첫번째 비교값
        int SecondCompare = 0; // 두번째 비교값

        GameManager.instance.ChangeKeyCheck(true);

        if(TalkData[CSVNum]["Talker"].ToString() == "Switch")
        { 
            int SwitchNameChecker = 0;
            for(int z = 0; z < GameManager.instance.Switch.Length; z++)
            {
                SwitchNameChecker++;
                if(TalkData[CSVNum]["Content"].ToString() ==  GameManager.instance.Switch[z].SwitchName)
                {  
                    GameManager.instance.PlusCSVNum(1);
                    CSVNum++;

                    IfTrue = TalkData[CSVNum]["Key"].ToString();

                    if(TalkData[CSVNum]["Content"].ToString() == "true")
                    {
                        for(int i = CSVNum; i < TalkData.Count; i++)
                        { 
                            GameManager.instance.PlusCSVNum(1);
                            CSVNum++;
                            if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                            { 
                                IfFalse = TalkData[CSVNum]["Key"].ToString();
                                break;
                            }
                        } 

                        if(GameManager.instance.Switch[z].Switch == true)
                        { 
                        
                            GameManager.instance.ChangeChoiceKey(IfTrue);
                        }
                        else
                        {                         
                            GameManager.instance.ChangeChoiceKey(IfFalse);                        
                        }
                        break;

                    }
                    else if(TalkData[CSVNum]["Content"].ToString() == "false")
                    { 
                        for(int i = CSVNum; i < TalkData.Count; i++)
                        { 
                            GameManager.instance.PlusCSVNum(1);
                            CSVNum++;
                            if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                            { 
                                IfFalse = TalkData[CSVNum]["Key"].ToString();
                                break;
                            }
                        }

                        if(GameManager.instance.Switch[z].Switch != true)
                        { 
                        
                            GameManager.instance.ChangeChoiceKey(IfTrue);
                        }
                        else
                        { 
                        
                            GameManager.instance.ChangeChoiceKey(IfFalse);                        
                        }
                        break;
                    }
                    else
                    { 
                        Debug.LogWarning("IfForName 이벤트 두번째 줄 Content부분에 잘못된 값이 들어왔습니다. CSVNum 값 " + CSVNum);
                        break;
                    }
                }
            }

            if(SwitchNameChecker > GameManager.instance.Switch.Length)
            { 
                Debug.LogWarning("IfForName 이벤트 첫번째 줄 Content부분에 잘못된 값이 들어왔습니다. CSVNum 값 " + CSVNum);
            }
            SwitchNameChecker = 0;
        }
        else if(TalkData[CSVNum]["Talker"].ToString() == "Var") 
        {
            int VarNameChecker = 0;
            for(int z = 0; z < GameManager.instance.Var.Length; z++)
            {
                VarNameChecker++;
                if(TalkData[CSVNum]["Content"].ToString() == GameManager.instance.Var[z].VarName)
                { 
                    GameManager.instance.PlusCSVNum(1);
                    CSVNum++;

                    IfTrue = TalkData[CSVNum]["Key"].ToString();
                    FirstCompare = int.Parse(TalkData[CSVNum]["Content"].ToString());

                    if(TalkData[CSVNum]["Event"].ToString() == "Range") //범위 지정 체크
                    { 
                        GameManager.instance.PlusCSVNum(1);
                        CSVNum++;        
                        SecondCompare = int.Parse(TalkData[CSVNum]["Content"].ToString());

                        for(int i = CSVNum; i < TalkData.Count; i++)
                        { 
                            GameManager.instance.PlusCSVNum(1);
                            CSVNum++;
                            if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                            { 
                                IfFalse = TalkData[CSVNum]["Key"].ToString();
                                break;
                            }
                        }       
                    
                        if(FirstCompare <= SecondCompare)
                        { 
                            if(FirstCompare <= GameManager.instance.Var[z].Var && GameManager.instance.Var[z].Var <= SecondCompare)
                            { 
                                GameManager.instance.ChangeChoiceKey(IfTrue);
                            }
                            else
                            { 
                                GameManager.instance.ChangeChoiceKey(IfFalse);
                            }
                        }
                        else
                        { 
                            if(FirstCompare >= GameManager.instance.Var[z].Var && GameManager.instance.Var[z].Var >= SecondCompare)
                            { 
                                GameManager.instance.ChangeChoiceKey(IfTrue);
                            }
                            else
                            { 
                                GameManager.instance.ChangeChoiceKey(IfFalse);
                            }                                
                        }
                    }
                    else if(TalkData[CSVNum]["Talker"].ToString() == "<") // < 의 경우
                    {
                        for(int i = CSVNum; i < TalkData.Count; i++)
                        { 
                            GameManager.instance.PlusCSVNum(1);
                            CSVNum++;
                            if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                            { 
                                IfFalse = TalkData[CSVNum]["Key"].ToString();
                                break;
                            }
                        } 
                    
                        if(FirstCompare > GameManager.instance.Var[z].Var) // 변경완료
                        { 
                            GameManager.instance.ChangeChoiceKey(IfTrue);
                        }
                        else
                        { 
                            GameManager.instance.ChangeChoiceKey(IfFalse);
                        }
                    }
                    else if(TalkData[CSVNum]["Talker"].ToString() == "<=") 
                    { 
                        for(int i = CSVNum; i < TalkData.Count; i++)
                        { 
                            GameManager.instance.PlusCSVNum(1);
                            CSVNum++;
                            if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                            { 
                                IfFalse = TalkData[CSVNum]["Key"].ToString();
                                break;
                            }
                        } 
                    
                        if(FirstCompare >= GameManager.instance.Var[z].Var) // 변경완료
                        { 
                            GameManager.instance.ChangeChoiceKey(IfTrue);
                        }
                        else
                        { 
                            GameManager.instance.ChangeChoiceKey(IfFalse);
                        }                
                    }
                    else if(TalkData[CSVNum]["Talker"].ToString() == "=" || TalkData[CSVNum]["Talker"].ToString() == "==")
                    { 
                         for(int i = CSVNum; i < TalkData.Count; i++)
                        { 
                            GameManager.instance.PlusCSVNum(1);
                            CSVNum++;
                            if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                            { 
                                IfFalse = TalkData[CSVNum]["Key"].ToString();
                                break;
                            }
                        } 
                    
                        if(FirstCompare == GameManager.instance.Var[z].Var)
                        { 
                            GameManager.instance.ChangeChoiceKey(IfTrue);
                        }
                        else
                        { 
                            GameManager.instance.ChangeChoiceKey(IfFalse);
                        }                 
                    }
                    else if(TalkData[CSVNum]["Talker"].ToString() == ">")
                    { 
                         for(int i = CSVNum; i < TalkData.Count; i++)
                        { 
                            GameManager.instance.PlusCSVNum(1);
                            CSVNum++;
                            if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                            { 
                                IfFalse = TalkData[CSVNum]["Key"].ToString();
                                break;
                            }
                        } 
                    
                        if(FirstCompare < GameManager.instance.Var[z].Var) // 변경완료
                        { 
                            GameManager.instance.ChangeChoiceKey(IfTrue);
                        }
                        else
                        { 
                            GameManager.instance.ChangeChoiceKey(IfFalse);
                        }                 
                    }
                    else if(TalkData[CSVNum]["Talker"].ToString() == ">=")
                    { 
                         for(int i = CSVNum; i < TalkData.Count; i++)
                        { 
                            GameManager.instance.PlusCSVNum(1);
                            CSVNum++;
                            if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                            { 
                                IfFalse = TalkData[CSVNum]["Key"].ToString();
                                break;
                            }
                        } 
                    
                        if(FirstCompare <= GameManager.instance.Var[z].Var) // 변경완료
                        { 
                            GameManager.instance.ChangeChoiceKey(IfTrue);
                        }
                        else
                        { 
                            GameManager.instance.ChangeChoiceKey(IfFalse);
                        }  
                    }
                    else
                    { 
                        Debug.LogWarning("if 이벤트 두번째 줄 Talker부분에 잘못된 값이 들어왔습니다. CSVNum 값 " + CSVNum);
                    }
                    break;
                }     
            }    
            if(VarNameChecker > GameManager.instance.Var.Length)
            { 
                Debug.LogWarning("if 이벤트 Content부분에 잘못된 값이 들어왔습니다. CSVNum 값 " + CSVNum);
            }
            VarNameChecker = 0;
        }
        else
        { 
            Debug.LogWarning("if 이벤트 talker부분에 잘못된 값이 들어왔습니다. CSVNum 값 " + CSVNum);    
        }
    }    

    private void IfCheckByVar(int CSVNum) 
    { 
        string IfTrue = ""; // 조건을 충족할때 키값
        string IfFalse = ""; // 조건을 충족하지 않을 때 키값
        int FirstGameManagerNum = 0; //  비교할 스위치나 변수의 값(첫번째 줄에 적힌거)
        int SecondGameManagerNum = 0; // 비교할 스위치나 변수의 값(두번째 줄에 적힌거)
        int ThirdGameManagerNum = 0; // 비교할 스위치나 변수의 값(세번째 줄에 적힌거)

        GameManager.instance.ChangeKeyCheck(true);

        if(TalkData[CSVNum]["Talker"].ToString() == "Switch")
        { 
            if(int.Parse(TalkData[CSVNum]["Content"].ToString()) < GameManager.instance.Switch.Length)
            { 
                FirstGameManagerNum = int.Parse(TalkData[CSVNum]["Content"].ToString());

                GameManager.instance.PlusCSVNum(1);
                CSVNum++;
                
                if(int.Parse(TalkData[CSVNum]["Content"].ToString()) < GameManager.instance.Switch.Length)
                { 
                    IfTrue = TalkData[CSVNum]["Key"].ToString();
                    SecondGameManagerNum = int.Parse(TalkData[CSVNum]["Content"].ToString());
                
                    for(int i = CSVNum; i < TalkData.Count; i++)
                    { 
                        GameManager.instance.PlusCSVNum(1);
                        CSVNum++;
                        if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                        { 
                            IfFalse = TalkData[CSVNum]["Key"].ToString();
                            break;
                        }
                    } 

                    if(GameManager.instance.Switch[FirstGameManagerNum].Switch == GameManager.instance.Switch[SecondGameManagerNum].Switch)
                    {                         
                        GameManager.instance.ChangeChoiceKey(IfTrue);
                    }
                    else if(GameManager.instance.Switch[FirstGameManagerNum].Switch != GameManager.instance.Switch[SecondGameManagerNum].Switch)
                    {            
                        GameManager.instance.ChangeChoiceKey(IfFalse);                        
                    }
                }
                else
                { 
                    Debug.LogWarning("ifByVar 이벤트 두번째 Content부분에 잘못된 값이 들어왔습니다 CSVNum 값 " + CSVNum);
                }
            }
            else
            { 
                Debug.LogWarning("ifByVar 이벤트 첫번째 Content부분에 잘못된 값이 들어왔습니다 CSVNum 값 " + CSVNum);
            }
        }
        else if(TalkData[CSVNum]["Talker"].ToString() == "Var")
        { 
            if(int.Parse(TalkData[CSVNum]["Content"].ToString()) < GameManager.instance.Var.Length)
            { 
                FirstGameManagerNum = int.Parse(TalkData[CSVNum]["Content"].ToString());

                GameManager.instance.PlusCSVNum(1);
                CSVNum++;
                
                if(int.Parse(TalkData[CSVNum]["Content"].ToString()) < GameManager.instance.Var.Length)
                {  
                    IfTrue = TalkData[CSVNum]["Key"].ToString();
                    SecondGameManagerNum = int.Parse(TalkData[CSVNum]["Content"].ToString());

                    if(TalkData[CSVNum]["Event"].ToString() == "Range") //범위 지정 체크
                    { 
                        GameManager.instance.PlusCSVNum(1);
                        CSVNum++;        

                        if(int.Parse(TalkData[CSVNum]["Content"].ToString()) < GameManager.instance.Var.Length)
                        { 
                            ThirdGameManagerNum = int.Parse(TalkData[CSVNum]["Content"].ToString());

                            for(int i = CSVNum; i < TalkData.Count; i++)
                            { 
                                GameManager.instance.PlusCSVNum(1);
                                CSVNum++;
                                if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                                { 
                                    IfFalse = TalkData[CSVNum]["Key"].ToString();
                                    break;
                                }
                            }       
                    
                            if(GameManager.instance.Var[SecondGameManagerNum].Var <= GameManager.instance.Var[ThirdGameManagerNum].Var)
                            { 
                                if(GameManager.instance.Var[SecondGameManagerNum].Var <= GameManager.instance.Var[FirstGameManagerNum].Var && GameManager.instance.Var[FirstGameManagerNum].Var <= GameManager.instance.Var[ThirdGameManagerNum].Var)
                                { 
                                    GameManager.instance.ChangeChoiceKey(IfTrue);
                                }
                                else
                                { 
                                    GameManager.instance.ChangeChoiceKey(IfFalse);
                                }
                            }
                            else
                            { 
                                if(GameManager.instance.Var[SecondGameManagerNum].Var >= GameManager.instance.Var[FirstGameManagerNum].Var && GameManager.instance.Var[FirstGameManagerNum].Var >= GameManager.instance.Var[ThirdGameManagerNum].Var)
                                { 
                                    GameManager.instance.ChangeChoiceKey(IfTrue);
                                }
                                else
                                { 
                                    GameManager.instance.ChangeChoiceKey(IfFalse);
                                }                                
                            }
                        }
                    }
                    else if(TalkData[CSVNum]["Talker"].ToString() == "<") // < 의 경우
                    {
                        for(int i = CSVNum; i < TalkData.Count; i++)
                        { 
                            GameManager.instance.PlusCSVNum(1);
                            CSVNum++;
                            if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                            { 
                                IfFalse = TalkData[CSVNum]["Key"].ToString();
                                break;
                            }
                        } 
                    
                        if(GameManager.instance.Var[SecondGameManagerNum].Var > GameManager.instance.Var[FirstGameManagerNum].Var) // 변경완료
                        { 
                            GameManager.instance.ChangeChoiceKey(IfTrue);
                        }
                        else
                        { 
                            GameManager.instance.ChangeChoiceKey(IfFalse);
                        }
                    }
                    else if(TalkData[CSVNum]["Talker"].ToString() == "<=") 
                    { 
                        for(int i = CSVNum; i < TalkData.Count; i++)
                        { 
                            GameManager.instance.PlusCSVNum(1);
                            CSVNum++;
                            if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                            { 
                                IfFalse = TalkData[CSVNum]["Key"].ToString();
                                break;
                            }
                        } 
                    
                        if(GameManager.instance.Var[SecondGameManagerNum].Var >= GameManager.instance.Var[FirstGameManagerNum].Var) // 변경완료
                        { 
                            GameManager.instance.ChangeChoiceKey(IfTrue);
                        }
                        else
                        { 
                            GameManager.instance.ChangeChoiceKey(IfFalse);
                        }                
                    }
                    else if(TalkData[CSVNum]["Talker"].ToString() == "=" || TalkData[CSVNum]["Talker"].ToString() == "==")
                    { 
                         for(int i = CSVNum; i < TalkData.Count; i++)
                        { 
                            GameManager.instance.PlusCSVNum(1);
                            CSVNum++;
                            if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                            { 
                                IfFalse = TalkData[CSVNum]["Key"].ToString();
                                break;
                            }
                        } 
                    
                        if(GameManager.instance.Var[SecondGameManagerNum].Var == GameManager.instance.Var[FirstGameManagerNum].Var)
                        { 
                            GameManager.instance.ChangeChoiceKey(IfTrue);
                        }
                        else
                        { 
                            GameManager.instance.ChangeChoiceKey(IfFalse);
                        }                 
                    }
                    else if(TalkData[CSVNum]["Talker"].ToString() == ">")
                    { 
                         for(int i = CSVNum; i < TalkData.Count; i++)
                        { 
                            GameManager.instance.PlusCSVNum(1);
                            CSVNum++;
                            if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                            { 
                                IfFalse = TalkData[CSVNum]["Key"].ToString();
                                break;
                            }
                        } 
                    
                        if(GameManager.instance.Var[SecondGameManagerNum].Var < GameManager.instance.Var[FirstGameManagerNum].Var) // 변경완료
                        { 
                            GameManager.instance.ChangeChoiceKey(IfTrue);
                        }
                        else
                        { 
                            GameManager.instance.ChangeChoiceKey(IfFalse);
                        }                 
                    }
                    else if(TalkData[CSVNum]["Talker"].ToString() == ">=")
                    { 
                         for(int i = CSVNum; i < TalkData.Count; i++)
                        { 
                            GameManager.instance.PlusCSVNum(1);
                            CSVNum++;
                            if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                            { 
                                IfFalse = TalkData[CSVNum]["Key"].ToString();
                                break;
                            }
                        } 
                    
                        if(GameManager.instance.Var[SecondGameManagerNum].Var <= GameManager.instance.Var[FirstGameManagerNum].Var) // 변경완료
                        { 
                            GameManager.instance.ChangeChoiceKey(IfTrue);
                        }
                        else
                        { 
                            GameManager.instance.ChangeChoiceKey(IfFalse);
                        }                 
                    }
                    else
                    { 
                        Debug.LogWarning("if 이벤트 두번째 줄 Talker부분에 잘못된 값이 들어왔습니다. CSVNum 값 " + CSVNum);
                    }
                }
            }
            else
            { 
                Debug.LogWarning("if 이벤트 Content부분에 잘못된 값이 들어왔습니다. CSVNum 값 " + CSVNum);
            }            
        }
        else
        { 
            Debug.LogWarning("if 이벤트 talker부분에 잘못된 값이 들어왔습니다. CSVNum 값 " + CSVNum);    
        }
    }   

    private void IfCheckByVarByName(int CSVNum) 
    { 
        string IfTrue = ""; // 조건을 충족할때 키값
        string IfFalse = ""; // 조건을 충족하지 않을 때 키값
        int FirstGameManagerNum = 0; 
        int SecondGameManagerNum = 0; 
        int ThirdGameManagerNum = 0;

        GameManager.instance.ChangeKeyCheck(true);

        if(TalkData[CSVNum]["Talker"].ToString() == "Switch")
        { 
            int SwitchNameChecker = 0;
            for(int z = 0; z < GameManager.instance.Switch.Length; z++)
            {
                SwitchNameChecker++;                
                if(TalkData[CSVNum]["Content"].ToString() ==  GameManager.instance.Switch[z].SwitchName)
                { 
                    FirstGameManagerNum = z;
                    break;
                }
            }

            if(SwitchNameChecker > GameManager.instance.Switch.Length)
            { 
                Debug.LogWarning("IfByVarByName 이벤트 첫번째 줄 Content부분에 잘못된 값이 들어왔습니다. CSVNum 값 " + CSVNum);
            }
            SwitchNameChecker = 0;

            GameManager.instance.PlusCSVNum(1);
            CSVNum++;          
            IfTrue = TalkData[CSVNum]["Key"].ToString();
            
            for(int z = 0; z < GameManager.instance.Switch.Length; z++)
            {
                SwitchNameChecker++;                
                if(TalkData[CSVNum]["Content"].ToString() ==  GameManager.instance.Switch[z].SwitchName)
                { 
                    SecondGameManagerNum = z;
                    break;
                }
            }

            if(SwitchNameChecker > GameManager.instance.Switch.Length)
            { 
                Debug.LogWarning("IfByVarByName 이벤트 두번째 줄 Content부분에 잘못된 값이 들어왔습니다. CSVNum 값 " + CSVNum);
            }
            SwitchNameChecker = 0;

            for(int i = CSVNum; i < TalkData.Count; i++)
            { 
                GameManager.instance.PlusCSVNum(1);
                CSVNum++;
                if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                { 
                    IfFalse = TalkData[CSVNum]["Key"].ToString();
                    break;
                }
            } 

            if(GameManager.instance.Switch[FirstGameManagerNum].Switch == GameManager.instance.Switch[SecondGameManagerNum].Switch)
            {                         
                GameManager.instance.ChangeChoiceKey(IfTrue);
            }
            else
            {                         
                GameManager.instance.ChangeChoiceKey(IfFalse);                        
            }
                        
        }
        else if(TalkData[CSVNum]["Talker"].ToString() == "Var") 
        {
            int VarNameChecker = 0;
            for(int z = 0; z < GameManager.instance.Var.Length; z++)
            {
                VarNameChecker++;
                if(TalkData[CSVNum]["Content"].ToString() == GameManager.instance.Var[z].VarName)
                {
                    FirstGameManagerNum = z;
                    break;
                }   
            }

            if(VarNameChecker > GameManager.instance.Var.Length)
            { 
                Debug.LogWarning("IfByVarByName 이벤트 첫번째 줄 Content부분에 잘못된 값이 들어왔습니다. CSVNum 값 " + CSVNum);
            }
            VarNameChecker = 0;

            GameManager.instance.PlusCSVNum(1);
            CSVNum++;
            IfTrue = TalkData[CSVNum]["Key"].ToString();

            for(int z = 0; z < GameManager.instance.Var.Length; z++)
            {
                VarNameChecker++;
                if(TalkData[CSVNum]["Content"].ToString() == GameManager.instance.Var[z].VarName)
                {
                    SecondGameManagerNum = z;
                    break;
                }   
            }

            if(VarNameChecker > GameManager.instance.Var.Length)
            { 
                Debug.LogWarning("IfByVarByName 이벤트 첫번째 줄 Content부분에 잘못된 값이 들어왔습니다. CSVNum 값 " + CSVNum);
            }
            VarNameChecker = 0;

            if(TalkData[CSVNum]["Event"].ToString() == "Range") //범위 지정 체크
            { 
                GameManager.instance.PlusCSVNum(1);
                CSVNum++;        

                for(int z = 0; z < GameManager.instance.Var.Length; z++)
                {
                    VarNameChecker++;
                    if(TalkData[CSVNum]["Content"].ToString() == GameManager.instance.Var[z].VarName)
                    {
                        ThirdGameManagerNum = z;
                        break;
                    }   
                }

                if(VarNameChecker > GameManager.instance.Var.Length)
                { 
                    Debug.LogWarning("IfByVarByName 이벤트 첫번째 줄 Content부분에 잘못된 값이 들어왔습니다. CSVNum 값 " + CSVNum);
                }
                VarNameChecker = 0;

                for(int i = CSVNum; i < TalkData.Count; i++)
                { 
                    GameManager.instance.PlusCSVNum(1);
                    CSVNum++;
                    if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                    { 
                        IfFalse = TalkData[CSVNum]["Key"].ToString();
                        break;
                    }
                }       
                    
                if(GameManager.instance.Var[SecondGameManagerNum].Var <= GameManager.instance.Var[ThirdGameManagerNum].Var)
                { 
                    if(GameManager.instance.Var[SecondGameManagerNum].Var <= GameManager.instance.Var[FirstGameManagerNum].Var && GameManager.instance.Var[FirstGameManagerNum].Var <= GameManager.instance.Var[ThirdGameManagerNum].Var)
                    { 
                        GameManager.instance.ChangeChoiceKey(IfTrue);
                    }
                    else
                    { 
                        GameManager.instance.ChangeChoiceKey(IfFalse);
                    }
                }
                else
                { 
                    if(GameManager.instance.Var[SecondGameManagerNum].Var >= GameManager.instance.Var[FirstGameManagerNum].Var && GameManager.instance.Var[FirstGameManagerNum].Var >= GameManager.instance.Var[ThirdGameManagerNum].Var)
                    { 
                        GameManager.instance.ChangeChoiceKey(IfTrue);
                    }
                    else
                    { 
                        GameManager.instance.ChangeChoiceKey(IfFalse);
                    }                                
                }
            }

            else if(TalkData[CSVNum]["Talker"].ToString() == "<") // < 의 경우
            {
                for(int i = CSVNum; i < TalkData.Count; i++)
                { 
                    GameManager.instance.PlusCSVNum(1);
                    CSVNum++;
                    if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                    { 
                        IfFalse = TalkData[CSVNum]["Key"].ToString();
                        break;
                    }
                } 
                    
                if(GameManager.instance.Var[SecondGameManagerNum].Var > GameManager.instance.Var[FirstGameManagerNum].Var) // 변경완료
                { 
                    GameManager.instance.ChangeChoiceKey(IfTrue);
                }
                else
                { 
                    GameManager.instance.ChangeChoiceKey(IfFalse);
                }
            }
            else if(TalkData[CSVNum]["Talker"].ToString() == "<=") 
            { 
                for(int i = CSVNum; i < TalkData.Count; i++)
                { 
                    GameManager.instance.PlusCSVNum(1);
                    CSVNum++;
                    if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                    { 
                        IfFalse = TalkData[CSVNum]["Key"].ToString();
                        break;
                    }
                } 
                    
                if(GameManager.instance.Var[SecondGameManagerNum].Var  >= GameManager.instance.Var[FirstGameManagerNum].Var) // 변경완료
                { 
                    GameManager.instance.ChangeChoiceKey(IfTrue);
                }
                else
                { 
                    GameManager.instance.ChangeChoiceKey(IfFalse);
                }                
            }
            else if(TalkData[CSVNum]["Talker"].ToString() == "=" || TalkData[CSVNum]["Talker"].ToString() == "==")
            { 
                    for(int i = CSVNum; i < TalkData.Count; i++)
                { 
                    GameManager.instance.PlusCSVNum(1);
                    CSVNum++;
                    if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                    { 
                        IfFalse = TalkData[CSVNum]["Key"].ToString();
                        break;
                    }
                } 
                    
                if(GameManager.instance.Var[SecondGameManagerNum].Var  == GameManager.instance.Var[FirstGameManagerNum].Var)
                { 
                    GameManager.instance.ChangeChoiceKey(IfTrue);
                }
                else
                { 
                    GameManager.instance.ChangeChoiceKey(IfFalse);
                }                 
            }
            else if(TalkData[CSVNum]["Talker"].ToString() == ">")
            { 
                    for(int i = CSVNum; i < TalkData.Count; i++)
                { 
                    GameManager.instance.PlusCSVNum(1);
                    CSVNum++;
                    if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                    { 
                        IfFalse = TalkData[CSVNum]["Key"].ToString();
                        break;
                    }
                } 
                    
                if(GameManager.instance.Var[SecondGameManagerNum].Var  < GameManager.instance.Var[FirstGameManagerNum].Var) // 변경완료
                { 
                    GameManager.instance.ChangeChoiceKey(IfTrue);
                }
                else
                { 
                    GameManager.instance.ChangeChoiceKey(IfFalse);
                }                 
            }
            else if(TalkData[CSVNum]["Talker"].ToString() == ">=")
            { 
                    for(int i = CSVNum; i < TalkData.Count; i++)
                { 
                    GameManager.instance.PlusCSVNum(1);
                    CSVNum++;
                    if(TalkData[CSVNum]["Event"].ToString() == "IfEnd")
                    { 
                        IfFalse = TalkData[CSVNum]["Key"].ToString();
                        break;
                    }
                } 
                    
                if(GameManager.instance.Var[SecondGameManagerNum].Var  <= GameManager.instance.Var[FirstGameManagerNum].Var) // 변경완료
                { 
                    GameManager.instance.ChangeChoiceKey(IfTrue);
                }
                else
                { 
                    GameManager.instance.ChangeChoiceKey(IfFalse);
                }  
            }
            else
            { 
                Debug.LogWarning("if 이벤트 두번째 줄 Talker부분에 잘못된 값이 들어왔습니다. CSVNum 값 " + CSVNum);
            }
        }
        else
        { 
            Debug.LogWarning("if 이벤트 talker부분에 잘못된 값이 들어왔습니다. CSVNum 값 " + CSVNum);    
        }
    } 
    
    
    public void SetMenuButton(bool check)
    { 
        if(check == true)
        { 
            OpenMenuButton.SetActive(true);
        }
        else if(check == false)
        { 
            OpenMenuButton.SetActive(false);
        }
        else
        { 
            Debug.LogWarning("TalkManager SetMenuButton 함수에 잘못된 값이 들어왔습니다!");    
        }
    }

    public void SetLogButton(bool check)
    { 
        if(check == true)
        { 
            OpenLogButton.SetActive(true);
        }
        else if(check == false)
        { 
            OpenLogButton.SetActive(false);
        }
        else
        { 
            Debug.LogWarning("TalkManager SetLogButton 함수에 잘못된 값이 들어왔습니다!");    
        }
    }

    private void OpenLog()
    {
        if(GameManager.instance.CantLogCheck() == false)
        { 
            GameManager.instance.PlaySE(SEDatabaseManager.instance.GetSE("Click"));
            GameManager.instance.isLog = true;
            Choices.SetActive(false);
            SetMenuButton(false);
            TalkBackground.SetActive(false);       
            LogBackground.SetActive(true);
            CreateLogTexts();

            LogBackground.transform.GetChild(0).GetChild(1).GetComponent<Scrollbar>().value = 0;
        }
    }

    private void CloseLog()
    { 
        GameManager.instance.PlaySE(SEDatabaseManager.instance.GetSE("Click"));
        GameManager.instance.isLog = false;
        DeleteLog();
        Choices.SetActive(true);
        SetMenuButton(true);
        TalkBackground.SetActive(true);
        LogBackground.SetActive(false);

        LogBackground.transform.GetChild(0).GetChild(1).GetComponent<Scrollbar>().value = 1;
    }

    public void LogButton()
    { 
        if(GameManager.instance.isLog == true)
        { 
            CloseLog();
        }
        else if(GameManager.instance.isLog == false)
        { 
            OpenLog();
        }
    }

    private void CreateLogTexts()
    { 
        if(LogTextPrefab != null)
        { 
            CreateLog();
        }           
    }

    private void CreateLog() // 로그 처리
    { 
        int LogCheck = GameManager.instance.LogNum;
        int LogCount = 0;

        for (int i = 0; i < GameManager.instance.GameLog.TalkLog.Length; i++)
        {
            if(GameManager.instance.GameLog.TalkLog[LogCheck] != "")
            {
                GameObject newLog = Instantiate(LogTextPrefab);
                newLog.transform.SetParent(LogScrollviewContent.transform);
                newLog.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                newLog.transform.GetChild(0).GetComponent<Text>().text = "";
                newLog.transform.GetChild(1).GetComponent<Text>().text = "";


                if(GameManager.instance.GameLog.TalkerNameLog[LogCheck] != "")
                {                     
                    newLog.transform.GetChild(0).GetComponent<Text>().text = GameManager.instance.GameLog.TalkerNameLog[LogCheck] + " : ";
                }

                char[] LogText =  GameManager.instance.GameLog.TalkLog[LogCheck].ToCharArray();
                { 
                    for(int j = 0; j < LogText.Length; j++)
                    {
                        if(LogText[j] != LineBreakForTalk)
                        { 
                            newLog.transform.GetChild(1).GetComponent<Text>().text += LogText[j];
                        }
                    }
                }
                 
                newLog.name = "Log_" + LogCount;     
                LogCount++;
            }
            
            
            LogCheck--;
            if(LogCheck < 0)
            { 
                LogCheck = GameManager.instance.GameLog.TalkLog.Length -1;
            }                     
        }    
    }

    private void DeleteLog()
    { 
        for(int i = 0; i < LogScrollviewContent.transform.childCount; i++)
        { 
            Destroy(LogScrollviewContent.transform.GetChild(i).gameObject);
        }
    }    

    private void ResetGame()
    { 
        Debug.Log("게임을 리셋합니다.");
        for(int i = 0; i < GameManager.instance.Talker.Length; i++) //화자 이미지 초기화
        {
            GameManager.instance.Talker[i] = null;
            TalkerImage[i].GetComponent<Image>().sprite = GameManager.instance.GetBlank();   
        } 
        TalkText.GetComponent<Text>().text = ""; //대사창 초기화

        BackgroundCanvas.transform.GetChild(0).GetComponent<Image>().sprite = BackgroundDatabaseManager.instance.BackGroundDB(0); // 배경초기화
        GameManager.instance.ChangeIsBGMOn(true); // 브금재생여부 초기화
        GameManager.instance.ChangeBGM(BGMDatabaseManager.instance.BGMDB(0)); // 브금초기화
        ChangeTalker(-1); // 화자 초기화
        ResetGameKey();
    }

    private void ReLoadGame() // 선택지 고려해서 리로드 할꺼면 밑에 로드게임 사용
    {
        Debug.Log("게임을 리로드 합니다.");
        StopAllCoroutines();
        for(int i = 0; i < TalkerImage.Length; i++)
        { 
            TalkerImage[i].GetComponent<Image>().sprite = GameManager.instance.GetBlank();   
        }


        TalkCanvas.SetActive(true);
        BackgroundCanvas.SetActive(true);

        BackgroundCanvas.transform.GetChild(0).GetComponent<Image>().sprite = BackgroundDatabaseManager.instance.GetBackGround(GameManager.instance.Background);

        GameManager.instance.ChangeBGM(BGMDatabaseManager.instance.GetBGM(GameManager.instance.BGM));
        if(GameManager.instance.GetIsBGMOn() == false)
        { 
            GameManager.instance.StopBGM();
        }
        else
        { 
            GameManager.instance.StartBGM();
        }

        TalkText.GetComponent<Text>().text = "";
        TalkData = CSVReader.Read(GameManager.instance.GetCSVName());
        for(int i = 0; i < TalkerImage.Length ;i++)
        {
            if(GameManager.instance.Talker[i] != null)
            { 
                TalkerPlus(i, GameManager.instance.Talker[i].Name);
            }         
        }
        ChangeTalker(GameManager.instance.GetTalkerNum());  

        if(GameManager.instance.IsFadeOut == true)
        { 
            EffectManager.instance.ReturnToFade();            
        }        
        else
        { 
            EffectManager.instance.RemoveFade();
        }

        NextTalk();
    }

    public void StartGame()
    {       
        TalkCanvas.SetActive(true);
        BackgroundCanvas.SetActive(true);
        TalkText.GetComponent<Text>().text = "";
        TalkData = CSVReader.Read(GameManager.instance.GetCSVName()); 
        Debug.Log("대본의 길이는 " + TalkData.Count);
        GameManager.instance.CantUseMenu = false;
        GameManager.instance.isMain = false; 
        NextTalk();
    }

    public void LoadGame()
    {
        StopAllCoroutines();
        for(int i = 0; i < TalkerImage.Length; i++) //화자 초기화
        { 
            TalkerImage[i].GetComponent<Image>().sprite = GameManager.instance.GetBlank();   
        }


        TalkCanvas.SetActive(true);
        BackgroundCanvas.SetActive(true);

        BackgroundCanvas.transform.GetChild(0).GetComponent<Image>().sprite = BackgroundDatabaseManager.instance.GetBackGround(GameManager.instance.Background);

        GameManager.instance.ChangeBGM(BGMDatabaseManager.instance.GetBGM(GameManager.instance.BGM)); // 브금처리
        if(GameManager.instance.GetIsBGMOn() == false)
        { 
            GameManager.instance.StopBGM();
        }
        else
        { 
            GameManager.instance.StartBGM();
        }

        TalkText.GetComponent<Text>().text = "";
        TalkerNameText.GetComponent<Text>().text = "";
        TalkData = CSVReader.Read(GameManager.instance.GetCSVName());
        for(int i = 0; i < TalkerImage.Length ;i++) // 화자 처리
        {
            if(GameManager.instance.Talker[i] != null)
            { 
                TalkerPlus(i, GameManager.instance.Talker[i].Name);
            }         
        }
        ChangeTalker(GameManager.instance.GetTalkerNum());  // 화자변경

        int ChoiceCount = 0;

        if(GameManager.instance.IsFadeOut == true)
        { 
            EffectManager.instance.ReturnToFade();            
        }
        else
        { 
            EffectManager.instance.RemoveFade();
        }

        // 나중에 key값 비교도 추가할 것(↓테스트로 급조한 코드)
        if(TalkData[GameManager.instance.GetCSVNum()]["Event"].ToString() == "ChoiceEnd")
        {
            Debug.Log("선택지 체크");
            // 만에 하나 csv파일 객기 부린답시고 맨 앞 열 가지고 장난질하면 버그남 ㅋㅋ
            for(int i = 1; i <= ChoiceButton.Length; i++)
            { 
                if(TalkData[GameManager.instance.GetCSVNum() - i]["Event"].ToString() == "Choice")
                { 
                    ChoiceCount ++;
                }
                else
                { 
                    break;    
                }
            }  
            
            GameManager.instance.ChangeCSVNum(GameManager.instance.GetCSVNum() - ChoiceCount);
            // 대사부분인데요 이거 걍 로그를 이용하는 걸로 변경하기위해 주석 처리
            /*
            for(int j = 1; j <= GameManager.instance.GetCSVNum(); j++)
            { 
                if(TalkData[GameManager.instance.GetCSVNum() - j]["Event"].ToString() == "")
                { 
                    TalkText.GetComponent<Text>().text = TalkData[GameManager.instance.GetCSVNum() - j]["Content"].ToString();
                    break;
                }
            }
            */
            //대사 처리 로그이용 버전
            TalkText.GetComponent<Text>().text = "";
            char[] ChoiceText = GameManager.instance.GameLog.TalkLog[GameManager.instance.LogNum].ToCharArray();
            for(int k = 0; k < ChoiceText.Length; k++)
            { 
                if(ChoiceText[k] != LineBreakForTalk)
                { 
                    TalkText.GetComponent<Text>().text += ChoiceText[k];
                }
                else
                { 
                    TalkText.GetComponent<Text>().text += "\n";
                }
            }

            TalkerNameText.GetComponent<Text>().text = GameManager.instance.GameLog.TalkerNameLog[GameManager.instance.LogNum]; 

            NextTalk();

        }
        else
        {
            //GameManager.instance.ChangeCSVNum(GameManager.instance.GetCSVNum() - 1); //코루틴으로 미리읽어오는 구조상 -1해야함 // Talk쪽 구조 바꾼거 테스트 <- 문제없음
            GameManager.instance.LogNum -= 1; 
            NextTalk();
        }
        GameManager.instance.CantUseMenu = false;
        GameManager.instance.isMain = false; 
    }
    
    public void StopAllCoroutinesForLoad()
    {
        StopAllCoroutines();
    }
}
