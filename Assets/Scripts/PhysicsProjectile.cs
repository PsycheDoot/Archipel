using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[RequireComponent(typeof(Rigidbody))]
//[RequireComponent(typeof(Collider))]
public class PhysicsProjectile : Projectile {
	void Start () {
		rigidBody = gameObject.GetComponent<Rigidbody>();
	}

	void FixedUpdate () {
		if (currentState == State.alive) {
			// Do nothing?
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

	public override void Fire (Vector3 pos, Vector3 dir) {
		//Debug.Log("Firing projectile");

		currentState = State.alive;
		Timer = 0;
		TimerOn = true;

		StartPos = pos;
		StartVel = dir.normalized * MagVelocity;

		gameObject.transform.position = pos;
		Debug.Log(StartVel);
		rigidBody.velocity = StartVel;
	}

	protected void Die () {
		Debug.Log("Died");
		currentState = State.waiting;
		//gameObject.transform.position = StartPos;
		TimerOn = false;
		rigidBody.Sleep();
	}

	//protected Vector3 GetProjectileVelocityInit () {
	//	Vector2 rad = Rotation * Mathf.Deg2Rad;
	//	return new Vector3(
	//			MagVelocity * Mathf.Cos(rad.x) * Mathf.Cos(rad.y),
	//			MagVelocity * Mathf.Sin(rad.x),
	//			MagVelocity * Mathf.Cos(rad.x) * Mathf.Sin(rad.y));
	//}

	new private void OnCollisionEnter (Collision collision) {
		if (currentState == State.alive) {
			//Debug.Log("hit");
			Die();
			//Vector3 vel = GetProjectilePosition(Timer) - transform.position;
			//rigidBody.velocity = vel;
			//Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
			//if (rb != null) rb.AddForceAtPosition(vel * 10f, collision.transform.position, ForceMode.Impulse);
			//SoundOnHit soh = collision.gameObject.GetComponent<SoundOnHit>();
			//if (soh != null) soh.Play();
		}
	}
}
