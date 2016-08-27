using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

//テキストボックス機能のマネージャ
public class TextManager : MonoBehaviour {
    static public TextManager I { get { return instance; } }
    static TextManager instance;
    void Awake() { instance = this; }
    public Text displayString;
    public Image[] charaImg;
    public Image bgImg;
    public GameObject[] buttons;
	public bool talking = false;
	public bool waiting = false;
	bool clicked = false;
    AudioClip audioClip;
    AudioSource audioSource;

    public bool active = true;

    Dictionary<string, string> replaceWords = new Dictionary<string, string>(); //置換する文字列をセットする
	[SerializeField]
	public Dictionary<string, int> variables = new Dictionary<string, int>();  //フラグ用変数の値
	List<Serif> serifList = new List<Serif>();

    float charaInterval = 0.05f;

    class Serif {
        public Serif(Words.Param param) {
            text = param.Text;
            talkID = param.TalkID;
            charaID = new int[4];
            charaID[0] = param.CharaID0;
            charaID[1] = param.CharaID1;
            charaID[2] = param.CharaID2;
            charaID[3] = param.CharaID3;
            face = new int[4];
            face[0] = param.Face0;
            face[1] = param.Face1;
            face[2] = param.Face2;
            face[3] = param.Face3;
            sound = param.Sound;
            bgID = param.Bg;
            if (param.Function == null || param.Function.Length <= 1) {
                function = null;
            } else {
                function = param.Function;
//                Debug.Log(function);
            }
            if (param.NextA != 0) { selectText[0] = param.SelectA; selectStep[0] = param.NextA; selection = true; }
            if (param.NextB != 0) { selectText[1] = param.SelectB; selectStep[1] = param.NextB; }
            if (param.NextC != 0) { selectText[2] = param.SelectC; selectStep[2] = param.NextC; }
            if (param.NextD != 0) { selectText[3] = param.SelectD; selectStep[3] = param.NextD; }
        }
        public int talkID;
        public string text;
        public string function;
        public int[] charaID;
        public int[] face;
        public int bgID;
        public string sound;
        public string[] selectText = new string[4];
        public int[] selectStep = new int[4];
        public bool selection = false;
    }

    void Start() {
        audioSource = GetComponent<AudioSource>();
        audioClip = GetComponent<AudioClip>();
	}

	public void LoadVariable(int id) {
		if (Container.container.ContainsKey("TextManager::variables" + id)) {
			variables = Container.container["TextManager::variables" + id] as Dictionary<string, int>;
		}
	}

	public void SaveVariable(int id) {
		Container.Set("TextManager::variables" + id, variables);
	}

	void Update() {
        if (!active) {
            return;
        }
        if (Input.GetMouseButtonDown(0) && !waiting) {
            clicked = true;
        }
        if (Input.GetMouseButtonUp(1)) {
			ScenarioManager.I.character.Register();
			Dump();
			//Container.Set("TextManager::variables", variables);
			//Container.Set("DoneEvent", ScenarioManager.I.doneEvent);
			//Container.Save();
		}
    }

    //指定文字列を追加
    public void PushText(int charaID, int charaPos, int face, string text, string sound, string function) {
        Words.Param word = new Words.Param();
        switch (charaPos) {
            case 0: word.CharaID0 = charaID; word.Face0 = face; break;
            case 1: word.CharaID1 = charaID; word.Face1 = face; break;
            case 2: word.CharaID2 = charaID; word.Face2 = face; break;
            case 3: word.CharaID3 = charaID; word.Face3 = face; break;

        }
        word.Text = text;
        word.Sound = sound;
        word.Function = function;
        serifList.Add(new Serif(word));
		Debug.Log("serifCount" + serifList.Count);
		//StartTalk();
	}

    //指定トークID、ステップ以降のセリフをセットする
    public void PushText(int talkID, int step, bool cutIn) {
        int currentStep = step;
        List<Serif> tempSerifList = new List<Serif>();
        while (true) {
//            Debug.Log(talkID + ", " + currentStep + ", " + "cutIn");
            if (!Excel<Words>.Item.list.Exists(o => o.TalkID == talkID && o.Step == currentStep)) {
                break;
            }
            Words.Param word = Excel<Words>.Item.list.Find(o => o.TalkID == talkID && o.Step == currentStep);
//            Debug.Log(word.Text);
            Serif serif = new Serif(word);
            tempSerifList.Add(serif);
            currentStep = word.Next;
        }
        if(cutIn){
            serifList.InsertRange(0, tempSerifList);
        }
        else{
            serifList.AddRange(tempSerifList);
        }
		//Debug.Log("serifCount" + serifList.Count);
		//StartTalk();
    }

