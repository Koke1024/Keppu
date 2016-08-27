using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Words : ScriptableObject
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
		
		public int TalkID;
		public int Step;
		public int Speaker;
		public int CharaID0;
		public int Face0;
		public int CharaID1;
		public int Face1;
		public int CharaID2;
		public int Face2;
		public int CharaID3;
		public int Face3;
		public int Bg;
		public string Text;
		public int Next;
		public string Sound;
		public string Function;
		public string SelectA;
		public int NextA;
		public string SelectB;
		public int NextB;
		public string SelectC;
		public int NextC;
		public string SelectD;
		public int NextD;
	}
}

