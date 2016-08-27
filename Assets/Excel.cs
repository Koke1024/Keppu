using UnityEngine;
using UnityEngine.UI;

public class Excel<T> where T : Object {
	static public T Item {
		get {
			if(item == null) {
				Init();
			}
			return item;
		}
	}
	static T item;

	static void Init() {
		T[] lists = Resources.LoadAll<T>("ExcelData");
		item = lists[0];
	}
}