    public void StartTalk() {
		StopAllCoroutines();
        //Debug.Log("StartTalk");
        if (serifList.Count == 0) {
            Debug.Log("ない");
            talking = false;
            return;
		}
		Serif serif = serifList[0];
		talking = true;
		waiting = false;
		//Debug.Log("talking" + talking);
		Sprite[] sprite = new Sprite[4];

        for (int i = 0; i < 4; ++i) {
            string filePath = "Chara/" + Excel<CharaTable>.Item.list[serif.charaID[i]].FileName + "/" + serif.face[i];
            //Debug.Log(i + "" + filePath);
            sprite[i] = Resources.Load<Sprite>(filePath);
            if (sprite[i]) {
                charaImg[i].sprite = sprite[i];
                charaImg[i].gameObject.SetActive(true);
            } else {
                charaImg[i].gameObject.SetActive(false);
            }
        }

        if (serif.bgID != 0) {
            string filePath = "BG/" + Excel<BgList>.Item.list[serif.bgID].FileName;
            //Debug.Log(filePath);
            Sprite bgSprite = Resources.Load<Sprite>(filePath);
            bgImg.sprite = bgSprite;
        }

        foreach (KeyValuePair<string, string> needle in replaceWords) {
            serif.text = serif.text.Replace("%" + needle.Key + "%", needle.Value);
		}
		serif.text = serif.text.Replace("%Name%", ScenarioManager.I.character.parameter.Name);
		gameObject.SetActive(true);
        IEnumerator coroutine;
        coroutine = NextChar();
        StartCoroutine(coroutine);
    }

    private IEnumerator NextChar() {
        //Debug.Log("Start");
        Serif serif = serifList[0];
        //Debug.Log(serif.function);
        if (serif.sound != null) {
            audioClip = Resources.Load<AudioClip>("Sound/" + serif.sound);
            audioSource.clip = audioClip;
            audioSource.Play();
        }
        if (serif.function != null) {
            //Debug.Log("関数" + serif.function);
            string[] function = serif.function.Split(':');
            ScenarioManager.I.SendMessage(function[0], function);
        }
        int currentChara = 0;
        while (true) {
            ++currentChara;
            if (clicked) {
                clicked = false;
                if (currentChara >= serif.text.Length) {
                    if (!serif.selection) {
                        //選択肢なければ次のセリフ
                        serifList.RemoveAt(0);
                        currentChara = 0;
                        StartTalk();
                    }
                    if (serif.selection) {
                        //選択肢あり
                        yield return new WaitForEndOfFrame();
                    }
                } else {
                    //テキスト全表示
                    currentChara = serif.text.Length;
                }
            }
            if (currentChara == serif.text.Length) {
                for(int i = 0; i < 4; ++i){
                    if (serif.selectStep[i] != 0) {
						buttons[i].SetActive(true);
                        buttons[i].GetComponentInChildren<Text>().text = serif.selectText[i];
                    }
                }
            }
            displayString.text = serif.text.Substring(0, Mathf.Min(currentChara, serif.text.Length));
            yield return new WaitForSeconds(charaInterval);
        }
    }

    //選択肢によるテキストは割り込み
    public void Select(int inputSelect) {
        buttons[0].SetActive(false);
        buttons[1].SetActive(false);
        buttons[2].SetActive(false);
        buttons[3].SetActive(false);
        StopAllCoroutines();
        Serif tempSerif = serifList[0];
        serifList.RemoveAt(0);
        PushText(tempSerif.talkID, tempSerif.selectStep[inputSelect - 1], true);
        clicked = false;
        StartTalk();
    }

    public void AddReplaceWord(string key, string word) {
        if (replaceWords.ContainsKey(key)) {
            replaceWords[key] = word;
        } else {
            replaceWords.Add(key, word);
        }
    }

    public void ClearReplaceWords() {
        replaceWords.Clear();
    }

    public void SetVariable(string valName, int value) {
        if (variables.ContainsKey(valName)) {
            variables[valName] = value;
        } else {
            variables.Add(valName, value);
        }
		Debug.Log(valName + " :=" + value);
    }

    public void AddVariable(string valName, int value) {
        if (variables.ContainsKey(valName)) {
            variables[valName] += value;
        } else {
            variables.Add(valName, value);
        }
		Debug.Log(valName + " + " + value);
		if(valName == "Date") {
			ScenarioManager.I.Date.text = TextManager.I.GetVariable("Date").ToString() + "日目";
		}
	}

    public int GetVariable(string valName) {
        if (variables.ContainsKey(valName)) {
            return variables[valName];
        } else {
            return 0;
        }
    }

    public void Dump() {
		Debug.Log("Dump");
		foreach (KeyValuePair<string, int> pair in variables) {
			Debug.Log(pair.Key + ":" + pair.Value);
		}
		foreach (int val in ScenarioManager.I.doneEvent) {
			Debug.Log("Event:" + val + " is done");
		}
    }
}