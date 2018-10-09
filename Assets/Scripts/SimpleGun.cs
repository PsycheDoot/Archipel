using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleGun : Gun {

	//public Voice(int clipSize) : base(clipSize) {
	//	source = gameObject.GetComponent<AudioSource>();
	//	LoadAudioClips();
	//	Reload();
	//}

	void Start () {
		Initialize();
		Reload();
	}

	public override void Reload () {
		SetBullets(MaxClipSize);
	}

	public override void Fire () {
			if (SubtractBullet()) {
				FireProjectile();
			}
		
	}
}


