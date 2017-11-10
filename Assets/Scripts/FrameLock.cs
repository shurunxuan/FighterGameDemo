using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameLock : MonoBehaviour
{

    public int FrameRate;

    void Awake()
    {
        Application.targetFrameRate = FrameRate;
    }
}
