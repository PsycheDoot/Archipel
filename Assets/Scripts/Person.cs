using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Person : MonoBehaviour {

	GameObject Gun;
	Voice piece;

	// Use this for initialization
	void Start () {
		piece = gameObject.GetComponentInChildren<Voice>();
		//piece = Gun.GetComponent<Voice>();
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Mouse0)) {
			piece.Fire();
		}
		if (Input.GetKeyDown(KeyCode.R)) {
			piece.Reload();
		}
	}
}
