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
            Debug.LogWarning("���� Ű�Ŵ����� 2���̻� �����մϴ�. Ÿ��Ʋ ȭ������ ���ư� ��쿡�� �������������� �� ���� ���ϰŵ��");
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
                
                if(KeyCheck.keyCode.ToString() == "None" || KeyCheck.keyCode == KeyCode.Space || KeyCheck.keyCode == KeyCode.Escape) // �� for�� ���������� ���� ������ �ߵ��Ǽ� None���� False�� �밡�ع�������
                { 
                    IsDuplication = true;
                }
                // Debug.Log(IsDuplication); �̰� bool�� üũ �����ָ� �ߺ��϶� �ݺ��Ǵ��� �߿��ϴ� �ι� ����

                if(IsDuplication == false)
                { 
                    Debug.Log("KeyChangeCheck " + KeyCheck.keyCode.ToString());
                    KeySetting.Key[(KeyAction)KeyNum] = KeyCheck.keyCode;
                    MenuManager.instance.UpdateKeyData();
                    KeyNum = -1;
                        
                    //������ Ű �����ϴ°� ���뿡�� ���ָ� �� ��
                    // ���⼭ ���ϰ� ���������� �ϰ������� ���ָ� �Ǵ°� ����
                    //-----------------------------------------

                    //SetKeyCheck = false; �����⼭ ��ư �ٲٴ°� ������ ��¥�� SetKeyCheck�� false�ٲ�
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
