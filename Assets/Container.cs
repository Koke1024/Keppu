using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Save;

public class Container {
	static bool logMode = true;
	static public Dictionary<string, object> container {
		get {
			if(_container == null) {
				_container = new Dictionary<string, object>();
            }
			return _container;
		}
	}
	static public Dictionary<string, object> _container;
	static public void Set(string key, object value) {
		if (container.ContainsKey(key)) {
			container[key] = value;
			if (logMode) {
				Debug.Log("Set value with Key:" + key);
			}
		} else {
			container.Add(key, value);
			if (logMode) {
				Debug.Log("Add new value with Key:" + key);
			}
		}
	}
	static public T Get<T>(string key) {
		if (container.ContainsKey(key)) {
			return (T)container[key];
		} else {
			if (logMode) {
				Debug.Log("Container doesn't has the Key:" + key);
			}
			return (T)container[key];
		}
    }
	static public void Save() {
		Serialize.Save("container", container);
		if (logMode) {
			Debug.Log("Save Container To PlayerPref");
		}
	}
	static public void Load() {
		_container = Serialize.Load<Dictionary<string, object>>("container");
		if (logMode) {
			Debug.Log("Load Container From PlayerPref");
		}
	}
}