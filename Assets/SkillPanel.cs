using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SkillPanel : MonoBehaviour {
	public Button button;
	public Image icon;
	public Text text;
	public GameObject scrollBoard;
	public SkillList.Param skill;

	public void Init(int i, SkillList.Param _skill, Character chara, bool enableGet, bool own) {
		skill = _skill;
        scrollBoard = GameObject.Find("ScrollBoard");
        int x = 0;
		int y = 450;
		transform.SetParent(scrollBoard.transform);
		transform.localScale = new Vector3(1, 1, 1);
		transform.localPosition = new Vector3(x, y + (-i * 150));
		text.text = skill.Name +
			"\n赤" + skill.Red +
			" 青" + skill.Blue +
			" 緑" + skill.Green +
			" 黄" + skill.Yellow +
			" 黒" + skill.Black;

		icon.sprite =
			Resources.Load<Sprite>("Skill/" + skill.Key);
		
		if (enableGet) {
			Button.ButtonClickedEvent clickEvent = new Button.ButtonClickedEvent();
			clickEvent.AddListener(() => {
				chara.GetSkill(skill.Key);
			});
			GetComponent<SkillPanel>().button.onClick = clickEvent;
			GetComponent<SkillPanel>().icon.color = Color.white;
			GetComponent<SkillPanel>().button.enabled = true;
		} else if (own) {
			GetComponent<SkillPanel>().icon.color = Color.gray;
			GetComponent<SkillPanel>().button.enabled = false;
			GetComponent<SkillPanel>().text.text = "習得済み";
		} else {
			GetComponent<SkillPanel>().icon.color = Color.gray;
			GetComponent<SkillPanel>().button.enabled = false;
		}
	}

	public void SetEnable(bool enableGet, Character chara) {
		string key = skill.Key;
		if (enableGet) {
			Button.ButtonClickedEvent clickEvent = new Button.ButtonClickedEvent();
			clickEvent.AddListener(() => {
				chara.GetSkill(key);
			});
			button.onClick = clickEvent;
		} else {
			icon.color = Color.gray;
		}
	}
}
