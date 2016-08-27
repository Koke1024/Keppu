using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;

public class Chara : Fighter {
    public GameObject[] skillButtonList;

    public override UnityAction<BaseEventData> SetAsTarget() {
        return (BaseEventData eventData) => {
            BattleManager.I.TargetFriend = this;
            BattleManager.I.ShowCharaWindow(this);
        };
    }

    public void Init(FighterData _data) {
		data = _data;
		lifeGauge.Init(data.maxLife, data.life);
        int[] arySkillID = data.skillList;
        EventTrigger.Entry entry;
        for (int i = 0; i < SET_SKILL_MAX; ++i) {
            if (i >= arySkillID.Length || data.skillList[i] == CHANGE_SKILL_CODE) {
				if(i == SET_SKILL_MAX - 1 && BattleManager.I.benchCharas.Count > 0) {
					data.skillList[i] = CHANGE_SKILL_CODE;
//					Debug.Log("change");
				} else {
					skillButtonList[i].SetActive(false);
//					Debug.Log("false");
					continue;
				}
			} else {
                data.skillList[i] = arySkillID[i];
            }
//            Debug.Log(data.skillList[i]);
            SkillList.Param skillInfo = SkillManager.I.GetSkillInfo(data.skillList[i]);

            EventTrigger trigger = skillButtonList[i].GetComponent<EventTrigger>();
            int skillID = i;
            //ホバー時情報表示
            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((BaseEventData eventData) => BattleManager.I.ShowSkillWindow(this, skillID));
            trigger.triggers.Add(entry);

            //離したら情報消去
            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerUp;
            entry.callback.AddListener(
                (BaseEventData eventData) => {
                    if (downChara == this && downskillID == skillID) {
                        BattleManager.I.ConfirmSkill();
                    }
                }
            );
            trigger.triggers.Add(entry);

            //離れたら消去
            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerExit;
            entry.callback.AddListener((BaseEventData eventData) => {
                downChara = null;
                downskillID = -1;
                BattleManager.I.ExitSkillWindow(this, skillID);
            });
            trigger.triggers.Add(entry);

            //押したものを登録
            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener((BaseEventData eventData) => {
                downChara = this;
                downskillID = skillID;
                BattleManager.I.ShowSkillWindow(this, skillID);
            });
            trigger.triggers.Add(entry);

            skillButtonList[i].GetComponent<Image>().sprite =
                Resources.Load<Sprite>("Skill/" + skillInfo.Key);
            //Debug.Log("Skill/" + skillInfo.Key);
        }
        EventTrigger charaButtonTrigger = charaButton.GetComponent<EventTrigger>();
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener(SetAsTarget());
        charaButtonTrigger.triggers.Add(entry);

        //離れたら消去
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerExit;
        entry.callback.AddListener((
            BaseEventData eventData) => {
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
        int[] manas = Chara.manaCnt;
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
            if (enableUse) {
                skillButtonList[i].GetComponent<Image>().color = Color.white;
                enableAct = true;
            } else {
                skillButtonList[i].GetComponent<Image>().color = Color.gray;
            }
        }
        if (GetBuffLevel(BUFF_WAIT) > 0 || IsDead()) {
            charaButton.GetComponent<Image>().color = Color.gray;
        } else {
            charaButton.GetComponent<Image>().color = Color.white;
        }
    }
}
