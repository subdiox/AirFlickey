using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.WSA.Input;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using TMPro;

public class FlickKeyboard : MonoBehaviour
{
    public TMP_Text textField;
    public GameObject keyA;
    public GameObject keyKA;
    public GameObject keySA;
    public GameObject keyTA;
    public GameObject keyNA;
    public GameObject keyHA;
    public GameObject keyMA;
    public GameObject keyYA;
    public GameObject keyRA;
    public GameObject keyWA;
    public GameObject keyMark;
    public GameObject keyFunc;
    public GameObject keyDelete;
    public GameObject keySpace;
    public GameObject keyReturn;
    public AudioClip buttonPress;
    public AudioClip buttonUnpress;

    private AudioSource audioSource;
    private GameObject pressedKey;
    private Vector3 pressedPosition3;
    private Vector3 releasedPosition3;
    private string[] charListA = new string[] { "あ", "あ", "い", "う", "え", "お" };
    private string[] charListKA = new string[] { "か", "か", "き", "く", "け", "こ" };
    private string[] charListSA = new string[] { "さ", "さ", "し", "す", "せ", "そ" };
    private string[] charListTA = new string[] { "た", "た", "ち", "つ", "て", "と" };
    private string[] charListNA = new string[] { "な", "な", "に", "ぬ", "ね", "の" };
    private string[] charListHA = new string[] { "は", "は", "ひ", "ふ", "へ", "ほ" };
    private string[] charListMA = new string[] { "ま", "ま", "み", "む", "め", "も" };
    private string[] charListYA = new string[] { "や", "や", "「", "ゆ", "」", "よ" };
    private string[] charListRA = new string[] { "ら", "ら", "り", "る", "れ", "ろ" };
    private string[] charListWA = new string[] { "わ", "わ", "を", "ん", "ー", "わ" };
    private string[] charListMark = new string[] { "記号", "、", "。", "？", "！", "、" };
    private string[][] charListFunc = new string[][] {
        new string[] { "か", "が", null },
        new string[] { "き", "ぎ", null },
        new string[] { "く", "ぐ", null },
        new string[] { "け", "げ", null },
        new string[] { "こ", "ご", null },
        new string[] { "さ", "ざ", null },
        new string[] { "し", "じ", null },
        new string[] { "す", "ず", null },
        new string[] { "せ", "ぜ", null },
        new string[] { "そ", "ぞ", null },
        new string[] { "た", "だ", null },
        new string[] { "ち", "ぢ", null },
        new string[] { "つ", "っ", "づ" },
        new string[] { "て", "で", null },
        new string[] { "と", "ど", null },
        new string[] { "は", "ば", "ぱ" },
        new string[] { "ひ", "び", "ぴ" },
        new string[] { "ふ", "ぶ", "ぷ" },
        new string[] { "へ", "べ", "ぺ" },
        new string[] { "ほ", "ぼ", "ぽ" },
        new string[] { "や", "ゃ", null },
        new string[] { "ゆ", "ゅ", null },
        new string[] { "よ", "ょ", null }
    };

    // Start is called before the first frame update
    void Start()
    {
        InteractionManager.InteractionSourceDetected += SourceDetected;
        InteractionManager.InteractionSourceUpdated += SourceUpdated;
        InteractionManager.InteractionSourceLost += SourceLost;
        InteractionManager.InteractionSourcePressed += SourcePressed;
        InteractionManager.InteractionSourceReleased += SourceReleased;
        audioSource = GetComponent<AudioSource>();
    }

    void SourceDetected(InteractionSourceDetectedEventArgs eventArgs)
    {
        Debug.Log("SourceDetected");
    }

    void SourceUpdated(InteractionSourceUpdatedEventArgs eventArgs)
    {
        Debug.Log("SourceUpdated");
        if (pressedKey != null)
        {
            eventArgs.state.sourcePose.TryGetPosition(out releasedPosition3);
            pressedKey.GetComponentInChildren<TMP_Text>().text = GetNextChar(GetCurrentDirection());
        }
    }

    void SourceLost(InteractionSourceLostEventArgs eventArgs)
    {
        Debug.Log("SourceLost");
        if (pressedKey != null)
        {
            eventArgs.state.sourcePose.TryGetPosition(out releasedPosition3);
            ProcessInput(GetCurrentDirection());
        }
    }

    void SourcePressed(InteractionSourcePressedEventArgs eventArgs)
    {
        Debug.Log($"SourcePressed {CoreServices.InputSystem.GazeProvider.GazeTarget}");
        pressedKey = CoreServices.InputSystem.GazeProvider.GazeTarget;
        eventArgs.state.sourcePose.TryGetPosition(out pressedPosition3);
        audioSource.PlayOneShot(buttonPress, 0.7F);
    }

