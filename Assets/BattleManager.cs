using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BattleManager : MonoBehaviour {
    static public BattleManager I { get { return instance; } }
    static BattleManager instance;
    public GameObject skillWindow;
    public GameObject charaModel;
    public GameObject enemyModel;
    public GameObject outlineTextModel;

    public Bench bench;
    Fighter changeChara;

	public struct StrBattleInfo {
		public int fieldID;
		public int talkID;
		public int characterID;
		public Fighter.FighterData[] myCharas;
		public Fighter.FighterData[] enemies;
	};

    public Image bg;

    public GameObject charaEffectImage;
    int fieldID;

    delegate IEnumerator IEnumAryCallback(int[] ary);
    public delegate void AryCallback(int[] ary);
    delegate IEnumerator IEnumCallback();
    public AryCallback manaCheck;
    List<IEnumCallback> buffCheck = new List<IEnumCallback>();
    List<IEnumCallback> enemyAction = new List<IEnumCallback>();
    Chara[] charas;
    Enemy[] enemies;
    public List<Chara> benchCharas = new List<Chara>();
    public Mana skillWindowMana;
    Fighter showingChara = null;
    int showingSkillID = -1;
	bool skillConfirm = false;

	public float turnInterval = 2.0f;
    Fighter targetEnemy;
    public Fighter TargetEnemy{
        get { return targetEnemy; }
        set { lastTarget = value; targetEnemy = value; }
    }
    Fighter targetFriend;
    public Fighter TargetFriend {
        get { return targetFriend; }
        set { lastTarget = value; targetFriend = value; }
    }
    public Fighter lastTarget;
    const int CHARA_MAX = 4;
    const int ENEMY_MAX = 3;

	public string debugEnemyKey;

	[SerializeField]
	public class BattleResult {
		bool win;	//勝敗
		int characterID;
	};

    void Update() {
        //Debug.Log(showingSkillID);
    }

    void Awake() {
        instance = this;
        Fighter.manaCnt = new int[Mana.MAX] { 0, 0, 0, 0, 0 };
        Fighter.consumeManaTotal = new int[Mana.MAX] { 0, 0, 0, 0, 0 };
        Fighter.enemyManaCnt = new int[Mana.MAX] { 0, 0, 0, 0, 0 };
    }

    void Start() {
        AudioClip tmpClip;
        /*
        tmpClip = Resources.Load<AudioClip>("Sound/BGM/Zest/Zest_bk");
        Sound.AddClip(tmpClip, 0);
        tmpClip = Resources.Load<AudioClip>("Sound/BGM/Zest/Zest_drum");
        Sound.AddClip(tmpClip, 1);
        tmpClip = Resources.Load<AudioClip>("Sound/BGM/Zest/Zest_melo");
        Sound.AddClip(tmpClip, 2);
        Sound.SetChannelVolume(0, 1);
        Sound.SetChannelVolume(1, 0);
        Sound.SetChannelVolume(2, 0);
         * */

        tmpClip = Resources.Load<AudioClip>("Sound/BGM/DivinityGarden/Violin");
        Sound.I.AddClip(tmpClip, 0);
        tmpClip = Resources.Load<AudioClip>("Sound/BGM/DivinityGarden/Guitar");
        Sound.I.AddClip(tmpClip, 1);
        tmpClip = Resources.Load<AudioClip>("Sound/BGM/DivinityGarden/Organ");
        Sound.I.AddClip(tmpClip, 2);
        tmpClip = Resources.Load<AudioClip>("Sound/BGM/DivinityGarden/Strings");
        Sound.I.AddClip(tmpClip, 3);
        tmpClip = Resources.Load<AudioClip>("Sound/BGM/DivinityGarden/Piano");
        Sound.I.AddClip(tmpClip, 4);
        tmpClip = Resources.Load<AudioClip>("Sound/BGM/DivinityGarden/Bass");
        Sound.I.AddClip(tmpClip, 5);
        tmpClip = Resources.Load<AudioClip>("Sound/BGM/DivinityGarden/Drums");
        Sound.I.AddClip(tmpClip, 6);
        Sound.I.SetChannelVolume(0, 1);
        Sound.I.SetChannelVolume(1, 1);
        Sound.I.SetChannelVolume(2, 1);
        Sound.I.SetChannelVolume(3, 1);
        Sound.I.SetChannelVolume(4, 0);
        Sound.I.SetChannelVolume(5, 0);
        Sound.I.SetChannelVolume(6, 0);
        Sound.I.PlayBGM();
        Fighter.mana = GameObject.Find("FriendMana").GetComponent<Mana>();
        Fighter.enemyMana = GameObject.Find("EnemyMana").GetComponent<Mana>();
        Init();
    }

    void InitChara(Fighter.FighterData[] charasData) {
		int count = charasData.Count(o => o.name != null);
		Debug.Log(count);
		charas = new Chara[count];
		for (int i = 0; i < count; ++i) {
			Fighter.FighterData charaData = charasData[i];
			GameObject chara = Instantiate(charaModel);
			chara.transform.SetParent(transform);
			charas[i] = chara.GetComponent<Chara>();
			charas[i].Init(charaData);
			manaCheck += charas[i].CheckMana;
			buffCheck.Add(charas[i].CheckBuff);
			if (i >= CHARA_MAX) {
				chara.SetActive(false);
			}
        }
	}

	void InitEnemy(Fighter.FighterData[] charasData) {
		int count = charasData.Count(o => o.name != null);
		Debug.Log(count);
		enemies = new Enemy[count];
        for (int i = 0; i < count; ++i) {
			Fighter.FighterData charaData = charasData[i];
            GameObject chara = Instantiate(enemyModel);
			chara.transform.SetParent(transform);
			enemies[i] = chara.GetComponent<Enemy>();
			enemies[i].Init(charaData);
			buffCheck.Add(enemies[i].CheckBuff);
			enemyAction.Add(enemies[i].Act);
			manaCheck += enemies[i].CheckMana;
		}
	}

	public void ShowBench(Fighter chara) {
		if(benchCharas.Count == 0) {
			return;
		}
        if (changeChara != null) {
            HideBench();
            return;
        }
        changeChara = chara;
        Debug.Log("ShowBench");
        bench.gameObject.SetActive(true);
        bench.Activate();
        StartCoroutine("IEnumShowBench", true);
    }

    IEnumerator IEnumShowBench(bool show) {
        while(true){
            if (show) {
                bench.gameObject.transform.Translate(-10, 0, 0);
            } else {
                bench.gameObject.transform.Translate(10, 0, 0);
            }
            if (bench.gameObject.transform.localPosition.x < 0 ||
                bench.gameObject.transform.localPosition.x > 530) {
                bench.gameObject.SetActive(show);
                yield break;
            }
            yield return null;
        }
    }

    public void HideBench() {
        changeChara = null;
        Debug.Log("HideBench");
        StartCoroutine("IEnumShowBench", false);
    }

	void SetChara() {
		for (int i = 0; i < charas.Length; ++i) {
			if (charas[i] == null) {
				break;
			}
			charas[i].transform.localPosition = new Vector3(0, -180 - 130 * i);
			charas[i].transform.localScale = new Vector3(1, 1);
			charas[i].charaButton.GetComponent<Image>().sprite = charas[i].GetIconSprite();
        }

		foreach (Fighter chara in benchCharas) {
			chara.charaButton.GetComponent<Image>().sprite = chara.GetIconSprite();
		}
		for (int i = 0; i < enemies.Length; ++i) {
			if (enemies[i] == null) {
				break;
			}
			Debug.Log(enemies.Length);
			int x = i * 180 - ((enemies.Length - 1) % 2 * 90);
			enemies[i].transform.localPosition = new Vector2(x, 0);
			enemies[i].transform.localScale = new Vector3(1, 1);
			enemies[i].charaButton.GetComponent<Image>().sprite = enemies[i].GetFullSprite();
		}
	}

    public void Substitution(int benchID) {
        for (int i = 0; i < charas.Length; ++i) {
            if (charas[i] == changeChara) {
                Chara tmp = charas[i];
                charas[i] = benchCharas[benchID];
                benchCharas[benchID] = tmp;
                benchCharas[benchID].gameObject.SetActive(false);
                charas[i].gameObject.SetActive(true);
            }
        }
        HideBench();
        SetChara();
        manaCheck(Fighter.manaCnt);
        CheckActiveChara();
        showingSkillID = -1;
        Fighter.isAttacking = false;
    }

    void Init() {
		StrBattleInfo battleInfo;
		if (Container.container.ContainsKey("battleInfo")) {
			battleInfo = (StrBattleInfo)Container.container["battleInfo"];
			Container.container.Remove("battleInfo");
			Debug.Log("ロード");
        } else {
			/* デバッグ用 */
			battleInfo = new StrBattleInfo();
			battleInfo.myCharas = new Fighter.FighterData[2] {
				new Fighter.FighterData(0) , new Fighter.FighterData(1)
			};
			//battleInfo.myCharas[0] = new Fighter.FighterData(0);
			//battleInfo.myCharas[1] = new Fighter.FighterData(1);
			battleInfo.enemies = new Fighter.FighterData[2] {
				new Fighter.FighterData(3), new Fighter.FighterData(4)
			};
			Debug.Log("デバッグ");
		}

		fieldID = battleInfo.fieldID;
        bg.sprite = Resources.Load<Sprite>("bg/" + Excel<FieldList>.Item.list[fieldID].Key);

		InitChara(battleInfo.myCharas);
        InitEnemy(battleInfo.enemies);
		SetChara();
		TargetFriend = charas[0];
        TargetEnemy = enemies[0];

        PopMana("Chara", new int[Mana.MAX]{
             Excel<FieldList>.Item.list[fieldID].Red,
             Excel<FieldList>.Item.list[fieldID].Blue,
             Excel<FieldList>.Item.list[fieldID].Green,
			 Excel<FieldList>.Item.list[fieldID].Yellow,
			 Excel<FieldList>.Item.list[fieldID].Black
		});

        PopMana("Enemy", new int[Mana.MAX]{
             Excel<FieldList>.Item.list[fieldID].Red,
             Excel<FieldList>.Item.list[fieldID].Blue,
             Excel<FieldList>.Item.list[fieldID].Green,
             Excel<FieldList>.Item.list[fieldID].Yellow,
             Excel<FieldList>.Item.list[fieldID].Black});
    }

    public void CallNewTurn() {
        StartCoroutine("NewTurn");
    }

	public void CheckDestroy() {
		bool destroy = true;
		foreach (Enemy enemy in enemies) {
			if (enemy == null) {
				continue;
			}
			if (!enemy.IsDead()) {
				destroy = false;
				break;
			}
		}
		if (destroy) {
			Container.container.Add("battleResult", true);
			Debug.Log("勝利");
			StopAllCoroutines();
			Application.LoadLevel("Scenario");
		}
		bool catastrophe = true;
		foreach (Chara chara in charas) {
			if (chara == null) {
				continue;
			}
			if (!chara.IsDead()) {
				catastrophe = false;
				break;
			}
		}
		if (catastrophe) {
			Container.container.Add("battleResult", false);
			Debug.Log("全滅");
			StopAllCoroutines();
			Application.LoadLevel("Scenario");
		}
	}

    /// <summary>
    /// ターン開始処理（マナ生成　敵行動　バフチェック）
    /// </summary>
    public IEnumerator NewTurn() {
		CheckDestroy();
        if (Fighter.isAttacking) {
            yield break;
        }
        skillWindow.SetActive(false);

		PopMana("Enemy", new int[Mana.MAX]{
             Excel<FieldList>.Item.list[fieldID].Red,
             Excel<FieldList>.Item.list[fieldID].Blue,
             Excel<FieldList>.Item.list[fieldID].Green,
			 Excel<FieldList>.Item.list[fieldID].Yellow,
			 Excel<FieldList>.Item.list[fieldID].Black
		});

		foreach (IEnumCallback coroutine in buffCheck) {
            yield return StartCoroutine(coroutine());
        }
        foreach (IEnumCallback coroutine in enemyAction) {
            yield return StartCoroutine(coroutine());
		}

		PopMana("Chara", new int[Mana.MAX]{
             Excel<FieldList>.Item.list[fieldID].Red,
             Excel<FieldList>.Item.list[fieldID].Blue,
             Excel<FieldList>.Item.list[fieldID].Green,
			 Excel<FieldList>.Item.list[fieldID].Yellow,
			 Excel<FieldList>.Item.list[fieldID].Black
		});
    }

    ///<summary>マナを加算し、マナチェックメソッドを呼び出す</summary>
    public void PopMana(string _tag, int[] aryManaCnt) {
		IEnumerator coroutine = IEnumPopMana(_tag, aryManaCnt);
        StartCoroutine(coroutine);
		/*
        for (int i = 0; i < 2; ++i) {
            if (Fighter.manaCnt[i] >= 10) {
                Sound.I.FadeChannelVolume(i + 1, 1.0f);
            } else {
                Sound.I.FadeChannelVolume(i + 1, 0);
            }
        }
         * */
		for (int i = 0; i < 3; ++i) {
            if (Fighter.manaCnt[i] >= 10) {
                Sound.I.FadeChannelVolume(i + 4, 1.0f);
            } else {
                Sound.I.FadeChannelVolume(i + 4, 0);
            }
        }
    }

    ///<summary>マナを加算し、マナチェックメソッドを呼び出す</summary>
    public void PopMana(string _tag, int red, int blue, int green, int yellow, int black) {
		PopMana(_tag, new int[] { red, blue, green, yellow, black });
	}

    ///<summary>マナを加算し、マナチェックメソッドを呼び出す</summary>
    IEnumerator IEnumPopMana(string _tag, int[] aryManaCnt) {
        for (int i = 0; i < Mana.MAX; ++i) {
            if (aryManaCnt[i] > 0) {
                if (_tag == "Chara") {
                    Fighter.manaCnt[i] += aryManaCnt[i];
                    Fighter.mana.SetManaText(i, Fighter.manaCnt[i]);
                    manaCheck(Fighter.manaCnt);
                } else {
					Fighter.enemyManaCnt[i] += aryManaCnt[i];
                    Fighter.enemyMana.SetManaText(i, Fighter.enemyManaCnt[i]);
                }
                Sound.I.PlaySound("Phone");
            }
		}
		yield break;
    }

    /// <summary>
    /// マナを消費する。足りなければfalse
    /// </summary>
    /// <param name="aryManaCnt"></param>
    /// <returns>消費可能</returns>
    public bool ConsumeMana(string _tag, int[] requireMana) {
        for (int i = 0; i < Mana.MAX; ++i) {
            if (_tag == "Chara") {
                if (Fighter.manaCnt[i] < requireMana[i]) {
                    return false;
                }
            } else {
                if (Fighter.enemyManaCnt[i] < requireMana[i]) {
                    return false;
                }
            }
        }
        for (int i = 0; i < Mana.MAX; ++i) {
            if (_tag == "Chara") {
                Fighter.manaCnt[i] -= requireMana[i];
                Fighter.consumeManaTotal[i] += requireMana[i];
                Fighter.mana.SetManaText(i, Fighter.manaCnt[i]);
            } else {
				Debug.Log(i + ":" + Fighter.enemyManaCnt[i] + " - " + requireMana[i]);
				Fighter.enemyManaCnt[i] -= requireMana[i];
                //Fighter.consumeManaTotal[i] += requireMana[i];
                Fighter.enemyMana.SetManaText(i, Fighter.enemyManaCnt[i]);
            }
        }
        manaCheck(Fighter.manaCnt);
        return true;
    }

    /// <summary>
    /// マナを消費する。足りなければfalse
    /// </summary>
    /// <param name="aryManaCnt"></param>
    /// <returns>消費可能</returns>
    public bool ConsumeMana(string _tag, int red, int blue, int green, int yellow, int black) {
        return ConsumeMana(_tag, new int[] { red, blue, green, yellow, black});
    }

    /// <summary>
    /// スキル説明ウィンドウ表示
    /// </summary>
    /// <param name="skillID">0～4</param>
    public void ShowSkillWindow(Fighter chara, int skillID) {
        skillWindow.SetActive(true);
        skillWindowMana.gameObject.SetActive(true);
        if (showingChara == chara && showingSkillID == skillID) {
			skillConfirm = true;
        } else {
			skillConfirm = false;
		}
        showingChara = chara;
        showingSkillID = skillID;
        SkillList.Param skillInfo = SkillManager.I.GetSkillInfo(chara.data.skillList[skillID]);
        skillWindow.transform.FindChild("Name").GetComponent<Text>().text = skillInfo.Name;
        skillWindow.transform.FindChild("Detail").GetComponent<Text>().text = skillInfo.Detail;
        for (int i = 0; i < Mana.MAX; ++i) {
            Color color = Color.white;
            if (Fighter.manaCnt[i] < skillInfo.Require[i]) {
                color = Color.red;
            }
            skillWindowMana.SetManaText(i, skillInfo.Require[i], color);
        }
    }

    /// <summary>
    /// スキル説明ウィンドウ消去
    /// </summary>
    /// <param name="skillID">0～4</param>
    public void ExitSkillWindow(Fighter chara, int skillID) {
        if (showingChara == chara && showingSkillID == skillID) {
            skillWindow.SetActive(false);
        }
    }

    /// <summary>
    /// キャラ情報ウィンドウ表示
    /// </summary>
    /// <param name="skillID">0～4</param>
    public void ShowCharaWindow(Fighter chara) {
        skillWindowMana.gameObject.SetActive(false);
        skillWindow.SetActive(true);
        showingChara = chara;
        showingSkillID = -1;
        skillWindow.transform.FindChild("Name").GetComponent<Text>().text = chara.data.name;
        string detail = "HP:" + chara.data.life + " / " + chara.data.maxLife + "\n";

        foreach(Chara.StrBuffInfo buff in chara.buffs) {
            BuffList.Param param = Excel<BuffList>.Item.list.Find(o => o.BuffID == buff.ID);
            detail = detail + "" + param.Detail + "(" + buff.Turn + ")";
        }
        skillWindow.transform.FindChild("Detail").GetComponent<Text>().text = detail;
    }

    public void ConfirmSkill() {
        if (!skillConfirm || Chara.isAttacking || (changeChara != null && showingSkillID != 4)) {
            Debug.Log(Chara.isAttacking);
            Debug.Log(changeChara);
            return;
        }
        if (showingChara != null) {
            showingChara.StartCoroutine("UseSkill", showingSkillID);
		}
		skillConfirm = false;
	}

    public IEnumerator CharaEffect(string charaKey) {
		yield break;//todo
		/*
        float totalFrame = 60.0f;
        charaEffectImage.SetActive(true);
        charaEffectImage.GetComponent<Image>().sprite = 
        Resources.Load<Sprite>("Chara/" + charaKey + "/Icon");
        charaEffectImage.GetComponent<Image>().color = new Color(1, 1, 1, 1);
        for (float currentFrame = 0; currentFrame <= totalFrame; currentFrame += 1.0f) {
            float y = -285.0f + (385.0f * currentFrame / totalFrame);
            charaEffectImage.transform.localPosition = new Vector3(0, y, 0);
            yield return null;
        }
        for (float currentFrame = 0; currentFrame <= totalFrame; currentFrame += 1.0f) {
            charaEffectImage.GetComponent<Image>().color = new Color(1, 1, 1, (totalFrame - currentFrame) / totalFrame);
            yield return null;
        }
        charaEffectImage.SetActive(false);
        yield break;
		*/
    }

    public void HideSkillWindow() {
        skillWindow.SetActive(false);
    }

    public void Attack(Fighter.StrAttack attack) {
        string targetTag = attack.targetTag;
        List<GameObject> aryEnemy = new List<GameObject>(GameObject.FindGameObjectsWithTag(targetTag));
        if (aryEnemy.Count == 0) {
            return;
        }

        if (targetEnemy && attack.targetTag == "Enemy" &&
            attack.target == Chara.TARGET_SOLO &&
            !targetEnemy.IsDead()) {
            targetEnemy.Damage(attack);
        } else {
            switch (attack.target) {
                case Chara.TARGET_SOLO:
                    aryEnemy.Sort((a, b) => Random.Range(0, 100) ); //todo
                    aryEnemy.First().GetComponent<Fighter>().Damage(attack);
                    break;
                case Chara.TARGET_ALL:
                    foreach (GameObject target in aryEnemy.Where
                        (o => !o.GetComponent<Fighter>().IsDead())) {
                        target.GetComponent<Fighter>().Damage(attack);
                    }
                    break;
                case Chara.TARGET_SELF:
                    attack.from.Damage(attack);
                    break;
                case Chara.TARGET_FRIEND:
                    targetFriend.Damage(attack);
                    break;
                case Chara.TARGET_RANDOM:
                    aryEnemy.Sort((a, b) => Random.Range(0, 100));
                    aryEnemy.First().GetComponent<Fighter>().Damage(attack);
                    break;
            }
        }
    }

    public void CheckActiveChara() {
		//Debug.Log(charas.Length);
        foreach (Chara chara in charas) {
			if(chara == null) {
				continue;
			}
            if (chara.enableAct) {
                return;
            }
        }
        CallNewTurn();
    }

    public Fighter FindPassiveOwner(string passiveName, string targetTag) {
        List<GameObject> aryEnemy = new List<GameObject>(GameObject.FindGameObjectsWithTag(targetTag));
        foreach (GameObject chara in aryEnemy) {
            Fighter fighter = chara.GetComponent<Fighter>();
            if (fighter.HasPassive(passiveName) > 0) {
                return fighter;
            }
        }
        return null;
    }

	static public int[] GetCharaSkill(int fighterID) {
		FighterList.Param fighter = Excel<FighterList>.Item.list.Find(o => o.ID == fighterID);
		if(fighter == null){
			return null;
		}
		SkillList.Param[] skillParamList = new SkillList.Param[] {
			Excel<SkillList>.Item.list.Find(o => o.Key == fighter.Skill1),
			Excel<SkillList>.Item.list.Find(o => o.Key == fighter.Skill2),
			Excel<SkillList>.Item.list.Find(o => o.Key == fighter.Skill3),
			Excel<SkillList>.Item.list.Find(o => o.Key == fighter.Skill4)
		};
		int[] ret = new int[] {
			(skillParamList[0] != null)?skillParamList[0].ID: 0,
			(skillParamList[1] != null)?skillParamList[1].ID: 0,
			(skillParamList[2] != null)?skillParamList[2].ID: 0,
			(skillParamList[3] != null)?skillParamList[3].ID: 0,
		};
		return ret;
    }

	public IEnumerator SkillMessage(string skillName) {
        GameObject textObject = Instantiate<GameObject>(outlineTextModel);
        textObject.transform.localScale = new Vector3(1, 1, 1);
        textObject.GetComponent<OutlineText>().Init(skillName, 64, 5, Color.red, Color.black);

        textObject.transform.SetParent(transform);
        for (int i = 0; i < 30; ++i) {
            textObject.transform.localPosition = new Vector3(550 - (i * 1100) / 60, 100);
            yield return null;
        }
        for (int i = 0; i < 30; ++i) {
            textObject.transform.localPosition = new Vector3(0, 100);
            yield return null;
        }
        for (int i = 30; i < 60; ++i) {
            textObject.transform.localPosition = new Vector3(550 - (i * 1100) / 60, 100);
            yield return null;
        }
        Destroy(textObject);
    }
}
