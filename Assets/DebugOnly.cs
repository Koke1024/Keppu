using UnityEngine;
using System.Collections;

public class DebugOnly : MonoBehaviour {
	static public readonly bool DEBUG = true;

	// Use this for initialization
	void Start () {
		if (DEBUG) {
			Destroy(gameObject);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
