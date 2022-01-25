using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class StartManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void StartGame(InputAction.CallbackContext context) {
		if (context.started) {
			SceneManager.LoadScene("Gym_Ludo", LoadSceneMode.Single);
		}
	}
}
