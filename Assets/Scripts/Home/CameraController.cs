﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	private Vector3 cameraPosition;

	// Update is called once per frame
	void Update () {
		Vector3 v = Camera.main.GetComponent<Transform> ().position;
		Vector3 mouse=Input.mousePosition;

		if(mouse.y<Screen.height/6 && v.y>-2)
			Camera.main.GetComponent<Transform> ().position = new Vector3( v.x , v.y- (float)0.1, v.z);

		if(mouse.y>Screen.height-Screen.height/6 && v.y<2)
			Camera.main.GetComponent<Transform> ().position = new Vector3( v.x , v.y+ (float)0.1, v.z);
		
		if(mouse.x<Screen.width/4 && v.x>-4)
				Camera.main.GetComponent<Transform> ().position = new Vector3( v.x - (float)0.1, v.y, v.z);

		if(mouse.x>Screen.width-Screen.width/4 && v.x<4)
				Camera.main.GetComponent<Transform> ().position = new Vector3( v.x + (float)0.1, v.y, v.z);
		

	}


}
