using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    void ButtonBlip()
    { 
        GameManager.instance.PlaySE(SEDatabaseManager.instance.GetSE("Click"));   
    }

    public void StartGame()
    { 
        GameManager.instance.StartGame();
        ButtonBlip();
    }

    public void ExitGame()
    {  
        Application.Quit(); 
        ButtonBlip();
    }

    //�̰� ���װ� �η����� ��ġ���� ���ϰڳ� �� �迭�� �߾�� �ϴµ� ����;
    public void FirstChoiceButton()
    {
        GameManager.instance.ChangeChoiceKey(GameManager.instance.GetFirstChoiceKey());
        Debug.Log("ChoiceKey�� ����Ǿ����ϴ� ���� �� " + GameManager.instance.GetChoiceKey());

        TalkManager.instance.TurnOffChoiceButton();
        TalkManager.instance.PrepareNextTalk();        
        GameManager.instance.isChoice = false;
        ButtonBlip();
    }

    public void SecondChoiceButton()
    {
        GameManager.instance.ChangeChoiceKey(GameManager.instance.GetSecondChoiceKey());
        Debug.Log("ChoiceKey�� ����Ǿ����ϴ� ���� �� " + GameManager.instance.GetChoiceKey());

        TalkManager.instance.TurnOffChoiceButton();
        TalkManager.instance.PrepareNextTalk();        
        GameManager.instance.isChoice = false;
        ButtonBlip();
    }

    public void ThirdChoiceButton()
    {
        GameManager.instance.ChangeChoiceKey(GameManager.instance.GetThirdChoiceKey());
        Debug.Log("ChoiceKey�� ����Ǿ����ϴ� ���� �� " + GameManager.instance.GetChoiceKey());

        TalkManager.instance.TurnOffChoiceButton();
        TalkManager.instance.PrepareNextTalk();        
        GameManager.instance.isChoice = false;
        ButtonBlip();
    }

    public void MenuButton() // GameManager.instance.PlaySE(SEDatabaseManager.instance.GetSE("Click"));�� MenuManager�� OpenMenu�� CloseMenu�� ������
    { 
        MenuManager.instance.MenuButton();
    }

    public void LogButton() // GameManager.instance.PlaySE(SEDatabaseManager.instance.GetSE("Click"));�� TalkManager�� OpenLog�� CloseLog�� ������
    { 
        TalkManager.instance.LogButton();
    }

    public void TextButton()
    { 
        GameManager.instance.TextButtonCheck = true;
    }

    public void OpenSaveButton()
    { 
        MenuManager.instance.OpenSaveMenu();
        ButtonBlip();
    }

    public void OpenLoadButton()
    {
        MenuManager.instance.OpenLoadMenu();
        ButtonBlip();
    }

    public void OptionButton()
    { 
        MenuManager.instance.OpenSettingMenu();
        ButtonBlip();
    }

    public void SaveButton(int num)
    { 
        ButtonBlip();
        if(MenuManager.instance.ThereIsSaveFile[num] == false)
        { 
            MenuManager.instance.SaveGameData(num);
        }
        else
        { 
            MenuManager.instance.OpenSaveCheckWindow(num);
        }        
    }

    /*
    public void SaveButton0()
    {
        ButtonBlip();
        if(MenuManager.instance.ThereIsSaveFile[0] == false)
        { 
            MenuManager.instance.SaveGameData(0);
        }
        else
        { 
            MenuManager.instance.OpenSaveCheckWindow(0);
        }
    }

    public void SaveButton1()
    {
        ButtonBlip();
        if(MenuManager.instance.ThereIsSaveFile[1] == false)
        { 
            MenuManager.instance.SaveGameData(1);
        }
        else
        { 
            MenuManager.instance.OpenSaveCheckWindow(1);
        }
    }

    public void SaveButton2()
    {
        ButtonBlip();
        if(MenuManager.instance.ThereIsSaveFile[2] == false)
        { 
            MenuManager.instance.SaveGameData(2);
        }
        else
        { 
            MenuManager.instance.OpenSaveCheckWindow(2);
        }
    }

    public void SaveButton3()
    {
        ButtonBlip();
        if(MenuManager.instance.ThereIsSaveFile[3] == false)
        { 
            MenuManager.instance.SaveGameData(3);
        }
        else
        { 
            MenuManager.instance.OpenSaveCheckWindow(3);
        }
    }
    */

    public void SaveCheckWindowYesButton()
    { 
        ButtonBlip();
        if(MenuManager.instance.isSave == true)
        { 
            MenuManager.instance.SaveGameDataInWindow();
        }
        else
        { 
            MenuManager.instance.LoadGameData();
        }
    }

    public void SaveCheckWindowNoButton()
    { 
        ButtonBlip();
        MenuManager.instance.CloseSaveCheckWindow();
    }

    public void SetKeyButton(int Num)
    { 
        ButtonBlip();
        KeyManager.instance.ChangeKeyNum(Num);
    }

    public void DeselectSetKeyButton()
    {
        KeyManager.instance.CancelSetKey();
    }

    public void ExitMenuButton()
    { 
        ButtonBlip();
        MenuManager.instance.OpenExitMenu();
    }

    public void ReturnTitleButton()
    {
        ButtonBlip();
        GameManager.instance.ReturnTitle();
    }

    public void ReturnGameButton()
    { 
        ButtonBlip();
        MenuManager.instance.CallCloseMenu();
    }

    public void ResetSettingButton()
    { 
        ButtonBlip();
        DatabaseManager.instance.LoadDefaultSettingData();
        MenuManager.instance.UpdateKeyData();
    }
}
