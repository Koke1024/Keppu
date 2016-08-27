using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OutlineText : MonoBehaviour {
    public GameObject[] texts;

    public void Init(string _text, int fontSize, int weight, Color fontColor, Color outlineColor) {
        int[] x = new int[5] { weight, 0, -weight, 0, 0 };
        int[] y = new int[5] { 0, weight, 0, -weight, 0 };
        for (int i = 0; i < 5; ++i) {
            texts[i].transform.localPosition = new Vector3(x[i], y[i]);
            texts[i].GetComponent<Text>().fontSize = fontSize;
            texts[i].GetComponent<Text>().text = _text;
            texts[i].GetComponent<Text>().color = outlineColor;
        }
        texts[4].GetComponent<Text>().color = fontColor;
    }

    public void SetColor(Color fontColor, Color outlineColor) {
        for (int i = 0; i < 5; ++i) {
            texts[i].GetComponent<Text>().color = outlineColor;
        }
        texts[4].GetComponent<Text>().color = fontColor;
    }
}
