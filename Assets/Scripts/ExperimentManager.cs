using UnityEngine;
using System.Collections;
using System.IO;

public class ExperimentManager : MonoBehaviour {
	
	// Enumerations for task, device, and handedness
	public enum Task {TranslationRotation, TranslationIntensity};
	public enum Device {Mouse, Leap};
	public enum Handedness {Left, Right, Unspecified};
	
	// Current task, device, and handedness settings
	public Device currentDevice;
	public Task currentTask;
	public Handedness handedness;
	public Vector3 currentLightTarget;
	
	public int subjectID;
	public int currentImage = -1;
	public int currentTrial = -1;
	public int handedSelection = 8;
	public int tasksPerTrial = 4;
	public int totalTrials = 3;
	public int[,] trials = {{0,1,2,3},{4,5,6,7},{8,9,10,11}};
	public int[] trialList;
	
	public struct TrialData {
		public int trialNumber;
		public Task task;
		public Device device;
		public Vector3 translation;
		public Vector3 orientation;
		public double intensity;
		public double time;
	}
	
	public struct SubjectData {
		public int subjectID;
		public Handedness handedness;
		public TrialData[] trialData;
	}
	
	private SubjectData subjectData;
	
	// s_Instance is used to cache the instance found in the scene so we don't have to look it up every time.
    private static ExperimentManager s_Instance = null;
	
	// This defines a static instance property that attempts to find the manager object in the scene and
    // returns it to the caller.
    public static ExperimentManager instance {
        get {
            if (s_Instance == null) {
                // This is where the magic happens.
                //  FindObjectOfType(...) returns the first AManager object in the scene.
                s_Instance =  FindObjectOfType(typeof (ExperimentManager)) as ExperimentManager;
            }
 
            // If it is still null, create a new instance
            if (s_Instance == null) {
                GameObject obj = new GameObject("ExperimentManager");
                s_Instance = obj.AddComponent(typeof (ExperimentManager)) as ExperimentManager;
                Debug.Log ("Could not locate an ExperimentManager object. \n ExperimentManager was Generated Automatically.");
				DontDestroyOnLoad(s_Instance);
            }
            return s_Instance;
        }
    }
	
	public static void ResetInstance() {
		s_Instance = null;
	}
 
    // Ensure that the instance is destroyed when the game is stopped in the editor.
    void OnApplicationQuit() {
        s_Instance = null;
    }
	
	void PrintTrials(int[,] trials) {
		string display = "trials: {";
		
		for (int i = 0; i < totalTrials; i++) {
			display += "{";
			for (int j = 0; j < tasksPerTrial; j++) {
				display += trials[i,j];
				if (tasksPerTrial - j > 1)
					display += ",";
			}
			display += "}";
			if (totalTrials - i > 1)
				display += ",";
		}
		
		display += "}";
		
		Debug.Log (display);
	}
	
	void Start() {
		// Randomize trial order
		System.Random r = new System.Random();
		for (int i = totalTrials - 1; i > 0; i--) {
			int j = r.Next(i + 1);
			// Debug.Log("swapping trial " + i + " with trial " + j);
			for (int k = 0; k < tasksPerTrial; k++) {
				int temp = trials[i,k];
				trials[i,k] = trials[j,k];
				trials[j,k] = temp;
			}			
		}		
		
		// Randomize task order
		for (int i = 0; i < totalTrials; i++) {
			// Debug.Log ("randomizing trial " + i);
			for (int j = tasksPerTrial - 1; j > 0; j--) {				
				int k = r.Next (j + 1);
				// Debug.Log("swapping task " + j + " with task " + k);
				int temp = trials[i,j];
				trials[i,j] = trials[i,k];
				trials[i,k] = temp;
				// PrintTrials(trials);
			}
		}
		
		// Flatten ordering and save in trialList
		trialList = new int[totalTrials * tasksPerTrial];
		for (int i = 0; i < totalTrials; i++) {
			for (int j = 0; j < tasksPerTrial; j++) {
				int index = i * tasksPerTrial + j;
				trialList[index] = trials[i,j];
			}			
		}
		
		// Setup subject data
		subjectData = CreateEmptySubjectData(tasksPerTrial * totalTrials);
	}
	
	private SubjectData CreateEmptySubjectData(int count) {
		SubjectData empty;
		
		empty.subjectID = subjectID;
		empty.handedness = handedness;
		empty.trialData = new TrialData[count];
		
		return empty;
	}
	
	private void PersistTrialData() {
		int lastTrial = currentTrial;
		
		print ("Trial Data:");
		subjectData.trialData[lastTrial].trialNumber = lastTrial;
		// print ("Time: " + subjectData.trialData[lastTrial].trialNumber);
		subjectData.trialData[lastTrial].time = SpotLight.selectedTime;
		// print ("Time: " + subjectData.trialData[lastTrial].time);
		subjectData.trialData[lastTrial].task = currentTask;
		// print ("Task: " + subjectData.trialData[lastTrial].task);
		subjectData.trialData[lastTrial].device = currentDevice;
		// print ("Device: " + subjectData.trialData[lastTrial].device);
		PersistTrialLightData(lastTrial);
	}
	
	private void PersistTrialLightData(int trial) {
		GameObject obj = GameObject.Find ("NewSpotLight");
		Light light = (Light) GameObject.Find ("NewSpotLight/light").light;
		PersistTrialLightSpatialData(trial, obj);
		PersistTrialLightIntensityData(trial, light);
	}
	
