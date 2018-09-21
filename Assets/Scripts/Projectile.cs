using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour {
	protected GameObject context;
	protected Rigidbody rigidBody;
	protected SphereCollider coll;


	private bool TimerOn = false;


	// Public Variables
	public float MagVelocity = 10;
	public float LiveTime = 2;
	public float Timer = 0f;


	//private float Time = 0;
	private Vector3 StartPos = Vector3.zero;
	private Vector2 Rotation = Vector2.zero;
	private Vector3 StartVel = Vector3.zero;
	private float Gravity = 9.8f;

	protected enum State {
		waiting,
		alive
	};
	protected State currentState = State.waiting;

	void Start () {
		rigidBody = gameObject.GetComponent<Rigidbody>();
	}

	void FixedUpdate () {
		if (currentState == State.alive) {
			gameObject.transform.position = GetProjectilePosition(Timer);
			rigidBody.Sleep();
		} else {
			//rigidBody.Sleep();
		}
		if (TimerOn) {
			Timer += Time.deltaTime;
			if (Timer > LiveTime) {
				Die();
			}
		}
	}

	public void SetDirection(Vector2 dir) {
		Rotation = dir;
	}

	public void Fire () {
		Debug.Log("Firing projectile");

		currentState = State.alive;
		Timer = 0;
		TimerOn = true;

		StartPos = gameObject.transform.position;
		StartVel = GetProjectileVelocityInit();
	}

	protected void Die () {
		Debug.Log("Died");
		currentState = State.waiting;
		//gameObject.transform.position = StartPos;
		TimerOn = false;
	}

	protected void Physics() {

	}

	protected Vector3 GetProjectileVelocityInit() {
		Vector2 rad = Rotation * Mathf.Deg2Rad;
		return new Vector3(
				MagVelocity * Mathf.Cos(rad.x) * Mathf.Cos(rad.y),
				MagVelocity * Mathf.Sin(rad.x),
				MagVelocity * Mathf.Cos(rad.x) * Mathf.Sin(rad.y) );
	}

	protected Vector3 GetProjectilePosition(float time) {
		return new Vector3(
			StartPos.x + (StartVel.x * time),
			StartPos.y + (StartVel.y * time) - (0.5f * Gravity * time * time),
			StartPos.z + (StartVel.z * time) );
	}

	private void OnCollisionEnter (Collision collision) {
		if (currentState == State.alive) {
			Debug.Log("hit");
			Die();
			Vector3 vel = GetProjectilePosition(Timer) - transform.position;
			rigidBody.velocity = vel;
			Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
			if (rb != null) rb.AddForceAtPosition(vel * 10f, collision.transform.position, ForceMode.Impulse);
			SoundOnHit soh = collision.gameObject.GetComponent<SoundOnHit>();
			if (soh != null) soh.Play();
		}
	}
}
