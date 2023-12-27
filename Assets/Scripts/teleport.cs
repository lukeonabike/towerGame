using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// test change
public class teleport : MonoBehaviour
{
    public Transform newLocation;
    public float teleportSpeed = 1f;

    private void OnTriggerEnter(Collider otherObject)
    {
        otherObject.GetComponent<PlayerMovement>().teleportPlayer(newLocation, teleportSpeed);
    }
}
 