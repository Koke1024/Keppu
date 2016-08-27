using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Mana : MonoBehaviour {
    public const int RED = 0;
    public const int BLUE = 1;
    public const int GREEN = 2;
    public const int YELLOW = 3;
    public const int BLACK = 4;
    public const int MAX = 5;
    public static readonly string[] paramKey = { "RED", "BLUE", "GREEN", "YELLOW", "BLACK" };
    public static readonly string[] paramString = { "赤", "青", "緑", "黄", "黒" };
    public Text[] manaText;

    public void SetManaText(int manaID, int val) {
        manaText[manaID].text = val.ToString();
    }

    public void SetManaText(int manaID, int val, Color color) {
        manaText[manaID].text = val.ToString();
        manaText[manaID].color = color;
    }
}
