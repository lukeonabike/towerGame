using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
 
    [SerializeField] private GameObject _centreOfTheUniverse;
    [SerializeField] private int _playerNumber;
    [SerializeField] private CharacterController cornChip;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float speedDefault = 10f;
    [SerializeField] private float jumpHeight  = 3f;
    [SerializeField] private float trampHeight = 4f;
    [SerializeField] private Transform groundPoint;
    [SerializeField] private LayerMask whatIsGround; 
    [SerializeField] private LayerMask whatIsTramp;
    [SerializeField] private LayerMask whatIsTrampStatic;
    [SerializeField] private LayerMask whatIsOneWayUp;
    [SerializeField] private Transform camMain;
    [SerializeField] private Transform camOther;
    [SerializeField] private SpriteRenderer theSR;
    [SerializeField] private SpriteRenderer theSROther;
    [SerializeField] private Animator anim;
    [SerializeField] private Animator animOther;
    [SerializeField] private GameObject teleportOrb;
    [SerializeField] private AnimationCurve _curve;

    private bool _teleporting = false;
    private Vector3 _teleportTarget;
    private Vector3 _teleportFrom;
    private float _teleportTime = 1;
    private float _teleportTimeStart = 0;
    private float _debounceTime = 0.25f;
    private float _lastJumpTime;
    private Vector2 _inputVectorRaw;
    private float _gravityAcceleration = 50f;  
    private float _gravityVelocity = 0f;
    private Vector3 _inputVectorCleaned;
    private bool _isGrounded;
    private bool _isJumping;
    private bool _isTramping;
    private float _lastGroundHeight = 0f;
    private bool _isTramped;
    private bool _isTrampStatic = false;
    private string _hori;
    private string _verti;
    private string _fire1;

    void Start()
    {
        if (_playerNumber == 1)
        {
            _hori = "Horizontal1";
            _verti = "Vertical1";
            _fire1 = "Fire11";
        }
        else if (_playerNumber == 2)
        {
            _hori = "Horizontal2";
            _verti = "Vertical2";
            _fire1 = "Fire21";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        
        // add in some gravity!!!
        if(cornChip.velocity.y >0)
        {
            _gravityVelocity = _gravityVelocity - _gravityAcceleration * Time.deltaTime;
            Physics.IgnoreLayerCollision(0,11,true); // allow the player to float up the one way ups
        }
        else
        {
            Physics.IgnoreLayerCollision(0,11,false); // allow the player to not go down the one way ups
            _gravityVelocity = _gravityVelocity - _gravityAcceleration * 3 * Time.deltaTime;
        }
        
        


        RaycastHit hit;
        
        if( Physics.Raycast(groundPoint.position,Vector3.down,out hit, 0.3f,   whatIsOneWayUp))
        {
            if(Time.time - _lastJumpTime >= _debounceTime) // Need to debounce the jump as the player detect thingy can hit the ground while still travelling upwards
            {
                _isGrounded=true;
                _isTramped=false;
                _isJumping=false;
                _isTramping=false;
                _lastGroundHeight = cornChip.transform.position.y;              
            }
            if(cornChip.velocity.y <= 0)
            {
                _gravityVelocity = 0;
            }
        }
        else if(Physics.Raycast(groundPoint.position,Vector3.down,out hit, 0.3f, whatIsGround))
        {
            if(Time.time - _lastJumpTime >= _debounceTime) // Need to debounce the jump as the player detect thingy can hit the ground while still travelling upwards
            {
                _isGrounded=true;
                _isTramped=false;
                _isJumping=false;
                _isTramping=false;
                _lastGroundHeight = cornChip.transform.position.y;
                _gravityVelocity = 0;
            }
        }
        else if(Physics.Raycast(groundPoint.position,Vector3.down,out hit, 0.3f, whatIsTramp))
        {
            _isGrounded=true;
            _isTramped=true;
            _isTrampStatic=false;
            _isJumping=false;
            _isTramping=false;
            _gravityVelocity = 0;
        }
        else if(Physics.Raycast(groundPoint.position,Vector3.down,out hit, 0.3f, whatIsTrampStatic))
        {
            _isGrounded=true;
            _isTramped=true;
            _isTrampStatic=true;
            _isJumping=false;
            _isTramping=false;
            _gravityVelocity = 0;
        }
        else
        {
            _isGrounded=false;
            _isTramped=false;
        }



        // need to move the player update into the loop of the physics engine // FixedUpdate
        _inputVectorRaw = new Vector2(Input.GetAxis(_hori), Input.GetAxis(_verti));        // get control inputs
        _inputVectorRaw = Vector2.ClampMagnitude(_inputVectorRaw,1);                                            // normalise the input vector
        Vector3 camF = transform.forward ;                                                  // get the camera/player forward direction
        Vector3 camR = transform.right   ;                                                  // get the camera/player right direction, we now have orientation
        camF.y = 0;                                                                         // force the camera F y magnitude to be zero... we are now flat
        camR.y = 0;                                                                         // force the camera R y magnitude to be zero... we are now flater
        _inputVectorCleaned = (camF*_inputVectorRaw.y + camR*_inputVectorRaw.x)  *  speed;                                     // make a vector for our characters movement and scale by the speed
        
        // Jumping
        if(Input.GetButtonDown(_fire1) && (_isGrounded==true) && (_isTramped==false))
        {
            _gravityVelocity += Mathf.Sqrt(2 * jumpHeight * _gravityAcceleration); // no idea what this equation is but it works... must be diff of Accel equation???
            _lastJumpTime = Time.time;   // need to keep the last jump time to debound the raycast thing
            _isGrounded = false;
            _isJumping = true;
            _isTramping = false;   
        }

        // Trampolining
        if(Input.GetButtonDown(_fire1) && (_isTramped==true))
        {
            _gravityVelocity += Mathf.Sqrt(2 * trampHeight * _gravityAcceleration); // no idea what this equation is but it works
            _lastJumpTime = Time.time;   // need to keep the last jump time to debound the raycast thing
            _isGrounded = false;
            _isJumping = false;
            _isTramping = !_isTrampStatic;
        }

        anim.SetBool("onGround",(_isGrounded || _isTramped));
        animOther.SetBool("onGround",(_isGrounded || _isTramped));

        // Move the legs!!
        // Set the total speed of the player
        anim.SetFloat("moveSpeed", Mathf.Abs(_inputVectorRaw.x) + Mathf.Abs(_inputVectorRaw.y)); // a cheap alternative to the true magnitude, this is faster to run though, no SQRT, and no ^2!
        animOther.SetFloat("moveSpeed", Mathf.Abs(_inputVectorRaw.x) + Mathf.Abs(_inputVectorRaw.y)); // a cheap alternative to the true magnitude, this is faster to run though, no SQRT, and no ^2!

            // FLip Character on left/right
            if(!theSR.flipX && _inputVectorRaw.x > 0)
            {
                theSR.flipX = true;
            }
            else if(theSR.flipX && _inputVectorRaw.x < 0)
            {
                theSR.flipX = false;
            }

        theSR.transform.LookAt(new Vector3(camMain.position.x,transform.position.y,camMain.position.z));
        theSROther.transform.LookAt(new Vector3(camOther.position.x,transform.position.y,camOther.position.z));

        // Flip the character on the oteher Camera
        if((Vector3.SignedAngle(camOther.forward,cornChip.velocity,camOther.up))>0 && (_inputVectorRaw.x !=0 || _inputVectorRaw.y !=0))
        {
                theSROther.flipX = true;
        }
        else if((Vector3.SignedAngle(camOther.forward,cornChip.velocity,camOther.up))<0 && (_inputVectorRaw.x !=0 || _inputVectorRaw.y !=0))
        {
                theSROther.flipX = false;
        }



        

        //code for the _teleporting   -- if true we override the player position
        if(_teleporting==true)
        {
            
            var timeThru = ((Time.time - _teleportTimeStart)/_teleportTime);
            cornChip.transform.position =  Vector3.Lerp(_teleportFrom ,_teleportTarget , _curve.Evaluate(timeThru));
            
            if(timeThru > 1)    // if the player has finished the lerping.
            {
                cornChip.transform.position = _teleportTarget;  // set it to the actual target for consistency
                cornChip.enabled = true;                                // re-enable player control
                _teleporting = false;                                   // end the _teleporting process
                _isGrounded = false;                                    // make sure that we are NOT on the ground when passing through objects
                _gravityVelocity = 0;                                   // set downward velocity to zero
                theSR.color = new Color(1f,1f,1f,1f);                   // make the player opaque
                theSROther.color = new Color(1f,1f,1f,1f);              // make the player opaque
                teleportOrb.SetActive(false);                           // let the player move again
                //Debug.Log("teleport finish");
            }
        }
        else
        {
            // move that character from the user controlls
            cornChip.Move(new Vector3(_inputVectorCleaned.x, _gravityVelocity ,_inputVectorCleaned.z) * Time.deltaTime);
        }
      
        
        // Make sure the player looks at the centre of the tower
        transform.LookAt(new Vector3(_centreOfTheUniverse.transform.position.x ,transform.position.y,_centreOfTheUniverse.transform.position.z));          

    }



    public void setSpeed(float newSpeed) // Sets the player speed to a new value
    {
        speed = newSpeed * speedDefault;
    }

    public void setJump(float newHeight) // sets the jump height to a new value
    {
        jumpHeight = newHeight * 3;   
    }

    public void resetSpeed() // reset the player speed to the default value
    {
        speed = speedDefault;
    }
    public void resetJump() // reset the player jump height to the default value TODO make this not hardcoded
    {
        jumpHeight = 3;
    }

    public void teleportPlayer(Transform newLocation, float timeToTransition)
    {
        if (_teleporting==false)
        {
            cornChip.SimpleMove(Vector3.zero);
            teleportOrb.SetActive(true);
            _teleporting = true;                                                             // enable _teleporting in the main loop
            _teleportTime = timeToTransition;                                                // set the _teleporting time var
            _teleportTarget = newLocation.position;                                                   // set the teleport target var
            _teleportFrom = cornChip.transform.position;

            _teleportTimeStart= Time.time;                
            //_isGrounded = false;
            cornChip.enabled=false;                                                         // turn off player control
            
            theSR.color = new Color(1f,1f,1f,0.3f);                                         // make the player a bit invisible
            theSROther.color = new Color(1f,1f,1f,0.3f);                                         // make the player a bit invisible
            //Debug.Log("teleport begin");
        }
    }

    public bool isTelporting()
    {
        return _teleporting;
    }

    public bool isGrounded()
    {
        return _isGrounded;
    }

    public bool isJupming()
    {
        return _isJumping;
    }

    public bool isTramping()
    {
        return _isTramping;
    }    

    public float getHeight()
    {
        return gameObject.transform.position.y;
    }


    
}
