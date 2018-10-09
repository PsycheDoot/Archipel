using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChuckAudioListener : MonoBehaviour {

	Vector3 lastPos;
	float lastRot;
	SoundManager sm;
	// Use this for initialization
	void Start () {
		sm = GameObject.Find("SoundManager").GetComponent<SoundManager>();
		sm.SetPosAudioListener(transform.position.x, transform.position.y, transform.position.z);
		sm.SetRotAudioListener(transform.eulerAngles.y * Mathf.Deg2Rad);
		lastPos = transform.position;
		lastRot = transform.eulerAngles.y;
	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log("Audio Listener Pos: " + lastPos);
		if (lastPos != transform.position) {
			//Debug.Log("update pos");
			sm.SetPosAudioListener(transform.position.x, transform.position.y, transform.position.z);
			lastPos = transform.position;
		}
		if (lastRot != transform.eulerAngles.y) {
			Debug.Log(transform.eulerAngles.y);
			sm.SetRotAudioListener(transform.eulerAngles.y * Mathf.Deg2Rad);
			lastRot = transform.eulerAngles.y;
		}
	}
}
