using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuffList : ScriptableObject {
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
		
		public int BuffID;
		public string IconName;
		public string Detail;
		public int CumulativeFlag;
	}
}

