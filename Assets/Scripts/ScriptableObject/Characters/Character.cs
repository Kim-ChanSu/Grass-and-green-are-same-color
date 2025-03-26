using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character")]
public class Character : ScriptableObject
{
    public string Name;
    public string KoreanName;
    public Sprite Face;
    public float TalkSpeed;
    //표정 넣을꺼면 배열 넣어서 숫자로 구별해도 됨
}
