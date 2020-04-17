using UnityEngine;

public class ClosestMeshTriangle : MonoBehaviour
{
    private Mesh _mesh;
    private MeshBoundsTree _tree;
    private Camera _mainCamera;
    
    
    private void Start()
    {
        _mesh = GetComponent<MeshFilter>().mesh;
        _tree = new MeshBoundsTree(_mesh, transform);
        print("done construction");
        
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, 100 * ray.direction, Color.red, 60);

            int[] hitTriangle = _tree.RaycastTriangle(ray);

            if (hitTriangle == null)
            {
                print("missed mesh bounds");
            }

            else
            {
                print("Hit and drew a triangle");
                
                Vector3 vertex0 = transform.TransformPoint(_mesh.vertices[hitTriangle[0]]);
                Vector3 vertex1 = transform.TransformPoint(_mesh.vertices[hitTriangle[1]]);
                Vector3 vertex2 = transform.TransformPoint(_mesh.vertices[hitTriangle[2]]);

                Debug.DrawLine(vertex0, vertex1, Color.cyan, 60);
                Debug.DrawLine(vertex1, vertex2, Color.cyan, 60);
                Debug.DrawLine(vertex2, vertex0, Color.cyan, 60);
            }
        }
        
        // Reference for what randomly drawing a triangle looks like
        if (Input.GetMouseButtonDown(1))
        {
            System.Random generator = new System.Random();
            int triangleIndex = generator.Next(0, _mesh.triangles.Length / 3);

            int[] hitTriangle =
            {
                _mesh.triangles[3 * triangleIndex], 
                _mesh.triangles[3 * triangleIndex + 1], 
                _mesh.triangles[3 * triangleIndex + 2]
            };
            
            print("Painting random triangle");

            Vector3 vertex0 = transform.TransformPoint(_mesh.vertices[hitTriangle[0]]);
            Vector3 vertex1 = transform.TransformPoint(_mesh.vertices[hitTriangle[1]]);
            Vector3 vertex2 = transform.TransformPoint(_mesh.vertices[hitTriangle[2]]);

            Debug.DrawLine(vertex0, vertex1, Color.cyan, 60);
            Debug.DrawLine(vertex1, vertex2, Color.cyan, 60);
            Debug.DrawLine(vertex2, vertex0, Color.cyan, 60);
        }
    }
}
