using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelDistortionController : MonoBehaviour {

	[SerializeField]
	private List<Material> mats;

	private int matIndex;
	[SerializeField]
	private Material targetMat;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		targetMat.SetFloat ("_Amount", Mathf.Abs( Mathf.Cos(Time.fixedTime * 1.5f)) * 0.05f);
		transform.Rotate (new Vector3(1.0f, 1.0f, 1.0f));
	}
}
