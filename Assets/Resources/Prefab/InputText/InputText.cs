using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class InputText : MonoBehaviour {
	public delegate void TextCallback(string str);
	public static InputText Create(TextCallback endEdit, string defaultName, string placeHolder, Vector2 position) {
		GameObject prefab = Resources.Load<GameObject>("Prefab/InputText/Prefab");
		GameObject inputObject = Instantiate(prefab);
		inputObject.transform.position = position;
        inputObject.transform.SetParent(GameObject.Find("Canvas").transform);
		inputObject.transform.localPosition = Vector3.zero;
		inputObject.transform.localScale = new Vector3(1, 1);

		InputText inputText = inputObject.GetComponent<InputText>();
		inputText.Init(endEdit, defaultName, placeHolder);
		return inputObject.GetComponent<InputText>();
	}

	public Text text;
	public Text placeHolder;
	TextCallback callbackFunction;

	public void Init(TextCallback endEdit, string defaultName, string placeHolderText) {
		callbackFunction = endEdit;
		this.placeHolder.text = placeHolderText;
		SetText(defaultName);
        Debug.Log(text.text);
	}

	public void EndEdit() {
		callbackFunction(text.text);
    }

	public void SetText(string text) {
		GetComponent<InputField>().text = text;
	}
}