using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ChuckAudioSource))]
[RequireComponent(typeof(Rigidbody))]
public class SoundOnHit : MonoBehaviour {

	ChuckAudioSource audioSource;
	public string sourceName = "BassKick";
	float startTime;
	Vector3 originalScale;

	// Use this for initialization
	void Start () {
		//AudioSource aus = gameObject.GetComponent<AudioSource>();
		audioSource = gameObject.GetComponent <ChuckAudioSource>();
		startTime = Time.time;
		originalScale = transform.localScale;
	}

	// Update is called once per frame
	float dur = .25f;
	void Update () {
		float t = (Time.time - startTime) / (dur);
		if (t <= 1f) {
			transform.localScale = originalScale + originalScale * Mathf.Abs(Mathf.Sin(Mathf.PI*t))*.1f;
		} else {
			transform.localScale = originalScale;
		}
	}

	public void Play() {
		//AudioSource aus = gameObject.GetComponent<AudioSource>();
		//aus.pitch = Random.Range(-0f, 2f);
		//aus.PlayOneShot(ac, 1);
		audioSource.PlayOnce();
	}

	public void Play(double pitch) {
		//AudioSource aus = gameObject.GetComponent<AudioSource>();
		//aus.pitch = (float)pitch;
		//aus.PlayOneShot(ac, 1);
		Play();
	}

	public void Play(AudioClip audioClip) {
		//AudioSource aus = gameObject.GetComponent<AudioSource>();
		//aus.pitch = Random.Range(-0f, 2f);
		//aus.PlayOneShot(audioClip, 1);
		Play();
	}

	private void AnimateHit() {
		startTime = Time.time;
	}

	private void OnCollisionEnter (Collision collision) {
		if (collision.relativeVelocity.magnitude > 1) {
			Play(1 + collision.relativeVelocity.magnitude * .1);
			gameObject.GetComponent<Rigidbody>().velocity = Vector3.Scale(collision.contacts[0].normal, collision.relativeVelocity) * .5f;
			AnimateHit();
		}
	}
}
