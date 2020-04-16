using System.Linq;
using UnityEngine;

public class BoundsTreeRoot
{
    public readonly Vector3[] Vertices;
    public readonly int[] Triangles;
    public readonly int NumTriangles;
    public Bounds[] TriangleBounds;

    private BoundsTree _tree;

    public BoundsTreeRoot(Mesh mesh)
    {
        Vertices = mesh.vertices;
        Triangles = mesh.triangles;
        NumTriangles = Triangles.Length / 3;
        InitializeTriangleBounds();
        
        int[] allTriangleIndices = Enumerable.Repeat(1, NumTriangles).ToArray();
        _tree = new BoundsTree(mesh.bounds, allTriangleIndices, this);
    }
    
    private void InitializeTriangleBounds()
    {
        TriangleBounds = new Bounds[NumTriangles];
        
        for (int tri = 0; tri < NumTriangles; tri++)
        {
            Vector3 vertex0 = Vertices[Triangles[3*tri]];
            Vector3 vertex1 = Vertices[Triangles[3*tri+1]];
            Vector3 vertex2 = Vertices[Triangles[3*tri+2]];

            Vector3 minCorner = new Vector3
            {
                [0] = Mathf.Min(vertex0[0], vertex1[0], vertex2[0]),
                [1] = Mathf.Min(vertex0[1], vertex1[1], vertex2[1]),
                [2] = Mathf.Min(vertex0[2], vertex1[2], vertex2[2])
            };

            Vector3 maxCorner = new Vector3
            {
                [0] = Mathf.Max(vertex0[0], vertex1[0], vertex2[0]),
                [1] = Mathf.Max(vertex0[1], vertex1[1], vertex2[1]),
                [2] = Mathf.Max(vertex0[2], vertex1[2], vertex2[2])
            };

            TriangleBounds[tri].SetMinMax(minCorner, maxCorner);
        }
    }
}

public class BoundsTree
{
    private Bounds _bounds;
    private int[] _containedTriangles;  // one-hot vector of triangle indices contained in _bounds
    private BoundsTreeRoot _root;
    
    private BoundsTree _leftChild = null;
    private BoundsTree _rightChild = null;
    
    
    public BoundsTree(Bounds bounds, int[] containedTriangles, BoundsTreeRoot root)
    {
        _bounds = bounds;
        _containedTriangles = containedTriangles;
        _root = root;

        int longestAxis = BoundsLongestAxis(_bounds);
        
        Bounds boundsL, boundsR;
        int[] containedTrisL, containedTrisR;
        (boundsL, boundsR, containedTrisL, containedTrisR) = PartitionTriangles(longestAxis);

        if (containedTrisL.Length > 0)
        {
            _leftChild = new BoundsTree(boundsL, containedTrisL, _root);
        }
        if (containedTrisR.Length > 0)
        {
            _rightChild = new BoundsTree(boundsR, containedTrisR, _root);
        }
    }

    private static int BoundsLongestAxis(Bounds bounds)
    {
        Vector3 boundsSides = bounds.max - bounds.min;
        int longestAxis = 0;

        if (boundsSides[1] > boundsSides[0] || boundsSides[2] > boundsSides[0]) {
            if (boundsSides[1] > boundsSides[2])
            {
                longestAxis = 1;
            }
            else
            {
                longestAxis = 2;
            }
        }

        return longestAxis;
    }

    private (Bounds, Bounds, int[], int[]) PartitionTriangles(int longestAxis)
    {
        Bounds boundsLeft = new Bounds();
        Bounds boundsRight = new Bounds();
        int[] containedTrianglesLeft = Enumerable.Repeat(0, _root.NumTriangles).ToArray();
        int[] containedTrianglesRight = Enumerable.Repeat(0, _root.NumTriangles).ToArray();

        float splitValue = _bounds.center[longestAxis];
        for (int tri = 0; tri < _root.NumTriangles; tri++)
        {
            if (_root.TriangleBounds[tri].center[longestAxis] <= splitValue)
            {
                containedTrianglesLeft[tri] = 1;
                boundsLeft.Encapsulate(_root.TriangleBounds[tri]);
            }
            else
            {
                containedTrianglesRight[tri] = 1;
                boundsRight.Encapsulate(_root.TriangleBounds[tri]);
            }
        }

        return (boundsLeft, boundsRight, containedTrianglesLeft, containedTrianglesRight);
    }
}
