using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CameraManager : MonoBehaviour {

	public static CameraManager Instance { get; private set; }

	[SerializeField]
	private float fieldOfViewAtRest = 40f;
	[SerializeField]
	private float fieldOfViewModifier = 2f;
	[SerializeField]
	private float fieldOfViewDamper = 1f;

	private CinemachineVirtualCamera virtualCamera;
	private CinemachineBasicMultiChannelPerlin virtualCameraNoise;
	private float endOfShakeTime = 0f;
	private float startOfShakeEaseOutTime = 0f;
	private float shakeAmplitude = 0f;
	private bool shaking;
	private Rigidbody2D avatarRigidbody;

	private void Awake() {
		if (Instance == null) {
			Instance = this;
		} else if (Instance != this) {
			Destroy(gameObject);
		}
		DontDestroyOnLoad(gameObject);
		virtualCamera = GetComponent<CinemachineVirtualCamera>();
		virtualCameraNoise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
	}

	// Start is called before the first frame update
	void Start() {
		GameObject avatar = GameObject.FindGameObjectWithTag("Player");
		if (avatar) {
			virtualCamera.Follow = avatar.transform;
			virtualCamera.LookAt = avatar.transform;
			avatarRigidbody = avatar.GetComponent<Rigidbody2D>();
		}
	}

	void Update() {
		ControlShake();
		ControlFieldOfView();
	}

	//Eases out and stops the camera shake when its planned duration ellapsed
	private void ControlShake() {
		if (Time.time < endOfShakeTime) { //Camera is still in its shake phase
			if (Time.time > (startOfShakeEaseOutTime)) {  //Ease out
				virtualCameraNoise.m_AmplitudeGain = Mathf.Lerp(shakeAmplitude, 0f, (Time.time - startOfShakeEaseOutTime) / (endOfShakeTime - startOfShakeEaseOutTime));
			}
		} else if (shaking) { //Stop the shake
			virtualCameraNoise.m_AmplitudeGain = 0f;
			shaking = false;
		}
	}

	//Controls the camera field
	private void ControlFieldOfView() {
		if (avatarRigidbody) {
			if (virtualCamera.m_Lens.Orthographic) {
				virtualCamera.m_Lens.OrthographicSize = Mathf.MoveTowards(virtualCamera.m_Lens.OrthographicSize, fieldOfViewAtRest + avatarRigidbody.velocity.magnitude * fieldOfViewModifier, fieldOfViewDamper * Time.deltaTime);
			} else {
				virtualCamera.m_Lens.FieldOfView = Mathf.MoveTowards(virtualCamera.m_Lens.FieldOfView, fieldOfViewAtRest + avatarRigidbody.velocity.magnitude * fieldOfViewModifier, fieldOfViewDamper * Time.deltaTime);
			}
		}
	}

	//Begins a camera shake with a specified duration, amplitude, frequency and easeOutDuration
	public void ShakeCamera(float duration, float amplitude, float frequency, float easeOutDuration) {
		startOfShakeEaseOutTime = Mathf.Max(Time.time + duration - easeOutDuration, 0f);
		if (endOfShakeTime < Time.time + duration) {
			endOfShakeTime = Time.time + duration;
		}
		if (virtualCameraNoise.m_AmplitudeGain < amplitude) {
			shakeAmplitude = amplitude;
			virtualCameraNoise.m_AmplitudeGain = amplitude;
		}
		if (virtualCameraNoise.m_FrequencyGain < frequency) {
			virtualCameraNoise.m_FrequencyGain = frequency;
		}
		shaking = true;
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(CameraManager))]
public class CameraManagerEditor : Editor {
	public override void OnInspectorGUI() {
		var cameraManager = (CameraManager)target;
		if (cameraManager == null) {
			return;
		}

		if (GUILayout.Button("Test camera shake")) {
			if (Application.isPlaying) {
				cameraManager.ShakeCamera(1.5f, 5f, 5f, 0.5f);
			}
		}
		DrawDefaultInspector();
	}
}
#endif