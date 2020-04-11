using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(Collider))]
public class OnExitSpringMesh : MonoBehaviour
{
    [Tooltip("How elastic the springy mesh is")]
    public float springConstant = 10.0f;
    [Tooltip("How quickly the springy mesh returns to rest")]
    public float springDamping = 5.0f;
    [Tooltip("How relatively small disruptions are to the mesh")]
    public float displacementScale = 5.0f;
    [Tooltip("How pointed/focused the mesh displacement force should be")]
    public float forceOffset = 0.1f;
    
    private Mesh _mesh;
    private Vector3[] _restVertices;
    private Vector3[] _displacedVertices;
    private Vector3[] _vertexVelocities;
    private Collider _collider;
    
    private readonly HashSet<Rigidbody> _enteredRigidbodies = new HashSet<Rigidbody>();
    
    private void Start()
    {
        _mesh = GetComponent<MeshFilter>().mesh;
        _restVertices = _mesh.vertices;
        _displacedVertices = new Vector3[_mesh.vertexCount];
        Array.Copy(_restVertices, _displacedVertices, _mesh.vertexCount);
        _vertexVelocities = new Vector3[_mesh.vertexCount];

        _collider = GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        UpdatePhysics();
        UpdateVisuals();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger) return;

        Rigidbody outgoingRigidBody = other.GetComponent<Rigidbody>();
        if (outgoingRigidBody)
        {
            // incomingRigidBody is passing through _collider to the outside, where force IS applied
            _enteredRigidbodies.Add(outgoingRigidBody);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;

        Rigidbody incomingRigidBody = other.GetComponent<Rigidbody>();
        if (incomingRigidBody && _enteredRigidbodies.Contains(incomingRigidBody))
        {
            // incomingRigidBody is passing through _collider to the inside, where NO force is applied
            _enteredRigidbodies.Remove(incomingRigidBody);
        }
    }

    #region RIGIDBODY PHYSICS

    private void UpdatePhysics()
    {
        foreach (Rigidbody curRigidbody in _enteredRigidbodies)
        {
            Vector3 closestMeshPoint = _collider.ClosestPoint(curRigidbody.position);
            Vector3 forceDirection = closestMeshPoint - curRigidbody.position;
            float displacement = forceDirection.magnitude;
            Vector3 forceTowardsRigidBody = springConstant * displacement * forceDirection;
            
            curRigidbody.AddForce(forceTowardsRigidBody);
            
            Vector3 offsetHitPoint = curRigidbody.position - (forceOffset * curRigidbody.velocity.normalized);
            AddDeformingForce(offsetHitPoint, curRigidbody.velocity.magnitude);
        }
    }

    #endregion
    
    #region MESH DEFORMATION VISUALS

    private void UpdateVisuals()
    {
        for (int vertexIndex = 0; vertexIndex < _displacedVertices.Length; vertexIndex++)
        {
            UpdateVertex(vertexIndex);
        }
        
        _mesh.SetVertices(_displacedVertices);
        _mesh.RecalculateNormals();
    }
    
    private void AddDeformingForce(Vector3 hitPoint, float force)
    {
        hitPoint = transform.InverseTransformPoint(hitPoint);
        for (int vertexIndex = 0; vertexIndex < _displacedVertices.Length; vertexIndex++)
        {
            AddForceToVertex(vertexIndex, hitPoint, force);
        }
    }

    private void AddForceToVertex(int vertexIndex, Vector3 hitPoint, float force)
    {
        Vector3 hitPointToVertex = _displacedVertices[vertexIndex] - hitPoint;
        hitPointToVertex *= displacementScale;
        float distanceScaledForce = force / (hitPointToVertex.sqrMagnitude + 1);
        float velocity = distanceScaledForce * Time.fixedDeltaTime;

        _vertexVelocities[vertexIndex] += velocity * hitPointToVertex.normalized;
    }

    private void UpdateVertex(int vertexIndex)
    {
        Vector3 velocity = _vertexVelocities[vertexIndex];

        // Adjust velocity to account for spring force
        Vector3 displacement = _displacedVertices[vertexIndex] - _restVertices[vertexIndex];
        displacement *= displacementScale;
        velocity -= displacement * (springConstant * Time.fixedDeltaTime);
        velocity *= 1.0f - springDamping * Time.fixedDeltaTime;
        _vertexVelocities[vertexIndex] = velocity;

        _displacedVertices[vertexIndex] += velocity * (Time.fixedDeltaTime / displacementScale);
    }

    #endregion
}
