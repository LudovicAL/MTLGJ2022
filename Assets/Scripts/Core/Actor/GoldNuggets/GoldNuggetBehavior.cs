using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoldNuggetBehavior : MonoBehaviour {

	[SerializeField]
	private AudioClip kechingClip;
	[SerializeField]
	private float speed = 7f;
	[SerializeField]
	private float pickUpDistance = 0.5f;
	private Transform target;
	private ParticleSystem particleSystemGlow;
	private ParticleSystem particleSystemDestroy;
	private SpriteRenderer spriteRenderer;
	private bool wasCollected;

	// Start is called before the first frame update
	void Start() {
		spriteRenderer = GetComponent<SpriteRenderer>();
		particleSystemGlow = GetComponent<ParticleSystem>();
		GameObject particleSystemDestroyGo = GameObject.Find("ParticleSystemDestroy");
		if (particleSystemDestroyGo) {
			particleSystemDestroy = particleSystemDestroyGo.GetComponent<ParticleSystem>();
		}
	}

    // Update is called once per frame
    void Update() {
		if (target) {
			if (Vector3.Distance(transform.position, target.position) < pickUpDistance && !wasCollected) {
				Collect();
			} else {
				transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
			}
		}
    }

	private void Collect() {
		wasCollected = true;
		spriteRenderer.enabled = false;
		if (particleSystemGlow) {
			Destroy(particleSystemGlow);
		}
		if (particleSystemDestroy) {
			particleSystemDestroy.Emit(20);
		}
		if (AudioManager.Instance && kechingClip) {
			AudioManager.Instance.PlayClip(kechingClip);
		}
		if (GameLoop.Instance) {
			GameLoop.Instance.IncrementScore();
		}
		GameObject.Destroy(this.gameObject, 5f);
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.CompareTag("Player")) {
			target = other.transform;
		}
	}
}
