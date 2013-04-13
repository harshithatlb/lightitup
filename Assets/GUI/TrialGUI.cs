using UnityEngine;
using System.Collections;

public class TrialGUI : MonoBehaviour {
	void OnGUI() {
		ExperimentManager instance = ExperimentManager.instance;
		
		// Render 'Next Trial' button
		if(GUI.Button(new Rect(Screen.width - 110,Screen.height - 30,100,20), "Next")) {
			instance.AdvanceLevel();
		}
		
		string imageLabel = "Scene: ";
		string deviceLabel = "Device: ";
		string taskLabel = "Task: ";
		
		string timeLabel = "Selected Time: " + SpotLight.selectedTime;
		
		if (instance.currentImage == -1)
			imageLabel += "Training";
		else
			imageLabel += instance.currentImage;
		
		if (instance.currentTask == ExperimentManager.Task.TranslationRotation)
			taskLabel += "Position + Rotation";
		else
			taskLabel += "Position + Intensity";
		
		if (instance.currentDevice == ExperimentManager.Device.Mouse)
			deviceLabel += "Mouse";
		else
			deviceLabel += "Leap";
		
		GUIStyle labelStyle = new GUIStyle();
		labelStyle.normal.textColor = Color.white;		
		labelStyle.alignment = TextAnchor.MiddleLeft;
		
		GUI.Label(new Rect(10, Screen.height - 80, 0, 0), timeLabel,labelStyle);
		GUI.Label(new Rect(10, Screen.height - 60, 0, 0), imageLabel,labelStyle);
		GUI.Label(new Rect(10, Screen.height - 40, 0, 0), deviceLabel,labelStyle);
		GUI.Label(new Rect(10, Screen.height - 20, 0, 0), taskLabel,labelStyle);
	}
}
