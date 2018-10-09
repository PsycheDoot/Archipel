using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChuckAudioSource : MonoBehaviour {

	SoundManager sm;
	public string SampleName = "";
	private string InstanceName = "";
	Vector3 lastPos;

	// Use this for initialization
	void Start () {
		InstanceName = Convert.ToString(gameObject.GetInstanceID());
		sm = GameObject.Find("SoundManager").GetComponent<SoundManager>();
		sm.AddSampleAudioSource(InstanceName, SampleName);
		sm.SetPosAudioSource(InstanceName, transform.position.x, transform.position.y, transform.position.z);
		lastPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (lastPos != transform.position) {
			//Debug.Log("update pos");
			sm.SetPosAudioSource(InstanceName, transform.position.x, transform.position.y, transform.position.z);
			lastPos = transform.position;
		}
	}

	public void PlayOnce() {
		sm.PlayAudioSource(InstanceName, 1);
	}
}
