using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	float radius = 3.5f;
	float time = 0;

	void Update () {
		time += Time.deltaTime;
		this.transform.position = new Vector3 (radius * Mathf.Cos (time / 2.0f), radius/ 1.5f * Mathf.Sin (time / 3.0f), (radius + 1.0f) * Mathf.Sin (time / 2.0f));
		this.transform.LookAt (Vector3.zero, Vector3.up);
	}
}
