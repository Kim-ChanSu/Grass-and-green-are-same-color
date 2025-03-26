using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum KeyAction {Talk, Skip, Menu, KeyCount}

public static class KeySetting 
{ 
    public static Dictionary<KeyAction, KeyCode> Key = new Dictionary<KeyAction, KeyCode>();
}

public class KeyManager : MonoBehaviour
{
    public KeyCode[] DefaultKeys = new KeyCode[] {KeyCode.Z, KeyCode.LeftControl, KeyCode.X};

    public static KeyManager instance;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);

            for(int i = 0; i < (int)KeyAction.KeyCount; i++)
            { 
                KeySetting.Key.Add((KeyAction)i, DefaultKeys[i]);
            }
        }
        else
        {
            Debug.LogWarning("씬에 키매니저가 2개이상 존재합니다. 타이틀 화면으로 돌아간 경우에는 문제없을꺼에요 앤 삭제 안하거든요");
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    /*
    void Update()
    {
        
        if(Input.GetKeyDown(KeySetting.Key[KeyAction.Talk]))
        { 
            Debug.Log("Talk");
        }
        if(Input.GetKey(KeySetting.Key[KeyAction.Skip]))
        { 
            Debug.Log("Skip");
        }
        if(Input.GetKeyDown(KeySetting.Key[KeyAction.Menu]))
        { 
            Debug.Log("Menu");
        }
        
    }
    */
  
    private void OnGUI()
    { 
        if(SetKeyCheck == true)
        {                 
            Event KeyCheck = Event.current;
            if(KeyCheck.isKey)
            { 
                bool IsDuplication = false;
       
                for(int i = 0; i < (int)KeyAction.KeyCount; i++)
                { 
                    if(KeySetting.Key[(KeyAction)i] == KeyCheck.keyCode)
                    { 
                        Debug.Log(KeySetting.Key[(KeyAction)i].ToString()); 
                        IsDuplication = true;
                        break;    
                    }    
                }
                
                if(KeyCheck.keyCode.ToString() == "None" || KeyCheck.keyCode == KeyCode.Space || KeyCheck.keyCode == KeyCode.Escape) // 저 for문 때문인지는 몰라도 여러번 발동되서 None으로 False를 통가해버리더라
                { 
                    IsDuplication = true;
                }
                // Debug.Log(IsDuplication); 이거 bool값 체크 안해주면 중복일때 반복되더라 중요하니 두번 적음

                if(IsDuplication == false)
                { 
                    Debug.Log("KeyChangeCheck " + KeyCheck.keyCode.ToString());
                    KeySetting.Key[(KeyAction)KeyNum] = KeyCheck.keyCode;
                    MenuManager.instance.UpdateKeyData();
                    KeyNum = -1;
                        
                    //설정한 키 저장하는건 이쯤에서 해주면 될 듯
                    // 여기서 안하고 설정닫을때 일괄적으로 해주면 되는거 ㅋㅋ
                    //-----------------------------------------

                    //SetKeyCheck = false; ▽저기서 버튼 바꾸는거 때문에 어짜피 SetKeyCheck가 false바뀜
                    var eventSystem = EventSystem.current;
                    eventSystem.SetSelectedGameObject(MenuManager.instance.ExitSettingButton, new BaseEventData(eventSystem));
                }
            }
           
        }
    }

    private int KeyNum = -1;
    private bool SetKeyCheck = false;

    public void ChangeKeyNum(int Num)
    { 
        KeyNum = Num;
        SetKeyCheck = true;
    }     

    public void CancelSetKey()
    { 
        KeyNum = -1;
        Invoke("MakeSetKeyCheckFalse", 0.01f);
    }

    void MakeSetKeyCheckFalse()
    { 
        SetKeyCheck = false;
    }

    public bool GetSetKeyCheck()
    { 
        return SetKeyCheck;
    }
}
