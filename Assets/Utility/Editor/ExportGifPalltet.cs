using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Utility {
	public class CaptureImagePalltet : EditorWindow {
		public static CaptureImagePalltet instance;
		private static bool capturing = false;
		private static int captureNum = 0;
		private static string gifName = string.Empty;
		private static string folderPath = "gifMake";
		private static int CAPTURE_MAX_NUM = 60;

		public static void ShowPallet(){
			instance = (CaptureImagePalltet)EditorWindow.GetWindow (typeof(CaptureImagePalltet));
			instance.titleContent = new GUIContent("Capture for GIF Exportion");
		}

		private void OnEnable(){}
		private void OnDisable(){}
		private void OnDestroy(){}

		private void OnGUI(){

			CAPTURE_MAX_NUM = EditorGUILayout.IntField ("Image Num" ,CAPTURE_MAX_NUM);

			if (GUILayout.Button("CapturingStart", GUILayout.Width(100), GUILayout.Height(50)) && !capturing && Application.isPlaying)
			{
				capturing = true;
				captureNum = 0;
			}
			EditorGUILayout.LabelField ("convert -delay 6 gifMake/*.png gifMake/out.gif");
		}

		private void Update(){
			if(capturing){

				if (captureNum < CAPTURE_MAX_NUM) {
					Application.CaptureScreenshot(folderPath + "/" + captureNum + ".png");
				} else {
					
					var projectFolder = System.IO.Directory.GetCurrentDirectory();
					Debug.Log (projectFolder);
					capturing = false;
				}
				captureNum++;
			}
		}
	}
}
