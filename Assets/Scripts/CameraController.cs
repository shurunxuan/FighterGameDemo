using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{

    public GameObject AttachedTo;
    public GameObject ShootingIndicator;
    public GameObject Enemies;
    public RawImage EnemyIndicator;
    public float DefaultSmothing;
    public float SpeedFactor;
    public float PositionFactor;
    public float RotationFactor;

    private Vector3 _offset;
    private Rigidbody _planeRigidbody;
    private int _enemyCount;
    private RawImage[] _enemyIndicators;
    private Texture2D[] _indicatorTextures;

    public Text Text;

    // Use this for initialization
    void Start()
    {
        _offset = transform.position - AttachedTo.transform.position;
        _planeRigidbody = AttachedTo.GetComponent<Rigidbody>();

        _enemyCount = Enemies.transform.childCount;
        _enemyIndicators = new RawImage[_enemyCount];
        for (int i = 0; i < _enemyCount; ++i)
        {
            _enemyIndicators[i] = Instantiate(EnemyIndicator);
            _enemyIndicators[i].transform.SetParent(EnemyIndicator.transform.parent, true);
            //_enemyIndicators[i].transform.parent = EnemyIndicator.transform.parent;
            _enemyIndicators[i].name = "EI" + Enemies.transform.GetChild(i).name;
        }
        EnemyIndicator.enabled = false;

        _indicatorTextures = new Texture2D[4];
        _indicatorTextures[0] = Resources.Load<Texture2D>("Textures/enemyIndicator");
        _indicatorTextures[1] = Resources.Load<Texture2D>("Textures/enemyIndicatorOut");
        _indicatorTextures[2] = Resources.Load<Texture2D>("Textures/enemyIndicatorTarget");
        _indicatorTextures[3] = Resources.Load<Texture2D>("Textures/enemyIndicatorLocked");
    }

    void FixedUpdate()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // The former attached object is gone
        if (AttachedTo == null)
        {
            AttachedTo = GameObject.FindGameObjectWithTag("FriendContainer");
        }


        if (_enemyCount != Enemies.transform.childCount)
        {
            // Can't find a previous enemy
            // Destroy all indicators
            for (int i = 0; i < _enemyCount; ++i)
            {
                Destroy(_enemyIndicators[i]);
            }
            _enemyCount = Enemies.transform.childCount;
            // Reset all indicators
            EnemyIndicator.enabled = true;
            _enemyIndicators = new RawImage[_enemyCount];
            for (int i = 0; i < _enemyCount; ++i)
            {
                _enemyIndicators[i] = Instantiate(EnemyIndicator);
                _enemyIndicators[i].transform.SetParent(EnemyIndicator.transform.parent, true);
                //_enemyIndicators[i].transform.parent = EnemyIndicator.transform.parent;
                _enemyIndicators[i].name = "EI" + Enemies.transform.GetChild(i).name;
            }
            EnemyIndicator.enabled = false;
        }

        float planeSpeed = _planeRigidbody.velocity.magnitude;
        Vector3 targetCameraPosition = AttachedTo.transform.position + transform.localToWorldMatrix.MultiplyVector(_offset);
        Quaternion targetCameraRotation = AttachedTo.transform.rotation;
        float positionSmothing = DefaultSmothing + Mathf.Sqrt(planeSpeed) * SpeedFactor * PositionFactor;
        float rotationSmothing = DefaultSmothing + Mathf.Sqrt(planeSpeed + 1) * SpeedFactor * RotationFactor;
        transform.position = Vector3.Lerp(transform.position, targetCameraPosition, positionSmothing * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetCameraRotation, rotationSmothing * 2.0f * Time.deltaTime);

        Vector3[] enemyScreenPosition = new Vector3[_enemyCount];

        for (int i = 0; i < _enemyCount; ++i)
        {
            enemyScreenPosition[i] = Camera.main.WorldToScreenPoint(Enemies.transform.GetChild(i).transform.position);
            if (enemyScreenPosition[i][2] < 0 || enemyScreenPosition[i][2] > 1000)
            {
                _enemyIndicators[i].rectTransform.localScale = Vector3.zero;
                continue;
            }
            if (enemyScreenPosition[i][0] < 0 || enemyScreenPosition[i][0] > Screen.width ||
                enemyScreenPosition[i][1] < 0 || enemyScreenPosition[i][1] > Screen.height ||
                enemyScreenPosition[i][2] < 0)
            {
                // The enemy is out of the screen

                // Set the inScreen property of the enemy
                Enemies.transform.GetChild(i).GetComponent<NpcController>().inScreen = false;

                // Change the texture to arrow
                _enemyIndicators[i].texture = _indicatorTextures[1];

                // Get the angle of the enemy
                float x = enemyScreenPosition[i][0] - Screen.width / 2.0f;
                float y = enemyScreenPosition[i][1] - Screen.height / 2.0f;
                float c = Mathf.Sqrt(x * x + y * y);
                float cc = 0.45f * Screen.height; // Suppose height is always less than width
                float ct = cc / c;
                float xx = ct * x;
                float yy = ct * y;

                // Set position
                _enemyIndicators[i].rectTransform.position = new Vector3(xx + Screen.width / 2.0f, yy + Screen.height / 2.0f, 0);

                // Set rotation
                float ang = Mathf.Atan2(y, x) * 180.0f / Mathf.PI - 90.0f;
                _enemyIndicators[i].rectTransform.eulerAngles = new Vector3(0, 0, ang);

                // Set scale
                _enemyIndicators[i].rectTransform.localScale = Vector3.one;
            }
            else
            {
                // The enemy is in the screen

                // Set the inScreen property of the enemy
                Enemies.transform.GetChild(i).GetComponent<NpcController>().inScreen = true;

                // Check if the enemy is target
                if (Enemies.transform.GetChild(i).name == ShootingIndicator.GetComponent<ShootController>().CurrentTargetName)
                {
                    if (Enemies.transform.GetChild(i).GetComponent<NpcController>().lockedOn)
                    {
                        // Change the texture to locked
                        _enemyIndicators[i].texture = _indicatorTextures[3];
                    }
                    else
                    {
                        // Change the texture to target
                        _enemyIndicators[i].texture = _indicatorTextures[2];
                    }
                }
                else
                {
                    // Change the texture to square
                    _enemyIndicators[i].texture = _indicatorTextures[0];
                }

                // Set position
                _enemyIndicators[i].rectTransform.anchoredPosition = enemyScreenPosition[i];

                // Set rotation
                _enemyIndicators[i].rectTransform.eulerAngles = Vector3.zero;

                // Set scale
                float scale = Mathf.Max(40f / enemyScreenPosition[i][2], 0.5f) * 2;
                _enemyIndicators[i].rectTransform.localScale = scale * Vector3.one;

            }
        }

        //Text.text = enemyscreenposition.ToString();

        //Text.rectTransform.anchoredPosition = enemyscreenposition;
    }
}
