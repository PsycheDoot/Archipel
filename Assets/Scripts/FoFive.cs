using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoFive : Gun {

	private string[] onomotopoea = { "Blaw!", "Blaka! Blaka!", "Skrrrraatt!", "Skibbi-di-pa-pa!", "DOOT DOOT DOOT!"};

	public FoFive(int clipSize) {
		
	}

	public override void Reload () {
		SetBullets(MaxClipSize);
	}

	public override void Fire () {
		SubtractBullet();
	}
}
