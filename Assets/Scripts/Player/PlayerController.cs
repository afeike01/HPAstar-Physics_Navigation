using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody))]
[RequireComponent (typeof (BoxCollider))]

public class PlayerController : MonoBehaviour 
{

    public float moveSpeed = 10.0f;
    public float rotationSpeed = 5.0f;
    public float gravity = 9.8f;
    public float maxVelocityChange = 10.0f;
    public bool canJump = true;
    public float jumpHeight = 2.0f;
    private bool isGrounded = false;

    private Rigidbody rB;

	// Use this for initialization
	void Awake () 
    {
        rB = GetComponent<Rigidbody>();
        rB.freezeRotation = true;
        //rB.useGravity = false;
	}
	
	// Update is called once per frame
	void Update () 
    {
	    
	}
    void FixedUpdate()
    {
        if (isGrounded)
        {
            //Movement
            Vector3 targetVelocity = ((transform.worldToLocalMatrix.MultiplyVector(transform.forward)) * Input.GetAxis("Vertical") * moveSpeed) + 
                ((transform.worldToLocalMatrix.MultiplyVector(transform.right)) * Input.GetAxis("Horizontal") * moveSpeed);
            targetVelocity = transform.TransformDirection(targetVelocity);
            targetVelocity *= moveSpeed;

            Vector3 velocity = rB.velocity;
            Vector3 velocityChange = (targetVelocity - velocity);
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0;
            rB.AddForce(velocityChange, ForceMode.VelocityChange);
            
            if (canJump && Input.GetButton("Jump"))
            {
                rB.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
            }
        }
        //Rotation
        rB.rotation = Quaternion.Euler(rB.rotation.eulerAngles + new Vector3(0, rotationSpeed * Input.GetAxis("Mouse X"), 0));

        //rB.AddForce(new Vector3(0, -gravity * rB.mass, 0));
        Vector3 downVector = transform.TransformDirection(-Vector3.up);
        if (Physics.Raycast(transform.position,downVector,2))
        {
            isGrounded = true;
        }
        else
            isGrounded = false;
    }
    
    private float CalculateJumpVerticalSpeed()
    {
        return Mathf.Sqrt(2 * jumpHeight * gravity);
    }
}
