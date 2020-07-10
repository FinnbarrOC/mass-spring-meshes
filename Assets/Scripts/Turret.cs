using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Turret : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] [Min(0)] private float roundsPerMinute = 600;
    [SerializeField] private AudioClip fireBulletSound;
    [SerializeField] [Range(0, 1)] private float bulletSoundVolume = 0.5f;

    [SerializeField] [Min(0)] private float turretAimSensitivity = 1;
    
    private AudioSource _firingAudioSource;
    

    private void Awake()
    {
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
            _firingAudioSource.Play();
            GameObject newBullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            Rigidbody rb = newBullet.GetComponent<Rigidbody>();
            rb.velocity += this.GetComponentInParent<Rigidbody>().velocity;
            yield return firingDelay;
        }
    }

    private void FixedUpdate()
    {
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
