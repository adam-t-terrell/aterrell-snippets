using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forces : MonoBehaviour {

	[HideInInspector]
	public Vector3 gameObjectSreenPoint;
	[HideInInspector]
	public Vector3 mousePreviousLocation;

	[HideInInspector]
	public Vector3 mouseCurLocation;

	private Rigidbody rb;

	void Start()
	{
		rb = GetComponent<Rigidbody> ();
	}

	void OnMouseDown()
	{
		//This grabs the position of the object in the world and turns it into the position on the screen
		gameObjectSreenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
		//Sets the mouse pointers vector3
		mousePreviousLocation = new Vector3(Input.mousePosition.x, Input.mousePosition.y, gameObjectSreenPoint.z);
	}

	[HideInInspector]
	public Vector3 force;
	[HideInInspector]
	public Vector3 objectCurrentPosition;
	[HideInInspector]
	public Vector3 objectTargetPosition;

	public float topSpeed = 10;
	void OnMouseDrag()
	{
		mouseCurLocation = new Vector3(Input.mousePosition.x, Input.mousePosition.y, gameObjectSreenPoint.z);
		force = mouseCurLocation - mousePreviousLocation;//Changes the force to be applied
		mousePreviousLocation = mouseCurLocation;
	}

	public void OnMouseUp()
	{
		//Makes sure there isn't a ludicrous speed
		if (rb.velocity.magnitude > topSpeed)
			force = rb.velocity.normalized * topSpeed;
	}

	public void FixedUpdate()
	{
		rb.velocity = force;
	}
}
