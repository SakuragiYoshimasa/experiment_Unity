﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour {

	Texture2D emmitionTex;
	int texSize = 1024;

	void Start () {
		emmitionTex = new Texture2D (texSize, texSize);

		for(int i = 0; i < texSize; i ++){
			for(int j = 0; j < texSize; j++){
				if (i % 64 == 0 || j % 64 == 0) {
					emmitionTex.SetPixel (i, j, new Color (1.0f, 1.0f, 1.0f));	
				} else {
					emmitionTex.SetPixel (i, j, new Color (32f / 255f, 32f / 255f, 32f / 255f));	
				}
			}
		}

		emmitionTex.Apply ();

		Material fMat = gameObject.GetComponent<Renderer> ().material;
		fMat.SetTexture ("_EmissionMap", emmitionTex);
		fMat.SetColor ("_EmissionColor", new Color(1.0f, 1.0f, 1.0f, 1.0f));

	}
	

	void Update () {

		//gameObject.GetComponent<Renderer> ().material = fMat;
	}
}
