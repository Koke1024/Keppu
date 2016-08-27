using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    static public GameManager I() { return instance;}
    static public GameManager instance;
    void Awake() { instance = this; }

	// Use this for initialization
	void Start () {
	}
}
