using UnityEngine;
using Save;
using System.Collections.Generic;
using UnityEngine.UI;

//シナリオシステムのマネージャ
public class ScenarioManager : MonoBehaviour {
    static public ScenarioManager I { get { return instance; } }
    static ScenarioManager instance;
    void Awake() { instance = this; }
    public Character character;
    public GameObject textBox;
    class VN {
		public const string DATE = "Date";
		public const string NAME = "Name";
	}
    public Text Date;
    struct StrRange {
        public StrRange(int _min, int _max) {
            min = _min;
            max = _max;
        }
        public readonly int min;
        public readonly int max;
    }
    struct StrEvent {
        public int talkID;
        public Dictionary<string, StrRange> conditions; //発生条件
        public int rate;  //発生確率
        public bool once;   //一度きり
		public int priority;
		public bool baseEvent;
        public void AddCondition(string var, int min, int max) {
            conditions.Add(var, new StrRange(min, max));
        }
        public StrEvent(int talkID, int rate, bool once, int priority, bool baseEvent) {
			this.talkID = talkID;
			this.rate = rate;
            this.once = once;
			this.priority = priority;
			this.baseEvent = baseEvent;
            conditions = new Dictionary<string, StrRange>();
        }
	}
	List<StrEvent> eventList = new List<StrEvent>();
	List<StrEvent> todayList = new List<StrEvent>();
	public List<int> doneEvent = new List<int>();
 
    void Start() {
		Init();

		if (Container.container.ContainsKey("Load")) {
			int id = Container.Get<int>("Load");
			Container.container.Remove("Load");
			character.Load(id);
		} else {
			character.SetNewData();
		}
		
		TextManager.I.SetVariable("Stamina", character.parameter.Stamina);
		Sound.I.AddClip("Sound/bgm1", 0);
        Sound.I.SetChannelVolume(0, 1.0f);
        Sound.I.PlayBGM();
        if (Container.container.ContainsKey("battleResult")) {
			if ((bool)Container.container["battleResult"]) {
				//character.Load(Serialize.Load<int>("characterID"));
				character.Load(Serialize.Load<int>("characterID"));
				Debug.Log("勝利して次の街");
				TextManager.I.LoadVariable(character.parameter.ID);
				TextManager.I.SetVariable("Place", 2);
			} else {
				Debug.Log("負けたのでやり直し");
				TextManager.I.SetVariable(VN.DATE, 0);
				TextManager.I.SetVariable("Place", 1);
			}
			Container.container.Remove("battleResult");
        }
    }

