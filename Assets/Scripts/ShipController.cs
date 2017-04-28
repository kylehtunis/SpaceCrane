﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    
	public GameObject cargoCrane;
    public float speed;
    public float distanceToHeldCargo;
    public float zoomSpeed;
    public float rotationSpeed;
	public float cargoPickupDistance;
	public float cargoMinDistanceHeld;
	public float cargoMaxDistanceHeld;

	private GameObject cargo;
    private Rigidbody rb;
	private LineRenderer lr;
    private Vector3 offset;
	private float cameraShakeTimeLeft;
	private Transform originalCameraPos;
	Vector3 beforeShakeCameraPos;
	private bool isCameraShaking;
    private float xR;
    private float yR;
    private float zR;
	private bool isHoldingCargo;
    private bool movementEnabled;

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
        movementEnabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        //handle camera shake effect
        if (isCameraShaking)
        {
            transform.localPosition = beforeShakeCameraPos + Random.insideUnitSphere * 0.7f;
            cameraShakeTimeLeft -= Time.deltaTime;
            if (cameraShakeTimeLeft < 0.0f)
            {
                isCameraShaking = false;
            }
        }
        if (movementEnabled)
        {
            float moveH = Input.GetAxis("HorizontalAD");
            float moveV = Input.GetAxis("VerticalSW");
            float moveZ = Input.GetAxis("ZAxis");
            float roll = -1 * Input.GetAxis("Roll");

            Vector3 movementH = transform.right * moveH;
            Vector3 movementV = transform.forward * moveV;
            Vector3 movementZ = transform.up * moveZ;
            rb.velocity = (movementH + movementV + movementZ) * speed * Time.deltaTime;

            zR = roll*rotationSpeed*Time.deltaTime;

            //handle cargo interaction
            if (Input.GetKeyDown(KeyCode.Space))
            {
                dropOrPickupCargo();
            }
        }

    }

	public void cameraShake(float duration){
		isCameraShaking = true;
		cameraShakeTimeLeft = duration;
		beforeShakeCameraPos = transform.localPosition;
	}


    void LateUpdate()
    {
        if (movementEnabled)
        {
            if (isHoldingCargo)
            {
                UpdateRotation(offset);
                transform.rotation = Quaternion.LookRotation(cargo.transform.position - transform.position, transform.up);
                transform.Rotate(0, 0, zR);
                //draw laser
                Vector3[] segment = new Vector3[2];
                segment[0] = cargoCrane.transform.position;
                segment[1] = cargo.transform.position;
                lr.SetPositions(segment);

                //check if connection is broken
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.forward, out hit))
                {
                    if (hit.transform.gameObject != cargo)
                    {
                        Debug.Log(hit.transform.gameObject.name);
                        CargoController temp = cargo.GetComponent<CargoController>();
                        dropOrPickupCargo();
                        temp.respawnCargo();
                        cameraShake(0.5f);
                    }
                }

            }
            else
            {
                UpdateRotation(transform.forward);
                transform.rotation = Quaternion.LookRotation(offset, transform.up);
                transform.Rotate(0, 0, zR);
            }
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
        newOffset = yRotation * newOffset;
        newOffset = xRotation * newOffset;
        //newOffset = zRotation * newOffset;
        newOffset.Normalize();
        distanceToHeldCargo += zoom * zoomSpeed * Time.deltaTime;
		distanceToHeldCargo = Mathf.Clamp(distanceToHeldCargo, cargoMinDistanceHeld, cargoMaxDistanceHeld);
        newOffset *= distanceToHeldCargo;
        if (isHoldingCargo)
        {
            cargo.transform.position = transform.position + newOffset;
        }
        //print(cargo.transform.position.ToString());
        offset = newOffset;
        //cargo.transform.rotation = Quaternion.LookRotation(-1*(cargo.transform.position - transform.position));
    }

	public void dropOrPickupCargo(){
		//if is not holding object, pick up object in crosshairs
		//if is holding, drop cargo
		if (!isHoldingCargo) {
			RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, cargoPickupDistance))
            {
                GameObject newCargo = hit.transform.gameObject;
                distanceToHeldCargo = (hit.transform.position - transform.position).magnitude;
                if (newCargo.GetComponent<CargoController>() != null)
                {
                    CargoController cargoCont = newCargo.GetComponent<CargoController>();
                    if (cargoCont.pickup())
                    {
                        cargo = newCargo;
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

    public void setMovementEnabled(bool set = true)
    {
        movementEnabled = set;
    }
}
