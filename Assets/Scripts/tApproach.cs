using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tApproach : MonoBehaviour
{
    public Vector3 startPos;
    public Vector3 endPos;
    public bool approaching;
    public float approachTimer = 10f;
    public float maxTimer = 10f;
    public bool debugResetPos;

    public bool tentacleReady = false;

    private void Update()
    {
        if (approaching)
        {
            approachTimer -= Time.deltaTime;
        }
        if(approachTimer < 0f)
        {
            approachTimer = 1f;
            approaching = false;
            tentacleReady = true;
        }
    }
    void FixedUpdate()
    {
        if (approaching)
        {
            transform.localPosition = Vector3.Lerp(startPos, endPos, 1-(approachTimer/maxTimer));
        }
        if (debugResetPos)
        {
            debugResetPos = false;
            resetPos();
        }
    }

    public void activate()
    {
        maxTimer = 10f;
        approachTimer = maxTimer;
        approaching = true;
    }
    public void activate(float spd)
    {
        maxTimer = 10f / spd;
        approachTimer = maxTimer;
        approaching = true;
    }
    public void resetPos()
    {
        transform.localPosition = startPos;
    }
}
