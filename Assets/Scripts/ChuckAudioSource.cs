using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChuckAudioSource : MonoBehaviour {

	SoundManager sm;
	public string SampleName = "";
	private string InstanceName = "";

	// Use this for initialization
	void Start () {
		InstanceName = Convert.ToString(gameObject.GetInstanceID());
		sm = GameObject.Find("SoundManager").GetComponent<SoundManager>();
		sm.AddSampleAudioSource(InstanceName, SampleName);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void PlayOnce() {
		sm.PlayAudioSource(InstanceName, 1);
	}
}
