﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    
	public GameObject cargoCrane;
    public float speed;
    public float distance;
    public float zoomSpeed;
    public float rotationSpeed;

	private GameObject cargo;
    private Rigidbody rb;
	private LineRenderer lr;
    private Vector3 offset;
    private float xR;
    private float yR;
    private float zR;
	private bool isHoldingCargo;

    // Use this for initialization
	//cargo crane is set to cargo when you are not holding anything to allow for regular first person movement
    void Start () {
		cargo = cargoCrane;
        offset = -1*(transform.position - cargo.transform.position);
        xR = 0;
        yR = 0;
        zR = 0;
        rb = GetComponent<Rigidbody>();
		lr = cargoCrane.GetComponent<LineRenderer> ();
		lr.positionCount = 0;
    }
	
	// Update is called once per frame
	void Update () {
        float moveH = Input.GetAxis("HorizontalAD");
        float moveV = Input.GetAxis("VerticalSW");
        float moveZ = Input.GetAxis("ZAxis");
        float roll = Input.GetAxis("Roll");

        Vector3 movementH = transform.right * moveH;
        Vector3 movementV = transform.forward * moveV;
        Vector3 movementZ = transform.up * moveZ;
        rb.velocity = (movementH + movementV + movementZ) * speed * Time.deltaTime;

        zR += roll;

		//handle cargo interaction
		if (Input.GetKeyDown (KeyCode.P)) {
			dropOrPickupCargo ();
		}

		//check if connection is broken
		if (isHoldingCargo) {
			RaycastHit hit;
			Physics.Raycast(transform.position, transform.forward, out hit);
			if (hit.transform.gameObject != cargo)
			{
				cargo.GetComponent<CargoController> ().respawnCargo ();
				dropOrPickupCargo ();
			}
		}
    }

    void LateUpdate() {
		if (isHoldingCargo) {
            UpdateRotation(offset);
            transform.rotation = Quaternion.LookRotation(cargo.transform.position - transform.position);
            transform.Rotate(0, 0, zR);
			//draw laser
			Vector3[] segment = new Vector3[2];
			segment [0] = cargoCrane.transform.position;
			segment [1] = cargo.transform.position;
			lr.SetPositions (segment);
		}
        else {
            UpdateRotation(transform.forward);
            transform.rotation = Quaternion.LookRotation(cargo.transform.position - transform.position);
            transform.Rotate(0, 0, zR);
        }
    }

    void UpdateRotation(Vector3 forward) {
        float moveV = Input.GetAxis("Horizontal");
        float moveH = Input.GetAxis("Vertical");
        float zoom = Input.GetAxis("Zoom");

        xR = -1 * moveH * Time.deltaTime * rotationSpeed;
        yR = moveV * Time.deltaTime * rotationSpeed;
        Quaternion yRotation = Quaternion.AngleAxis(yR, transform.up);
        Quaternion xRotation = Quaternion.AngleAxis(xR, transform.right);
        //Quaternion zRotation = Quaternion.AngleAxis(zR, new Vector3(0, 0, 1));
        Vector3 newOffset = new Vector3();
        newOffset = forward;
        newOffset = xRotation * newOffset;
        newOffset = yRotation * newOffset;
        //newOffset = zRotation * newOffset;
        newOffset.Normalize();
        distance += zoom * zoomSpeed * Time.deltaTime;
        distance = Mathf.Clamp(distance, 5, 25);
        newOffset *= distance;
        cargo.transform.position = transform.position + newOffset;
        //print(cargo.transform.position.ToString());
        offset = newOffset;
        //cargo.transform.rotation = Quaternion.LookRotation(-1*(cargo.transform.position - transform.position));
    }

	public void dropOrPickupCargo(){
		//if is not holding object, pick up object in crosshairs
		//if is holding, drop cargo
		if (!isHoldingCargo) {
			Debug.Log ("Pickup cargo");
			RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit))
            {
                GameObject newCargo = hit.transform.gameObject;
                distance = hit.distance;
                if (newCargo.GetComponent<CargoController>() != null)
                {
                    CargoController cargoCont = newCargo.GetComponent<CargoController>();
                    if (cargoCont.pickup())
                    {
                        cargo = newCargo;
						cargoCrane.transform.position = new Vector3 (0, -1, 0);
						lr.positionCount = 2;
                        isHoldingCargo = true;
                    }
                }
            }
		} else {
			if (cargo.GetComponent<CargoController> ().putDown ()) {
                cargo = cargoCrane;
				lr.positionCount = 0;
				isHoldingCargo = false;
			}
		}
	}
}
