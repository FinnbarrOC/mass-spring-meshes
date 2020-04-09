using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerOnMouse : MonoBehaviour
{
    public GameObject objectToSpawn;
    public float heightOffset;
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                Vector3 spawnPosition = hit.point;
                spawnPosition.y += heightOffset;
                
                Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);
            }
        }
    }
}
