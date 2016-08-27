using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FieldList : ScriptableObject {
	public List<Param> list {
		get {
			return sheets[0].list;
		}
	}
	public List<Sheet> sheets = new List<Sheet> ();

	[System.SerializableAttribute]
	public class Sheet
	{
		public string name = string.Empty;
		public List<Param> list = new List<Param>();
	}

	[System.SerializableAttribute]
	public class Param
	{
		
		public int ID;
		public string Key;
		public string Name;
		public int Red;
		public int Blue;
		public int Green;
		public int Yellow;
		public int Black;
	}
}