	//日常+追加イベントを並列に行うようにする
    void Init() {
		if (Container.container.ContainsKey("DoneEvent")) {
			doneEvent = Container.container["DoneEvent"] as List<int>;
        }
		StrEvent setEvent;

		//初日
		//森の中で目覚めた俺は、記憶を失っていた。
		setEvent = new StrEvent(101, 100, true, 100, true);
        setEvent.AddCondition(VN.DATE, 0, 0);
		setEvent.AddCondition("Place", 0, 0);
		eventList.Add(setEvent);
		//教会到着
		//到着したのは大きな教会のある村。
		setEvent = new StrEvent(201, 100, true, 100, false);
        setEvent.AddCondition("Place", 1, 1);
        eventList.Add(setEvent);
		//調査開始
		//近頃、この村では人間が忽然と消えるという。
		setEvent = new StrEvent(202, 100, true, 100, false);
        setEvent.AddCondition("Church", 1, 1);
		setEvent.AddCondition("Place", 1, 1);
		eventList.Add(setEvent);
        //ナゾ解明RankC
        setEvent = new StrEvent(203, 100, true, 100, false);
		setEvent.AddCondition("Place", 1, 1);
		setEvent.AddCondition("Search", 0, 1);
		setEvent.AddCondition("Date", 10, 20);
		eventList.Add(setEvent);
        //ナゾ解明RankB
        setEvent = new StrEvent(204, 100, true, 100, false);
		setEvent.AddCondition("Place", 1, 1);
		setEvent.AddCondition("Search", 2, 2);
		setEvent.AddCondition("Date", 10, 20);
		eventList.Add(setEvent);
        //ナゾ解明RankA
        setEvent = new StrEvent(205, 100, true, 100, false);
		setEvent.AddCondition("Place", 1, 1);
		setEvent.AddCondition("Search", 3, 4);
		setEvent.AddCondition("Date", 10, 20);
		eventList.Add(setEvent);
        //日常

		//ガラパ
		//科学と機械の街、○○に到着した。
		setEvent = new StrEvent(301, 100, true, 100, false);
		setEvent.AddCondition("Date", 11, 11);
		setEvent.AddCondition("Place", 2, 2);
		eventList.Add(setEvent);

		//ガラパ
		//ガラパさんは、俺たちが世話になっている宿屋の主人だ。
		setEvent = new StrEvent(302, 100, true, 100, false);
		setEvent.AddCondition("Date", 12, 12);
		setEvent.AddCondition("Place", 2, 2);
		eventList.Add(setEvent);

		//教会日常
		setEvent = new StrEvent(2003, 100, false, 10, true);
		setEvent.AddCondition("Place", 0, 1);
		setEvent.AddCondition("Detect", 0, 0);
		eventList.Add(setEvent);

		//教会日常
		setEvent = new StrEvent(2001, 100, false, 10, true);
		setEvent.AddCondition("Place", 0, 1);
		setEvent.AddCondition("Detect", 1, 10);
		eventList.Add(setEvent);

		//街日常
		setEvent = new StrEvent(3001, 100, false, 10, true);
		setEvent.AddCondition("Place", 2, 2);
		eventList.Add(setEvent);

		eventList.Sort((o1, o2) => { return o2.priority - o1.priority; });
	}

    public void Update() {
		if (!TextManager.I.talking) {
			character.CommitParameter(true);
			Debug.Log("todayList : " + todayList.Count);
			if (todayList.Count == 0) {
				StartDay();
			}
			else if (todayList.Count > 0) {
				Push(todayList[0].talkID);
				todayList.RemoveAt(0);
			}
			TextManager.I.StartTalk();
		}
    }

    public void StartDay() {
		//Debug.Log("StartTalk");
		CheckEvent();
		Push(todayList[0].talkID);
		todayList.RemoveAt(0);
	}

    public void UseStamina(string[] args) {
        int val = character.AddStamina(-int.Parse(args[1]));
        if (val < 0) {
            TextManager.I.PushText(1, 0, 1, "体力が" + Mathf.Abs(val) + "減った", null, null);
        }
    }

    public void Rest(string[] args) {
        character.AddStamina(character.parameter.maxStamina / 2);
    }

    //能力値に応じ体力減少
    public void Training(string[] args) {
        int sum = 0;

        float accidentRate = 0.5f * (50 - TextManager.I.GetVariable("Stamina")) / 50.0f;
        Debug.Log("ケガ率:" + accidentRate);
        if (accidentRate > Random.Range(0, 1.0f)) {
            string[] accidentArgs = { null, "-20", "-20", "-20", "-20", "-20" };
            TextManager.I.PushText(1, 0, 19, "グギッ！", "Accident", null);
            TextManager.I.PushText(1, 0, 15, "ケガをしてしまった……。", null, null);
            SetParam(accidentArgs);
        } else {
            for (int i = 0; i < Mana.MAX; ++i) {
                sum += int.Parse(args[i + 1]);
            }
            SetParam(args);
            character.AddStamina(-sum);
        }
    }

    public void SetParam(string[] args) {
        for (int i = 0; i < Mana.MAX; ++i) {
            character.StackParameter(i, int.Parse(args[i + 1]));
        }
        Debug.Log("Training");
	}

	public void SetFlag(string[] args) {
		TextManager.I.SetVariable(args[1], int.Parse(args[2]));
	}

	public void AddFlag(string[] args) {
		TextManager.I.AddVariable(args[1], int.Parse(args[2]));
	}

