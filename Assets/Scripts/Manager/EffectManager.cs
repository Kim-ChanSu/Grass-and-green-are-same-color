using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectManager : MonoBehaviour
{
    public static EffectManager instance;

    void Awake() 
    {
        if (instance == null)
        {
            DontDestroyOnLoad(this.gameObject);
            instance = this;
        }
        else
        {
            Debug.LogWarning("씬에 EffectManager가 2개이상 존재합니다.");
            Destroy(gameObject);
        }

     
    }

    [SerializeField]
    private GameObject EffectCanvas; 
    [SerializeField]
    private GameObject EffectImage;  
    [SerializeField]
    private GameObject FlashImage;  
    [SerializeField]
    private GameObject EndingCreditImage; 

    [SerializeField]
    private float EffectAlphaMin;
    [SerializeField]
    private float EffectAlphaMax;

    private float EffectTime = 0;

    private float BackgroundMoveSpeed = 2;

    private float EndingCreditTime = 25;


    IEnumerator SetFadeIn(float FadeTime)
    { 
        if(EffectImage.GetComponent<Image>().color.a > EffectAlphaMin)
        { 
            FadeInEffect(FadeTime);
            yield return new WaitForSeconds(0.1f);
            StartCoroutine(SetFadeIn(FadeTime)); 
        }
        else
        { 
            Color color = EffectImage.GetComponent<Image>().color;
            color.a = 0.0f;
            EffectImage.GetComponent<Image>().color = color;
            EffectTime = 0;
            EffectEnd();
        }        
    }

    IEnumerator SetFadeOut(float FadeTime)
    { 
        if(EffectImage.GetComponent<Image>().color.a < EffectAlphaMax)
        { 
            FadeOutEffect(FadeTime);
            yield return new WaitForSeconds(0.1f);
            StartCoroutine(SetFadeOut(FadeTime)); 
        }
        else
        { 
            Color color = EffectImage.GetComponent<Image>().color;
            color.a = 1.0f;
            EffectImage.GetComponent<Image>().color = color;
            EffectTime = 0;
            TalkManager.instance.OffTalkerImageBase();
            //TalkManager.instance.OnTalkBackground();
            GameManager.instance.IsFadeOut = true;
            EffectCanvas.GetComponent<Canvas>().sortingOrder = 1;
            EffectEnd();
        }        
    }

    public void FadeInEffect(float FadeTime)
    { 
        EffectTime += 1 / FadeTime / 10;
        Color color = EffectImage.GetComponent<Image>().color;
        color.a = Mathf.Lerp(1, 0, EffectTime);
        EffectImage.GetComponent<Image>().color = color;        
    }

    public void FlashEffect(float FadeTime)
    { 
        EffectTime += 1 / FadeTime / 10;
        Color color = FlashImage.GetComponent<Image>().color;
        color.a = Mathf.Lerp(1, 0, EffectTime);
        FlashImage.GetComponent<Image>().color = color;        
    }

    public void FadeOutEffect(float FadeTime)
    { 
        EffectTime += 1 / FadeTime / 10;
        Color color = EffectImage.GetComponent<Image>().color;
        color.a = Mathf.Lerp(0, 1, EffectTime);
        EffectImage.GetComponent<Image>().color = color;        
    }

    IEnumerator SetFlash(float FlashTime)
    { 
        if(FlashImage.GetComponent<Image>().color.a > EffectAlphaMin)
        { 
            FlashEffect(FlashTime);
            yield return new WaitForSeconds(0.1f);
            StartCoroutine(SetFlash(FlashTime)); 
        }
        else
        { 
            Color color = FlashImage.GetComponent<Image>().color;
            color.a = 0f;
            FlashImage.GetComponent<Image>().color = color;
            EffectTime = 0;
            EffectEnd();            
        }         
    }

    IEnumerator ChangeBackgroundByMoveLeft()
    { 
        if(EffectImage.GetComponent<RectTransform>().anchoredPosition.x >= -1270)
        { 
            //EffectImage.GetComponent<RectTransform>().anchoredPosition = new Vector3(EffectImage.GetComponent<RectTransform>().anchoredPosition.x - 5.0f, EffectImage.GetComponent<RectTransform>().anchoredPosition.y);
            //yield return new WaitForSeconds(0.01f);
            EffectTime += Time.deltaTime/BackgroundMoveSpeed;
            EffectImage.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(new Vector2(0, 0), new Vector2(-1280, 0), EffectTime);
            yield return null;
            StartCoroutine(ChangeBackgroundByMoveLeft()); 
        }
        else
        { 
            Color color = EffectImage.GetComponent<Image>().color;
            color.a = 0f;
            EffectImage.GetComponent<Image>().color = color;
            TalkManager.instance.OnTalkBackground();
            EffectEnd();
        }         
    }

    private void EffectEnd()
    { 
        Debug.Log("효과가 끝났습니다.");
        GameManager.instance.isEffect = false;
        //EffectImage.GetComponent<Image>().color = new Color(255/255f, 255/255f, 255/255f, 0f);
        //EffectCanvas.SetActive(false);
        EffectImage.GetComponent<Image>().sprite = GameManager.instance.GetWhite();
        EffectImage.GetComponent<RectTransform>().anchoredPosition = new Vector3(0,0);
        
        EffectTime = 0;
        if(GameManager.instance.IsFadeOut == false)
        { 
            EffectCanvas.GetComponent<Canvas>().sortingOrder = 5;
        }

        FlashImage.GetComponent<Image>().color = new Color(1, 1, 1, 0);

    }

    private void EffectStart()
    { 
        EffectTime = 0;
        GameManager.instance.isEffect = true;
        //EffectCanvas.SetActive(true);
    }

    public void ChangeEffectImageColor(string ChangeColor)
    { 
        if(ChangeColor == "Red")
        { 
            EffectImage.GetComponent<Image>().color = new Color(1, 0, 0, EffectImage.GetComponent<Image>().color.a);

        }
        else if(ChangeColor == "Black")
        { 
            EffectImage.GetComponent<Image>().color = new Color(0, 0, 0, EffectImage.GetComponent<Image>().color.a);

        }
        else if(ChangeColor == "White")
        { 
            EffectImage.GetComponent<Image>().color = new Color(1, 1, 1, EffectImage.GetComponent<Image>().color.a);
        }
        else if(ChangeColor == "Blue")
        { 
            EffectImage.GetComponent<Image>().color = new Color(0, 0, 1, EffectImage.GetComponent<Image>().color.a);
        }
        else if(ChangeColor == "Green")
        { 
            EffectImage.GetComponent<Image>().color = new Color(0, 1, 0, EffectImage.GetComponent<Image>().color.a);
        }
        else if(ChangeColor == "Yellow")
        { 
            EffectImage.GetComponent<Image>().color = new Color(1, 0.92f, 0.016f, EffectImage.GetComponent<Image>().color.a);
        }
        else
        {
            Color color;

            if(EffectImage.GetComponent<Image>().color.a <= EffectAlphaMin)
            {
                ColorUtility.TryParseHtmlString("#" + ChangeColor + "00", out color);
                EffectImage.GetComponent<Image>().color = color;    

                Color Alphacolor = EffectImage.GetComponent<Image>().color;
                Alphacolor.a = 0f;
                EffectImage.GetComponent<Image>().color = Alphacolor;                
            }
            else
            {            
                ColorUtility.TryParseHtmlString("#" + ChangeColor, out color);
                EffectImage.GetComponent<Image>().color = color;
            }
        }
        GameManager.instance.FadeColor = ChangeColor;
    }

    public void ChangeFlashImageColor(string ChangeColor)
    { 
        if(ChangeColor == "Red")
        { 
            FlashImage.GetComponent<Image>().color = new Color(1, 0, 0, FlashImage.GetComponent<Image>().color.a);

        }
        else if(ChangeColor == "Black")
        { 
            FlashImage.GetComponent<Image>().color = new Color(0, 0, 0, FlashImage.GetComponent<Image>().color.a);

        }
        else if(ChangeColor == "White")
        { 
            FlashImage.GetComponent<Image>().color = new Color(1, 1, 1, FlashImage.GetComponent<Image>().color.a);
        }
        else if(ChangeColor == "Blue")
        { 
            FlashImage.GetComponent<Image>().color = new Color(0, 0, 1, FlashImage.GetComponent<Image>().color.a);
        }
        else if(ChangeColor == "Green")
        { 
            FlashImage.GetComponent<Image>().color = new Color(0, 1, 0, FlashImage.GetComponent<Image>().color.a);
        }
        else if(ChangeColor == "Yellow")
        { 
            FlashImage.GetComponent<Image>().color = new Color(1, 0.92f, 0.016f, FlashImage.GetComponent<Image>().color.a);
        }
        else
        {
            Color color;

            if(FlashImage.GetComponent<Image>().color.a <= EffectAlphaMin)
            {
                ColorUtility.TryParseHtmlString("#" + ChangeColor + "00", out color);
                FlashImage.GetComponent<Image>().color = color;    

                Color Alphacolor = FlashImage.GetComponent<Image>().color;
                Alphacolor.a = 0f;
                FlashImage.GetComponent<Image>().color = Alphacolor;                
            }
            else
            {            
                ColorUtility.TryParseHtmlString("#" + ChangeColor, out color);
                FlashImage.GetComponent<Image>().color = color;
            }
        }
    }

    public void Flash(float FlashTime)
    { 
        Debug.Log("설정된 Flash의 시간은 " + FlashTime);
        EffectStart();
        Color color = FlashImage.GetComponent<Image>().color;
        color.a = 1.0f;
        FlashImage.GetComponent<Image>().color = color;
        StartCoroutine(SetFlash(FlashTime));
    }

    public void FadeIn(float FadeTime)
    { 
        EffectCanvas.GetComponent<Canvas>().sortingOrder = 5;
        GameManager.instance.IsFadeOut = false;      
        TalkManager.instance.OnTalkerImageBase();
        TalkManager.instance.OffTalkBackground();
        Debug.Log("설정된 페이드 인의 시간은 " + FadeTime);
        EffectStart();
        StartCoroutine(SetFadeIn(FadeTime)); 
    }

    public void FadeOut(float FadeTime)
    {
        TalkManager.instance.OffTalkBackground();
        Debug.Log("설정된 페이드 아웃의 시간은 " + FadeTime);
        EffectStart();
        StartCoroutine(SetFadeOut(FadeTime)); 
    }

    public void BackgroundMoveLeft(Sprite MoveSprite)
    { 
        EffectStart();
        EffectCanvas.GetComponent<Canvas>().sortingOrder = 1;
        EffectImage.GetComponent<Image>().sprite = MoveSprite; 
        EffectImage.GetComponent<Image>().color = new Color(1,1,1,1);
        StartCoroutine(ChangeBackgroundByMoveLeft());         
    }

    public void ReturnToFade()
    { 
        ChangeEffectImageColor(GameManager.instance.FadeColor);
        TalkManager.instance.OffTalkerImageBase();
        Color color = EffectImage.GetComponent<Image>().color;
        color.a = 1.0f;
        EffectImage.GetComponent<Image>().color = color;
        EffectCanvas.GetComponent<Canvas>().sortingOrder = 1;
    }

    public void RemoveFade()
    { 
        Color color = EffectImage.GetComponent<Image>().color;
        color.a = 0;
        TalkManager.instance.OnTalkerImageBase();
        EffectImage.GetComponent<Image>().color = color;
        EffectCanvas.GetComponent<Canvas>().sortingOrder = 5;        
    }

    public void EndingCredit()
    { 
        EffectStart();
        EffectImage.GetComponent<Image>().color = new Color(0, 0, 0, EffectImage.GetComponent<Image>().color.a);
        StartCoroutine(SetFadeOutForEnding()); 
        EndingCreditImage.SetActive(true);
    }

    public void RemoveEndingCredit()
    { 
        EndingCreditImage.SetActive(false);
        EndingCreditImage.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,0);
    }

    IEnumerator SetFadeOutForEnding()
    { 
        if(EffectImage.GetComponent<Image>().color.a < EffectAlphaMax)
        { 
            EffectTime += Time.deltaTime/BackgroundMoveSpeed;
            Color color = EffectImage.GetComponent<Image>().color;
            color.a = Mathf.Lerp(0, 1, EffectTime); 
            EffectImage.GetComponent<Image>().color = color;
            yield return null;
            StartCoroutine(SetFadeOutForEnding()); 
        }
        else
        { 
            Color color = EffectImage.GetComponent<Image>().color;
            color.a = 1.0f;
            EffectImage.GetComponent<Image>().color = color;
            EffectTime = 0;
            StartCoroutine(SetEndingCredit());
        }        
    }

    IEnumerator SetEndingCredit()
    { 
        if(EndingCreditImage.GetComponent<RectTransform>().anchoredPosition.y < 3190)
        { 
            EffectTime += Time.deltaTime/EndingCreditTime;
            EndingCreditImage.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(new Vector2(0, 0), new Vector2(0, 3200), EffectTime);
            yield return null;
            StartCoroutine(SetEndingCredit()); 
        }
        else
        { 

            Color color = EffectImage.GetComponent<Image>().color;
            color.a = 0f;
            EffectImage.GetComponent<Image>().color = color;
            EffectEnd();
        }           
    } 

}
