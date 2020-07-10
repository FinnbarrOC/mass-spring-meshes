using Mirror;
using UnityEngine;


    public class Bullet : NetworkBehaviour
    {
        public float destroyAfter = 5;
        public Rigidbody rigidBody;
        public float force = 1000;

        
        private void Start()
        {
            Destroy(gameObject, destroyAfter);
            rigidBody.AddForce(transform.up * force);
        }

        private void OnTriggerEnter(Collider co)
        {
            Destroy(gameObject);
        }
    }