    void SourceReleased(InteractionSourceReleasedEventArgs eventArgs)
    {
        Debug.Log($"SourceReleased {pressedKey.name}");
        eventArgs.state.sourcePose.TryGetPosition(out releasedPosition3);
        ProcessInput(GetCurrentDirection());
    }

    /*
       現在のフリック方向(direction)を取得する関数
       [direction]
        0 : 現在位置がキーをタップした位置から0.05未満しかずれていないとき
        1 : 現在位置がキーをタップした位置から左方向にずれたと判定されるとき
        2 : 現在位置がキーをタップした位置から上方向にずれたと判定されるとき
        3 : 現在位置がキーをタップした位置から右方向にずれたと判定されるとき
        4 : 現在位置がキーをタップした位置から下方向にずれたと判定されるとき
    */
    int GetCurrentDirection()
    {
        int direction = -1;
        if (pressedKey != null && pressedPosition3 != null && releasedPosition3 != null)
        {
            Vector2 relativePosition2 = releasedPosition3 - pressedPosition3;
            float angle = Vector2.Angle(Vector2.right, relativePosition2);
            Debug.Log($"relativePosition magnitude: {relativePosition2.magnitude}");
            if (relativePosition2.magnitude < 0.02) // フリックしなかったと判定する基準
            {
                direction = 0;
            }
            else if (angle < 45)
            {
                Debug.Log("Direction: right");
                direction = 3;
            }
            else if (angle > 135)
            {
                Debug.Log("Direction: left");
                direction = 1;
            }
            else if (relativePosition2.y > 0)
            {
                Debug.Log("Direction: up");
                direction = 2;
            }
            else
            {
                Debug.Log("Direction: down");
                direction = 4;
            }
        }
        return direction;
    }

    /*
       この状態(direction)で放した場合に入力される文字を返す関数
       [direction]
       -1 : 初期状態で表示されていた文字列を返す
        0 : キーをタップした位置で放したときに入力される文字を返す
        1 : キーをタップして左にフリックして放したときに入力される文字を返す
        2 : キーをタップして上にフリックして放したときに入力される文字を返す
        3 : キーをタップして右にフリックして放したときに入力される文字を返す
        4 : キーをタップして下にフリックして放したときに入力される文字を返す
    */
    string GetNextChar(int direction)
    {
        if (direction >= -1 && direction <= 4)
        {
            if (pressedKey == keyA)
            {
                return charListA[direction + 1];
            }
            else if (pressedKey == keyKA)
            {
                return charListKA[direction + 1];
            }
            else if (pressedKey == keySA)
            {
                return charListSA[direction + 1];
            }
            else if (pressedKey == keyTA)
            {
                return charListTA[direction + 1];
            }
            else if (pressedKey == keyNA)
            {
                return charListNA[direction + 1];
            }
            else if (pressedKey == keyHA)
            {
                return charListHA[direction + 1];
            }
            else if (pressedKey == keyMA)
            {
                return charListMA[direction + 1];
            }
            else if (pressedKey == keyYA)
            {
                return charListYA[direction + 1];
            }
            else if (pressedKey == keyRA)
            {
                return charListRA[direction + 1];
            }
            else if (pressedKey == keyWA)
            {
                return charListWA[direction + 1];
            }
            else if (pressedKey == keyMark)
            {
                return charListMark[direction + 1];
            }
        }
        return pressedKey.GetComponentInChildren<TMP_Text>().text;
    }

    void ProcessInput(int direction)
    {
        if (pressedKey == keySpace)
        {
            textField.text += " ";
        }
        else if (pressedKey == keyDelete)
        {
            if (!string.IsNullOrEmpty(textField.text))
            {
                textField.text = textField.text.Substring(0, textField.text.Length - 1);
            }
        }
        else if (pressedKey == keyReturn)
        {
            textField.text += "\n";
        }
        else if (pressedKey == keyFunc)
        {
            string targetChar = textField.text.Substring(textField.text.Length - 1, 1);
            for (int i = 0; i < charListFunc.Length; i ++)
            {
                for (int j = 0; j < 3; j ++)
                {
                    if (charListFunc[i][j] == targetChar)
                    {
                        int k = (j + 1) % 3;
                        while (charListFunc[i][k] == null)
                        {
                            k = (k + 1) % 3;
                        }
                        textField.text = textField.text.Substring(0, textField.text.Length - 1) + charListFunc[i][k];
                        return;
                    }
                }
            }
        }
        else
        {
            string nextChar = GetNextChar(direction);
            if (nextChar != null)
            {
                textField.text += nextChar;
            }
        }
        pressedKey.GetComponentInChildren<TMP_Text>().text = GetNextChar(-1);
        pressedKey = null;
        audioSource.PlayOneShot(buttonUnpress, 0.7F);
    }
}
