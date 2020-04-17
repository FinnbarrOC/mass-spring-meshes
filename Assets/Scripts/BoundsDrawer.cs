using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundsDrawer : MonoBehaviour
{
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
