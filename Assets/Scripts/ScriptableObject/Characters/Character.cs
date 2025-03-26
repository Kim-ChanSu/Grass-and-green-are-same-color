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
    //ǥ�� �������� �迭 �־ ���ڷ� �����ص� ��
}
