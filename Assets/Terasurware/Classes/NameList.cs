using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NameList : ScriptableObject
{	
	public List<Sheet> sheets = new List<Sheet> ();
	public List<Param> list {
		get {
			return sheets[0].list;
		}
	}

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
		public string NAME;
	}
}

