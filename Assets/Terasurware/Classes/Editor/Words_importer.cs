using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;
using System.Xml.Serialization;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

public class Words_importer : AssetPostprocessor {
	private static readonly string filePath = "Assets/Resources/ExcelData/Xls/Words.xls";
	private static readonly string exportPath = "Assets/Resources/ExcelData/Xls/Words.asset";
	private static readonly string[] sheetNames = { "Sheet1", };
	
	static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		foreach (string asset in importedAssets) {
			if (!filePath.Equals (asset))
				continue;
				
			Words data = (Words)AssetDatabase.LoadAssetAtPath (exportPath, typeof(Words));
			if (data == null) {
				data = ScriptableObject.CreateInstance<Words> ();
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

					Words.Sheet s = new Words.Sheet ();
					s.name = sheetName;
				
					for (int i=1; i <= sheet.LastRowNum; i++) {
						IRow row = sheet.GetRow (i);
						ICell cell = null;
						
						Words.Param p = new Words.Param ();
						
					cell = row.GetCell(0); p.TalkID = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(1); p.Step = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(2); p.Speaker = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(3); p.CharaID0 = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(4); p.Face0 = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(5); p.CharaID1 = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(6); p.Face1 = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(7); p.CharaID2 = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(8); p.Face2 = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(9); p.CharaID3 = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(10); p.Face3 = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(11); p.Bg = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(12); p.Text = (cell == null ? "" : cell.StringCellValue);
					cell = row.GetCell(13); p.Next = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(14); p.Sound = (cell == null ? "" : cell.StringCellValue);
					cell = row.GetCell(15); p.Function = (cell == null ? "" : cell.StringCellValue);
					cell = row.GetCell(16); p.SelectA = (cell == null ? "" : cell.StringCellValue);
					cell = row.GetCell(17); p.NextA = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(18); p.SelectB = (cell == null ? "" : cell.StringCellValue);
					cell = row.GetCell(19); p.NextB = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(20); p.SelectC = (cell == null ? "" : cell.StringCellValue);
					cell = row.GetCell(21); p.NextC = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(22); p.SelectD = (cell == null ? "" : cell.StringCellValue);
					cell = row.GetCell(23); p.NextD = (int)(cell == null ? 0 : cell.NumericCellValue);
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
