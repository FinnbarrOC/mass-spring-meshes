﻿using System;
using System.Collections;
using UnityEngine;


public class MeshBoundsTree
{
    private readonly Transform _transform;
    private readonly Vector3[] _vertices;
    private readonly int[] _triangles;
    public readonly int NumTriangles;
    public Bounds[] TriangleBounds;

    private readonly MeshBoundsTreeNode _node;

    public MeshBoundsTree(Mesh mesh, Transform transform)
    {
        _transform = transform;
        _vertices = mesh.vertices;
        _triangles = mesh.triangles;
        NumTriangles = _triangles.Length / 3;
        Bounds totalBounds = InitializeTriangleBounds();
        
        BitArray allTriangleIndices = new BitArray(NumTriangles, true);
        _node = new MeshBoundsTreeNode(totalBounds, allTriangleIndices, NumTriangles, this);
    }
    
    private Bounds InitializeTriangleBounds()
    {
        Bounds totalBounds = new Bounds();
        TriangleBounds = new Bounds[NumTriangles];
        
        for (int tri = 0; tri < NumTriangles; tri++)
        {
            Vector3 vertex0 = _transform.TransformPoint(_vertices[_triangles[3*tri]]);
            Vector3 vertex1 = _transform.TransformPoint(_vertices[_triangles[3*tri+1]]);
            Vector3 vertex2 = _transform.TransformPoint(_vertices[_triangles[3*tri+2]]);

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
            totalBounds.Encapsulate(TriangleBounds[tri]);
        }

        return totalBounds;
    }

    public int[] RaycastTriangle(Ray ray)
    {
        int tri = _node.RaycastTriangleIndex(ray);

        if (tri == -1)
        {
            return null;
        }

        return new[]
        {
            _triangles[3 * tri], 
            _triangles[3 * tri + 1], 
            _triangles[3 * tri + 2]
        };
    }
}


internal class MeshBoundsTreeNode
{
    private Bounds _bounds;
    private readonly BitArray _triangles;  // true where triangle at given index is contained in _bounds
    private readonly int _numTriangles;    // number of triangles contained in _bounds
    private readonly MeshBoundsTree _root;
    
    private readonly MeshBoundsTreeNode _childLeft = null;
    private readonly MeshBoundsTreeNode _childRight = null;
    
    
    public MeshBoundsTreeNode(Bounds bounds, BitArray triangles, int numTriangles, MeshBoundsTree root)
    {
        _bounds = bounds;
        _triangles = triangles;
        _numTriangles = numTriangles;
        _root = root;

        int longestAxis = BoundsLongestAxis(_bounds);
        
        BitArray trianglesLeft, trianglesRight;
        int numTrianglesLeft, numTrianglesRight;
        (trianglesLeft, trianglesRight, numTrianglesLeft, numTrianglesRight) = PartitionTriangles(longestAxis);
        
        Bounds boundsLeft, boundsRight;
        (boundsLeft, boundsRight) = GenerateBounds(trianglesLeft);

        if (numTrianglesLeft > 1)
        {
            _childLeft = new MeshBoundsTreeNode(boundsLeft, trianglesLeft, numTrianglesLeft, _root);
        }
        if (numTrianglesRight > 1)
        {
            _childRight = new MeshBoundsTreeNode(boundsRight, trianglesRight, numTrianglesRight, _root);
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

    private (BitArray, BitArray, int, int) PartitionTriangles(int longestAxis)
    {
        float splitValue = _bounds.center[longestAxis];
        
        // Assign triangles to either _childLeft or _childRight
        BitArray trianglesLeft = new BitArray(_root.NumTriangles, false);
        BitArray trianglesRight = new BitArray(_root.NumTriangles, false);

        int numTrianglesLeft = 0;
        int numTrianglesRight = 0;
        int swapIndexLeft = -1;
        int swapIndexRight = -1;
        
        for (int tri = 0; tri < _root.NumTriangles; tri++)
        {
            if (!_triangles[tri]) continue;
            
            if (_root.TriangleBounds[tri].center[longestAxis] <= splitValue)
            {
                trianglesLeft[tri] = true;
                numTrianglesLeft++;
                swapIndexLeft = tri;  // Keep track of a "true" position if we need to swap

            }
            else
            {
                trianglesRight[tri] = true;
                numTrianglesRight++;
                swapIndexRight = tri;  // Keep track of a "true" position if we need to swap
            }
        }
        
        // Failsafe: if one child has no triangles, swap it one from the other child
        if (_numTriangles > 1)
        {
            if (numTrianglesLeft == 0)
            {
                trianglesRight[swapIndexRight] = false;
                trianglesLeft[swapIndexRight] = true;
            }
            else if (numTrianglesRight == 0)
            {
                trianglesLeft[swapIndexLeft] = false;
                trianglesRight[swapIndexLeft] = true;
            }
        }

        return (trianglesLeft, trianglesRight, numTrianglesLeft, numTrianglesRight);
    }

    private (Bounds, Bounds) GenerateBounds(BitArray trianglesLeft)
    {
        Bounds boundsLeft = new Bounds();
        Bounds boundsRight = new Bounds();
        
        for (int tri = 0; tri < _root.NumTriangles; tri++)
        {
            if (trianglesLeft[tri])
            {
                boundsLeft.Encapsulate(_root.TriangleBounds[tri]);
            }
            else
            {
                boundsRight.Encapsulate(_root.TriangleBounds[tri]);
            }
        }

        return (boundsLeft, boundsRight);
    }

    public int RaycastTriangleIndex(Ray ray)
    {
        if (!_bounds.IntersectRay(ray))
        {
            return -1;
        }
        
        BoundsDrawer.DrawBounds(_bounds, Color.green, 60);
        
        // Base Case
        if (_childLeft is null && _childRight is null)
        {
            // _bounds contains 1 triangle, so return its index
            bool[] trianglesArray = new bool[_root.NumTriangles];
            _triangles.CopyTo(trianglesArray, 0);
            return Array.IndexOf(trianglesArray, true);
        }
        
        // Recursive Case
        int leftResult = -1;
        int rightResult = -1;

        if (_childLeft != null)
        {
            leftResult = _childLeft.RaycastTriangleIndex(ray);
        }
        if (_childRight != null)
        {
            rightResult = _childRight.RaycastTriangleIndex(ray);
        }

        return Math.Max(leftResult, rightResult);
    }
}
