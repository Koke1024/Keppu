using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Gauge : MonoBehaviour {
	public GameObject inner;
	public GameObject frame;
	public GameObject back;
	public float scale;

	public GameObject point;
	float nowValue;
	float dispValue;
    float maxValue;
    public int WIDTH;
    public int HEIGHT;
	
	public void Init(float _maxValue, float _nowValue){
		maxValue = _maxValue;
		nowValue = _nowValue;
		dispValue = nowValue;
	}
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		dispValue = dispValue * 0.95f + nowValue * 0.05f;
		float rate = dispValue / maxValue;
		if (rate < 0.01f) {
			inner.GetComponent<Image>().enabled = false;
		}
		else{
			inner.GetComponent<Image>().enabled = true;
		}
        rate = Mathf.Min(1.0f, rate);
        inner.GetComponent<RectTransform>().sizeDelta = new Vector2(
            WIDTH * rate * scale, HEIGHT * scale);
        frame.GetComponent<RectTransform>().sizeDelta = new Vector2(
            WIDTH * scale, HEIGHT * scale);
        back.GetComponent<RectTransform>().sizeDelta = new Vector2(
            WIDTH * scale, HEIGHT * scale);
	}
	
	public void SetNowValue(float value){
		nowValue = value;
		//dispValue = nowValue;
	}
}
