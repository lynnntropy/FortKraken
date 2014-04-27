﻿using UnityEngine;
using System;
using System.Collections;

public static class Utilities
{
    /// <summary>
    /// Casts a ray from the current mouse position and intersects it with a zPlane at the origin
    /// to derive world coordinates from the current mouse position in screen coordinates. Because
    /// the z-plane intersects the origin the z value of all possible calculated world coordinates
    /// will always be. If the intersection was successful the given action is performed and the 
    /// mouse world position is passed as a parameter.
    /// </summary>
    /// <param name="action">The action that should be performed. The world cooridinates of the mouse position are passed as a parameter</param>
    public static void PerformActionAtMouseWorldPosition(Action<Vector3> action)
    {
        Ray worldRay = Camera.current.ScreenPointToRay(new Vector3(Event.current.mousePosition.x, Camera.current.pixelHeight - Event.current.mousePosition.y, 0.0f));
        Plane zPlane = new Plane(Vector3.back, Vector3.zero);
        float distance = 0.0f;
        if (zPlane.Raycast(worldRay, out distance))
        {
            action(worldRay.GetPoint(distance));
        }
    }

    /// <summary>
    /// Parses the given string vor colour values
    /// and returns an according colour object.
    /// </summary>
    /// <param name="data">Data string of format: RGBA(RRR.RRR, GGG.GGG, BBB.BBB, A.AAA)</param>
    /// <returns>Color object with RGB values RRR GGG and BBB and AAA</returns>
    public static Color ParseColorString(string data)
    {
        // truncate.
        data = data.Substring(5, 25);
        string[] rgbValues = data.Split(',');
        float r = 0; float g = 0; float b = 0; float a = 0;
        if (rgbValues.Length >= 4)
        {
            float.TryParse(rgbValues[0], out r);
            float.TryParse(rgbValues[1], out g);
            float.TryParse(rgbValues[2], out b);
            float.TryParse(rgbValues[3], out a);
        }

        return new Color(r, g, b, a);
    }

    public static ColorStates DetermineColorState(Collider2D collider)
    {
        return ColorStates.IDLE;
    }

    /// <summary>
    /// Calculates the next matching position from the given input position on the grid 
    /// with the given grid size. The grids origin is assumed to be (0/0)
    /// </summary>
    /// <param name="input">Input position to calcualte the grid position from</param>
    /// <param name="gridSize">Size of the grid</param>
    /// <returns>Closest grid position</returns>
    public static Vector3 SnapToGrid(Vector3 input, float gridSize)
    {
        input.x = gridSize * Mathf.Round(input.x / gridSize);
        input.y = gridSize * Mathf.Round(input.y / gridSize);

        return input;
    }

    /// <summary>
    /// Aligns the given input position with either the x or y Axis depending to which
    /// axis the input position is closer
    /// </summary>
    /// <param name="input">Input position to align the axis with</param>
    /// <param name="startingPoint">The position when started draging</param>
    /// <returns>Position aligned to x or y axis</returns>
    public static Vector3 AlignWithAxis(Vector3 input, Vector2 startingPoint)
    {
        Vector2 deltaMove = (Vector2)input - startingPoint;
        if (Mathf.Abs(deltaMove.x) > Mathf.Abs(deltaMove.y))  // Align to x axis
        {
            input.y = startingPoint.y;
        }
        else  // Align to y axis
        {
            input.x = startingPoint.x;
        }

        return input;
    }

    /// <summary>
    /// Snaps to the given input to the given snap values.
    /// </summary>
    /// <param name="input">Input position to snap from</param>
    /// <param name="startingPoint">The position when started draging</param>
    /// <param name="xSnap">Snap value in x direction</param>
    /// <param name="ySnap">Snap value in y direction</param>
    /// <returns>Position snapped to given values</returns>
    public static Vector3 SnapToValues(Vector3 input, Vector2 startingPoint, float xSnap, float ySnap)
    {
        Vector2 deltaMove = new Vector2(input.x, input.y) - startingPoint;
        deltaMove.x = xSnap * Mathf.Round(deltaMove.x / xSnap);
        deltaMove.y = ySnap * Mathf.Round(deltaMove.y / ySnap);
        input = startingPoint + deltaMove;

        return input;
    }

    /// <summary>
    /// Checks if ther is another polygon objects close by and tries to snap to its vertex
    /// </summary>
    /// <param name="parent">Collider that is manipulated</param>
    /// <param name="input">Input position to snap from</param>
    /// <param name="snappingDistance">Distance at which snap is valid</param>
    /// <returns>Actual position of the point if found otherwise it return the input vector unchanged</returns>
    public static Vector3 SnapToVertex(Collider2D parent, Vector3 input, float snappingDistance)
    {
        // Find polygon close to it
        Collider2D[] closeColliders = Physics2D.OverlapCircleAll(input, snappingDistance);
        for (int i = 0; i < closeColliders.Length; i++)
        {
            var collider = closeColliders[i];
            if (parent == collider)
                continue;

            var polygon = collider as PolygonCollider2D;

            if (polygon != null)
            {
                var vertices = polygon.GetWorldPoints();
                input = getVertexWithinRadius(vertices, input, snappingDistance);
            }

            var edge = collider as EdgeCollider2D;
            if (edge != null)
            {
                var vertices = edge.GetWorldPoints();
                Debug.Log(vertices.Length);
                input = getVertexWithinRadius(vertices, input, snappingDistance);
            }

            var rectangle = collider as BoxCollider2D;
            if (rectangle != null)
            {
                var vertices = rectangle.GetWorldCorners();
                input = getVertexWithinRadius(vertices, input, snappingDistance);
            }
        }
        return input;
    }

    private static Vector3 getVertexWithinRadius(Vector2[] vertices, Vector3 testVertex, float radius)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            float diff = ((Vector2)testVertex - vertices[i]).magnitude;
            if (diff <= radius)
            {
                return new Vector3(vertices[i].x, vertices[i].y, 0);
            }
        }

        return testVertex;
    }
}
