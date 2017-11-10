using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileController : MonoBehaviour
{

    private bool _targeting = false;
    private String _targetName = "";
    private GameObject _target;
    private Vector3 _targetDirection;

    private float _liveTime = 0.0f;

    private string _shooter = "";

    // Use this for initialization
    void Start()
    {
        _shooter = transform.parent.gameObject.name;
        transform.SetParent(null);
    }

    // Update is called once per frame
    void Update()
    {
        if (_targeting)
        {
            _target = GameObject.Find(_targetName);
            if (_target == null)
            {
                Positioning(transform.forward);
                return;
            }

            Quaternion q = transform.rotation;
            q.SetLookRotation(_target.transform.position - transform.position);
            transform.rotation = Quaternion.Lerp(transform.rotation, q, 0.4f);
        }
        else
        {
            Quaternion q = transform.rotation;
            q.SetLookRotation(_targetDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, q, 1);
        }
        gameObject.GetComponent<Rigidbody>().velocity = transform.forward * 150.0f;
    }

    void FixedUpdate()
    {
        _liveTime += Time.fixedDeltaTime;
        if (_liveTime > 10.0f)
        {
            Bomb();
        }
    }

    void Targeting(String targetName)
    {
        _targeting = true;
        _targetName = targetName;
        _target = GameObject.Find(targetName);
        if (_target == null)
            Positioning(transform.forward);
    }

    void Positioning(Vector3 targetDirection)
    {
        _targeting = false;
        _targetDirection = targetDirection;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform.parent.gameObject.name == _shooter) return;
        if (other.gameObject.tag == "Enemy" || other.gameObject.tag == "Friend" || other.gameObject.tag == "Player")
        {
            other.gameObject.transform.parent.gameObject.SendMessage("DamageTaken", 51.0f);
        }
        Bomb();
    }

    void Bomb()
    {
        Destroy(gameObject);
    }
}
