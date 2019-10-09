using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugDestroyer : MonoBehaviour
{
    void Awake()
    {
        GameObject debugObject = GameObject.Find("[Debug Updater]");
		if(debugObject != null)
			Destroy(debugObject);

		Physics.autoSimulation = false;
		Application.targetFrameRate = -1;
    }
}
