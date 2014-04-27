﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;


/// <summary>
/// Static class containing extension methods for the VectorPath2D object which are
/// only used within the custom editor. The functions in this class are implemented
/// seperatly from the VectorPath2D object to prevent usage within game code.
/// </summary>
public static class VectorPath2DEditorExtensions  
{
    /// <summary>
    /// Updates the center of the path so it's the mean position of all points
    /// </summary>
    public static void UpdateCenter(this VectorPath2D path)
    {
        Vector2 meanPosition = Vector2.zero;

        for (int i = 0; i < path.points.Length; i++)
        {
            meanPosition += path.points[i];
        }
        meanPosition = meanPosition / path.points.Length;

        Vector2[] updatedVertices = new Vector2[path.points.Length];
        for (int i = 0; i < path.points.Length; i++)
        {
            updatedVertices[i] = path.points[i] - meanPosition;
        }
        path.transform.position += new Vector3(meanPosition.x, meanPosition.y, 0);
        path.points = updatedVertices;
    }

    /// <summary>
    /// Inserts a new vertex into the path.
    /// </summary>
    /// <param name="position">The position in world cooridnates where the new vertex should be inserted</param>
    /// <param name="forceLast">Forces the function to insert the vertex at the end of the point array</param>
    /// <returns>Retruns true if insertion was successful</returns>
    public static bool InsertVertex(this VectorPath2D path, Vector2 position, bool forceLast = false)
    {
        Undo.RecordObject(path, string.Format("Insert vertex in {0}", path.name));

        var worldPos = new Vector2(path.transform.position.x, path.transform.position.y);
        position = position - worldPos;
        var vertices = path.points;

        // Handle easy case first.
        if (vertices == null || vertices.Length == 0)
        {
            vertices = new Vector2[1];
            vertices[0] = position;
            path.points = vertices;
            return true;
        }

        Vector2[] newVertices = new Vector2[vertices.Length + 1];

        int insertIdx = -1;
        if (forceLast == true || vertices.Length < 2)
        {
            insertIdx = vertices.Length;
        }
        else
        {
            // Find closest line segment.
            float minDist = float.PositiveInfinity;
            for (int i = 0; i < vertices.Length; i++)
            {
                int ii = (i + 1) % vertices.Length;
                float dist = GeometryFunctions.DistnacePointSegmentSquared(vertices[i], vertices[ii], position);
                if (dist < minDist)
                {
                    minDist = dist;
                    insertIdx = ii;
                }
            }
        }

        // Insert new vertex.
        if (insertIdx == -1 || insertIdx >= newVertices.Length)
            return false;

        System.Array.Copy(vertices, 0, newVertices, 0, insertIdx);
        System.Array.Copy(vertices, insertIdx, newVertices, insertIdx + 1, (vertices.Length - insertIdx));
        newVertices[insertIdx] = position;
        vertices = newVertices;

        path.points = vertices;

        return true;
    }

    /// <summary>
    /// Deletes the vertex with the specified id within the given path. 
    /// The id corresponds to the array position within the point array of the path.
    /// </summary>
    /// <param name="vertexId">ID of the vertex that should be deleted</param>
    /// <returns>Retruns true if path is still valid</returns>
    public static bool DeleteVertex(this VectorPath2D path, int vertexId)
    {
        Undo.RecordObjects(new Object[] { path, path.transform }, string.Format("Delete vertex in {0}", path.name));

        // Delete polygon if only one vertex left.
        if (path.points.Length <= 1)
        {
            return false;
        }

        var tmp = new Vector2[path.points.Length - 1];
        System.Array.Copy(path.points, 0, tmp, 0, vertexId);
        System.Array.Copy(path.points, vertexId + 1, tmp, vertexId, tmp.Length - vertexId);

        path.points = tmp;

        path.UpdateCenter();
        return true;
    }

    /// <summary>
    /// Tries to select a vertex at the given position. Takes 
    /// specified selection radius into account. Returns the
    /// index of the vertex within the given path. Returns
    /// -1 if no vertex could be selected.
    /// </summary>
    /// <param name="position">The position at which the vertex should be selected</param>
    /// <returns>Returns the index of the selected vertex within the path. (-1 if selection failed)</returns>
    public static int TrySelectVertex(this VectorPath2D path, Vector2 position)
    {
        var vertices = path.GetWorldPoints();
        for (int i = 0; i < vertices.Length; i++)
        {
            var vertex = vertices[i];
            if ((vertex - position).magnitude <= BrushSettingsWindow.VertexSize)
            {
                return i;
            }
        }
        return -1;
    }

    public static void UpdateVertexPosition(this VectorPath2D path, Vector3 newPosition, int selectedVertex)
    {
        Undo.RecordObject(path, string.Format("Move vertex {0} in {1}", selectedVertex, path.name));
        Vector2[] vertices = path.points;
        vertices[selectedVertex] = path.GetLocalPoint(newPosition);
        path.points = vertices;
    }
}
