using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class PerFaceNormals : MonoBehaviour
{
    private Mesh _mesh;

    
    private void Awake()
    {
        _mesh = GetComponent<MeshFilter>().mesh;
    }

    private void Start()
    {
        int trianglesLength = _mesh.triangles.Length;
        
        int[] newTriangles = new int[trianglesLength];
        Vector3[] newVertices = new Vector3[trianglesLength];
        Vector3[] newNormals = new Vector3[trianglesLength];

        for (int triangleIndex = 0; triangleIndex < trianglesLength; triangleIndex++)
        {
            newTriangles[triangleIndex] = triangleIndex;
            newVertices[triangleIndex] = _mesh.vertices[_mesh.triangles[triangleIndex]];

            if (triangleIndex % 3 == 2)
            {
                // We've assigned all 3 vertices of the current triangle, so now assign normals
                Vector3 thisTriangleNormal = GetTriangleNormal(newVertices[triangleIndex],
                    newVertices[triangleIndex - 1], newVertices[triangleIndex - 2]);

                newNormals[triangleIndex] = thisTriangleNormal;
                newNormals[triangleIndex - 1] = thisTriangleNormal;
                newNormals[triangleIndex - 2] = thisTriangleNormal;
            }
        }

        _mesh.vertices = newVertices;
        _mesh.triangles = newTriangles;
        _mesh.normals = newNormals;
    }

    private static Vector3 GetTriangleNormal(Vector3 cornerA, Vector3 cornerB, Vector3 cornerC)
    {
        return -Vector3.Cross((cornerB - cornerA), (cornerC - cornerA)) / 2;
    }
}
