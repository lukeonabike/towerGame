using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMovement : MonoBehaviour
{
    [SerializeField] private GameObject _centreOfTheUniverse;
    [SerializeField] private GameObject _playerToTrack;
    [SerializeField] private GameObject _cameraPivot;
    [SerializeField] private GameObject _cameraBlock;
    [SerializeField] private Camera _cameraCamera;
    private float _playerHeight;
    private bool _playerTeleport;
    private bool _playerTeleportLast;
    private float _teleportLockHeight;
    private float _trampLockHeight;
    private float _cameraHeight;    
    private float _lerpFrom; 
    private float _lerpTarget;
    private float _lerpTime;
    [SerializeField] private float _lerpMultiplier = 3;
    private float _lerpStartage = 0;
    private bool _playerIsGrounded;
    private bool _playerIsGroundedLast;
    [SerializeField] private AnimationCurve _curve;
    [SerializeField] private bool _showDebug = false;

    private List<List<float>> _cameraZoneArray = new List<List<float>>();
    private GameObject[] _cameraZones;
 
    
    


    // Start is called before the first frame update
    void Start()
    {
        if (_cameraZones==null)
        {
            _cameraZones = GameObject.FindGameObjectsWithTag("CameraZone");
        }

        foreach(GameObject cameraZoneSingle in _cameraZones)
        {
            _cameraZoneArray.Add(new List<float> {cameraZoneSingle.transform.position.y,cameraZoneSingle.GetComponent<cameraZone>().getHeight(),cameraZoneSingle.GetComponent<cameraZone>().getZoom(),cameraZoneSingle.GetComponent<cameraZone>().getAngle()});
            // [height of the zone start, Camera Height offset, zoom, angle]
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void LateUpdate()
    {

        _playerHeight = _playerToTrack.GetComponent<PlayerMovement>().getHeight();
        _playerIsGrounded = _playerToTrack.GetComponent<PlayerMovement>().isGrounded();
        _playerTeleport = _playerToTrack.GetComponent<PlayerMovement>().isTelporting();

        if(_playerTeleport && !_playerTeleportLast)
        {
            //Debug.Log("\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tTeleport start");
            _teleportLockHeight = _playerHeight - _cameraHeight;
        }
        else if(!_playerTeleport && _playerTeleportLast)
        {
            //Debug.Log("\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tTeleport END");
        }



        
        if(_lerpTime>1)                     // this stops the bad situation where teh player is in the air at the end of the lerp...
        {
            _lerpFrom = _lerpTarget;
        }

        

        if(!_playerIsGrounded && _playerIsGroundedLast)
        {   // Player has jumped - w00t
            if(_lerpTime<1)
            {
                // we are already lerping, assumed that camera is not at this point (i.e it is lower, DO NOT RESET THE START POINT)
                // nothing needs to happen here
                // Debug.Log("Player has JUMPED - Already Lerping: _lerpFrom=" + _lerpFrom);
                _trampLockHeight = _playerHeight - _cameraHeight;
            }
            else
            {
                // Player has done a legit jump... get their height... We can assume the camera is at this height already
                _lerpFrom = _playerHeight;
                //Debug.Log("player has JUMPED: _lerpFrom=" + _lerpFrom);
            }
        }
        else if(_playerIsGrounded && !_playerIsGroundedLast)
        {   // Player has landed
            if(_lerpTime < 1 )
            {
                if (Mathf.Abs(_lerpTarget - _playerHeight)>0.05) // this avoids changing the lerp is the player jumps on the spot
                                                                // it should only change the lerp properties if they go UPWARDS in height (on landing)
                                                                // this also smooths out some tiny jumps in camera when jumping on the spot...
                                                                //                                                                ...NFI why
                {
                    // Player has landed with a lerp already under way, reset the lerp from, to and time !
                    _lerpFrom = _cameraBlock.transform.localPosition.y;
                    _lerpTarget = _playerHeight;
                    _lerpStartage = Time.time; 
                    //Debug.Log("Player has Landeed - but we already lepring - but we should set the landing spot to the new one ");
                }
                else
                {
                    // Debug.Log("Player has Landeed - but we already lepring to this height anywayy!!!");
                }
            }
            else
            {
                // This starts the lerp, we have landed at a height different to where we were
                _lerpTarget = _playerHeight;
                _lerpStartage = Time.time;
                //Debug.Log("player has landed - at this height " + _lerpTarget);
                //Debug.Log("player has landed - wee can start the lerp " + _lerpStartage);
            }
        }
            
        
        /// keep all these together /////.///////////////////////////////
        var lerpDistance = Mathf.Abs(_lerpFrom - _lerpTarget);         //
        if (lerpDistance<1) lerpDistance = 1;                          //
        _lerpTime = (Time.time - _lerpStartage) * _lerpMultiplier;     //
        if (_lerpTime > 1) _lerpTime = 1;                              //
        _cameraHeight = Mathf.Lerp(_lerpFrom,_lerpTarget,_lerpTime);   //
        if(_showDebug) Debug.Log($"LerpFrom: {_lerpFrom}\t\tLerpTarget: {_lerpTarget}\t\tLerpTime: {_lerpTime}");
        /////////////////////////////////////////////////////////////////    


        /// SPECIAL CONSIDERATIONS BELOW !!!!

        // Just lock the camera height to the lerp target when it is close enough
        if(Mathf.Abs(_lerpTarget-_cameraHeight)<0.05)
        {
            _cameraHeight = _lerpTarget;
            _lerpFrom     = _lerpTarget;
            _lerpTime     = 1;
        }  
 
        /// SPECIAL CONSIDERATIONS TO OVERRULE THE LERP /////////////////////////////
        //  FALLING - lock the camera to the height of the player as they fall past the height where they jumped from.
        //  TELEPORTING - lock the camera in place in relation to the player keeping the height difference the same
        //  TRAMPING (NON STATIC TRAMP) - LOCK AN FOLLOW THEM!!!
        //  These are moslyt mutually exclusive... but best to keep the falling at the end of the chain.

        if(_playerTeleport)
        {
            _cameraHeight = _playerHeight - _teleportLockHeight;          // Teleporting
            _lerpFrom     = _playerHeight - _teleportLockHeight;
            _lerpTarget   = _playerHeight - _teleportLockHeight;
        }

        if (_playerToTrack.GetComponent<PlayerMovement>().isTramping())  // player is Tramping
        {
            _lerpTarget   = _playerHeight;  // simply drag the target to the player
        }

        if ((_playerHeight < _cameraHeight) )  // player is falling -- KEEP AT END OF ITEMS SO IT WILL ALWAYS OVERRULE THE REST
        {
            _cameraHeight = _playerHeight;  
            _lerpFrom     = _playerHeight;
            _lerpTarget   = _playerHeight;
        }

        

        // set some variables to use on the next round!
        _playerIsGroundedLast = _playerIsGrounded;
        _playerTeleportLast   = _playerTeleport;
      
       /// FINALLY WE SET EVERYTHING
        transform.forward = new Vector3(_playerToTrack.transform.localPosition.x,0f,_playerToTrack.transform.localPosition.z);  // Rotate the Camera with the PLayer
        _cameraBlock.transform.localPosition = new Vector3(0f,_cameraHeight,_cameraBlock.transform.localPosition.z);  // SET THE CAMERA BASE HEIGHT
        

        var i = 0;
        while(i < _cameraZoneArray.Count)
        {
            if (i == _cameraZoneArray.Count - 1 )
            {
                //Debug.Log("you are in the top zone - No lerps happen here");
                transform.localPosition = new Vector3(0f, _cameraZoneArray[i][1] ,0f);   // SET THE CAMERA HEIGHT OFFSET PER THE ZONE
                _cameraCamera.orthographicSize =_cameraZoneArray[i][2];             // SET THE CAMERA ZOOM PER THE ZONE          
                i = _cameraZoneArray.Count;                                         // Ensure we out of the loop
                
            }
            else if (_cameraHeight >= _cameraZoneArray[i][0] && _cameraHeight < _cameraZoneArray[i+1][0])
            {
                var lerpTimeSettings = (_cameraHeight -  _cameraZoneArray[i][0]) /(_cameraZoneArray[i+1][0] - _cameraZoneArray[i][0]) ;
                transform.localPosition = new Vector3(0f, Mathf.Lerp(_cameraZoneArray[i][1],_cameraZoneArray[i+1][1], _curve.Evaluate(lerpTimeSettings)) ,0f);   // SET THE CAMERA HEIGHT OFFSET PER THE ZONE
                _cameraCamera.orthographicSize = Mathf.Lerp(_cameraZoneArray[i][2],_cameraZoneArray[i+1][2], _curve.Evaluate(lerpTimeSettings));            // SET THE CAMERA ZOOM PER THE ZONE          
                i = _cameraZoneArray.Count;                                                                                                                 // Ensure we out of the loop
            }
            
            i = i + 1;
        }
    }
}
