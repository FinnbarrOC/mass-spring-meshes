using UnityEngine;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(LineRenderer))]
public class TriangleHoverDrawer : MonoBehaviour
{
    [SerializeField] private float drawOffset = 0.1f;
    
    private Camera _camera;
    private LineRenderer _lineRenderer;
    
    // For calculations
    private RaycastHit _hit;
    private MeshCollider _meshCollider;
    private Mesh _mesh;
    private Transform _hitTransform;
    private int _vertexIndex0, _vertexIndex1, _vertexIndex2;
    private Vector3 _position0, _position1, _position2;

    private void Start()
    {
        _camera = GetComponent<Camera>();
        _lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (!Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out _hit))
            return;

        _meshCollider = _hit.collider as MeshCollider;
        if (_meshCollider == null || _meshCollider.sharedMesh == null)
            return;

        _mesh = _meshCollider.sharedMesh;
        _hitTransform = _hit.collider.transform;
        
        _vertexIndex0 = _mesh.triangles[_hit.triangleIndex * 3 + 0];
        _vertexIndex1 = _mesh.triangles[_hit.triangleIndex * 3 + 1];
        _vertexIndex2 = _mesh.triangles[_hit.triangleIndex * 3 + 2];
        
        _position0 = _hitTransform.TransformPoint(_mesh.vertices[_vertexIndex0]);
        _position1 = _hitTransform.TransformPoint(_mesh.vertices[_vertexIndex1]);
        _position2 = _hitTransform.TransformPoint(_mesh.vertices[_vertexIndex2]);
        
        _position0 += drawOffset * _mesh.normals[_vertexIndex0];
        _position1 += drawOffset * _mesh.normals[_vertexIndex1];
        _position2 += drawOffset * _mesh.normals[_vertexIndex2];
        
        _lineRenderer.SetPositions(new []{_position0, _position1, _position2});
    }
}