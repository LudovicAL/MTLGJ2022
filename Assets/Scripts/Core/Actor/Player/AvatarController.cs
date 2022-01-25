using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AvatarController : MonoBehaviour {

	private enum JetpackPhase {
		off,
		starting,
		thrusting,
		stoping
	}

	[Header("Movement")]
	[Tooltip("Maximum force of a thrust impulse")]
	[SerializeField]
	private float maxThrustSpeed = 200f;
	[Tooltip("Time required to achieve smallest delay between thrust impulses")]
	[SerializeField]
	private float timeToFullThrust = 0.25f;
	[Tooltip("Time required to stop the audio effects")]
	[SerializeField]
	private float timeToFullStop = 2f;
	[Tooltip("Variation applied to time between thrust impulses")]
	[SerializeField]
	private float variationToThrustTimes = 0.3f;
	[Tooltip("Minimum delay between thrusts")]
	[SerializeField]
	private float minDelayBetweenThrusts = 0.2f;
	[Tooltip("Maximum delay between thrusts")]
	[SerializeField]
	private float maxDelayBetweenThrusts = 1f;
	[Tooltip("Maximum velocity achievable with the jetpack")]
	[SerializeField]
	private float maxVelocity = 5f;
	[Tooltip("Number of particles emitted with each burst")]
	[SerializeField]
	private int numberOfParticleByBurst = 3;
	[Tooltip("Quantity of fuel used by the jetpack per second")]
	[SerializeField]
	private float fuelConsumption = 1f;
	[Tooltip("Quantity of fuel resplenished per second")]
	[SerializeField]
	private float refuelSpeed = 0.5f;
	[Tooltip("Fuel tank size")]
	[SerializeField]
	private float fuelTankSize = 5f;
	private float availableFuel;
	private float thrustProcedureBeginTime = 0f;   //The time the thrust procedure started
	private Vector2 thrustDirection;    //The direction of the thrust
	private float timeOfNextThrustImpulse = 0f;    //The time of the next thrust impulse
	private Rigidbody2D rb;
	private Image fuelBar;
	private JetpackPhase jetpackPhase = JetpackPhase.off;
	private float delayBetweenThustsAtBeginOfProcedure;
	private float currentDelayBetweenThrusts;
	private ParticleSystem particleSystemFire;
	private ParticleSystem particleSystemSmoke;
	private Transform aimLineTransform;
	private Transform circleTransform;
	private Quaternion currentBodyRotation;

	[Space(10)]
	[Header("Audio")]
	[Tooltip("Minimum pitch of a sound")]
	[SerializeField]
	private float minPitch = 0.8f;
	[Tooltip("Maximum pitch of a sound")]
	[SerializeField]
	private float maxPitch = 1.2f;
	[Tooltip("AudioClip used for the thrust sound effect")]
	[SerializeField]
	private AudioClip thrustAudioClip;

	private void Awake() {
		rb = this.GetComponent<Rigidbody2D>();
		availableFuel = fuelTankSize;
		currentDelayBetweenThrusts = maxDelayBetweenThrusts;
		Transform particleSystemFireTransform = transform.Find("Fire");
		if (particleSystemFireTransform) {
			particleSystemFire = particleSystemFireTransform.GetComponent<ParticleSystem>();
		}
		Transform particleSystemSmokeTransform = transform.Find("Smoke");
		if (particleSystemSmokeTransform) {
			particleSystemSmoke = particleSystemSmokeTransform.GetComponent<ParticleSystem>();
		}
		aimLineTransform = transform.Find("Line");
		circleTransform = transform.Find("Circle");
	}

	// Start is called before the first frame update
	void Start() {
		GameObject fuelBarGo = GameObject.Find("FuelBar");
		if (fuelBarGo) {
			fuelBar = fuelBarGo.GetComponent<Image>();
		}
    }

	void Update() {
		computeCurrentBodyRotation();
		RotateBody();
		AimAtMouse();
		ManageFuel();
	}

	// Update is called once per frame
	void FixedUpdate() {
		Move();
	}

	public void Die() {
		circleTransform.gameObject.SetActive(false);
		aimLineTransform.gameObject.SetActive(false);
		if (GameLoop.Instance) {
			GameLoop.Instance.GameEnded("YOU LOST");
		}
	}

	//Applies a force in a direction, duh
	private void ApplyForce(Vector2 desiredDirection, float desiredSpeed) {
		float acceleration = desiredSpeed / Time.deltaTime;
		float force = rb.mass * acceleration;
		rb.AddForce(desiredDirection * force);
	}

	//Manages the available fuel and displays it
	private void ManageFuel() {
		if (availableFuel > 0f && (jetpackPhase == JetpackPhase.starting || jetpackPhase == JetpackPhase.thrusting)) {
			availableFuel -= fuelConsumption * Time.deltaTime;
		} else if (availableFuel < fuelTankSize && (jetpackPhase == JetpackPhase.stoping || jetpackPhase == JetpackPhase.off)) {
			availableFuel += refuelSpeed * Time.deltaTime;
		}
		if (fuelBar) {
			fuelBar.fillAmount = availableFuel / fuelTankSize;
		}
	}

	//Moves the avatar
	private void Move() {
		switch (jetpackPhase) {
			case JetpackPhase.off:
				break;
			case JetpackPhase.starting:
				PerformStartingJetpack();
				break;
			case JetpackPhase.thrusting:
				PerformThrustingJetpack();
				break;
			case JetpackPhase.stoping:
				PerformStopingJetpack();
				break;
		}
	}

	private void AddMovementForce() {
		Vector2 accel = Helper.ComputeAccelerationToReachDestination(thrustDirection * 9999f, transform, maxVelocity, 1f, rb, timeToFullThrust);
		if (accel.magnitude > maxThrustSpeed) {
			accel.Normalize();
			accel *= maxThrustSpeed;
		}
		rb.AddForce(accel);
	}

	private void PerformStartingJetpack() {
		if (thrustDirection.magnitude == 0f) {
			StopJetpack();
			return;
		}
		if (availableFuel == 0f) {
			StopJetpack();
			return;
		}
		if (Time.time > (thrustProcedureBeginTime + timeToFullThrust)) {
			StabilizeJetPack();
			return;
		}
		if (Time.time < timeOfNextThrustImpulse) {
			return;
		}
		AddMovementForce();
		PlayJetpackSoundEffect();
		SetTimeOfNextThrustImpulse();
		BurstSmokeParticles();
		BurstFireParticles();
	}

	private void PerformThrustingJetpack() {
		if (thrustDirection.magnitude == 0f) {
			StopJetpack();
			return;
		}
		if (availableFuel <= 0f) {
			StopJetpack();
			return;
		}
		if (Time.time < timeOfNextThrustImpulse) {
			return;
		}
		AddMovementForce();
		PlayJetpackSoundEffect();
		SetTimeOfNextThrustImpulse();
		BurstSmokeParticles();
		BurstFireParticles();
	}

	private void PerformStopingJetpack() {
		if (thrustDirection.magnitude != 0f && availableFuel > 1f) {
			StartJetpack();
			return;
		}
		if (Time.time < timeOfNextThrustImpulse) {
			return;
		}
		if (Time.time > (thrustProcedureBeginTime + timeToFullStop)) {
			TurnOffJetPack();
		}
		PlayJetpackSoundEffect();
		SetTimeOfNextThrustImpulse();
		BurstSmokeParticles();
	}

	//Sets the time of the next thrust impulse
	private void SetTimeOfNextThrustImpulse() {
		switch (jetpackPhase) {
			case JetpackPhase.starting:
				currentDelayBetweenThrusts = Mathf.Lerp(delayBetweenThustsAtBeginOfProcedure, minDelayBetweenThrusts, (Time.time - thrustProcedureBeginTime) / timeToFullThrust);
				timeOfNextThrustImpulse = Time.time + currentDelayBetweenThrusts + Random.Range(-variationToThrustTimes, variationToThrustTimes);
				break;
			case JetpackPhase.thrusting:
				timeOfNextThrustImpulse = Time.time + 0.2f + Random.Range(-variationToThrustTimes, variationToThrustTimes);
				break;
			case JetpackPhase.stoping:
				currentDelayBetweenThrusts = Mathf.Lerp(delayBetweenThustsAtBeginOfProcedure, maxDelayBetweenThrusts, (Time.time - thrustProcedureBeginTime) / timeToFullStop);
				timeOfNextThrustImpulse = Time.time + currentDelayBetweenThrusts + Random.Range(-variationToThrustTimes, variationToThrustTimes);
				break;
		}
	}

	//Play one jetpack sound effect
	private void PlayJetpackSoundEffect() {
		if (AudioManager.Instance && thrustAudioClip) {
			float pitchModifier = 0.75f + 0.5f * (1f- (currentDelayBetweenThrusts - minDelayBetweenThrusts) / (maxDelayBetweenThrusts - minDelayBetweenThrusts));
			AudioManager.Instance.PlayClip(thrustAudioClip, false, 0, 0.2f, minPitch * pitchModifier, maxPitch * pitchModifier);
		}
	}

	void OnDrawGizmos() {
		// Draw a yellow sphere at the transform's position
		if (Application.isPlaying) {
			Gizmos.color = Color.red;
			Gizmos.DrawRay(transform.position, -rb.velocity);
			Gizmos.color = Color.blue;
			Gizmos.DrawRay(transform.position, thrustDirection);
		}
	}

	private void StartJetpack() {
		//Debug.Log("Jetpack starting");
		thrustProcedureBeginTime = Time.time;
		jetpackPhase = JetpackPhase.starting;
		delayBetweenThustsAtBeginOfProcedure = currentDelayBetweenThrusts;
	}

	private void StabilizeJetPack() {
		//Debug.Log("Jetpack stabilizing");
		jetpackPhase = JetpackPhase.thrusting;
		currentDelayBetweenThrusts = minDelayBetweenThrusts;
	}

	private void StopJetpack() {
		//Debug.Log("Jetpack stopping");
		thrustProcedureBeginTime = Time.time;
		jetpackPhase = JetpackPhase.stoping;
		delayBetweenThustsAtBeginOfProcedure = currentDelayBetweenThrusts;
	}

	private void TurnOffJetPack() {
		//Debug.Log("Jetpack is off");
		jetpackPhase = JetpackPhase.off;
		currentDelayBetweenThrusts = maxDelayBetweenThrusts;
	}

	private void AimAtMouse() {
		if (aimLineTransform) {
			Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
			Vector3 directionVector = (mousePos - transform.position).normalized;
			float angle = Mathf.Atan2(directionVector.x, directionVector.y) * Mathf.Rad2Deg;
			aimLineTransform.rotation = Quaternion.AngleAxis(-angle, Vector3.forward);
		}
	}

	private void computeCurrentBodyRotation() {
		if (thrustDirection.magnitude > 0) {
			currentBodyRotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.up, thrustDirection));
		}
	}

	private void RotateBody() {
		if (circleTransform) {
			circleTransform.transform.rotation = currentBodyRotation;
		}
	}

	public void BurstFireParticles() {
		if (particleSystemFire) {
			particleSystemFire.transform.rotation = currentBodyRotation;
			particleSystemFire.Emit(numberOfParticleByBurst);
		}
	}

	public void BurstSmokeParticles() {
		if (particleSystemSmoke) {
			particleSystemSmoke.transform.rotation = currentBodyRotation;
			particleSystemSmoke.Emit(numberOfParticleByBurst / 2);
		}
	}

	//Triggered when the player press a button to move his avatar
	public void OnMove(InputAction.CallbackContext context) {
		thrustDirection = context.ReadValue<Vector2>().normalized;
		if (context.started && availableFuel > 0f && (jetpackPhase == JetpackPhase.off || jetpackPhase == JetpackPhase.stoping)) {
			StartJetpack();
		} else if (context.canceled && (jetpackPhase == JetpackPhase.starting || jetpackPhase == JetpackPhase.thrusting)) {
			StopJetpack();
		}
	}

	private void OnCollisionEnter2D(Collision2D collision) {
		if (collision.gameObject.layer == 5) {  //Projectiles
			Die();
			return;
		}
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(AvatarController))]
public class AvatarControllerEditor : Editor {
	public override void OnInspectorGUI() {
		var avatarController = (AvatarController)target;
		if (avatarController == null) {
			return;
		}

		if (GUILayout.Button("Test particle burst")) {
			avatarController.BurstSmokeParticles();
			avatarController.BurstFireParticles();
		}
		DrawDefaultInspector();
	}
}
#endif