	public void PrintArgs(string[] args) {
        foreach (string arg in args) {
            Debug.Log(arg);
        }
    }

    void Push(int talkID) {
		Debug.Log("開始:" + talkID);
        TextManager.I.PushText(talkID, 10, false);
    }

    void Push(int talkID, bool interrupt) {
        TextManager.I.PushText(talkID, 10, interrupt);
    }

    void CheckEvent() {
		Debug.Log("イベント抽選開始" + TextManager.I.GetVariable(VN.DATE));
        foreach (int talkID in doneEvent) {
			eventList.RemoveAll(o => o.talkID == talkID);
		}
		todayList = eventList.FindAll(eventData => {
			bool enable = true;
			foreach (KeyValuePair<string, StrRange> condition in eventData.conditions) {
				int variable = TextManager.I.GetVariable(condition.Key);
/*
				Debug.Log("[" + eventData.talkID + "]EventCondition:"
					+ condition.Key + ":" + condition.Value.min + " <= "
					 + variable + " <= " + condition.Value.max);
*/				
				if (variable < condition.Value.min || condition.Value.max < variable) {
					enable = false;
					//Debug.Log("Disable");
					break;
				}
			}
			if(enable && eventData.baseEvent) {
				//Debug.Log("Enable");
				return true;
			}
			if (enable && Random.Range(0, 100) <= eventData.rate) {
				Debug.Log("Push" + eventData.talkID);
				todayList.Add(eventData);
				if (eventData.once) {
					doneEvent.Add(eventData.talkID);
				}
				return true;
			}
			return false;
        });
        character.CommitParameter(true);
    }

    public void Move(string[] args) {
        TextManager.I.SetVariable("Place", int.Parse(args[1]));
	}

	void Search(string[] args) {

		if (TextManager.I.GetVariable("Place") == 1) {
			int search = TextManager.I.GetVariable("Search");
			int[] talkID = { 211, 212, 213, 221 };
			if (search > 3) {
				search = 3;
			}
			Push(talkID[search]);
			TextManager.I.SetVariable("Search", search + 1);
		} else {
			Push(321);
		}
	}

	void SetParameterText(string[] args) {
        character.SetStatusText(int.Parse(args[1]));
    }

	/// <summary>
	/// 1:FieldID, 2:talkID 3～:EnemyID
	/// </summary>
	/// <param name="args"></param>
	public void StartBattle(string[] args) {
		BattleManager.StrBattleInfo battleInfo = new BattleManager.StrBattleInfo();
		battleInfo.fieldID = int.Parse(args[1]);
		battleInfo.talkID = int.Parse(args[2]);
		battleInfo.characterID = character.parameter.ID;
		battleInfo.enemies = new Fighter.FighterData[args.Length - 3];
		Debug.Log(battleInfo.enemies.Length);
		int enemyKey = 0;
		Debug.Log(args.Length);
		for (int i = 3; i < args.Length; ++i) {
			Debug.Log(i);
			int enemyID = int.Parse(args[i]);

			FighterList.Param enemy = Excel<FighterList>.Item.list.Find(o => o.ID == enemyID);
			battleInfo.enemies[enemyKey] = new Fighter.FighterData(enemy);
			++enemyKey;
        }

		battleInfo.myCharas = new Fighter.FighterData[2] {
			character.GetFighterData(), new Fighter.FighterData(1)
		};
		TextManager.I.SaveVariable(character.parameter.ID);
		Container.container.Add("battleInfo", battleInfo);
        TextManager.I.active = false;
        Application.LoadLevel("Battle");
    }

	public void DebugStartBattle() {
		StartBattle(new string[]{"", "1", "2100", "2", "3", "4"});
	}

	public void Training(int type) {
		string[] args = new string[Mana.MAX + 1] {"", "0", "0", "0", "0", "0" };
		args[type] = 20.ToString();
        Training(args);
	}

	public void Register(string[] args) {
		character.Register();
	}
}