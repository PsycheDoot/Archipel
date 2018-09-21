using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
public class SoundOnHit : MonoBehaviour {

	public AudioClip ac;

	// Use this for initialization
	void Start () {
		//AudioSource aus = gameObject.GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Play() {
		AudioSource aus = gameObject.GetComponent<AudioSource>();
		aus.pitch = Random.Range(-0f, 2f);
		aus.PlayOneShot(ac, 1);
	}

	private void OnCollisionEnter (Collision collision) {
		if (collision.relativeVelocity.magnitude > Vector3.one.magnitude) {
			Play();
		}
	}
}
