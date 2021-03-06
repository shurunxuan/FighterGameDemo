﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShootController : MonoBehaviour
{

    public GameObject Plane;
    public GameObject TargetArrow;
    public GameObject Missile;
    public GameObject Enemies;

    public String CurrentTargetName = "No Target";

    private RectTransform rectTransform;

    private GameObject _currentTarget;
    private bool _targetAcquired = false;
    private int _targetIndex = -1;
    private Vector3 _arrowPlaneOffset;

    private bool _aiming = false;
    private bool _lockedOn = false;

    private int _missileCount = 0;
    private Vector3 _aimingDirection;

    // Use this for initialization
    void Start()
    {
        _arrowPlaneOffset = TargetArrow.transform.position - Plane.transform.position;

        rectTransform = gameObject.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        TargetArrow.transform.position = Plane.transform.localToWorldMatrix.MultiplyVector(_arrowPlaneOffset) + Plane.transform.position;

        ArrayList inScreenEnemies = new ArrayList();

        for (int i = 0; i < Enemies.transform.childCount; ++i)
        {
            if (Enemies.transform.GetChild(i).GetComponent<NpcController>().inScreen)
                if ((Enemies.transform.GetChild(i).transform.position - Plane.transform.position).magnitude < 400.0f)
                {
                    inScreenEnemies.Add(Enemies.transform.GetChild(i).gameObject);
                }
        }

        if (_targetAcquired) // Already had a target, which means _currentTarget is valid
        {
            if ((_currentTarget.transform.position - Plane.transform.position).magnitude < 400.0f)
            {
                // Current target is in a valid range
                // Do nothing
            }
            else
            {
                // Current target is out of range
                _currentTarget.GetComponent<NpcController>().targeted = false;
                _targetAcquired = false;
                _targetIndex = -1;
            }
            // Set arrow direction
            Quaternion q = new Quaternion();
            q.SetLookRotation(_currentTarget.transform.position - TargetArrow.transform.position);
            TargetArrow.transform.rotation = q;
        }
        else
        {
            // Try to acquire a new target
            if (inScreenEnemies.Count > 0)
            {
                // Have a valid target
                _targetIndex = 0;
                _currentTarget = (GameObject)inScreenEnemies[0];
                _currentTarget.GetComponent<NpcController>().targeted = true;
                _targetAcquired = true;
                // Reset the locker
                //rectTransform.position = new Vector3(Screen.width / 2.0f, Screen.height / 2.0f);
                _aimingDirection = Plane.transform.localToWorldMatrix.MultiplyVector(Vector3.forward);
                // Show the locker
                rectTransform.localScale = Vector3.one * Screen.dpi / 96.0f;
                _lockedOn = false;
            }
            else
            {
                // Don't have any valid target
                _targetIndex = -1;
                _targetAcquired = false;
            }
        }

        if (Input.GetButtonDown("ChangeTarget"))
        {
            // If target acquired and no enemy in screen
            if (_targetAcquired && inScreenEnemies.Count == 0)
            {
                // You can't change anything!
                _targetIndex = -1;
            }
            else
            {
                // Manually change a target
                if (inScreenEnemies.Count > 0)
                {
                    // Change enemy property
                    _currentTarget.GetComponent<NpcController>().lockedOn = false;
                    _currentTarget.GetComponent<NpcController>().targeted = false;
                    // Have valid targets
                    _targetIndex = (_targetIndex + 1) % inScreenEnemies.Count;
                    _targetAcquired = true;
                    _currentTarget = (GameObject)inScreenEnemies[_targetIndex];
                    _currentTarget.GetComponent<NpcController>().targeted = true;
                    // Reset the locker
                    //rectTransform.position = new Vector3(Screen.width / 2.0f, Screen.height / 2.0f);
                    _aimingDirection = Plane.transform.localToWorldMatrix.MultiplyVector(Vector3.forward);
                    // Show the locker
                    rectTransform.localScale = Vector3.one * Screen.dpi / 96.0f;
                    _lockedOn = false;
                }
                else
                {
                    // Don't have any valid target
                    if (_currentTarget != null)
                    {
                        _currentTarget.GetComponent<NpcController>().targeted = false;
                    }
                    _targetIndex = -1;
                    _targetAcquired = false;
                }
            }
        }
        TargetArrow.GetComponent<Renderer>().enabled = _targetAcquired;

        CurrentTargetName = _targetAcquired ? _currentTarget.name : "No Target";

        if (_targetAcquired) // Set the locker
        {
            if (_currentTarget.GetComponent<NpcController>().inScreen && !_aiming) // Enemy is in screen but not aiming
            {
                // Show the locker
                rectTransform.localScale = Vector3.one * Screen.dpi / 96.0f;
                // Reset the locker
                //rectTransform.position = new Vector3(Screen.width / 2.0f, Screen.height / 2.0f);
                _aimingDirection = Plane.transform.localToWorldMatrix.MultiplyVector(Vector3.forward);
                // Start aiming
                _aiming = true;
            }
            else if (_currentTarget.GetComponent<NpcController>().inScreen && _aiming) // Enemy is in screen and aiming
            {
                // Set locker position
                Vector3 targetScreenPosition = Camera.main.WorldToScreenPoint(_currentTarget.transform.position);
                float distance = (new Vector3(rectTransform.position.x, rectTransform.position.y) - new Vector3(targetScreenPosition.x, targetScreenPosition.y))
                    .magnitude;
                float worldDistance = (Plane.transform.position - _currentTarget.transform.position).magnitude;

                // rectTransform.position =
                //       Vector3.Lerp(rectTransform.position, targetScreenPosition,
                //           1f * Time.deltaTime);

                Vector3 targetToPlane = (_currentTarget.transform.position - Plane.transform.position).normalized;
                _aimingDirection = Vector3.Lerp(_aimingDirection, targetToPlane, 2.8f * Time.deltaTime);
                _aimingDirection = Vector3.Lerp(_aimingDirection, Plane.transform.localToWorldMatrix.MultiplyVector(Vector3.forward), 1.2f * Time.deltaTime);
                Vector3 lockerWorldPosition = _aimingDirection * 1500.0f + Plane.transform.position;
                Vector3 lockerScreenPosition = Camera.main.WorldToScreenPoint(lockerWorldPosition);
                lockerScreenPosition.Set(lockerScreenPosition.x, lockerScreenPosition.y, 0);
                rectTransform.position = lockerScreenPosition;

                // float horizontalAxis = Input.GetAxis("Horizontal");
                // float verticalAxis = Input.GetAxis("Vertical");
                // rectTransform.position += new Vector3(0, verticalAxis, 0) * 10;

                // float x = rectTransform.position.x - Screen.width / 2;
                // float y = rectTransform.position.y - Screen.height / 2;
                // float r = Mathf.Sqrt(x * x + y * y);
                // float sin = y / r;
                // float angle = Mathf.Asin(sin);
                // angle = angle + horizontalAxis * 0.1f;
                // sin = Mathf.Sin(angle);
                // y = sin * r;
                // x = Mathf.Sqrt(r * r - y * y);
                // rectTransform.position = new Vector3(x + Screen.width / 2, y + Screen.height / 2, 0);

                if (!_lockedOn && distance < 20.0f) // Not locked on but in locking range
                {
                    // Lock on
                    _lockedOn = true;
                    // Hide the locker
                    rectTransform.localScale = Vector3.zero;
                    // Change enemy property
                    _currentTarget.GetComponent<NpcController>().lockedOn = true;
                }

            }
            else // Enemy is out of screen
            {
                if (_lockedOn) // Locked on but target get out of the screen
                {
                    // No lock on
                    _lockedOn = false;
                    // Reset the locker
                    rectTransform.position = new Vector3(Screen.width / 2.0f, Screen.height / 2.0f);
                    // Change enemy property
                    _currentTarget.GetComponent<NpcController>().lockedOn = false;
                }
                // Hide the locker
                rectTransform.localScale = Vector3.zero;
                // Stop aiming
                _aiming = false;
            }
        }

        // Player Fire
        if (Input.GetButtonDown("Fire1"))
        {
            GameObject newMissile = Instantiate(Missile, Missile.transform.parent);
            newMissile.name = "Missile" + _missileCount;
            ++_missileCount;
            newMissile.transform.position = Missile.transform.position;
            newMissile.transform.rotation = Missile.transform.rotation;
            newMissile.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = true;
            newMissile.transform.GetChild(0).gameObject.GetComponent<CapsuleCollider>().enabled = true;
            newMissile.GetComponent<Rigidbody>().isKinematic = false;
            newMissile.GetComponent<MissileController>().enabled = true;
            if (!_aiming)
            {
                // Not aiming, head directly forward
                newMissile.SendMessage("Positioning", Plane.transform.forward);
            }
            else if (!_lockedOn)
            {
                // Aiming, but not locked on
                newMissile.SendMessage("Positioning", _aimingDirection);
            }
            else
            {
                // Aiming and locked on
                if (_currentTarget != null) newMissile.SendMessage("Targeting", _currentTarget.name);
            }
        }
    }

    void TargetDestroyed()
    {
        _targetAcquired = false;
        _aiming = false;
        _lockedOn = false;
    }
}
