using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

public class RadialMenu : MonoBehaviour
{
	private float radius = 60f;
	private int count = 0;
	// Use integers for now, switch to better rep when gearwheel becomes flexible
	public int att = 0;

    void OnEnable()
    {
		ArrangeRadialMenu();
    }

    private void ArrangeRadialMenu() {
		List<GameObject> buttons = new List<GameObject>();
		this.count = 0;
		foreach (Transform child in transform) {
			if (!child.gameObject.activeInHierarchy) continue;
			count++;
            buttons.Add(child.gameObject);
		}
		for (int i = 0; i < count; i++)
		{
			float angle = i * Mathf.PI * 2f / count;
			float x = Mathf.Cos(angle) * radius;
			float y = Mathf.Sin(angle) * radius;
			buttons[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
		}
	}

	public void OnGearSelect(InputAction.CallbackContext context) {
		if (!gameObject.activeInHierarchy) return;
		Vector2 dir = context.ReadValue<Vector2>();
		if (dir.magnitude < 0.9f) return;;
		float angle = Mathf.Atan2(dir.y, dir.x);
		if (angle < 0) angle = 2 * Mathf.PI + angle;
		att = Mathf.FloorToInt(((angle + (Mathf.PI / this.count)) % (2 * Mathf.PI)) / (2 * Mathf.PI / this.count));
		// Debug.Log(att + "|" + angle);
		for (int i = 0; i < this.count; i++) {
			if (i != att) transform.GetChild(i).GetComponent<Button>().interactable = false;
			if (i == att) transform.GetChild(i).GetComponent<Button>().interactable = true;
		}
	}
}
