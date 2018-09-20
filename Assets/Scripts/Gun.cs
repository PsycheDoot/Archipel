using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public abstract class Gun : MonoBehaviour {

	public int MaxClipSize = 0;
	public int ProjectilePoolSize = 5;
	public Vector2 ProjectileRotOffset = new Vector2(0, 90);

	protected Vector3 Direction = Vector3.zero;
	protected int Bullets = 0;

	protected List<GameObject> ProjectileObjects = new List<GameObject>();
	protected Queue<Projectile> ProjectilePool = new Queue<Projectile>();


	public abstract void Fire ();
	public abstract void Reload ();

	protected void InitializeProjectilePool(GameObject prefab) {
		// Create projectile gameobjects
		for (int i = 0; i < ProjectilePoolSize; i++) {
			GameObject go = GameObject.Instantiate(prefab);
			ProjectileObjects.Add(go);
			Projectile pro = go.GetComponent<Projectile>();
			ProjectilePool.Enqueue(pro);
		}
	}

	protected bool FireProjectile() {
		Projectile pro = ProjectilePool.Dequeue();
		pro.transform.position = transform.position;
		pro.SetDirection(new Vector2(-transform.parent.eulerAngles.x + ProjectileRotOffset.x, 
			-transform.parent.parent.eulerAngles.y + ProjectileRotOffset.y));
		pro.Fire();
		ProjectilePool.Enqueue(pro);
		return true;
	}

	protected bool SubtractBullet() {
		if (Bullets < 0 || Bullets == 0) {
			Bullets = 0;
			return false;
		}
		Bullets -= 1;
		return true;
	}

	protected bool SetBullets(int num) {
		if (num > MaxClipSize || num <= 0) return false;
		Bullets = num;
		return true;
	}
}
