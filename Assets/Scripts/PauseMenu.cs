using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PauseMenu : MonoBehaviour
{
	[SerializeField] private GameObject UI;
	[SerializeField] private Button defaultButton;
	private float prevScale = 1f;
    private bool paused = false;
	private InputActionMap controls;
	
	void Start() {
		controls = GetComponent<PlayerInput>().actions.FindActionMap("Player");
	}
	
	public void OnPause(InputAction.CallbackContext context) {
		if (!context.started) return;
		TogglePause();
	}
	
	public void TogglePause() {
		if (!paused) {
			Debug.Log(controls);
			controls.Disable();
			paused = true;
			prevScale = Time.timeScale;
			Time.timeScale = 0f;
			UI.SetActive(true);
			EventSystem.current.SetSelectedGameObject(null);
			EventSystem.current.SetSelectedGameObject(defaultButton.gameObject);
		} else {
			controls.Enable();
			paused = false;
			Time.timeScale = prevScale;
			UI.SetActive(false);
		}
	}
	
	public void QuitGame()
    {
        Debug.Log("Quitting game");
		#if UNITY_EDITOR
		EditorApplication.isPlaying = false;
		#else
        Application.Quit();
		#endif
    }
}
