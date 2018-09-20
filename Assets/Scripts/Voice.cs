using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class Voice : Gun {

	[SerializeField] public AudioClip[] audioClips;
	public AudioSource source;

	//public Voice(int clipSize) : base(clipSize) {
	//	source = gameObject.GetComponent<AudioSource>();
	//	LoadAudioClips();
	//	Reload();
	//}

	void Start () {
		source = gameObject.GetComponent<AudioSource>();
		InitializeProjectilePool(Resources.Load<GameObject>("Projectiles/Projectile"));
		LoadAudioClips();
		Reload();
	}


	private void LoadAudioClips() {
		audioClips = Resources.LoadAll<AudioClip>("Gun Sounds");
	}

	public override void Reload () {
		SetBullets(MaxClipSize);
	}

	public override void Fire () {
		if (audioClips != null && audioClips.Length > 0) {
			if (SubtractBullet()) {
				int i = (int)UnityEngine.Random.Range(0f, audioClips.Length);
				source.clip = audioClips[i];
				source.Play();
				FireProjectile();
			}
		}
	}
}
