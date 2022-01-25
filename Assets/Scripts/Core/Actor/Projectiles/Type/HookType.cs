using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookType : ProjectileType {

	// Start is called before the first frame update
	void Start() {
		Rigidbody2D rb = GetComponent<Rigidbody2D>();
		if (rb) {
			transform.rotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.up, rb.velocity));
		}
    }

    // Update is called once per frame
    void Update() {
        
    }

	[SerializeField] private int projectileId = -1;
	public override int ProjectileId {
		get {
			if (projectileId == -1)
				return this.gameObject.GetInstanceID();
			else
				return projectileId;
		}
		set {
			projectileId = value;
		}
	}
}
