using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Save;
using System.Collections;

[Serializable]
public class Character : MonoBehaviour {
    int[] stackStatus = new int[Mana.MAX];
    public Text[] mana;
    public Gauge gauge;
	RaderChart rader;

	[Serializable]
	public class SuccessCharaParam {
		override public string ToString() {
			return 
				"Name:" + Name 
				 + "\nStamina:" + Stamina + " / " + maxStamina
				 + "\nMana:" + Mana[0] + ", " + Mana[1] + ", " + Mana[2] + ", " + Mana[3] + ", " + Mana[4];
		}
		public int ID;
		public string Name;
		public int Stamina;
		public int maxStamina;
		public int[] Mana;
		public Dictionary<string, int> Skills;
		public Dictionary<string, int> Flags;
	};
	public SuccessCharaParam parameter;

	// Use this for initialization
    void Start() {
		/*
		GameObject prefab = Resources.Load<GameObject>("Prefab/RaderChart/Prefab");
		GameObject raderObject = Instantiate(prefab);
		rader = raderObject.GetComponent<RaderChart>();
		rader.gameObject.AddComponent<DebugOnly>();
		rader.Init(5, new float[5] { 1, 1, 1, 1, 1}, 100);
		rader.SetPosition(new Vector2(350, 550));
		raderObject.transform.SetParent(transform);
		*/
	}

	public void SetNewData() {
		Debug.Log("new data");
		parameter = new SuccessCharaParam();
		parameter.Mana = new int[Mana.MAX];
		parameter.Skills = new Dictionary<string, int>();
		parameter.Flags = new Dictionary<string, int>();
		parameter.Stamina = parameter.maxStamina = 100;
        parameter.ID = GetNewCharacterID();

		if (Container.container.ContainsKey("CharaName")) {
			parameter.Name = Container.Get<string>("CharaName");
		} else {
			parameter.Name = "デフォルトくん";
        }
		Debug.Log(parameter.Name);
        foreach (SkillList.Param param in Excel<SkillList>.Item.list) {
			int skillLevel = TextManager.I.GetVariable(param.Key);
			if (skillLevel > 0) {
				parameter.Skills.Add(param.Key, skillLevel);
				Debug.Log(param.Key + "習得済み" + skillLevel);
			}
		}
		Serialize.Save<int>("characterID", parameter.ID);
		SetLifeGauge();
	}

	public int GetNewCharacterID() {
		var characterList = LoadList();

		foreach (var key in characterList.Keys) {
			Debug.Log(key + ":" + characterList[key].Name);
		}
		for (int i = 0; ; ++i) {
			if (!characterList.ContainsKey(i)) {
				Debug.Log("new ID is " + i);
				return i;
			}
        }
	}

	public void Load(int id) {
		Debug.Log("load character " + id);
		parameter = new SuccessCharaParam();
		parameter.Mana = new int[Mana.MAX];
		parameter.Skills = new Dictionary<string, int>();
		parameter.Flags = new Dictionary<string, int>();

		Debug.Log("Load" + id);
		Dictionary<int, SuccessCharaParam> characterList;
		if (PlayerPrefs.HasKey("RegisterCharacterList")) {
			characterList = Serialize.Load<Dictionary<int, SuccessCharaParam>>("RegisterCharacterList");
		} else {
			characterList = new Dictionary<int, SuccessCharaParam>();
		}
		if (characterList.ContainsKey(id)) {
			parameter = characterList[id];
			Debug.Log("Load Success " + id);
			Debug.Log(parameter.ToString());

			TextManager.I.variables = Serialize.Load<Dictionary<string, int>>("Variable");
		} else {
			Debug.Log("There is not character data with " + id);
			parameter.maxStamina = parameter.Stamina = 100;	//todo
        }
		Serialize.Save<int>("characterID", parameter.ID);
		SetLifeGauge();
    }

	void SetLifeGauge() {
		Debug.Log(parameter.maxStamina + ", " + parameter.Stamina);
		gauge.Init(parameter.maxStamina, parameter.Stamina);
    }

	// Update is called once per frame
	void Update() {
	}

    public void StackParameter(int param, int val) {
        stackStatus[param] += val;
    }

    public void CommitParameter(bool pushText) {
		for (int i = 0; i < Mana.MAX; ++i) {
            if (stackStatus[i] == 0) {
				continue;
			}
			parameter.Mana[i] += stackStatus[i];
			if (parameter.Mana[i] < 0) {
				parameter.Mana[i] = 0;
			}
			string text;
			if (pushText) { 
				if (stackStatus[i] > 0) {
					text = Mana.paramString[i] + "が" + stackStatus[i] + "増えた";
				} else {
					text = Mana.paramString[i] + "が" + (-stackStatus[i]) + "減った";
				}
				TextManager.I.PushText(1, 0, 1, text, "Coin", "SetParameterText:" + i);
			}

            TextManager.I.AddVariable(Mana.paramKey[i], stackStatus[i]);
            stackStatus[i] = 0;
		}
		float[] chartValues = new float[Mana.MAX];
		float sum = 0;
		for (int i = 0; i < Mana.MAX; ++i) {
			sum += parameter.Mana[i];
        }
		if (sum > 0) {
			for (int i = 0; i < Mana.MAX; ++i) {
				chartValues[i] = parameter.Mana[i] / sum * 3;
			}
			//rader.UpdateValue(chartValues);
		}
    }

    public void SetStatusText(int manaID) {
        StartCoroutine("SetManaText", manaID);
    }

