﻿using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(VectorPath2D))]
public class BrushVectorPath2D : Editor 
{
    private static Vector2 _selectedVertexStartPosition;    // Necessary for axis aligned movement.
    private static bool _isDrawing;
    private static int _selectedVertex = -1;

    public bool isManipulatingPath { get; private set; }

    [MenuItem("GameObject/Create 2D Objects/2D Path &l")]
    public static void CreatePath()
    {
        var go = new GameObject("Path2D");
        var path = go.AddComponent<VectorPath2D>();
        path.points = null;

        activateDrawing(go);
    }

    [DrawGizmo(GizmoType.SelectedOrChild | GizmoType.NotSelected | GizmoType.Pickable)]
    static void DrawPath2D(VectorPath2D path, GizmoType gizmoType)
    {
        BrushSettingsWindow.Initialize();
        var lineColor = ColorPalletReader.GetLineColor(ColorStates.SELECTED);
        if (gizmoType == GizmoType.NotSelected)
        {
            lineColor = ColorPalletReader.GetLineColor(ColorStates.PATH);
        }

        drawPath(path, lineColor);
    }

    private static void drawPath(VectorPath2D path, Color color)
    {
        Vector2[] vertices = path.GetWorldPoints();
        if (vertices == null)
            return;

        for (int i = 0; i < vertices.Length; i++)
        {
            int ii = (i + 1) % vertices.Length;
            // Draw edge
            if (ii != 0)
            {
                Gizmos.color = color;
                Gizmos.DrawLine(vertices[i], vertices[ii]);
            }

            // Draw vertex
            if (Selection.activeGameObject == path.gameObject)
            {
                Gizmos.color = ColorPalletReader.GetVertexColor(ColorStates.PATH);
                if (i == _selectedVertex)
                    Gizmos.color = ColorPalletReader.GetVertexColor(ColorStates.SELECTED);
                Gizmos.DrawSphere(vertices[i], BrushSettingsWindow.VertexSize);
                if (BrushSettingsWindow.ShowVertexInfo == true)
                    Handles.Label(vertices[i] + new Vector2(0.1f, 0.4f), path.GetLocalPoint(vertices[i]).ToString());
            }
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // Update center button.
        if (GUILayout.Button("Update Center") == true)
        {
            var path = target as VectorPath2D;
            if (path != null)
                path.UpdateCenter();
        }
    }

    void OnSceneGUI()
    {
        var prefabType = PrefabUtility.GetPrefabType(target);
        if (prefabType == PrefabType.Prefab || prefabType == PrefabType.ModelPrefab)
            return;

        var path = target as VectorPath2D;

        if (path == null)
            return;
        int controlId = GUIUtility.GetControlID(FocusType.Passive);

        if (_isDrawing == true)
        {
            isManipulatingPath = true;
            if (Event.current.type == EventType.MouseUp)
            {
                if (Event.current.button == 0)  // Left mouse button adds new vertex to the polygon.
                {
                    Utilities.PerformActionAtMouseWorldPosition((p) =>
                    {
                        if (BrushSettingsWindow.SnapToGrid == true)
                        {
                            p = Utilities.SnapToGrid(p, BrushSettingsWindow.GridSize);
                        }
                        path.InsertVertex(p, true);
                        EditorUtility.SetDirty(target);
                    });
                }
                else if (Event.current.button == 1)  // Right mouse button stops drawing and closes the polygon.
                {
                    _isDrawing = false;
                    isManipulatingPath = false;
                    path.UpdateCenter();
                }
            }
        }

        if (Selection.activeGameObject == path.gameObject)
        {
            if (Event.current.type == EventType.MouseDown && Event.current.clickCount > 1)
            {
                Utilities.PerformActionAtMouseWorldPosition((p) =>
                {
                    path.InsertVertex(p);
                    EditorUtility.SetDirty(target);
                });
            }
            if (Event.current.type == EventType.MouseMove && _isDrawing == false && Event.current.type != EventType.MouseDrag)
            {
                // Select vertex
                Utilities.PerformActionAtMouseWorldPosition(p => _selectedVertex = path.TrySelectVertex(p));
                if (_selectedVertex >= 0)
                {
                    _selectedVertexStartPosition = path.GetWorldPoint(path.points[_selectedVertex]);
                    HandleUtility.Repaint();
                }
            }
        }


        if (_selectedVertex != -1 && Event.current.button == 1)
        {
            var deleteOption = new GUIContent();
            deleteOption.text = "Delete Vertex";
            EditorUtility.DisplayCustomMenu(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0), new GUIContent[] { deleteOption }, -1, (userData, options, selected) =>
            {
                switch (selected)
                {
                    case 0:
                        if (path.DeleteVertex(_selectedVertex) == false)
                            Undo.DestroyObjectImmediate(path.gameObject);
                        break;
                    default:
                        break;
                }
            }, null);
        }

        // Manipulate polygon vertices
        if (Event.current.type == EventType.MouseDrag && _selectedVertex != -1 && Event.current.button == 0)
        {
            // Update position of the selected vertex
            Utilities.PerformActionAtMouseWorldPosition((p) =>
            {
                if (BrushSettingsWindow.SnapToGrid == true)
                {
                    p = Utilities.SnapToGrid(p, BrushSettingsWindow.GridSize);
                }
                else if (Event.current.modifiers == EventModifiers.Shift)    // Align with axis when holding shift.
                {
                    p = Utilities.AlignWithAxis(p, _selectedVertexStartPosition);
                }
                else if (Event.current.modifiers == EventModifiers.Control)
                {
                    float xMove = EditorPrefs.GetFloat("MoveSnapX");
                    float yMove = EditorPrefs.GetFloat("MoveSnapY");
                    p = Utilities.SnapToValues(p, _selectedVertexStartPosition, xMove, yMove);
                }
                path.UpdateVertexPosition(p, _selectedVertex);
                HandleUtility.Repaint();
            });
        }

        if (Event.current.type == EventType.MouseUp && _selectedVertex != -1)
        {
            _selectedVertex = -1;
        }

        if (_selectedVertex != -1)
        {
            DefaultHandles.Hidden = true;
        }
        else
        {
            DefaultHandles.Hidden = false;
        }

        if ((Event.current.type == EventType.Layout && _isDrawing == true) || _selectedVertex != -1)
        {
            HandleUtility.AddDefaultControl(controlId);
        }
    }

    private static void activateDrawing(GameObject go)
    {
        Selection.activeGameObject = go;
        _isDrawing = true;
    }
}
