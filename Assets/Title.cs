using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Title : MonoBehaviour {
	public GameObject titleText;
	public GameObject[] button;
	[SerializeField]
	GameObject startButton;

	InputText nameField = null;

	string charaName = "";

	// Use this for initialization
	void Start () {
		StartCoroutine("TextMove");

		Dictionary<int, Character.SuccessCharaParam> list = Character.LoadList();
		foreach (KeyValuePair<int, Character.SuccessCharaParam> param in list) {
			Debug.Log(param.Value.ID + ":" + param.Value.Name);
        }
    }

	IEnumerator TextMove() {
		while (titleText.transform.localPosition.y > 200) {
			titleText.transform.Translate(0, -2, 0);
			yield return null;
		}
    }
	
	// Update is called once per frame
	void Update () {
	}

	public void ShowNameField() {
		if (charaName.Length > 0) {
			//入力済みならゲーム開始
			Container.Set("CharaName", charaName);
			Application.LoadLevel("Scenario");
		} else if(!nameField) {
			//入力フィールドを作る
			int id = Random.Range(0, Excel<NameList>.Item.list.Count);
            string name = Excel<NameList>.Item.list[id].NAME;
			charaName = name;
			//nameField = InputText.Create(SetCharaName, "名前", new Vector2(400, 640));
			nameField = InputText.Create(SetCharaName, name, "名前", new Vector2(400, 640));
			GameObject.Find("Buttons").SetActive(false);
			startButton.SetActive(true);
		}
	}

	public void SetCharaName(string str) {
		charaName = str;
    }

	public void StartNewScenario() {
        if (charaName.Length > 0) {
		}
	}

	public void LoadScenario() {
		Container.Set("Load", 1);
		Application.LoadLevel("Scenario");
	}

	public void Option() {
		Character.ClearRegisterCharacter();
		Debug.Log("全消去");
	}

	public void Exit() {
		Application.Quit();
		Debug.Log("実質終了");
	}

	public void StartRotate() {
		StartCoroutine(Rotate());
	}

	IEnumerator Rotate() {
		for(int i = 0; i < 30; ++i) {
			transform.Rotate(new Vector3(0, 0, 1), -90.0f / 30.0f);
			for(int t = 0; t < 4; ++t) {
				button[t].transform.Rotate(new Vector3(0, 0, 1), 90.0f / 30.0f);
            }
			yield return null;
		}
	}
}