	private void PersistTrialLightSpatialData(int trial, GameObject light) {
		subjectData.trialData[trial].translation = light.transform.localPosition;
		// Debug.Log ("Light Position: " + subjectData.trialData[trial].translation);
		subjectData.trialData[trial].orientation = light.transform.localRotation.eulerAngles;
		// Debug.Log ("Light Rotation: " + subjectData.trialData[trial].orientation);
	}
	
	private void PersistTrialLightIntensityData(int trial, Light light) {
		subjectData.trialData[trial].intensity = light.intensity;
		// Debug.Log ("Light Intensity: " + subjectData.trialData[trial].intensity);
	}
	
	private void WriteTrialDataToFile(string filename) {
		StreamWriter writer;
		writer  = new StreamWriter(filename);
		
		// ID, Handedness, Trial Number, Device, Task, Time, Pos X, Pos Y, Pos Z, Rot X, Rot Y, Rot Z, Intensity
		for (int i = 0; i < totalTrials * tasksPerTrial; i++) {
			string output = "";
			output += subjectData.subjectID + ",\t";
			output += subjectData.handedness + ",\t";
			output += subjectData.trialData[i].trialNumber + ",\t";
			output += subjectData.trialData[i].device + ",\t";
			output += subjectData.trialData[i].task + ",\t";
			output += subjectData.trialData[i].time + ",\t";
			output += subjectData.trialData[i].translation.x + ",\t";
			output += subjectData.trialData[i].translation.y + ",\t";
			output += subjectData.trialData[i].translation.z + ",\t";
			output += subjectData.trialData[i].orientation.x + ",\t";
			output += subjectData.trialData[i].orientation.y + ",\t";
			output += subjectData.trialData[i].orientation.z + ",\t";
			output += subjectData.trialData[i].intensity;
			writer.WriteLine(output);
		}
		
		writer.Flush();
		writer.Close();
	}
	
	public void AdvanceLevel() {
		// Save trial data before advancing
		if (currentTrial >= 0)
				PersistTrialData();
		
		currentTrial++;
		
		if (currentTrial < (totalTrials * tasksPerTrial)) {			
			Application.LoadLevel("Leap_Project");
			// Debug.Log ("setting up trial: " + currentTrial);
			UpdateReferenceVariables();
		} else if (currentTrial >= 12) {
			// Save data
			WriteTrialDataToFile(subjectData.subjectID + ".csv");
			Application.LoadLevel("End");
		}
	}
	
	// Update the current device and task
	private void UpdateReferenceVariables() {
		UpdateDevice();
		UpdateTask();
		UpdateImage();
		UpdateLightTarget();
	}
	
	// Task modes:
	// --------------------------|
	//              Mouse | Leap |
	// --------------------------|
	// Trans./Rot. |   0  |   2  |
	// Trans./Int. |   1  |   3  |
	// --------------------------|
	
	// Update the current device based on the current trial
	private void UpdateDevice() {
		if (trialList[currentTrial] % 2 == 0)
			currentTask = Task.TranslationRotation;
		else
			currentTask = Task.TranslationIntensity;
	}
	
	// Update the current task based on the current trial
	private void UpdateTask() {
		if (trialList[currentTrial] % tasksPerTrial < tasksPerTrial / 2)
			currentDevice = Device.Mouse;
		else
			currentDevice = Device.Leap;
	}
	
	private void UpdateImage() {
		currentImage = trialList[currentTrial] / tasksPerTrial;
	}
	
	private void UpdateLightTarget() {
		switch (currentImage) {
			case 0:
				currentLightTarget = new Vector3(-1.554065f, 2.502152f, 1.987616f);
				break;
			case 1:
				currentLightTarget = new Vector3(2.439487f, 3.929598f, 5.44588f);
				break;
			case 2:
				currentLightTarget = new Vector3(0.2521598f, 2.502152f, 1.575271f);
				break;
			default:
				currentLightTarget = new Vector3(0f, 0f, 0f);
				break;
		}
	}

	private void SetupLevel(int newLevel) {		
		for (int i = 0; i < totalTrials; ++i) {
			if (i != newLevel) {
				string targetTag = "Trial" + i;
				GameObject[] toDeactivate = GameObject.FindGameObjectsWithTag(targetTag);
				foreach (GameObject obj in toDeactivate) {
					obj.SetActive(false);
				}
			}
		}
	}
	
	private void OnLevelWasLoaded (int newLevel) {
		if (Application.loadedLevelName == "Leap_Project" && currentTrial >= 0) {
			SetupLevel(trialList[currentTrial]/tasksPerTrial);
		}
	}
	
	private void Update() {
		CheckForKeyUps();
	}
	
	private void CheckForKeyUps() {
		bool changeLeap = false;
		if (Input.GetKeyUp(KeyCode.Z)) {
			currentDevice = Device.Leap;
			currentTask = Task.TranslationRotation;
			changeLeap = true;
		} else if (Input.GetKeyUp(KeyCode.X)) {
			currentDevice = Device.Leap;
			currentTask = Task.TranslationIntensity;
			changeLeap = true;
		} else if (Input.GetKeyUp(KeyCode.C)) {
			currentDevice = Device.Mouse;
			currentTask = Task.TranslationRotation;
			changeLeap = true;
		} else if (Input.GetKeyUp(KeyCode.V)) {
			currentDevice = Device.Mouse;
			currentTask = Task.TranslationIntensity;
			changeLeap = true;
		}
		if (changeLeap) {
			int leapMode = currentTask == Task.TranslationRotation ? 0 : 1;
			LeapInput.ChangeMode(currentDevice == Device.Leap ? leapMode : -5);
			changeLeap = false;
		}
	}
}