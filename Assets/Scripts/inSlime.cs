using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// a change, and again, and again
public class inSlime : MonoBehaviour
{
    [SerializeField] private float slimeSpeed = 1.0f;
    [SerializeField] private float slimeJump = 1.0f;
    // Start is called before the first frame update

    private void OnTriggerExit(Collider otherObject)
    {
        //Debug.Log(otherObject);
        otherObject.GetComponent<PlayerMovement>().resetSpeed();
        otherObject.GetComponent<PlayerMovement>().resetJump();
    }
    private void OnTriggerEnter(Collider otherObject)
    {
        //Debug.Log(otherObject);
        otherObject.GetComponent<PlayerMovement>().setSpeed(slimeSpeed);
        otherObject.GetComponent<PlayerMovement>().setJump(slimeJump);
    }


}
