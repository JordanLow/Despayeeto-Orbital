using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
	[SerializeField] float jumpSpeed = 4f;
	[SerializeField] float moveSpeed = 10f;
	
	private CharacterController controller;
	private Vector3 curVel;
	
	void Start()
    {
        controller = GetComponent<CharacterController>();
		curVel = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
		if (curVel.y > Physics.gravity.y) curVel.y += Physics.gravity.y * Time.deltaTime;
        controller.Move(curVel * Time.deltaTime);
    }
	
	public void OnMove(InputValue val) {
		Vector2 direction = val.Get<Vector2>() * moveSpeed;
		curVel.x = direction.x;
		curVel.z = direction.y;
	}
	
	public void OnJump() {
		curVel.y = jumpSpeed;
	}
	
	public void OnPan(InputValue val) {
		Vector2 angle = val.Get<Vector2>();
	}
}
