using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Presets : MonoBehaviour
{
    void Awake()
    {
		//Settings mainly to improve performance on phones
		Physics.autoSimulation = false;
		Application.targetFrameRate = -1; //phones generally pre-set max framerate to 30
		QualitySettings.vSyncCount = 0;

	#if UNITY_EDITOR
		QualitySettings.SetQualityLevel(0);
	#endif

		//Hack: Destroy the debug object created by the SRP
        GameObject debugObject = GameObject.Find("[Debug Updater]");
		if(debugObject != null)
			Destroy(debugObject);
    }
}
