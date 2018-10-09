using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(AudioSource))]
public abstract class Gun : MonoBehaviour {

	public Vector3 forward { get; set; }
	public GameObject projectile;

	public int MaxClipSize = 0;
	public int ProjectilePoolSize = 5;
	public Vector2 ProjectileRotOffset = new Vector2(0, 90);

	protected Vector3 Direction = Vector3.zero;
	protected int Bullets = 0;

	protected List<GameObject> ProjectileObjects = new List<GameObject>();
	protected Queue<Projectile> ProjectilePool = new Queue<Projectile>();


	public abstract void Fire ();
	public abstract void Reload ();

	public void Initialize() {
		if (projectile == null)
			InitializeProjectilePool(Resources.Load<GameObject>("Projectiles/Projectile"));
		else
			InitializeProjectilePool(projectile);
	}

	protected void InitializeProjectilePool(GameObject prefab) {
		// Create projectile gameobjects
		for (int i = 0; i < ProjectilePoolSize; i++) {
			GameObject go = GameObject.Instantiate(prefab);
			ProjectileObjects.Add(go);
			Projectile pro = go.GetComponent<Projectile>();
			ProjectilePool.Enqueue(pro);
		}
	}

	protected virtual bool FireProjectile() {
		Projectile pro = ProjectilePool.Dequeue();
		pro.Fire(transform.position, transform.forward);
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
