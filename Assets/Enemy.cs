using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;
using System.Linq;

public class Enemy : Fighter {
	bool[] enableSkillFlg = new bool[4];
    public override UnityAction<BaseEventData> SetAsTarget() {
        return (BaseEventData eventData) => {
            BattleManager.I.TargetEnemy = this;
            BattleManager.I.ShowCharaWindow(this);
        };
    }

    void Start() {
    }
	
	// Update is called once per frame
	void Update () {
	}

    public IEnumerator Act() {
		for(int i = 0; i < 4; ++i) {
			if (enableSkillFlg[i]) {
				yield return StartCoroutine("UseSkill", i);
				break;
			}
		}
	}

	public void Init(FighterData _data) {
		data = _data;
		lifeGauge.Init(data.maxLife, data.life);
		_data.skillList.CopyTo(data.skillList, 0);
		Debug.Log(data.skillList[0] + "," + data.skillList[1] + "," + data.skillList[2] + "," + data.skillList[3]);

		charaButton.GetComponent<Image>().sprite = GetFullSprite();

		EventTrigger.Entry entry;
		EventTrigger charaButtonTrigger = charaButton.GetComponent<EventTrigger>();
		entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerEnter;
		entry.callback.AddListener(SetAsTarget());
		charaButtonTrigger.triggers.Add(entry);

		//離れたら消去
		entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerExit;
		entry.callback.AddListener((
			BaseEventData eventData) =>
		{
			downChara = null;
			downskillID = -1;
			BattleManager.I.ExitSkillWindow(this, -1);
		}
		);
		charaButtonTrigger.triggers.Add(entry);
	}

	/// <summary>
	/// 現在のマナ数に応じてスキルボタンの画像の明度を切り替える
	/// </summary>
	/// <param name="mana">それぞれのマナ個数</param>
	public void CheckMana(int[] mana) {
		int[] manas = GetMyManaCnt();
		enableAct = false;
		for (int i = 0; i < data.skillList.Length; ++i) {
			SkillList.Param skillInfo = SkillManager.I.GetSkillInfo(data.skillList[i]);
			bool enableUse = true;
			if (skillInfo.ID == CHANGE_SKILL_CODE && BattleManager.I.benchCharas.Count == 0) {
				enableUse = false;
			}
			if (GetBuffLevel(BUFF_WAIT) > 0) {
				charaButton.GetComponent<Image>().color = Color.gray;
				enableUse = false;
			}
			if (skillInfo.Active == 0) {
				enableUse = false;
			} else {
				for (int t = 0; t < Mana.MAX; ++t) {
					if (manas[t] < skillInfo.Require[t]) {
						enableUse = false;
						break;
					}
				}
				if (IsDead()) {
					enableUse = false;
				}
			}
			enableSkillFlg[i] = enableUse;
		}
	}
}
