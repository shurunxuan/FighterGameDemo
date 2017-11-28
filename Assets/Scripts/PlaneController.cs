using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaneController : MonoBehaviour
{

    private Rigidbody rb;

    private Transform ForcePointLeftTransform;
    private Transform ForcePointRightTransform;
    private Transform ForcePointFrontTransform;
    private Transform ForcePointBackTransform;

    public float rollFactor;
    public float pitchFactor;
    public float speedFactor;
    public float maxSpeed;

    // Being targeted by an NPC
    public bool beingTargeted = false;

    private float _beingTargetedTimer = 0;


    // Use this for initialization
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        ForcePointLeftTransform = transform.Find("ForcePointLeft");
        ForcePointRightTransform = transform.Find("ForcePointRight");
        ForcePointFrontTransform = transform.Find("ForcePointFront");
        ForcePointBackTransform = transform.Find("ForcePointBack");
    }

    void FixedUpdate()
    {
        // If not being targeted for a while, clear beingTargeted flag
        _beingTargetedTimer += Time.fixedDeltaTime;
        if (_beingTargetedTimer > 0.1f)
        {
            beingTargeted = false;
        }
    }

    void Update()
    {
        Vector3 upVector = transform.localToWorldMatrix.MultiplyVector(Vector3.up).normalized;
        Vector3 frontVector = transform.localToWorldMatrix.MultiplyVector(Vector3.forward).normalized;

        float horizontalAxis = Input.GetAxis("Horizontal");
        float verticalAxis = Input.GetAxis("Vertical");
        float accelerateAxis = Input.GetAxis("Accelerate");
        float brakeAxis = Input.GetAxis("Brake");

        rb.velocity = frontVector * Vector3.Dot(rb.velocity, frontVector);
        if (rb.velocity.magnitude > maxSpeed) rb.velocity = frontVector * maxSpeed;

        Vector3 leftPosition = ForcePointLeftTransform.position;
        Vector3 rightPosition = ForcePointRightTransform.position;
        Vector3 frontPosition = ForcePointFrontTransform.position;
        Vector3 backPosition = ForcePointBackTransform.position;

        //float wingSpeed = rb.GetPointVelocity(leftPosition).magnitude;

        //text.text = (rb.velocity.magnitude).ToString();


        rb.AddForceAtPosition(upVector * horizontalAxis * rollFactor, leftPosition, ForceMode.Force);
        rb.AddForceAtPosition(-upVector * horizontalAxis * rollFactor, rightPosition, ForceMode.Force);
        rb.AddForceAtPosition(-upVector * verticalAxis * pitchFactor, frontPosition, ForceMode.Force);
        rb.AddForceAtPosition(upVector * verticalAxis * pitchFactor, backPosition, ForceMode.Force);
        rb.AddForceAtPosition(frontVector * (1 - accelerateAxis) * speedFactor, frontPosition, ForceMode.Force);
        rb.drag = 0.5f * (1f - brakeAxis);
    }

    void BeingTargeted()
    {
        // Set beingTargeted flag to true
        beingTargeted = true;
        // Reset _beingTargetedTimer
        _beingTargetedTimer = 0;
    }

    void DamageTaken(float damage)
    {
        Debug.Log("Player taken damage " + damage);
    }
}
