using System;
using System.Collections;
using Mirror;
using Unity.Mathematics;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Vehicles.Aeroplane;

public class Turret : MonoBehaviour
{
    private NetworkIdentity _netID;
    private AeroplaneUserControl4Axis _controller;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] [Min(0)] private float roundsPerMinute = 600;
    [SerializeField] private AudioClip fireBulletSound;
    [SerializeField] [Range(0, 1)] private float bulletSoundVolume = 0.5f;

    [SerializeField] [Min(0)] private float turretAimSensitivity = 1;
    
    private AudioSource _firingAudioSource;
    

    private void Awake()
    {
        _controller = gameObject.transform.root.GetComponent<AeroplaneUserControl4Axis>();
        _netID = gameObject.transform.root.GetComponent<NetworkIdentity>();
        _firingAudioSource = gameObject.AddComponent<AudioSource>();
        _firingAudioSource.playOnAwake = false;
        _firingAudioSource.clip = fireBulletSound;
        _firingAudioSource.volume = bulletSoundVolume;
    }

    private void OnValidate()
    {
        if (_firingAudioSource) _firingAudioSource.volume = bulletSoundVolume;
    }

    
    
    private void Update()
    {
        if (!_netID.isLocalPlayer)
        {
            Debug.Log("not player!");
            return;
        }    
        
        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(FireBullets());
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopCoroutine(FireBullets());
        }
    }

    private IEnumerator FireBullets()
    {
        WaitForSeconds firingDelay = new WaitForSeconds(60 / roundsPerMinute);
        
        while (Input.GetMouseButton(0))
        {
            yield return new WaitForEndOfFrame();
            //RpcPlayShootSound();
            Debug.Log("fire 1");
            Vector3 vel = this.GetComponentInParent<Rigidbody>().velocity;
            
            _controller.CmdSpawnObject(bulletSpawnPoint.position, bulletSpawnPoint.rotation, vel);
            //CmdSpawnBullet(null); //newBullet);
            yield return firingDelay;
        }
    }

    // this is called on the tank that fired for all observers
    /*[ClientRpc]
    void RpcPlayShootSound()
    {
        _firingAudioSource.Play();
    }
*/
    private void FixedUpdate()
    {
        if (!_netID.isLocalPlayer) return;
        
        if (Input.GetKey(KeyCode.Space))
        {
            transform.Rotate(-turretAimSensitivity * CrossPlatformInputManager.GetAxis("Mouse Y"), 
                turretAimSensitivity * CrossPlatformInputManager.GetAxis("Mouse X"), 0);
            Quaternion rot = transform.rotation;

            transform.rotation = Quaternion.Euler(rot.eulerAngles.x, rot.eulerAngles.y, 0);
        }

        else
        {
            transform.localRotation = quaternion.identity;
        }
    }
}
