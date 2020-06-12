using Mirror;
using UnityEngine;
using UnityEngine.AI;


public class ProjectileShooter : NetworkBehaviour
{
    [Header("Firing")]
    public KeyCode shootKey = KeyCode.Space;
    public GameObject projectilePrefab;
    public Transform projectileMount;

    void Update()
    {
        // shoot
        if (Input.GetKeyDown(shootKey))
        {
            CmdFire(projectileMount.position, projectileMount.rotation);
        }
    }

    // this is called on the server
    [Command]
    void CmdFire(Vector3 origin, Quaternion direction)
    {
        GameObject projectile = Instantiate(projectilePrefab, origin, direction);
        NetworkServer.Spawn(projectile);
        RpcOnFire();
    }

    // this is called on the GameObject that fired for all observers
    [ClientRpc]
    void RpcOnFire()
    {
        // TODO: play shooting animation or something
    }
}