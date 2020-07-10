using UnityEngine;


public class FollowShipCamera : EBase
{
    private float _currentFOV = 60;
    
    public Transform turret;
    public Transform turretPivot;
    public Transform _airplaneTarget;
    public Transform _airplane_Pivot;
    
    private Transform _target;
    private Transform _camera_Pivot;

    

    [HideInInspector] public bool IsZTarget;
    [HideInInspector] public bool IsGrabbing;

    [SerializeField] private Camera _camera;

    //_settings currently not in use.  TODO swap the variables below for _settings variables
    //[SerializeField] private Data.Camera _settings;

    [SerializeField] AnimationCurve _FOVPerBoostSecond;
    [SerializeField] float draftingAddedIntensity = 10;
    [SerializeField] float _FOVPerTechMult = 20;
    [SerializeField] AnimationCurve _shakeMagnitudePerBoostSecond;
    [SerializeField] AnimationCurve _shakeRoughnessPerBoostSecond;
    [SerializeField] private float fieldOfViewChangeRate = 1f;
    [SerializeField] private float zTargetFOV = 40f;
    [SerializeField] private float grabFOV = 30f;

    public float CamDistance = 0.2f;

    [SerializeField] float defaultDistanceLerpRate = 45f;
    private float lagDistanceLerpRate = 8f;

    private float overrideTimer = 0;
    private float camDragChangeRate = 80f;

    // CameraShaker _cameraShaker;
    // CameraShakeInstance _currentCameraShake;

    [SerializeField] private float _freezeFrameSeconds = 0.05f;
    [SerializeField] float _minDistanceFromCamSqr = 1000f;
    [SerializeField] float _atMinDistanceCamLerpRate = 80;
    [SerializeField] float backingUpLeewayAngle = 15;
    [SerializeField] float backingUpMaxDistanceFromShip = 10;

    public bool _pullBack = false; //for boost powerups

    public Rigidbody _playerRB;
    private Vector3 _velocity = Vector3.one;


    private void Awake()
    {
        _camera = GetComponent<Camera>();
        Transform.parent = null;
        _camera.fieldOfView = _currentFOV;
        //_cameraShaker = GetComponentInChildren<CameraShaker>();
        DontDestroyOnLoad(gameObject);
    }

    // public void SetUpCamera(Transform target, Rigidbody playerRB, Transform pivot) {
    //     _target = target;
    //     _playerRB = playerRB;
    //     _camera_Pivot = pivot;
    //     DoReset();
    // }
    //
    // public void DoReset() {
    //     Transform.position = _camera_Pivot.position;
    //     Transform.LookAt(_target.transform);
    // }

    //      public void SetCameraIntensity(float currentBoostTimer, bool isDrafting, float techSpeedMult) {
    //          _currentFOV = _FOVPerBoostSecond.Evaluate(currentBoostTimer);
    // _currentFOV += isDrafting ? draftingAddedIntensity : 0;
    // _currentFOV += techSpeedMult * _FOVPerTechMult;
    // float camMag = _shakeMagnitudePerBoostSecond.Evaluate(currentBoostTimer);
    //          float camRoughness = _shakeRoughnessPerBoostSecond.Evaluate(currentBoostTimer);
    //
    //          SetCamShake(camMag, camRoughness);
    //      }
    //
    //      void SetCamShake(float mag, float roughness) {
    //          if (_currentCameraShake == null) {
    //              if (mag != 0 && roughness != 0) {
    //
    //                  _currentCameraShake = _cameraShaker.StartShake(mag, roughness, 0);
    //                  _currentCameraShake.DeleteOnInactive = false;
    //              }
    //          } else {
    //              if (mag != 0 && roughness != 0) {
    //                  switch (_currentCameraShake.CurrentState) {
    //                      case CameraShakeState.FadingOut:
    //                          _currentCameraShake.StartFadeIn(0);
    //                          break;
    //                      case CameraShakeState.Inactive:
    //                          _currentCameraShake.StartFadeIn(0);
    //                          break;
    //                      default:
    //                          break;
    //                  }
    //                  _currentCameraShake.Magnitude = mag;
    //                  _currentCameraShake.Roughness = roughness;
    //              } else {
    //                  _currentCameraShake.StartFadeOut(0);
    //              }
    //
    //          }
    //      }

    private void FixedUpdate()
    {
        CameraUpdate(Time.fixedDeltaTime);
    }

    
    void CameraUpdate(float deltaTime)
    {
        if (Input.GetKey(KeyCode.Space))
        {
            _target = turret;
            _camera_Pivot = turretPivot;
            this.transform.parent = _camera_Pivot;
            this.transform.localPosition = Vector3.zero;
            this.transform.localRotation = Quaternion.identity;
            return;
        }
        else
        {
            this.transform.parent = null;
            _target = _airplaneTarget;
            _camera_Pivot = _airplane_Pivot;
        }
        if (_target)
        {
            //Quaternion newRot = Quaternion.Euler(_target.eulerAngles.x, _target.eulerAngles.y, 0);
            //Transform.rotation = newRot;

            Vector3 awayVectorNorm = (Transform.position - _target.position).normalized;
            Vector3 rate = Vector3.zero;

            //handles ship backing up into camera
            float angleVelocityVsAway = Vector3.Angle(_playerRB.velocity, awayVectorNorm);
            if (angleVelocityVsAway < backingUpLeewayAngle)
            {
                float distanceToUse = Mathf.Lerp(0, backingUpMaxDistanceFromShip,
                    1 - (angleVelocityVsAway / backingUpLeewayAngle));
                rate = _target.forward * Mathf.Clamp(_target.InverseTransformDirection(_playerRB.velocity).z * .1f,
                    -distanceToUse, 0);
            }

            Vector3 newPos = _camera_Pivot.position + (awayVectorNorm * (CamDistance)) + rate;
            Vector3 pos =
                (Vector3.SmoothDamp(Transform.position, newPos, ref _velocity, 0)); //camDragChangeRate * .003f 
            Transform.position = pos;
            //Transform.rotation = Quaternion.LookRotation(-awayVectorNorm);
            Transform.rotation = _target.rotation;
        }

        float tgFOV = IsZTarget ? zTargetFOV : (IsGrabbing ? grabFOV : _currentFOV);
        _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, tgFOV, deltaTime * fieldOfViewChangeRate);
    }
}