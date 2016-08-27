using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Bench : MonoBehaviour {
    public GameObject[] benchChara;

    public void Activate() {
        int i = 0;
        foreach (Chara chara in BattleManager.I.benchCharas) {
            benchChara[i].GetComponent<Image>().sprite = chara.charaButton.GetComponent<Image>().sprite;
            ++i;
        }
    }
}
