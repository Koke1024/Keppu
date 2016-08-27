using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class Fighter : MonoBehaviour {
    static public Fighter downChara;
    static public int downskillID;
	public bool enableAct;

	static public int[] manaCnt = new int[5];
	static public int[] enemyManaCnt = new int[5];
    static public int[] consumeManaTotal;
    static public Mana mana;
    static public Mana enemyMana;

    public const int TARGET_SOLO = 1;
    public const int TARGET_ALL = 2;
    public const int TARGET_SELF = 3;
    public const int TARGET_FRIEND = 4;
    public const int TARGET_RANDOM = 5;

    public const int BUFF_WAIT = 1;
    public const int BUFF_ARMOR = 2;
    public const int BUFF_DEFENSE_DOWN = 3;
    public const int BUFF_ROCK = 4;
    public const int BUFF_FREEZE = 5;
    public const int BUFF_BLIZZARD = 6;
    public const int BUFF_REGENERATION = 7;
    public const int BUFF_POISON = 8;
	public const int BUFF_FULL_POWER = 9;
	public const int BUFF_CURSE = 10;

	public const int ATK_TYPE_NORMAL = 1;
    public const int ATK_TYPE_MAGIC = 2;
    public const int ATK_TYPE_SPECIAL = 3;

    public const string PSV_CRITICAL_ARROW = "CriticalArrow";
    public const string PSV_HOLY_ARMOR = "HolyArmor";
    public const string PSV_DEFENDER = "Defender";

    public abstract UnityAction<BaseEventData> SetAsTarget();

    public static bool isAttacking = false;

    public const int CHANGE_SKILL_CODE = 0;
    public const int SET_SKILL_MAX = 5;

    public Gauge lifeGauge;
    public GameObject charaButton;
    public struct StrAttack {
        public Fighter from;
        public float pow;
        public int type;
        public int target;
        public string targetTag;
        public int buffID;
        public int buffLevel;
        public int buffTurn;
    }

    //buffID, Level, Turn
    public struct StrBuffInfo {
        public int ID;
        public int Turn;
        public int Level;
    }
    public List<StrBuffInfo> buffs = new List<StrBuffInfo>();

	[Serializable]
    public class FighterData {
        public string name;      
        public string key;
        public int[] skillList;

		public int maxLife;
		public int life;
		public int attack;

		public override string ToString() {
			return name + "[" + key + "]" + life + "/" + maxLife + " ATK:" + attack + "(" + skillList[0] + ", " + skillList[1] + ", " + skillList[2] + ", " + skillList[3] + ")";
		}

		public FighterData(FighterList.Param param) {
			name = param.Name;
			key = param.Key;
			maxLife = param.Life;
			life = param.Life;
			attack = param.Attack;
			skillList = BattleManager.GetCharaSkill(param.ID);
		}

		public FighterData(int ID) {
			Init(ID);
        }

		public FighterData(string _name, string _key, int _maxLife, int _life, int _attack) {
			name = _name;
			key = _key;
			skillList = null;
			maxLife = _maxLife;
			life = _life;
			attack = _attack;
		}

		void Init(int ID) {
			Debug.Log("FighterList::Init[" + ID + "]");
			FighterList.Param param = Excel<FighterList>.Item.list.Find(o => o.ID == ID);
			name = param.Name;
			key = param.Key;
			maxLife = param.Life;
			life = param.Life;
			attack = param.Attack;
			skillList = BattleManager.GetCharaSkill(param.ID);
			Debug.Log(this);
		}
	}

	public Sprite GetIconSprite() {
		Debug.Log(data.key);
		return Resources.Load<Sprite>("Chara/" + data.key + "/Icon");
	}

	public Sprite GetFullSprite() {
		return Resources.Load<Sprite>("Chara/" + data.key + "/Full");
	}

	public FighterData data = null;

    void Start() {
    }

    public StrAttack CreateAttack() {
        StrAttack attack = new StrAttack();
        attack.from = this;
        attack.pow = data.attack;
        attack.type = ATK_TYPE_NORMAL;
        attack.targetTag = GetEnemyTag();
        attack.target = TARGET_SOLO;
        if (HasPassive(PSV_CRITICAL_ARROW) > 0) {
            int addAtk = (GetMyManaCnt(Mana.YELLOW) / 10) * 5;
            attack.pow += addAtk;
        }
        if (GetBuffLevel(BUFF_FULL_POWER) > 0) {
            attack.pow *= 1.0f + ((GetBuffLevel(BUFF_FULL_POWER)) * 0.3f);
        }
        return attack;
	}

	public int[] GetMyManaCnt() {
		if (tag == "Chara") {
			return manaCnt;
		} else {
			return enemyManaCnt;
		}
	}

	public int GetMyManaCnt(int color) {
		if (tag == "Chara") {
			return manaCnt[color];
		} else {
			return enemyManaCnt[color];
		}
	}

	public int[] GetEnemyManaCnt() {
		if (tag == "Chara") {
			return enemyManaCnt;
		} else {
			return manaCnt;
		}
	}

	public int GetEnemyManaCnt(int color) {
		if (tag == "Chara") {
			return enemyManaCnt[color];
		} else {
			return manaCnt[color];
		}
	}

	public int HasPassive(string passiveKey) {
        if (data.skillList == null || IsDead()) {
            return 0;
        }
        foreach (int id in data.skillList) {
            SkillList.Param skillInfo = SkillManager.I.GetSkillInfo(id);
            if (skillInfo.Key == passiveKey) {
                for (int t = 0; t < Mana.MAX; ++t) {
                    if (GetMyManaCnt(t) < skillInfo.Require[t]) {
                        return 0;
                    }
                }
                //Debug.Log("スキル持ち[" + passiveKey + "]");
                return 1;
            }
        }
        return 0;
    }

    public IEnumerator Flash() {
        for (int i = 0; i < 3; ++i) {
            charaButton.GetComponent<Image>().color = Color.black;
            yield return new WaitForSeconds(0.1f);
            charaButton.GetComponent<Image>().color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
        yield break;
    }

    public void Damage(StrAttack attack) {
        if (IsDead()) {
            return;
        }
        StartCoroutine("Flash");
        int armor = GetBuffLevel(BUFF_ARMOR);
		if (armor > 0 && attack.type == ATK_TYPE_NORMAL) {
			attack.pow -= armor * 5;
			StrAttack atk = CreateAttack();
			atk.pow = armor * 5;
			atk.type = ATK_TYPE_SPECIAL;
			attack.from.Damage(atk);
		}
		if (GetBuffLevel(BUFF_CURSE) > 0 && attack.type != ATK_TYPE_SPECIAL) {
			StrAttack atk = CreateAttack();
			atk.pow = attack.pow;
			atk.type = ATK_TYPE_SPECIAL;
			attack.from.Damage(atk);
		}
		if (GetBuffLevel(BUFF_FREEZE) > 0) {
            Sound.I.PlaySound("Magic");
            attack.pow = 0;
        }
        if (GetBuffLevel(BUFF_DEFENSE_DOWN) > 0) {
            float rate = 1.0f + GetBuffLevel(BUFF_DEFENSE_DOWN) * 0.2f;
            attack.pow = (int)((float)attack.pow * rate);
        }
        if (HasPassive(PSV_HOLY_ARMOR) > 0 && attack.pow > 0) {
            if (GetMyManaCnt(Mana.YELLOW) >= 1) {
                attack.pow -= 10;
                Sound.I.PlaySound("Magic");
                BattleManager.I.ConsumeMana(tag, 0, 0, 0, 1, 0);
            }
        }
        if (attack.type == ATK_TYPE_NORMAL) {
            Fighter defender = BattleManager.I.FindPassiveOwner(PSV_DEFENDER, tag);
            if (defender != null && defender != this) {
                StrAttack atk = attack;
                atk.pow = (int)(atk.pow * 0.3f);
                atk.type = ATK_TYPE_SPECIAL;
                atk.buffID = 0;
                defender.Damage(atk);
                attack.pow = (int)(attack.pow * 0.7f);
                //Debug.Log("→" + attack.pow);
            }
        }
        if (attack.pow < 0) {
            attack.pow = 0;
        }
		data.life -= (int)(Mathf.Round(attack.pow));
//        Debug.Log(attack.pow + "ダメージ");
        if (attack.pow > 0) {
            if (attack.type == ATK_TYPE_NORMAL) {
                Sound.I.PlaySound("Slash");
//                Debug.Log(attack.from.name);
            } else if (attack.type == ATK_TYPE_MAGIC) {
                Sound.I.PlaySound("Magic");
                //Debug.Log(attack.from.name);
            }
        }
//        Debug.Log(attack.buffID);
        lifeGauge.SetNowValue(data.life);
        if (attack.buffID > 0) {
            AddBuff(attack.buffID, attack.buffLevel, attack.buffTurn);
        }
        if (data.life <= 0) {
            Dead();
        }
    }

    public void AddBuff(int buffID, int buffLevel, int buffTurn) {
        StrBuffInfo buff;
        if (GetBuffLevel(buffID) > 0) {
            BuffList.Param buffParam = Excel<BuffList>.Item.list.Find(o => o.BuffID == buffID);
            if (buffParam.CumulativeFlag > 1) {
                int index = buffs.FindIndex(o => o.ID == buffID);
                buff = buffs[index];
                buff.Turn = buffTurn;
                buff.Level += 1;
                buffs[index] = buff;
                return;
            }
            CureBuff(buffID);
        }
        buff = new StrBuffInfo();
        buff.ID = buffID;
        buff.Level = buffLevel;
        buff.Turn = buffTurn;
        buffs.Add(buff);
    }

    public IEnumerator CheckBuff(){
        Debug.Log(data.name + "CheckBuff");
        if (IsDead()) {
            yield break;
        }
        charaButton.GetComponent<Image>().color = Color.white;
        Debug.Log(buffs.Count);
        for(int i = 0; i < buffs.Count; ++i) {
            StrBuffInfo buffInfo = buffs[i];
            int buffLevel = buffInfo.Level;
            buffInfo.Turn -= 1;
            int buffTurn = buffInfo.Turn;
            buffs[i] = buffInfo;
            Debug.Log(buffs[i].Turn);
            //Debug.Log( + "さんの" + buffInfo.ID + "バフ(LV:" + buffLevel + ") あと" + buffTurn);

            if (buffInfo.ID == BUFF_BLIZZARD) {
                StrAttack attack = CreateAttack();
                attack.pow = 10 + buffInfo.Level * 5;
                attack.target = Chara.TARGET_ALL;
                attack.type = ATK_TYPE_MAGIC;
                BattleManager.I.PopMana(tag, 0, 3, 0, 0, 0);
                BattleManager.I.Attack(attack);
                BattleManager.I.StartCoroutine("SkillMessage", "氷嵐");
                AddBuff(BUFF_WAIT, 1, 1);
                yield return new WaitForSeconds(1);
            }

            if (buffInfo.ID == BUFF_REGENERATION) {
                Heal(buffLevel * 10, false);
                yield return new WaitForSeconds(1);
            }

            if (buffInfo.ID == BUFF_POISON) {
                StrAttack attack = CreateAttack();
                attack.pow = GetBuffLevel(BUFF_POISON) * 5;
                attack.type = ATK_TYPE_SPECIAL;
                Damage(attack);
                yield return new WaitForSeconds(1);
            }

            if (buffTurn <= 0) {
                if (buffInfo.ID == BUFF_ROCK) {
                    AddBuff(BUFF_WAIT, 1, 2);
                    Sound.I.PlaySound("Heli");
                    yield return new WaitForSeconds(1);
                }
            }
        }
        CureBuffAuto();
    }

    public int GetBuffLevel(int buffID) {
        if (!buffs.Exists(o => o.ID == buffID)) {
            return 0;
        }
        return buffs.FindLast(o => o.ID == buffID).Level;
    }

    public void CureBuff(int buffID) {
        buffs.RemoveAll(o => o.ID == buffID);
    }

    public void CureBuffAuto() {
        buffs.RemoveAll(o => o.Turn <= 0 || o.Level <= 0);
    }

    public void Dead() {
        charaButton.GetComponent<Image>().color = Color.gray;
		data.life = 0;
        gameObject.tag = "Dead" + gameObject.tag;
        Sound.I.PlaySound("Accident");
        StopAllCoroutines();
    }

    public void Heal(int healLife, bool relive) {
        if(!relive && IsDead()){
            return;
        }
        if (data.life == data.maxLife) {
            return;
        }
        Sound.I.PlaySound("Heal");
        charaButton.GetComponent<Image>().color = Color.white;
		data.life = data.life + healLife;
        if (data.life <= 0 || data.life > data.maxLife) {
			data.life = data.maxLife;
        }
        gameObject.tag = gameObject.tag.Replace("Dead", "");
        lifeGauge.SetNowValue(data.life);
    }

    string GetEnemyTag() {
        if (tag == "Enemy") {
            return "Chara";
        } else {
            return "Enemy";
        }
    }

    public bool IsDead() {
        return data.life <= 0;
    }

    public IEnumerator UseSkill(int skillID) {
		Debug.Log(data.life + "/" + data.maxLife);
        if (IsDead()) {
            yield break;
        }
        if (GetBuffLevel(BUFF_WAIT) > 0) {
            yield break;
        }
        while (true) {
            if (!isAttacking) {
                break;
            }
            yield return null;
        }
        SkillList.Param skillInfo = SkillManager.I.GetSkillInfo(data.skillList[skillID]);
        Debug.Log(skillInfo.Name);
        isAttacking = true;
        //charaButton.GetComponent<Image>().color = Color.yellow;
        if (skillInfo.Active == 1) {
            if (BattleManager.I.ConsumeMana(tag, skillInfo.Require)) {
                if (skillInfo.ID != CHANGE_SKILL_CODE) {
                    BattleManager.I.StartCoroutine("CharaEffect", data.key);
                    BattleManager.I.StartCoroutine("SkillMessage", skillInfo.Name);
                    AddBuff(BUFF_WAIT, 1, 1);
                }
                string skillFunction = "Skill" + skillInfo.Key;
                Debug.Log(skillFunction);
                yield return StartCoroutine(skillFunction);
                BattleManager.I.manaCheck(GetMyManaCnt());
            } else {
                Debug.Log(skillInfo.Name + " is not Ready");
                if (tag == "Chara") {
                    Sound.I.PlaySound("Heli");
                }
                isAttacking = false;
                yield break;
            }
        } else {
            Debug.Log("Non Active");
        }
        yield return new WaitForSeconds(1);
        isAttacking = false;
        BattleManager.I.CheckActiveChara();

		BattleManager.I.CheckDestroy();
	}

    /// <summary>
    /// 対象の防御力を下げる。効果は5回まで重複する

    /// </summary>
    IEnumerator SkillRockRain() {
        StrAttack attack = CreateAttack();
        attack.pow = 10;
        attack.target = Chara.TARGET_SOLO;
        attack.buffID = BUFF_DEFENSE_DOWN;
        attack.buffLevel = 1;
        attack.buffTurn = 3;
        BattleManager.I.Attack(attack);
        yield break;
    }

    IEnumerator SkillRockBreath() {
        StrAttack attack = CreateAttack();
        attack.pow = 10;
        attack.target = Chara.TARGET_SOLO;
        attack.buffID = BUFF_ROCK;
        attack.buffLevel = 1;
        attack.buffTurn = 1;
        BattleManager.I.Attack(attack);
        yield break;
    }

    IEnumerator SkillRockArmor() {
        AddBuff(BUFF_ARMOR, 1, 3);
        yield break;
    }

    IEnumerator SkillIceBurn() {
        StrAttack attack = CreateAttack();
        attack.pow = 15;
        attack.target = Chara.TARGET_SOLO;
        attack.type = ATK_TYPE_MAGIC;
        BattleManager.I.Attack(attack);
        BattleManager.I.PopMana(tag, 0, 3, 0, 0, 0);
        yield break;
    }

    IEnumerator SkillFreeze() {
        if (tag == "Chara") {
            if (BattleManager.I.lastTarget == null) {
                BattleManager.I.TargetEnemy.AddBuff(BUFF_FREEZE, 1, 3);
                BattleManager.I.TargetEnemy.AddBuff(BUFF_WAIT, 1, 1);
            } else {
                BattleManager.I.lastTarget.AddBuff(BUFF_FREEZE, 1, 3);
                BattleManager.I.lastTarget.AddBuff(BUFF_WAIT, 1, 1);
            }
        } else {
            StrAttack attack = CreateAttack();
            attack.pow = 0;
            attack.target = Chara.TARGET_SOLO;
            attack.type = ATK_TYPE_SPECIAL;
            attack.buffID = BUFF_FREEZE;
            attack.buffLevel = 1;
            attack.buffTurn = 3;
            BattleManager.I.Attack(attack);
        }
        yield break;
    }

    IEnumerator SkillPray() {
        BattleManager.I.PopMana(tag, 0, 0, 0, 8, 0);
        yield break;
    }

    IEnumerator SkillBlizzard() {
        AddBuff(BUFF_BLIZZARD, 1, 4);
        yield break;
    }

    IEnumerator SkillHealLight() {
        Fighter target = BattleManager.I.TargetFriend;
        int healVal = 20 + GetMyManaCnt(Mana.YELLOW) * 5;
        if (target == null) {
            Heal(healVal, false);
        } else {
            target.Heal(healVal, false);
        }
        yield break;
    }

    IEnumerator SkillHolyLight() {
        StrAttack attack = CreateAttack();
        attack.pow = 0;
        attack.target = Chara.TARGET_ALL;
        attack.targetTag = tag;
        attack.buffID = BUFF_REGENERATION;
        attack.buffLevel = 1;
        attack.buffTurn = 5;
        BattleManager.I.Attack(attack);
        yield break;
    }

    IEnumerator SkillPunishment() {
        StrAttack attack = CreateAttack();
        attack.pow = 70;
        attack.target = Chara.TARGET_ALL;
        BattleManager.I.Attack(attack);
        yield break;
    }

    IEnumerator SkillQuickArrow() {
        StrAttack attack = CreateAttack();
        attack.pow = 10;
        attack.target = Chara.TARGET_SOLO;
        int yellowCnt = GetMyManaCnt(Mana.YELLOW);
        for (int i = 0; i < 2 + (yellowCnt / 10); ++i) {
            BattleManager.I.Attack(attack);
            yield return new WaitForSeconds(0.5f);
        }
        yield break;
    }

    IEnumerator SkillArrowRain() {
        StrAttack attack = CreateAttack();
        attack.pow = 10;
        attack.target = Chara.TARGET_ALL;
        BattleManager.I.Attack(attack);
        yield break;
    }

    IEnumerator SkillArcherBreath() {
        BattleManager.I.PopMana(tag, 5, 0, 0, 0, 0);
        yield break;
    }

    IEnumerator SkillBite() {
        StrAttack attack = CreateAttack();
        attack.pow = 5;
        attack.target = Chara.TARGET_SOLO;
        for (int i = 0; i < 5; ++i) {
            BattleManager.I.Attack(attack);
            yield return new WaitForSeconds(0.2f);
        }
        yield break;
    }

    IEnumerator SkillPoison() {
        StrAttack attack = CreateAttack();
        attack.pow = 10;
        attack.target = Chara.TARGET_SOLO;
        attack.buffID = BUFF_POISON;
        attack.buffLevel = 1;
        attack.buffTurn = 5;
        BattleManager.I.Attack(attack);
        yield break;
    }

    IEnumerator SkillDark1() {
        StrAttack attack = CreateAttack();
        attack.pow = 10;
        attack.target = Chara.TARGET_ALL;
        attack.type = Chara.ATK_TYPE_MAGIC;
        attack.buffID = BUFF_DEFENSE_DOWN;
        attack.buffLevel = 1;
        attack.buffTurn = 5;
        BattleManager.I.Attack(attack);
        yield break;
    }

    IEnumerator SkillDark2() {
        StrAttack attack = CreateAttack();
        attack.pow = 10;
        attack.target = Chara.TARGET_RANDOM;
        attack.type = Chara.ATK_TYPE_MAGIC;
        attack.buffID = BUFF_DEFENSE_DOWN;
        attack.buffLevel = 1;
        attack.buffTurn = 5;
        for (int i = 0; i < 2; ++i) {
            BattleManager.I.Attack(attack);
            yield return new WaitForSeconds(0.4f);
        }
        yield break;
    }

    IEnumerator SkillDark3() {
        StrAttack attack = CreateAttack();
        attack.pow = 10;
        attack.target = Chara.TARGET_SOLO;
        attack.type = Chara.ATK_TYPE_MAGIC;
        attack.buffID = BUFF_DEFENSE_DOWN;
        attack.buffLevel = 1;
        attack.buffTurn = 5;
        BattleManager.I.Attack(attack);
        yield break;
	}

	IEnumerator SkillRasadom() {
		StrAttack attack = CreateAttack();
		attack.pow = 30;
		attack.target = Chara.TARGET_ALL;
		attack.type = Chara.ATK_TYPE_MAGIC;
		attack.buffID = BUFF_DEFENSE_DOWN;
		attack.buffLevel = 1;
		attack.buffTurn = 5;
		BattleManager.I.Attack(attack);
		yield break;
	}

	IEnumerator SkillShadow() {
		int[] consumeManas = new int[Mana.MAX];
		GetMyManaCnt().CopyTo(consumeManas, 0);
		consumeManas[Mana.BLACK] = 0;
		int popBlackMana = 0;
		for(int i = 0; i < Mana.MAX; ++i) {
			popBlackMana += consumeManas[i];
		}
		BattleManager.I.ConsumeMana(tag, consumeManas);
		Debug.Log(popBlackMana);
		BattleManager.I.PopMana(tag, 0, 0, 0, 0, popBlackMana);
		yield break;
	}

	IEnumerator SkillFullSwing() {
        StrAttack attack = CreateAttack();
        attack.pow = 20;
        attack.target = Chara.TARGET_ALL;
        attack.type = Chara.ATK_TYPE_NORMAL;
        attack.buffID = BUFF_WAIT;
        attack.buffLevel = 1;
        attack.buffTurn = 2;
        BattleManager.I.Attack(attack);
        yield break;
    }

    IEnumerator SkillThrowNet() {
        StrAttack attack = CreateAttack();
        attack.pow = 20;
        attack.target = Chara.TARGET_SOLO;
        attack.type = Chara.ATK_TYPE_NORMAL;
        attack.buffID = BUFF_WAIT;
        attack.buffLevel = 1;
        attack.buffTurn = 1;
        BattleManager.I.Attack(attack);
        yield break;
    }

    IEnumerator SkillFullPower() {
        AddBuff(BUFF_FULL_POWER, 1, 3);
        Heal(30, false);
        yield break;
    }

    IEnumerator SkillHomerun() {
        StrAttack attack = CreateAttack();
        attack.pow = 80;
        attack.target = Chara.TARGET_SOLO;
        attack.type = Chara.ATK_TYPE_NORMAL;
        AddBuff(BUFF_WAIT, 1, 3);
        BattleManager.I.Attack(attack);
        yield break;
	}

	IEnumerator SkillChange() {
		Debug.Log("SkillChange");
		BattleManager.I.ShowBench(this);
		yield break;
	}

	IEnumerator SkillCurse() {
		AddBuff(BUFF_CURSE, 1, 5);
		Sound.I.PlaySound("Accident");
		yield break;
	}
}
