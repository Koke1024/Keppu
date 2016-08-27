using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour {
    static public SkillManager I { get { return instance; } }
    static SkillManager instance;
    public Character chara;
    void Awake() { instance = this; }
	public GameObject skillPanelModel;
	public GameObject skillBoardModel;
	public GameObject skillBoard;
	Dictionary<int, GameObject> panelList = new Dictionary<int, GameObject>();

	void Update() {
		if (skillBoard != null) {
			if (skillBoard.transform.localPosition.y < -115) {
				skillBoard.transform.localPosition =
					new Vector3(
						skillBoard.transform.localPosition.x,
						-115);
			}
			if (skillBoard.transform.localPosition.y > 150 * panelList.Count - 950) {
				skillBoard.transform.localPosition =
					new Vector3(
						skillBoard.transform.localPosition.x,
						150 * panelList.Count - 950);
			}
		}
    }

	// Use this for initialization
    void Start() {
	}

    public void Init(bool b) {
		if (skillBoard) {
			Destroy(skillBoard);
			skillBoard = null;
		}
		if (!b) {
			return;
		}
		skillBoard = Instantiate(skillBoardModel);
		skillBoard.transform.SetParent(transform);
		skillBoard.transform.localScale = new Vector3(1, 1, 1);
		skillBoard.transform.localPosition = new Vector3(0, 0);
		foreach(KeyValuePair<int, GameObject> panel in panelList) {
			Destroy(panel.Value);
		}
		panelList.Clear();
		int i = 0;
        foreach (SkillList.Param skill in Excel<SkillList>.Item.list) {
			if (skill.Red == 0 && skill.Blue == 0 && skill.Green == 0 && skill.Yellow == 0 && skill.Black == 0) {
				++i;
				continue;
			}
			GameObject panel = Instantiate(skillPanelModel);
			panelList.Add(i, panel);
			++i;
        }
		Reset();
	}

	public void Reset() {
		int i = 0;
		foreach (KeyValuePair<int, GameObject> panel in panelList) {
			bool enableGet = true;
			bool own = false;
			SkillList.Param skill = GetSkillInfo(panel.Key);
			if (chara.parameter.Mana[Mana.RED] < skill.Red ||
				chara.parameter.Mana[Mana.BLUE] < skill.Blue ||
				chara.parameter.Mana[Mana.GREEN] < skill.Green ||
				chara.parameter.Mana[Mana.YELLOW] < skill.Yellow ||
				chara.parameter.Mana[Mana.BLACK] < skill.Black) {
				enableGet = false;
			}
			if (skill.RequireSkill != 0) {
				if (!chara.parameter.Skills.ContainsKey(GetSkillInfo(skill.RequireSkill).Key)) {
					panel.Value.GetComponent<SkillPanel>().text.text = GetSkillInfo(skill.RequireSkill).Name + "が必要";
					enableGet = false;
				}
			}
			if (chara.parameter.Skills.ContainsKey(skill.Key)) {
				own = true;
				enableGet = false;
			}
			panel.Value.GetComponent<SkillPanel>().Init(i, skill, chara, enableGet, own);
			++i;
		}
	}

    public SkillList.Param GetSkillInfo(int skillCode) {
        //スキルコードからスキル情報構造体を取得する
        SkillList.Param ret = Excel<SkillList>.Item.list.FindLast(o => o.ID == skillCode);
        if (ret == null) {
            Debug.Log("No Skill");
        }
        return ret;
    }

    public SkillList.Param GetSkillInfo(string key) {
        //スキルキーからスキル情報構造体を取得する
        SkillList.Param ret = Excel<SkillList>.Item.list.FindLast(o => o.Key == key);
        if (ret == null) {
            Debug.Log("No Skill");
        }
        return ret;
    }

    public void SwitchWindow() {
		bool b = TextManager.I.active;
		Init(b);
		TextManager.I.active = !b;
		TextManager.I.waiting = b;
		GetComponent<Image>().enabled = b;
		//ScenarioManager.I.textBox.SetActive(!b);
	}
}