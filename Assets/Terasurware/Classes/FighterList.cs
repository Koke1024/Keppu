using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FighterList : ScriptableObject
{	
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
		public int Life;
		public int Attack;
		public string Skill1;
		public string Skill2;
		public string Skill3;
		public string Skill4;
	}
}