    public int AddStamina(int val) {
        int stamina = Mathf.Min(Mathf.Max(parameter.Stamina + val, 0), parameter.maxStamina);
        if (stamina < 30) {
            gauge.inner.GetComponent<Image>().color = Color.red;
        } else {
            gauge.inner.GetComponent<Image>().color = Color.white;
        }
        int delta = stamina - parameter.Stamina;

		if (delta != 0) {
			string text = "";
			if (delta > 0) {
				text = "体力が" + delta + "増えた";
			} else if(delta < 0) {
				text = "体力が" + (-delta) + "減った";
			}
			TextManager.I.PushText(1, 0, 1, text, null, null);
		}

        gauge.SetNowValue(stamina);
		parameter.Stamina = stamina;
		TextManager.I.SetVariable("Stamina", parameter.Stamina);
        return delta;
	}

	/// <summary>
	/// ポイント消費無しで直接スキル習得
	/// </summary>
	/// <param name="key"></param>
	public void GetSkillDirect(string key) {
		if (parameter.Skills.ContainsKey(key)) {
			parameter.Skills[key] += 1;
			//TextManager.I.PushText(1, 0, 1, param.Name + "のレベルが" + parameter.Skills[key] + "になった", null, null);
		} else {
			parameter.Skills.Add(key, 1);
			//TextManager.I.PushText(1, 0, 1, param.Name + "を習得した", null, null);
		}
		TextManager.I.SetVariable(key, parameter.Skills[key]);
		//Sound.I.PlaySound("Levelup");
	}

	public void GetSkill(string key) {
        SkillList.Param param = Excel<SkillList>.Item.list.Find(o => o.Key == key);
        Debug.Log("GetSkill" + key);
        Debug.Log(param.Name);

        if(
            TextManager.I.GetVariable("RED") < param.Red ||
            TextManager.I.GetVariable("BLUE") < param.Blue ||
            TextManager.I.GetVariable("GREEN") < param.Green ||
            TextManager.I.GetVariable("YELLOW") < param.Yellow
            ) {
			//TextManager.I.PushText(1, 0, 1, "たりない", null, null);
			Sound.I.PlaySound("Heli");
            return;
        }
        
        StackParameter(Mana.RED, -param.Red);
        StackParameter(Mana.BLUE, -param.Blue);
        StackParameter(Mana.GREEN, -param.Green);
        StackParameter(Mana.YELLOW, -param.Yellow);
        if (parameter.Skills.ContainsKey(key)) {
            parameter.Skills[key] += 1;
            //TextManager.I.PushText(1, 0, 1, param.Name + "のレベルが" + parameter.Skills[key] + "になった", null, null);
        } else {
            parameter.Skills.Add(param.Key, 1);
            //TextManager.I.PushText(1, 0, 1, param.Name + "を習得した", null, null);
        }
        TextManager.I.SetVariable(key, parameter.Skills[key]);
        Sound.I.PlaySound("Levelup");
		CommitParameter(false);
		SkillManager.I.Reset();
		SetStatusText(Mana.RED);
		SetStatusText(Mana.BLUE);
		SetStatusText(Mana.GREEN);
		SetStatusText(Mana.YELLOW);
		SetStatusText(Mana.BLACK);
	}

    public Fighter.FighterData GetFighterData() {
        Fighter.FighterData fighterData = new Fighter.FighterData(0);
        //fighterData.charaType = 1;
        fighterData.key = "Player";
        fighterData.name = "主人公";
        fighterData.skillList = new int[4];
		fighterData.life = 300;
		fighterData.maxLife = 300;
		int i = 0;
        foreach (string skillKey in parameter.Skills.Keys) {
            SkillList.Param param = SkillManager.I.GetSkillInfo(skillKey);
            fighterData.skillList[i] = param.ID;
            ++i;
        }
        return fighterData;
	}

	public void Register() {
		Dictionary<int, SuccessCharaParam> characterList = new Dictionary<int, SuccessCharaParam>();
		if (PlayerPrefs.HasKey("RegisterCharacterList")) {
			characterList = Serialize.Load<Dictionary<int, SuccessCharaParam>>("RegisterCharacterList");
		}
		int count = characterList.Count;
		characterList.Add(count + 1, parameter);
		Serialize.Save("RegisterCharacterList", characterList);
		Serialize.Save("Variable", TextManager.I.variables);
		Application.LoadLevel("Title");
		Debug.Log("Register" + count + 1);
	}

	static public Dictionary<int, SuccessCharaParam> LoadList() {
		Debug.Log("LoadList");
		Dictionary<int, SuccessCharaParam> characterList;
		if (PlayerPrefs.HasKey("RegisterCharacterList")) {
			characterList = Serialize.Load<Dictionary<int, SuccessCharaParam>>("RegisterCharacterList");
			Debug.Log("characterList Count:" + characterList.Count);
		} else {
			characterList = new Dictionary<int, SuccessCharaParam>();
		}
		return characterList;
    }

	public static void ClearRegisterCharacter() {
		Debug.Log("ClearAll");
		PlayerPrefs.DeleteKey("RegisterCharacterList");
		PlayerPrefs.DeleteAll();
	}

	///<summary>マナを加算し、マナチェックメソッドを呼び出す</summary>
	public IEnumerator SetManaText(int manaID) {
		int i = manaID;
        int currentMana = int.Parse(mana[i].text);
		if (currentMana != parameter.Mana[i]) {
			mana[i].text = parameter.Mana[i].ToString();
            Sound.I.PlaySound("Phone");
			yield return new WaitForSeconds(1.0f);
		}
	}
}