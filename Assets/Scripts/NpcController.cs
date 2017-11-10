using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcController : MonoBehaviour
{
    public GameObject ShootingIndicator;

    // Being seen by the camera
    public bool inScreen = false;
    // Being targeted by player (camera)
    public bool targeted = false;
    // Being locked on by player
    private bool _lockedOn = false;
    public bool lockedOn

    {
        get { return _lockedOn; }
        set
        {
            _lockedOn = value;
            if (!value)
            {
                State = NpcState.FindingTarget;
            }
        }
    }
    // Being targeted by an NPC
    public bool beingTargeted = false;

    public float rollFactor;
    public float pitchFactor;
    public float speedFactor;
    public float maxSpeed;

    public float maneuverRate = 1;

    public GameObject Targeting;

    public float health = 100.0f;

    public float _roll = 0; // (-1, 1) = (left, right)
    public float _pitch = 0; // (-1, 1) = (up, down)
    public float _accel = 1; // (-1, 1) = (full throttle, no throttle)
    public float _brake = 1; // (-1, 1) = (full brake, no brake)

    private Rigidbody rb;

    private Transform ForcePointLeftTransform;
    private Transform ForcePointRightTransform;
    private Transform ForcePointFrontTransform;
    private Transform ForcePointBackTransform;

    private Collider[] hitColliders;

    private GameObject _missile;

    private int _missileCount = 0;

    private float _targetingTimer = 0;
    private float _targetTimeOut = 0;
    private float _beingTargetedTimer = 0;
    public enum NpcState
    {
        FindingTarget,
        FollowingTarget,
        EmergencyDodging,
        Dodging,
        Targeting,
        Firing
    }

    public NpcState State = NpcState.FindingTarget;

    // Use this for initialization
    void Start()
    {
        State = NpcState.FindingTarget;

        rb = gameObject.GetComponent<Rigidbody>();
        ForcePointLeftTransform = transform.Find("ForcePointLeft");
        ForcePointRightTransform = transform.Find("ForcePointRight");
        ForcePointFrontTransform = transform.Find("ForcePointFront");
        ForcePointBackTransform = transform.Find("ForcePointBack");

        _missile = transform.Find("Missile").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        // If not being targeted for a while, clear beingTargeted flag
        if (beingTargeted)
        {
            _beingTargetedTimer += Time.deltaTime;
            if (_beingTargetedTimer > 0.1f)
            {
                beingTargeted = false;
                State = NpcState.FindingTarget;
            }
        }

        if (Targeting == null)
        {
            // Target is gone, return to FindingTarget
            State = NpcState.FindingTarget;
        }

        // Being targeted, need emergency doging
        if (beingTargeted || _lockedOn)
        {
            State = NpcState.EmergencyDodging;
        }

        hitColliders = Physics.OverlapSphere(transform.position, 15f);
        // Find an obstacle, need to dogde it
        if (hitColliders.Length > 1)
        {
            State = NpcState.Dodging;
        }

        // State Machine
        if (State == NpcState.FindingTarget)
        {
            _targetTimeOut = 0;

            if (tag == "EnemyContainer")
            {
                GameObject player = GameObject.FindGameObjectWithTag("PlayerContainer");
                GameObject[] targets = GameObject.FindGameObjectsWithTag("FriendContainer");
                float minDis = (player.transform.position - transform.position).magnitude;
                Targeting = player;
                foreach (var target in targets)
                {
                    float dis = (target.transform.position - transform.position).magnitude;
                    if (dis < minDis)
                    {
                        minDis = dis;
                        Targeting = target;
                    }
                }
            }
            else if (tag == "FriendContainer")
            {
                GameObject[] targets = GameObject.FindGameObjectsWithTag("EnemyContainer");
                float minDis = Single.PositiveInfinity;
                foreach (var target in targets)
                {
                    float dis = (target.transform.position - transform.position).magnitude;
                    if (dis < minDis)
                    {
                        minDis = dis;
                        Targeting = target;
                    }
                }
            }

            if (Targeting != null)
            {
                State = NpcState.FollowingTarget;
            }
            else
            {
                _accel = -1;
            }
        }
        else if (State == NpcState.FollowingTarget)
        {
            _targetTimeOut += Time.deltaTime;
            if (_targetTimeOut >= 30)
            {
                State = NpcState.FindingTarget;
            }

            if (Targeting != null)
            {
                Vector3 targetPosition = Targeting.transform.position - Targeting.transform.localToWorldMatrix.MultiplyVector(Vector3.back) * 30;
                Vector3 targetVector = transform.position - targetPosition;
                Vector3 directionVector = new Vector3(Vector3.Dot(transform.right, targetVector),
                    Vector3.Dot(transform.up, targetVector), Vector3.Dot(transform.forward, targetVector)).normalized;

                _roll = directionVector.x * -maneuverRate;
                _pitch = directionVector.y * maneuverRate;
                if (directionVector.z < 0)
                {
                    _brake = 1;
                    _accel = (2 * directionVector.z + 1) * maneuverRate;
                }
                else
                {
                    _accel = 1;
                    _brake = (-2 * directionVector.z + 1) * maneuverRate;
                }
            }

            if (InsideTargetArea(Targeting, 30.0f, 60.0f))
            {
                _targetingTimer = 0;
                State = NpcState.Targeting;
            }

        }
        else if (State == NpcState.Targeting)
        {
            if (Targeting != null)
            {
                Targeting.SendMessage("BeingTargeted");
                _targetingTimer += Time.deltaTime;

                //_roll = 0;
                //_pitch = 0;

                Vector3 targetPosition = Targeting.transform.position;
                Vector3 targetVector = transform.position - targetPosition;
                Vector2 directionVector = new Vector2(Vector3.Dot(transform.right, targetVector),
                    Vector3.Dot(transform.up, targetVector)).normalized;

                _roll = -directionVector.x * 0.2f;
                _pitch = directionVector.y * 0.2f;

                float targetSpeed = Targeting.GetComponent<Rigidbody>().velocity.magnitude;
                float selfSpeed = gameObject.GetComponent<Rigidbody>().velocity.magnitude;
                if (selfSpeed < targetSpeed)
                {
                    _accel = -1;
                    _brake = 1;
                }
                else
                {
                    _accel = 1;
                    _brake = -1;
                }

                if (!InsideTargetArea(Targeting, 45.0f, 80.0f))
                {
                    State = NpcState.FollowingTarget;
                    _targetingTimer = 0;
                }
            }
            if (_targetingTimer >= 3.0f)
            {
                State = NpcState.Firing;
            }
        }
        else if (State == NpcState.Dodging)
        {
            // No need dodging, go back to previous state
            if (hitColliders.Length == 1)
            {
                State = NpcState.FindingTarget;
                return;
            }
            Vector3 directionVector = Vector3.zero;
            foreach (Collider collider1 in hitColliders)
            {
                // self
                if (collider1.gameObject.transform.parent.gameObject.name == name) continue;

                Vector3 targetPosition = collider1.gameObject.transform.position;
                Vector3 targetVector = transform.position - targetPosition;
                directionVector += new Vector3(Vector3.Dot(transform.right, targetVector),
                    Vector3.Dot(transform.up, targetVector), Vector3.Dot(transform.forward, targetVector));
            }

            directionVector.Normalize();

            _roll = directionVector.x * maneuverRate;
            _pitch = directionVector.y * -maneuverRate;
            if (directionVector.z > 0)
            {
                _brake = 1;
                _accel = (-2 * directionVector.z + 1) * maneuverRate;
            }
            else
            {
                _accel = 1;
                _brake = (2 * directionVector.z + 1) * maneuverRate;
            }
        }
        else if (State == NpcState.EmergencyDodging)
        {
            _roll = 0;
            _pitch = -1;

        }
        else if (State == NpcState.Firing)
        {

            GameObject newMissile = Instantiate(_missile, _missile.transform.parent);
            newMissile.name = "Missile" + name + _missileCount;
            ++_missileCount;
            newMissile.transform.position = _missile.transform.position;
            newMissile.transform.rotation = _missile.transform.rotation;
            newMissile.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = true;
            newMissile.transform.GetChild(0).gameObject.GetComponent<CapsuleCollider>().enabled = true;
            newMissile.GetComponent<Rigidbody>().isKinematic = false;
            newMissile.GetComponent<MissileController>().enabled = true;

            if (Targeting != null) newMissile.SendMessage("Targeting", Targeting.name);


            State = NpcState.FindingTarget;
        }
    }

    void FixedUpdate()
    {
        Vector3 upVector = transform.localToWorldMatrix.MultiplyVector(Vector3.up).normalized;
        Vector3 frontVector = transform.localToWorldMatrix.MultiplyVector(Vector3.forward).normalized;

        float horizontalAxis = _roll > 1 ? 1 : _roll < -1 ? -1 : _roll;
        float verticalAxis = _pitch > 1 ? 1 : _pitch < -1 ? -1 : _pitch;
        float accelerateAxis = _accel > 1 ? 1 : _accel < -1 ? -1 : _accel;
        float brakeAxis = _brake > 1 ? 1 : _brake < -1 ? -1 : _brake;

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

    void DamageTaken(float damage)
    {
        health -= damage;
        if (health <= 0)
            Dead();
    }

    void Dead()
    {
        Debug.Log(name + " dead");
        if (targeted)
        {
            ShootingIndicator.SendMessage("TargetDestroyed");
        }
        Destroy(gameObject);
    }

    bool InsideTargetArea(GameObject target, float angle, float radius)
    {
        Vector3 targetPosition = target.transform.position - transform.position;
        float distance = targetPosition.magnitude;
        float cos = Vector3.Dot(targetPosition, transform.forward) / distance;
        float ang = Mathf.Acos(cos);

        return ang <= angle * Mathf.Deg2Rad && distance <= radius;
    }

    void BeingTargeted()
    {
        // Set beingTargeted flag to true
        beingTargeted = true;
        // Reset _beingTargetedTimer
        _beingTargetedTimer = 0;
    }
}
