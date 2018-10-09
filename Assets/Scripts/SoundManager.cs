using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		OSCHandler.Instance.Init();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void PlayAudioSource(string sourceName, float volume) {
		List<object> data = new List<object>();
		data.Add(sourceName);
		data.Add(volume);
		OSCHandler.Instance.SendMessageToClient("ChuckAudioEngine", "Archipel/AudioEngine/PlayAudioSource", data);
	}

	public void AddSampleAudioSource (string sourceName, string sampleName) {
		List<object> data = new List<object>();
		data.Add(sourceName);
		data.Add(sampleName);
		OSCHandler.Instance.SendMessageToClient("ChuckAudioEngine", "Archipel/AudioEngine/AddSampleAudioSource", data);
	}

	public void SetPosAudioSource (string sourceName, float x, float y, float z) {
		List<object> data = new List<object>();
		data.Add(sourceName);
		data.Add(x);
		data.Add(y);
		data.Add(z);
		OSCHandler.Instance.SendMessageToClient("ChuckAudioEngine", "Archipel/AudioEngine/SetPosAudioSource", data);
	}

	public void SetPosAudioListener (float x, float y, float z) {
		List<object> data = new List<object>();
		data.Add(x);
		data.Add(y);
		data.Add(z);
		OSCHandler.Instance.SendMessageToClient("ChuckAudioEngine", "Archipel/AudioEngine/SetPosAudioListener", data);
	}

	public void SetRotAudioListener (float x) {
		List<object> data = new List<object>();
		data.Add(x);
		OSCHandler.Instance.SendMessageToClient("ChuckAudioEngine", "Archipel/AudioEngine/SetRotAudioListener", data);
	}
}
