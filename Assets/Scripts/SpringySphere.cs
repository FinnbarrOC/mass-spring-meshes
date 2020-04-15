using System;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(SphereCollider))]
public class SpringySphere : MonoBehaviour
{
	[Tooltip("How elastic the springy mesh is")]
	public float springConstant = 10.0f;
	[Tooltip("How quickly the springy mesh returns to rest")]
	public float springDamping = 5.0f;
	[Tooltip("How relatively small disruptions are to the mesh")]
	public float displacementScale = 5.0f;
	[Tooltip("How pointed/focused the mesh displacement force should be")]
	public float forceOffset = 0.1f;

	public float draftPer100 = 0.3f;
	private float _radiusSquared;
	public Rigidbody player;
	
	private Mesh _mesh;
	private Vector3[] _restVertices;
	private Vector3[] _displacedVertices;
	private Vector3[] _vertexVelocities;
	private SphereCollider _sphereCollider;

	private void Start()
	{
		_sphereCollider = GetComponent<SphereCollider>();
		_radiusSquared = Mathf.Pow((_sphereCollider.radius * transform.localScale.x), 2);

		_mesh = GetComponent<MeshFilter>().mesh;
		_restVertices = _mesh.vertices;
		_displacedVertices = new Vector3[_mesh.vertexCount];
		Array.Copy(_restVertices, _displacedVertices, _mesh.vertexCount);
		_vertexVelocities = new Vector3[_mesh.vertexCount];
	}

	private void FixedUpdate()
	{
		UpdatePhysics();
		UpdateVisuals();
	}
	
	#region RIGIDBODY PHYSICS

	private void UpdatePhysics()
	{
		Vector3 playerPosition = player.transform.position;
		float distanceToCenterSquared = (playerPosition - transform.position).sqrMagnitude;
		
		if (distanceToCenterSquared > _radiusSquared)
		{
			float distanceToWall = Mathf.Sqrt(distanceToCenterSquared - _radiusSquared);
			Vector3 directionTowardsPlayer = (transform.position - playerPosition).normalized;
			player.AddForce(directionTowardsPlayer * (distanceToWall * springConstant));

			Vector3 offsetHitPoint = playerPosition - (forceOffset * player.velocity.normalized);
			AddDeformingForce(offsetHitPoint, player.velocity.magnitude);
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