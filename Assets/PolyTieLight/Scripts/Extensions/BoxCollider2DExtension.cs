﻿using UnityEngine;
using System.Collections;
using System.Linq;

public static class BoxCollider2DExtension 
{
    /// <summary>
    /// Returns the corners of this box collider in world coordinates.
    /// </summary>
    /// <param name="box">Box collider to calculate the corner from</param>
    /// <returns>Vector2 array of length 4 containing the corners in world coordinates</returns>
    public static Vector2[] GetWorldCorners(this BoxCollider2D box)
    {
        var corners = new Vector2[4];
        var scale = (Vector2)box.transform.localScale;
        var trans = (Vector2)box.transform.position + box.center;
        var rot = box.transform.localRotation;

        corners[0] = trans + (Vector2)(rot * Vector2.Scale(new Vector2(-box.size.x, box.size.y) * 0.5f, scale));
        corners[1] = trans + (Vector2)(rot * Vector2.Scale(box.size * 0.5f, scale));
        corners[2] = trans + (Vector2)(rot * Vector2.Scale(new Vector2(box.size.x, -box.size.y) * 0.5f, scale));
        corners[3] = trans - (Vector2)(rot * Vector2.Scale(box.size * 0.5f, scale));

        return corners;
    }

    public static void SetWorldCenter(this BoxCollider2D box, Vector3 position)
    {
        box.transform.position = position - new Vector3(box.center.x, box.center.y, 0);
    }
}
