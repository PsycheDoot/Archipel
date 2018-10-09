using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sequencer : MonoBehaviour {
	public bool play = true;

	public ChuckAudioSource[] AudioSources = new ChuckAudioSource[12];

	private int bars = 2;
	private int notescale = 16;
	//public int steps { get { return stepsPerBeat * beatsPerBar * bars; } set { } }
	public int tempo = 120;

	private int stepCount = 0;
	private float stepTimer = 0f;
	private int[][] BeatGrid;

	// Use this for initialization
	void Start () {
		BeatGrid = new int[AudioSources.Length][];
		for (int i = 0; i < AudioSources.Length; i++) {
			BeatGrid[i] = new int[notescale * bars];
		}
		BeatGrid[0] = new int[] { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 };
		BeatGrid[1] = BeatGrid[0];
		BeatGrid[2] = new int[] { 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 1, 0 };
		BeatGrid[6] = new int[] { 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0,
								  0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		BeatGrid[7] = BeatGrid[6];
	}
	
	// Update is called once per frame
	void Update () {
		if (play) {
			stepTimer += Time.deltaTime;
			if (stepTimer > StepTime()) {
				stepTimer = 0;
				Step();
			}
		}
	}

	void Step() {
		for(int i = 0; i < AudioSources.Length; i++) {
			int arrLen = BeatGrid[i].Length;
			if (BeatGrid[i][stepCount%arrLen] > 0) {
				//AudioSources[i].PlayOnce();
				SimpleGun sg = AudioSources[i].gameObject.GetComponentInChildren<SimpleGun>();
				if (sg != null) sg.Fire();
			}
		}
		stepCount = (stepCount + 1) % (bars * notescale);
		//Debug.Log(stepCount);
	}

	public void Play() {
		Step();
		play = true;
	}

	public float BeatTime() { return 60f / tempo; }
	public float StepTime () { return (BeatTime() * 4f) / notescale; }
}
