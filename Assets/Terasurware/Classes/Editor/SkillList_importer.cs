using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;
using System.Xml.Serialization;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

public class SkillList_importer : AssetPostprocessor {
	private static readonly string filePath = "Assets/Resources/ExcelData/Xls/SkillList.xls";
	private static readonly string exportPath = "Assets/Resources/ExcelData/Xls/SkillList.asset";
	private static readonly string[] sheetNames = { "Sheet1", };
	
	static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		foreach (string asset in importedAssets) {
			if (!filePath.Equals (asset))
				continue;
				
			SkillList data = (SkillList)AssetDatabase.LoadAssetAtPath (exportPath, typeof(SkillList));
			if (data == null) {
				data = ScriptableObject.CreateInstance<SkillList> ();
				AssetDatabase.CreateAsset ((ScriptableObject)data, exportPath);
				data.hideFlags = HideFlags.NotEditable;
			}
			
			data.sheets.Clear ();
			using (FileStream stream = File.Open (filePath, FileMode.Open, FileAccess.Read)) {
				IWorkbook book = new HSSFWorkbook (stream);
				
				foreach(string sheetName in sheetNames) {
					ISheet sheet = book.GetSheet(sheetName);
					if( sheet == null ) {
						Debug.LogError("[QuestData] sheet not found:" + sheetName);
						continue;
					}

					SkillList.Sheet s = new SkillList.Sheet ();
					s.name = sheetName;
				
					for (int i=1; i <= sheet.LastRowNum; i++) {
						IRow row = sheet.GetRow (i);
						ICell cell = null;
						
						SkillList.Param p = new SkillList.Param ();
						
					cell = row.GetCell(0); p.ID = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(1); p.Key = (cell == null ? "" : cell.StringCellValue);
					cell = row.GetCell(2); p.Name = (cell == null ? "" : cell.StringCellValue);
					cell = row.GetCell(3); p.Active = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(4); p.Red = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(5); p.Blue = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(6); p.Green = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(7); p.Yellow = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(8); p.Black = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(9); p.Require[Mana.RED] = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(10); p.Require[Mana.BLUE] = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(11); p.Require[Mana.GREEN] = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(12); p.Require[Mana.YELLOW] = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(13); p.Require[Mana.BLACK] = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(14); p.Detail = (cell == null ? "" : cell.StringCellValue);
					cell = row.GetCell(15); p.RequireSkill = (int)(cell == null ? 0 : cell.NumericCellValue);
						s.list.Add (p);
					}
					data.sheets.Add(s);
				}
			}

			ScriptableObject obj = AssetDatabase.LoadAssetAtPath (exportPath, typeof(ScriptableObject)) as ScriptableObject;
			EditorUtility.SetDirty (obj);
		}
	}
}
