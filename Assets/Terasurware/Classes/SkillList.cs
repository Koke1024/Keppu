using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillList : ScriptableObject
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
		public string Key;
		public string Name;
		public int Active;
		public int Red;
		public int Blue;
		public int Green;
		public int Yellow;
		public int Black;
		public int[] Require = new int[Mana.MAX];
		public string Detail;
		public int RequireSkill;
	}
}

