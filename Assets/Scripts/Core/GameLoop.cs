using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoop : MonoBehaviour {

	public static GameLoop Instance { get; private set; }

	[SerializeField]
	private int delayBeforeGameEnd = 3;
	private bool gameHasEnded;
	private int score = 0;
	private int nbAsteroidAsStart;
	private TextMeshProUGUI scoreText;

	private void Awake() {
		if (Instance == null) {
			Instance = this;
		} else if (Instance != this) {
			Destroy(gameObject);
		}
		DontDestroyOnLoad(gameObject);
	}

	// Start is called before the first frame update
	void Start() {
		GameObject scoreGo = GameObject.Find("TextScore");
		if (scoreGo) {
			scoreText = scoreGo.GetComponent<TextMeshProUGUI>();
		}
		nbAsteroidAsStart = GameObject.FindGameObjectsWithTag("Asteroid").Length;
		scoreText.text = "0/" + nbAsteroidAsStart.ToString();
	}

	// Update is called once per frame
	void Update() {
		
    }

	public void IncrementScore() {
		score++;
		if (scoreText) {
			scoreText.text = score.ToString() + "/" + nbAsteroidAsStart.ToString();
		}
		if (score >= nbAsteroidAsStart) {
			GameEnded("YOU WON");
		}
	}

	public void GameEnded(string message) {
		if (!gameHasEnded) {
			gameHasEnded = true;
			if (message.Length > 0) {
				GameObject endGameTextGo = GameObject.Find("EndGameText");
				if (endGameTextGo) {
					TextMeshProUGUI textComponent = endGameTextGo.GetComponent<TextMeshProUGUI>();
					if (textComponent) {
						textComponent.text = message;
						textComponent.enabled = true;
					}
				}
			}
			StartCoroutine(EndingCoroutine());
		}
	}

	IEnumerator EndingCoroutine() {
		//Print the time of when the function is first called.
		Debug.Log("Game will end in " + delayBeforeGameEnd + " seconds");

		//yield on a new YieldInstruction that waits for 5 seconds.
		yield return new WaitForSeconds(delayBeforeGameEnd);

		//After we have waited 5 seconds print the time again.
		UnityEngine.SceneManagement.SceneManager.LoadScene("Menu", LoadSceneMode.Single);
	}
}
