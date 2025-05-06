using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
	[SerializeField] float jumpSpeed = 4f;
	[SerializeField] float moveSpeed = 8f;
	[SerializeField] float turnSpeed = 10f;
	
	private CharacterController controller;
	private Vector3 relativeDir;
	
	void Start()
    {
        controller = GetComponent<CharacterController>();
		relativeDir = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
		Vector3 cameraForward = Camera.main.transform.forward; 
		cameraForward.y = 0;
        Vector3 cameraRight = Camera.main.transform.right; 
		cameraRight.y = 0;
		if (relativeDir.y > Physics.gravity.y) relativeDir.y += Physics.gravity.y * Time.deltaTime;
		Vector3 curDir = relativeDir.z * cameraForward + relativeDir.x * cameraRight;
		curDir.y = relativeDir.y;
		controller.Move(curDir * Time.deltaTime);
		Vector3 curFaceDirection = curDir;
		curFaceDirection.y = 0;
		if (curFaceDirection == Vector3.zero) return;
		transform.rotation = Quaternion.LookRotation(curFaceDirection);
    }
	
	public void OnMove(InputValue val) {
		Vector2 direction = val.Get<Vector2>() * moveSpeed;
		relativeDir.x = direction.x;
		relativeDir.z = direction.y;
	}
	
	public void OnJump() {
		relativeDir.y = jumpSpeed;
	}
}
