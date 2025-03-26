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
            Debug.LogWarning("���� ��ũ �Ŵ����� 2���̻� �����մϴ�.");
            Destroy(gameObject);
        }
    }

    [SerializeField]
    private GameObject[] TalkerImage; // ���� �߾� ������ ������ 0 1 2
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
        Debug.Log("�뺻�� ���̴� " + TalkData.Count);
        NextTalk();
    }
    */


    private bool CheckNextTalk; // �������̴°� ���������� �ɾ��
    /*
     ���� ���� ���� ��ŵ������ �ڷ�ƾ�� �������� �׷��� ��¥�� ��� �ѱ�� �� ������ ������ 
    �����°� ���� ��ƾ��� �귯���ϱ� ��ȭ �̺�Ʈ�� �����Ǹ� false�� �ٲ�� �ϸ� ��

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
                //GameManager.instance.isTalk= false; //�ӽ�
                if(GameManager.instance.CantTalkCheck() == false && CheckNextTalk == false)
                { 
                    CheckNextTalk = true;
                    PrepareNextTalk();
                }    
            }          
        }        
    }

    /* �̺�Ʈ ���
        Choice <- �������� �ǹ� �Ѵ�. �������� ���� ���̸� ���� �࿡�� ����� �Ѵ�. �������� Key�� ���� ���� �־���ϸ� ResetKey �̺�Ʈ�� ����Ǳ� ������ Branch�� ������ ������ ���� ���� �ุ ����ȴ�.
        ChoiceEnd <- ������ ������ �������� �ǹ��Ѵ�.
        ResetKey <- �������� ���� ������ Ű ���� �ʱ�ȭ��Ų��.
        ChangeKey <- Key���� �ٲ۴�. CSV���� Key�� �ٲ� Key���� �Է��ϸ� �ȴ�. �̰��Է��ϸ� Ű�� �ٽ� ���ϰ� �ȴ�.
        If <- ������ �б��Ų��.(�ڼ��Ѱ� ��ȹ�� ����)

        ChangeCSV(�̱���) csv������ �ٲ۴�

        AddTalker <- ȭ��(Talker)�� �߰��Ѵ�. Talker�� �߰��� �ι��� �̸��� ���� Content���� ȭ�ڰ� �� ��ȣ�� �Է��ϸ� �ȴ�. �� �� �� ������ 0 1 2
        RemoveTalker <- ȭ��(Talker)�� �����Ѵ�. Content�� ���ŵ� ��ȣ�� �Է��ϸ� �ȴ�. �� �� �� ������ 0 1 2
        TalkerBack <- ��� ȭ�ڸ� ������ �ʴ� ���·� �����Ѵ�.(�̹����� ����)
        
        Bookmark <- Content�� ������ �־� �ּ��̳� ��ġ Ȯ�ο����� ����. 
        FindBookmark <- Bookmark�� ã�´� Talker�� ã�� ������ ������ Content�� ã�� ������ �ִ´�.(��ġ�ؾ���)
        IncreaseVar <- �������� ���� ��Ų��. Talker�� �ٲ� ���� ��ȣ Content�� ���Ұ��� �Է��Ѵ�.
        ChangeSwitch <- ����ġ�� �����Ѵ�. Talker�� �ٲ� ����ġ ��ȣ Content�� true���� false���� �Է��Ѵ�.

    */

    /*          
        �̰� ���� ���������� �����غ��ϱ�
        �ڷ�ƾ���� �ҷ����� �ص� ���� ���������� ���� �Ǵ� �κ��� ��� �̺�Ʈ�� �ϳ� �ƴѰ�...? 
        ���ӽ��� -> "TalkData"������ csv������ ������ ����Ʈ�� �� 
        �̺�Ʈ�� �о�ͼ� �̺�Ʈ�� �´� ������ �����Ų �� �ڷ�ƾ���� ���� �࿡ �ִ� �̺�Ʈ�� �����Ŵ
        �̶� ��糪 ���� �κе� �ð��� �ʿ��� �κп����� bool���� ���ؼ� �ش� �ϴ� ������ �Ϸ� �ɶ����� ���� �࿡ ����
        �ڷ�ƾ�� �ݺ� �߻���Ű�ٰ� ������ �Ϸ�Ȱ� Ȯ��(bool)���� �ٲ�� ȣ���� ���߰� ���� ������ �ݺ��ϰ� ��
        �̶� Choice�̺�Ʈ ���� �׷��� ���� ��ư������ ���� �̺�Ʈ�� ���� ��
        ��¥�� �Ϲ� �̺�Ʈ�ʵ��� �ٷ� �ٷ� �Ѿ������ ū ���� ��������(bool������ �ȸ���) 
        ����κп��� �������� ���� ������ Ȯ���Ѵ��� ���ʿ��� ������ �ҷ����� �Ǳ��ϴµ� �׷��� ���ʿ��� ������ Ȯ�εǸ� �ҷ��� �� �ְ� public ���� �ϵ簡
        ���� ��Ĵ�� bool������ �ϵ簡�̰�
        ��ȭ���� �� Ű �Է� üũ�Ҷ� �ҷ����� �ǰ�... �������� �� ���ֵ� �ݺ�ȣ�� �� ���� ���帣�� ������״� ����� ����? 
        �ڷ�ƾ�� �� �޸� �Դ°� �ƴϸ� void�� ��ĥ �ʿ�� ������
    */

    public void PrepareNextTalk()
    { 
        GameManager.instance.PlusCSVNum(1);
        NextTalk();        
    }


    private void Talking(int i) // Talk�� ���� ��ȭ �ڵ尡 ���°�
    { 
        TalkText.GetComponent<Text>().text = "";
        GameManager.instance.isTalk = true;

        TalkerNameText.GetComponent<Text>().text = "";
        OnTalkBackground();

        if(TalkData[i]["Talker"].ToString() == "0")
        { 
            if(GameManager.instance.Talker[0] != null)
            {
                Debug.Log("�����ι��� üũ �Ǿ����ϴ�.");
                ChangeTalker(0);
                TalkerNameText.GetComponent<Text>().text = GameManager.instance.Talker[0].KoreanName;   
                TypingSpeed = GameManager.instance.Talker[0].TalkSpeed;
            }
            else
            { 
                TalkerNameText.GetComponent<Text>().text = TalkData[i]["Talker"].ToString();
                Debug.Log("������ �ι��� �����Ƿ� �̸��� ����մϴ�.");                
            }
        }
        else if(TalkData[i]["Talker"].ToString() == "1")
        { 
            if(GameManager.instance.Talker[1] != null)
            {
                Debug.Log("�����ι��� üũ �Ǿ����ϴ�.");
                ChangeTalker(1);
                TalkerNameText.GetComponent<Text>().text = GameManager.instance.Talker[1].KoreanName;  
                TypingSpeed = GameManager.instance.Talker[1].TalkSpeed;
            }
            else
            { 
                TalkerNameText.GetComponent<Text>().text = TalkData[i]["Talker"].ToString();
                Debug.Log("������ �ι��� �����Ƿ� �̸��� ����մϴ�.");                
            }
        }
        else if(TalkData[i]["Talker"].ToString() == "2")
        { 
            if(GameManager.instance.Talker[2] != null)
            { 
                Debug.Log("�����ι��� üũ �Ǿ����ϴ�.");
                ChangeTalker(2);
                TalkerNameText.GetComponent<Text>().text = GameManager.instance.Talker[2].KoreanName; 
                TypingSpeed = GameManager.instance.Talker[2].TalkSpeed;
            }
            else
            { 
                TalkerNameText.GetComponent<Text>().text = TalkData[i]["Talker"].ToString();
                Debug.Log("������ �ι��� �����Ƿ� �̸��� ����մϴ�.");                
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
                        Debug.Log("�����ι��� üũ �Ǿ����ϴ�.");
                        //���߿� ���ϴ»�� �ٲ�°� �߰�
                        // ������ TalkData[i]["Talker"].ToString()�� ""�ΰ� �ƴѰ� �����ؼ� �����ϸ� ��ĭ�϶� ȭ�� �ȹٲ�� ���Ѽ� �ִµ�
                        // �׷� ������ ���� �־������
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
                Debug.Log("������ �ι��� �����Ƿ� �̸��� ����մϴ�.");
            }
            TalkerCheck = 0;
        }      

        TalkLog(TalkData[i]["Content"].ToString()); // �α�

        //���ϴ� �ӵ� �����Ҳ��� ���⼭ 0.1f �ٲ���.
        StartCoroutine(Typing(TalkData[i]["Content"].ToString(), TypingSpeed));
           
    }
    
    // �⺻
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
                    GameManager.instance.isTalk= false;//�ӽ�
                    Debug.Log("���ϱ� ��ŵ");
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
                    Debug.Log("���ϱ� ��ŵ");
                    break;                   
                }
            }
            */
            GameManager.instance.TextReading = false;
            GameManager.instance.isTalk= false;//�ӽ�
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

    // �⺻
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
                    Debug.Log("KeyCheck������ �ɷ��ֽ��ϴ�.");

                    if(TalkData[GameManager.instance.GetCSVNum()]["Branch"].ToString() == GameManager.instance.GetChoiceKey())
                    {
                        Debug.Log("Branch = ChoiceKey������ ������ ����մϴ�.");
                        NextTalkCheck();
                    }
                    else
                    {
                        Debug.Log("Branch != ChoiceKey������ ������ ��ŵ�մϴ�.");
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
            Debug.LogWarning("Event�� �߸��� ���� ���Խ��ϴ� ���� CSVNum�� ���� " + i);
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
            // �̰� �̷��� ¥�� ���߿� �ҷ��ö� �� �����͵鵵 �о�� �ҵ�
            // �Ϲ� ����ΰ� Event ""�� üũ�ؾ� �ҵ�
            if(ChoiceCount < 3)
            {
                Debug.Log(ChoiceCount + " �� ������ ����");                       
                
                ChoiceButton[ChoiceCount].transform.GetChild(0).GetComponent<Text>().text = TalkData[i]["Content"].ToString();
                if(ChoiceCount == 0)
                { 
                    GameManager.instance.ChangeFirstChoiceKey(TalkData[i]["Key"].ToString());
                    Debug.Log("0�� �������� Ű ���� " + GameManager.instance.GetFirstChoiceKey());
                }
                if(ChoiceCount == 1)
                { 
                    GameManager.instance.ChangeSecondChoiceKey(TalkData[i]["Key"].ToString());
                    Debug.Log("1�� �������� Ű ���� " + GameManager.instance.GetSecondChoiceKey());
                }
                if(ChoiceCount == 2)
                { 
                    GameManager.instance.ChangeThirdChoiceKey(TalkData[i]["Key"].ToString());
                    Debug.Log("2�� �������� Ű ���� " + GameManager.instance.GetThirdChoiceKey());
                }
                ChoiceButton[ChoiceCount].SetActive(true);
                ChoiceCount++;
            }
            else
            { 
                Debug.Log("�������� 3���� �Ѿ�ϴ�.");               
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
                        Debug.LogWarning("�������� " + ChoiceButton.Length +"�� �̻��Դϴ�");
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
            Debug.Log("KeyCheck�� ���� �Ǿ����ϴ�. ���� ���� " + GameManager.instance.GetKeyCheck());

            GameManager.instance.isChoice = true;
            ChoiceCount = 0;
            Debug.Log("������ ������ ������ üũ");

            var eventSystem = EventSystem.current;
            eventSystem.SetSelectedGameObject(ChoiceButton[0], new BaseEventData(eventSystem));
            
            /* ���⼭�� �������� ������ �Ѿ�� ������ �̰� �������� �ʴ´�.
                PrepareNextTalk();
            */
        }
       else
       { 
            yield return null;
            StartCoroutine(ChoiceEnd(i));            
       }
    }

    public void TurnOffChoiceButton() // ������ �������� ��ư�� ������� �ϱ��
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

        Debug.Log("Ű(������)������ �ʱ�ȭ �Ǿ����ϴ�");        
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

    private void ChangeTalker(int i) // -1 �Ͻ� ���� 
    {
        GameManager.instance.ChangeTalkerNum(i); 
        Debug.Log("ȭ�� ���� : " + i);
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
            Debug.LogWarning("TalkManager�� ChangeTalker(int i)�� �߸��� ���� ���Խ��ϴ�.");   
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
                        Debug.Log("��ġ�ϴ� Bookmark�� ã�ҽ��ϴ�. CSVnum = " + i);
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
                Debug.Log(ChangeNum + "�� ����(" + GameManager.instance.Var[ChangeNum].VarName + ")�� " +  IncreaseNum + "��ŭ �����Ͽ����ϴ�. ���� �� = " + GameManager.instance.Var[ChangeNum].Var);
                
            }
            else
            { 
                Debug.LogWarning("IncreaseVar �̺�Ʈ Talker�� �߸��� ���� ���Խ��ϴ�. ���� CSVNum�� " + CSVNum);
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
                    Debug.Log(i + "�� ����(" + GameManager.instance.Var[i].VarName + ")�� " +  IncreaseNum + "��ŭ �����Ͽ����ϴ�. ���� �� = " + GameManager.instance.Var[i].Var);
                    break;
                }
                Check++;
            }

            if(Check >= GameManager.instance.Var.Length)
            { 
                Debug.LogWarning("IncreaseVarByName �̺�Ʈ Talker�� �߸��� ���� ���Խ��ϴ�. ���� CSVNum�� " + CSVNum);
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
                Debug.Log(ChangeNum + "�� ����(" + GameManager.instance.Var[ChangeNum].VarName + ")�� " +  GameManager.instance.Var[IncreaseNum].Var + "��ŭ �����Ͽ����ϴ�. ���� �� = " + GameManager.instance.Var[ChangeNum].Var);                
            }
            else
            { 
                Debug.LogWarning("IncreaseVarByVar �̺�Ʈ Talker�� Content �߸��� ���� ���Խ��ϴ�. ���� CSVNum�� " + CSVNum);
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
                Debug.LogWarning("IncreaseVarByVarByName �̺�Ʈ Talker�� �߸��� ���� ���Խ��ϴ�. ���� CSVNum�� " + CSVNum);
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
                Debug.LogWarning("IncreaseVarByVarByName �̺�Ʈ Content�� �߸��� ���� ���Խ��ϴ�. ���� CSVNum�� " + CSVNum);
            }
            Check = 0;

            GameManager.instance.Var[ChangeNum].Var += GameManager.instance.Var[IncreaseNum].Var;
            Debug.Log(ChangeNum + "�� ����(" + GameManager.instance.Var[ChangeNum].VarName + ")�� " +  GameManager.instance.Var[IncreaseNum].Var + "��ŭ �����Ͽ����ϴ�. ���� �� = " + GameManager.instance.Var[ChangeNum].Var);
            
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
                Debug.Log(VarNum  + "�� ����(" + GameManager.instance.Var[VarNum].VarName + ")�� " +  ChangeNum+ "�� ����Ǿ����ϴ�. ���� �� = " + GameManager.instance.Var[VarNum].Var);
                
            }
            else
            { 
                Debug.LogWarning("ChangeVar �̺�Ʈ Talker�� �߸��� ���� ���Խ��ϴ�. ���� CSVNum�� " + CSVNum);
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
                    Debug.Log(VarNum  + "�� ����(" + GameManager.instance.Var[VarNum].VarName + ")�� " +  GameManager.instance.Var[ChangeNum].Var + "�� ����Ǿ����ϴ�. ���� �� = " + GameManager.instance.Var[VarNum].Var);
                }
                else
                { 
                    Debug.LogWarning("ChangeVar �̺�Ʈ Content�� �߸��� ���� ���Խ��ϴ�. ���� CSVNum�� " + CSVNum);
                }
            }
            else
            { 
                Debug.LogWarning("ChangeVar �̺�Ʈ Talker�� �߸��� ���� ���Խ��ϴ�. ���� CSVNum�� " + CSVNum);
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
                    Debug.Log(i + "�� ����(" + GameManager.instance.Var[i].VarName + ")�� " +  ChangeNum + "���� ����Ǿ����ϴ�. ���� �� = " + GameManager.instance.Var[i].Var);
                    break;
                }
                Check++;
            }

            if(Check >= GameManager.instance.Var.Length)
            { 
                Debug.LogWarning("IncreaseVarByName �̺�Ʈ Talker�� �߸��� ���� ���Խ��ϴ�. ���� CSVNum�� " + CSVNum);
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
                Debug.LogWarning("ChangeVarByVarByName �̺�Ʈ Talker�� �߸��� ���� ���Խ��ϴ�. ���� CSVNum�� " + CSVNum);
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
                Debug.LogWarning("ChangeVarByVarByName �̺�Ʈ Content�� �߸��� ���� ���Խ��ϴ�. ���� CSVNum�� " + CSVNum);
            }
            Check = 0;

            GameManager.instance.Var[VarNum].Var = GameManager.instance.Var[ChangeNum].Var;
            Debug.Log(VarNum + "�� ����(" + GameManager.instance.Var[VarNum].VarName + ")�� " +  GameManager.instance.Var[ChangeNum].Var + "���� ����Ǿ����ϴ�. ���� �� = " + GameManager.instance.Var[VarNum].Var);

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
                Debug.Log(VarNum + "�� ����(" + GameManager.instance.Var[VarNum].VarName + ")�� ���� ����ŭ �����Ͽ����ϴ�. ���� �� = " + GameManager.instance.Var[VarNum].Var);                
            }
            else if(SecondRandNum > FirstRandNum)
            { 
                GameManager.instance.Var[VarNum].Var += Random.Range(FirstRandNum, SecondRandNum);
                Debug.Log(VarNum + "�� ����(" + GameManager.instance.Var[VarNum].VarName + ")�� ���� ����ŭ �����Ͽ����ϴ�. ���� �� = " + GameManager.instance.Var[VarNum].Var);                
            }
            else
            { 
                Debug.LogWarning("IncreaseVarByRand �̺�Ʈ�� �߸��� ���� ���Խ��ϴ�. ���� CSVNum�� " + CSVNum);
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
                Debug.LogWarning("IncreaseVarByRandByName �̺�Ʈ Talker�� �߸��� ���� ���Խ��ϴ�. ���� CSVNum�� " + CSVNum);
            }
            Check = 0;

            if(FirstRandNum > SecondRandNum)
            {
                GameManager.instance.Var[VarNum].Var += Random.Range(SecondRandNum,FirstRandNum);
                Debug.Log(VarNum + "�� ����(" + GameManager.instance.Var[VarNum].VarName + ")�� ���� ����ŭ �����Ͽ����ϴ�. ���� �� = " + GameManager.instance.Var[VarNum].Var);                
            }
            else if(SecondRandNum > FirstRandNum)
            { 
                GameManager.instance.Var[VarNum].Var += Random.Range(FirstRandNum, SecondRandNum);
                Debug.Log(VarNum + "�� ����(" + GameManager.instance.Var[VarNum].VarName + ")�� ���� ����ŭ �����Ͽ����ϴ�. ���� �� = " + GameManager.instance.Var[VarNum].Var);                
            }
            else
            { 
                Debug.LogWarning("IncreaseVarByRandByName �̺�Ʈ�� �߸��� ���� ���Խ��ϴ�. ���� CSVNum�� " + CSVNum);
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
                Debug.Log(VarNum + "�� ����(" + GameManager.instance.Var[VarNum].VarName + ")�� ���� ������ ����Ǿ����ϴ�. ���� �� = " + GameManager.instance.Var[VarNum].Var);                
            }
            else if(SecondRandNum > FirstRandNum)
            { 
                GameManager.instance.Var[VarNum].Var = Random.Range(FirstRandNum, SecondRandNum);
                Debug.Log(VarNum + "�� ����(" + GameManager.instance.Var[VarNum].VarName + ")�� ���� ������ ����Ǿ����ϴ�. ���� �� = " + GameManager.instance.Var[VarNum].Var);                
            }
            else
            { 
                Debug.LogWarning("ChangeVarByRand �̺�Ʈ�� �߸��� ���� ���Խ��ϴ�. ���� CSVNum�� " + CSVNum);
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
                Debug.LogWarning("ChangeVarByRandByName �̺�Ʈ Talker�� �߸��� ���� ���Խ��ϴ�. ���� CSVNum�� " + CSVNum);
            }
            Check = 0;

            if(FirstRandNum > SecondRandNum)
            {
                GameManager.instance.Var[VarNum].Var = Random.Range(SecondRandNum,FirstRandNum);
                Debug.Log(VarNum + "�� ����(" + GameManager.instance.Var[VarNum].VarName + ")�� ���� ������ ����Ǿ����ϴ�. ���� �� = " + GameManager.instance.Var[VarNum].Var);                
            }
            else if(SecondRandNum > FirstRandNum)
            { 
                GameManager.instance.Var[VarNum].Var = Random.Range(FirstRandNum, SecondRandNum);
                Debug.Log(VarNum + "�� ����(" + GameManager.instance.Var[VarNum].VarName + ")�� ���� ������ ����Ǿ����ϴ�. ���� �� = " + GameManager.instance.Var[VarNum].Var);                
            }
            else
            { 
                Debug.LogWarning("ChangeVarByRandByName �̺�Ʈ�� �߸��� ���� ���Խ��ϴ�. ���� CSVNum�� " + CSVNum);
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
                    Debug.Log(ChangeNum + "��° ����ġ(" + GameManager.instance.Switch[ChangeNum].SwitchName + ")�� true�� ����Ǿ����ϴ�.");
                }
                else if(TalkData[CSVNum]["Content"].ToString() == "false")
                { 
                    GameManager.instance.Switch[ChangeNum].Switch = false;
                    Debug.Log(ChangeNum + "��° ����ġ(" + GameManager.instance.Switch[ChangeNum].SwitchName + ")�� false�� ����Ǿ����ϴ�.");
                }
                else
                { 
                    Debug.LogWarning("ChangeSwitch �̺�Ʈ Content�� �߸��� ���� ���Խ��ϴ�. ���� CSVNum�� " + CSVNum);
                }
            }
            else
            { 
                Debug.LogWarning("ChangeSwitch �̺�Ʈ Talker�� �߸��� ���� ���Խ��ϴ�. ���� CSVNum�� " + CSVNum);
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
                        Debug.Log(i + "��° ����ġ(" + GameManager.instance.Switch[i].SwitchName + ")�� true�� ����Ǿ����ϴ�.");
                    }
                    else if(TalkData[CSVNum]["Content"].ToString() == "false")
                    { 
                        GameManager.instance.Switch[i].Switch = false;
                        Debug.Log(i + "��° ����ġ(" + GameManager.instance.Switch[i].SwitchName + ")�� false�� ����Ǿ����ϴ�.");
                    }
                    else
                    { 
                        Debug.LogWarning("ChangeSwitchByName �̺�Ʈ Content�� �߸��� ���� ���Խ��ϴ�. ���� CSVNum�� " + CSVNum);
                    }
                    break;
                }
                Check ++;
            }


            if(Check >= GameManager.instance.Switch.Length)
            { 
                Debug.LogWarning("ChangeSwitchByName �̺�Ʈ Talker�� �߸��� ���� ���Խ��ϴ�. ���� CSVNum�� " + CSVNum);
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
                Debug.Log(ChangeNum + "��° ����ġ(" + GameManager.instance.Switch[ChangeNum].SwitchName + ")�� "+ GameManager.instance.Switch[IncreaseNum].SwitchName + "�� ����Ǿ����ϴ�.");
            }
            else
            { 
                Debug.LogWarning("ChangeSwitch �̺�Ʈ Talker�� Content�� �߸��� ���� ���Խ��ϴ�. ���� CSVNum�� " + CSVNum);
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
                Debug.LogWarning("ChangeSwitchByVarByName �̺�Ʈ Talker�� �߸��� ���� ���Խ��ϴ�. ���� CSVNum�� " + CSVNum);
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
                Debug.LogWarning("ChangeSwitchByVarByName �̺�Ʈ Content�� �߸��� ���� ���Խ��ϴ�. ���� CSVNum�� " + CSVNum);
            }
            Check = 0;

            GameManager.instance.Switch[ChangeNum].Switch = GameManager.instance.Switch[IncreaseNum].Switch;
            Debug.Log(ChangeNum + "��° ����ġ(" + GameManager.instance.Switch[ChangeNum].SwitchName + ")�� "+ GameManager.instance.Switch[IncreaseNum].SwitchName + "�� ����Ǿ����ϴ�.");

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
        Debug.Log("CSV������ �ٲߴϴ�.");
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

    private void IfCheck(int CSVNum) //switch �κ��� ��������
    { 
        string IfTrue = ""; // ������ �����Ҷ� Ű��
        string IfFalse = ""; // ������ �������� ���� �� Ű��
        int GameManagerNum = 0; // ���� ����ġ�� ������ ��
        int FirstCompare = 0; // ù��° �񱳰�
        int SecondCompare = 0; // �ι�° �񱳰�

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
                    Debug.LogWarning("if �̺�Ʈ �ι�° �� Content�κп� �߸��� ���� ���Խ��ϴ�. CSVNum �� " + CSVNum);
                }

            }
            else
            { 
                Debug.LogWarning("if �̺�Ʈ Content�κп� �߸��� ���� ���Խ��ϴ� CSVNum �� " + CSVNum);
            }
        }
        else if(TalkData[CSVNum]["Talker"].ToString() == "Var") // ������ ���⼭ ���� �̰� �������� ����;;;;
        { 
            if(int.Parse(TalkData[CSVNum]["Content"].ToString()) < GameManager.instance.Var.Length)
            { 
                GameManagerNum = int.Parse(TalkData[CSVNum]["Content"].ToString());

                GameManager.instance.PlusCSVNum(1);
                CSVNum++;

                IfTrue = TalkData[CSVNum]["Key"].ToString();
                FirstCompare = int.Parse(TalkData[CSVNum]["Content"].ToString());

                if(TalkData[CSVNum]["Event"].ToString() == "Range") //���� ���� üũ
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
                else if(TalkData[CSVNum]["Talker"].ToString() == "<") // < �� ���
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
                    
                    if(FirstCompare > GameManager.instance.Var[GameManagerNum].Var) // ����Ϸ�
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
                    
                    if(FirstCompare >= GameManager.instance.Var[GameManagerNum].Var) // ����Ϸ�
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
                    
                    if(FirstCompare < GameManager.instance.Var[GameManagerNum].Var) // ����Ϸ�
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
                    
                    if(FirstCompare <= GameManager.instance.Var[GameManagerNum].Var) // ����Ϸ�
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
                    Debug.LogWarning("if �̺�Ʈ �ι�° �� Talker�κп� �߸��� ���� ���Խ��ϴ�. CSVNum �� " + CSVNum);
                }

            }
            else
            { 
                Debug.LogWarning("if �̺�Ʈ Content�κп� �߸��� ���� ���Խ��ϴ�. CSVNum �� " + CSVNum);
            }            
        }
        else
        { 
            Debug.LogWarning("if �̺�Ʈ talker�κп� �߸��� ���� ���Խ��ϴ�. CSVNum �� " + CSVNum);    
        }
    }   

   
    private void IfCheckByName(int CSVNum) 
    { 
        string IfTrue = ""; // ������ �����Ҷ� Ű��
        string IfFalse = ""; // ������ �������� ���� �� Ű��
        int FirstCompare = 0; // ù��° �񱳰�
        int SecondCompare = 0; // �ι�° �񱳰�

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
                        Debug.LogWarning("IfForName �̺�Ʈ �ι�° �� Content�κп� �߸��� ���� ���Խ��ϴ�. CSVNum �� " + CSVNum);
                        break;
                    }
                }
            }

            if(SwitchNameChecker > GameManager.instance.Switch.Length)
            { 
                Debug.LogWarning("IfForName �̺�Ʈ ù��° �� Content�κп� �߸��� ���� ���Խ��ϴ�. CSVNum �� " + CSVNum);
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

                    if(TalkData[CSVNum]["Event"].ToString() == "Range") //���� ���� üũ
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
                    else if(TalkData[CSVNum]["Talker"].ToString() == "<") // < �� ���
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
                    
                        if(FirstCompare > GameManager.instance.Var[z].Var) // ����Ϸ�
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
                    
                        if(FirstCompare >= GameManager.instance.Var[z].Var) // ����Ϸ�
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
                    
                        if(FirstCompare < GameManager.instance.Var[z].Var) // ����Ϸ�
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
                    
                        if(FirstCompare <= GameManager.instance.Var[z].Var) // ����Ϸ�
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
                        Debug.LogWarning("if �̺�Ʈ �ι�° �� Talker�κп� �߸��� ���� ���Խ��ϴ�. CSVNum �� " + CSVNum);
                    }
                    break;
                }     
            }    
            if(VarNameChecker > GameManager.instance.Var.Length)
            { 
                Debug.LogWarning("if �̺�Ʈ Content�κп� �߸��� ���� ���Խ��ϴ�. CSVNum �� " + CSVNum);
            }
            VarNameChecker = 0;
        }
        else
        { 
            Debug.LogWarning("if �̺�Ʈ talker�κп� �߸��� ���� ���Խ��ϴ�. CSVNum �� " + CSVNum);    
        }
    }    

    private void IfCheckByVar(int CSVNum) 
    { 
        string IfTrue = ""; // ������ �����Ҷ� Ű��
        string IfFalse = ""; // ������ �������� ���� �� Ű��
        int FirstGameManagerNum = 0; //  ���� ����ġ�� ������ ��(ù��° �ٿ� ������)
        int SecondGameManagerNum = 0; // ���� ����ġ�� ������ ��(�ι�° �ٿ� ������)
        int ThirdGameManagerNum = 0; // ���� ����ġ�� ������ ��(����° �ٿ� ������)

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
                    Debug.LogWarning("ifByVar �̺�Ʈ �ι�° Content�κп� �߸��� ���� ���Խ��ϴ� CSVNum �� " + CSVNum);
                }
            }
            else
            { 
                Debug.LogWarning("ifByVar �̺�Ʈ ù��° Content�κп� �߸��� ���� ���Խ��ϴ� CSVNum �� " + CSVNum);
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

                    if(TalkData[CSVNum]["Event"].ToString() == "Range") //���� ���� üũ
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
                    else if(TalkData[CSVNum]["Talker"].ToString() == "<") // < �� ���
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
                    
                        if(GameManager.instance.Var[SecondGameManagerNum].Var > GameManager.instance.Var[FirstGameManagerNum].Var) // ����Ϸ�
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
                    
                        if(GameManager.instance.Var[SecondGameManagerNum].Var >= GameManager.instance.Var[FirstGameManagerNum].Var) // ����Ϸ�
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
                    
                        if(GameManager.instance.Var[SecondGameManagerNum].Var < GameManager.instance.Var[FirstGameManagerNum].Var) // ����Ϸ�
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
                    
                        if(GameManager.instance.Var[SecondGameManagerNum].Var <= GameManager.instance.Var[FirstGameManagerNum].Var) // ����Ϸ�
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
                        Debug.LogWarning("if �̺�Ʈ �ι�° �� Talker�κп� �߸��� ���� ���Խ��ϴ�. CSVNum �� " + CSVNum);
                    }
                }
            }
            else
            { 
                Debug.LogWarning("if �̺�Ʈ Content�κп� �߸��� ���� ���Խ��ϴ�. CSVNum �� " + CSVNum);
            }            
        }
        else
        { 
            Debug.LogWarning("if �̺�Ʈ talker�κп� �߸��� ���� ���Խ��ϴ�. CSVNum �� " + CSVNum);    
        }
    }   

    private void IfCheckByVarByName(int CSVNum) 
    { 
        string IfTrue = ""; // ������ �����Ҷ� Ű��
        string IfFalse = ""; // ������ �������� ���� �� Ű��
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
                Debug.LogWarning("IfByVarByName �̺�Ʈ ù��° �� Content�κп� �߸��� ���� ���Խ��ϴ�. CSVNum �� " + CSVNum);
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
                Debug.LogWarning("IfByVarByName �̺�Ʈ �ι�° �� Content�κп� �߸��� ���� ���Խ��ϴ�. CSVNum �� " + CSVNum);
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
                Debug.LogWarning("IfByVarByName �̺�Ʈ ù��° �� Content�κп� �߸��� ���� ���Խ��ϴ�. CSVNum �� " + CSVNum);
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
                Debug.LogWarning("IfByVarByName �̺�Ʈ ù��° �� Content�κп� �߸��� ���� ���Խ��ϴ�. CSVNum �� " + CSVNum);
            }
            VarNameChecker = 0;

            if(TalkData[CSVNum]["Event"].ToString() == "Range") //���� ���� üũ
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
                    Debug.LogWarning("IfByVarByName �̺�Ʈ ù��° �� Content�κп� �߸��� ���� ���Խ��ϴ�. CSVNum �� " + CSVNum);
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

            else if(TalkData[CSVNum]["Talker"].ToString() == "<") // < �� ���
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
                    
                if(GameManager.instance.Var[SecondGameManagerNum].Var > GameManager.instance.Var[FirstGameManagerNum].Var) // ����Ϸ�
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
                    
                if(GameManager.instance.Var[SecondGameManagerNum].Var  >= GameManager.instance.Var[FirstGameManagerNum].Var) // ����Ϸ�
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
                    
                if(GameManager.instance.Var[SecondGameManagerNum].Var  < GameManager.instance.Var[FirstGameManagerNum].Var) // ����Ϸ�
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
                    
                if(GameManager.instance.Var[SecondGameManagerNum].Var  <= GameManager.instance.Var[FirstGameManagerNum].Var) // ����Ϸ�
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
                Debug.LogWarning("if �̺�Ʈ �ι�° �� Talker�κп� �߸��� ���� ���Խ��ϴ�. CSVNum �� " + CSVNum);
            }
        }
        else
        { 
            Debug.LogWarning("if �̺�Ʈ talker�κп� �߸��� ���� ���Խ��ϴ�. CSVNum �� " + CSVNum);    
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
            Debug.LogWarning("TalkManager SetMenuButton �Լ��� �߸��� ���� ���Խ��ϴ�!");    
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
            Debug.LogWarning("TalkManager SetLogButton �Լ��� �߸��� ���� ���Խ��ϴ�!");    
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

    private void CreateLog() // �α� ó��
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
        Debug.Log("������ �����մϴ�.");
        for(int i = 0; i < GameManager.instance.Talker.Length; i++) //ȭ�� �̹��� �ʱ�ȭ
        {
            GameManager.instance.Talker[i] = null;
            TalkerImage[i].GetComponent<Image>().sprite = GameManager.instance.GetBlank();   
        } 
        TalkText.GetComponent<Text>().text = ""; //���â �ʱ�ȭ

        BackgroundCanvas.transform.GetChild(0).GetComponent<Image>().sprite = BackgroundDatabaseManager.instance.BackGroundDB(0); // ����ʱ�ȭ
        GameManager.instance.ChangeIsBGMOn(true); // ���������� �ʱ�ȭ
        GameManager.instance.ChangeBGM(BGMDatabaseManager.instance.BGMDB(0)); // ����ʱ�ȭ
        ChangeTalker(-1); // ȭ�� �ʱ�ȭ
        ResetGameKey();
    }

    private void ReLoadGame() // ������ ����ؼ� ���ε� �Ҳ��� �ؿ� �ε���� ���
    {
        Debug.Log("������ ���ε� �մϴ�.");
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
        Debug.Log("�뺻�� ���̴� " + TalkData.Count);
        GameManager.instance.CantUseMenu = false;
        GameManager.instance.isMain = false; 
        NextTalk();
    }

    public void LoadGame()
    {
        StopAllCoroutines();
        for(int i = 0; i < TalkerImage.Length; i++) //ȭ�� �ʱ�ȭ
        { 
            TalkerImage[i].GetComponent<Image>().sprite = GameManager.instance.GetBlank();   
        }


        TalkCanvas.SetActive(true);
        BackgroundCanvas.SetActive(true);

        BackgroundCanvas.transform.GetChild(0).GetComponent<Image>().sprite = BackgroundDatabaseManager.instance.GetBackGround(GameManager.instance.Background);

        GameManager.instance.ChangeBGM(BGMDatabaseManager.instance.GetBGM(GameManager.instance.BGM)); // ���ó��
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
        for(int i = 0; i < TalkerImage.Length ;i++) // ȭ�� ó��
        {
            if(GameManager.instance.Talker[i] != null)
            { 
                TalkerPlus(i, GameManager.instance.Talker[i].Name);
            }         
        }
        ChangeTalker(GameManager.instance.GetTalkerNum());  // ȭ�ں���

        int ChoiceCount = 0;

        if(GameManager.instance.IsFadeOut == true)
        { 
            EffectManager.instance.ReturnToFade();            
        }
        else
        { 
            EffectManager.instance.RemoveFade();
        }

        // ���߿� key�� �񱳵� �߰��� ��(���׽�Ʈ�� ������ �ڵ�)
        if(TalkData[GameManager.instance.GetCSVNum()]["Event"].ToString() == "ChoiceEnd")
        {
            Debug.Log("������ üũ");
            // ���� �ϳ� csv���� ���� �θ���ð� �� �� �� ������ �峭���ϸ� ���׳� ����
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
            // ���κ��ε��� �̰� �� �α׸� �̿��ϴ� �ɷ� �����ϱ����� �ּ� ó��
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
            //��� ó�� �α��̿� ����
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
            //GameManager.instance.ChangeCSVNum(GameManager.instance.GetCSVNum() - 1); //�ڷ�ƾ���� �̸��о���� ������ -1�ؾ��� // Talk�� ���� �ٲ۰� �׽�Ʈ <- ��������
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
