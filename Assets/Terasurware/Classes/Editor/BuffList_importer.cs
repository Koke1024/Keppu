using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;
using System.Xml.Serialization;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

public class BuffList_importer : AssetPostprocessor {
	private static readonly string filePath = "Assets/ExcelData/Xls/BuffList.xls";
	private static readonly string exportPath = "Assets/ExcelData/BuffList.asset";
	private static readonly string[] sheetNames = { "Sheet1", };
	
	static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		foreach (string asset in importedAssets) {
			if (!filePath.Equals (asset))
				continue;
				
			BuffList data = (BuffList)AssetDatabase.LoadAssetAtPath (exportPath, typeof(BuffList));
			if (data == null) {
				data = ScriptableObject.CreateInstance<BuffList> ();
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

					BuffList.Sheet s = new BuffList.Sheet ();
					s.name = sheetName;
				
					for (int i=1; i<= sheet.LastRowNum; i++) {
						IRow row = sheet.GetRow (i);
						ICell cell = null;
						
						BuffList.Param p = new BuffList.Param ();
						
					cell = row.GetCell(0); p.BuffID = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(1); p.IconName = (cell == null ? "" : cell.StringCellValue);
					cell = row.GetCell(2); p.Detail = (cell == null ? "" : cell.StringCellValue);
					cell = row.GetCell(3); p.CumulativeFlag = (int)(cell == null ? 0 : cell.NumericCellValue);
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
