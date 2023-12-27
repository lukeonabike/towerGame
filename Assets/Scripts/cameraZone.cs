using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class cameraZone : MonoBehaviour
{
    [SerializeField] private float _cameraRadius;
    [SerializeField] private float _cameraHeight = 40f;
    [SerializeField] private float _cameraZoom = 8.0f;
    [SerializeField] private float _cameraAngle = 15.0f;

    // Start is called before the first frame update
    public float getRadius()
    {
        return _cameraRadius;
    }

    public float getHeight()
    {
        return _cameraHeight;
    }

    public float getZoom()
    {
        return _cameraZoom;
    }
    public float getAngle()
    {
        return _cameraAngle;
    }
}
