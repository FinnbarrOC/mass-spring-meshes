using UnityEngine;


public static class Utils
{
    public static bool RayTriangleIntersect(Ray ray, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2)
    {
        Vector3 triangleNormal = Vector3.Cross((vertex1 - vertex0), (vertex2 - vertex0)).normalized;

        float q = Vector3.Dot(triangleNormal, vertex0);
        float numerator = q - Vector3.Dot(triangleNormal, ray.origin);
        float denominator = Vector3.Dot(triangleNormal, ray.direction);

        // Check if ray.direction is parallel to plane
        if (Mathf.Approximately(denominator, 0)) {
            return false;
        }
        
        Vector3 intersectionPoint = ray.origin + (numerator / denominator) * ray.direction;
        
        // Check that ray lands inside all 3 triangle edges
        bool inSide0 = Vector3.Dot(Vector3.Cross((vertex1 - vertex0), intersectionPoint - vertex0), triangleNormal) >= 0;
        bool inSide1 = Vector3.Dot(Vector3.Cross((vertex2 - vertex1), intersectionPoint - vertex1), triangleNormal) >= 0;
        bool inSide2 = Vector3.Dot(Vector3.Cross((vertex0 - vertex2), intersectionPoint - vertex2), triangleNormal) >= 0;

        return (inSide0 && inSide1 && inSide2);
    }
    
    public static void DrawBounds(Bounds bounds, Color color, float duration)
    {
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;
  
        Vector3 frontTopLeft = new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z);
        Vector3 frontTopRight = new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z);
        Vector3 frontBottomLeft = new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z);
        Vector3 frontBottomRight = new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z);
        Vector3 backTopLeft = new Vector3(center.x - extents.x, center.y + extents.y, center.z + extents.z);
        Vector3 backTopRight = new Vector3(center.x + extents.x, center.y + extents.y, center.z + extents.z);
        Vector3 backBottomLeft = new Vector3(center.x - extents.x, center.y - extents.y, center.z + extents.z);
        Vector3 backBottomRight = new Vector3(center.x + extents.x, center.y - extents.y, center.z + extents.z);
        
        Debug.DrawLine(frontTopLeft, frontTopRight, color, duration);
        Debug.DrawLine(frontTopRight, frontBottomRight, color, duration);
        Debug.DrawLine(frontBottomRight, frontBottomLeft, color, duration);
        Debug.DrawLine(frontBottomLeft, frontTopLeft, color, duration);
         
        Debug.DrawLine(backTopLeft, backTopRight, color, duration);
        Debug.DrawLine(backTopRight, backBottomRight, color, duration);
        Debug.DrawLine(backBottomRight, backBottomLeft, color, duration);
        Debug.DrawLine(backBottomLeft, backTopLeft, color, duration);
         
        Debug.DrawLine(frontTopLeft, backTopLeft, color, duration);
        Debug.DrawLine(frontTopRight, backTopRight, color, duration);
        Debug.DrawLine(frontBottomRight, backBottomRight, color, duration);
        Debug.DrawLine(frontBottomLeft, backBottomLeft, color, duration);
    }
}
