﻿using UnityEngine;
using System.Collections;
using System.Linq;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
[RequireComponent(typeof(Camera))]

public class Physics2DDebugRenderer : MonoBehaviour
{
    private static bool _hasEventRegistered;

    private const int _circleSegmentsCount = 20;

    private PolygonCollider2D[] _polygons;
    private CircleCollider2D[] _circles;
    private EdgeCollider2D[] _edges;
    private BoxCollider2D[] _boxes;
    private Joint2D[] _joints;
    private VectorPath2D[] _paths;
    private Vector3[, ,] _circleSegments;
    private Vector2[][] _vertices;
    private Vector2[][] _pathPoints;
    private Vector2[][] _edgePoints;
    private int[][] _indices;

    private ColorStates[] _polygonColours;
    private ColorStates[] _circleColours;
    private ColorStates[] _boxColours;

    private GameObject _lastSelected;
    private Material _debugMaterial;
    private Vector3 _prevPosition;
    private bool _isInitialized;

    public void Start()
    {
        init();
    }

    public void OnPostRender()
    {
        renderDebugData();
    }
    
#if UNITY_EDITOR
    void Update()
    {
        if (Application.isEditor == true)
        {
            if (_isInitialized == false)
                init();

            // Snap to grid if toggled.
            /*bool toogleGrid = EditorPrefs.GetBool("SnapToGrid");
            if (toogleGrid == true)
            {
                float gridSize = EditorPrefs.GetFloat("GridSize");
                if (Selection.transforms.Length > 0 && Selection.transforms[0].position != _prevPosition)
                {
                    for (int i = 0; i < Selection.transforms.Length; i++)
                    {
                        Transform trans = Selection.transforms[i];
                        Vector3 newPos = Vector3.zero;
                        newPos.x = gridSize * Mathf.Round(trans.position.x / gridSize);
                        newPos.y = gridSize * Mathf.Round(trans.position.y / gridSize);
                        newPos.z = gridSize * Mathf.Round(trans.position.z / gridSize);
                        trans.position = newPos;
                    }
                    _prevPosition = Selection.transforms[0].position;
                }
            }*/

            loadPolygons();
            loadCircles();
            loadRectangles();
            loadJoints();
        }
    }

    public void OnRenderObject()
    {
        if (Application.isEditor == true && Application.isPlaying == false)
        {
            if (_isInitialized == false)
                init();
            renderDebugData();

            if (_hasEventRegistered == false)
            {
                _hasEventRegistered = true;
                SceneView.onSceneGUIDelegate += onSceneView;
            }
        }
    }

    private void onSceneView(SceneView sceneView)
    {
        if (Event.current.type == EventType.mouseDown && Event.current.button == 0)
        {
            loadPolygons();
            loadCircles();
            loadRectangles();
            loadJoints();
            if (_polygons != null)
            {
                for (int i = 0; i < _polygons.Length; i++)
                {
                    Utilities.PerformActionAtMouseWorldPosition((p) =>
                    {
                        if (_polygons[i].OverlapPoint(p) == true)
                        {
                            Selection.activeGameObject = _polygons[i].gameObject;
                            _lastSelected = Selection.activeGameObject;

                            return;
                        }
                    });
                }
            }
            if (_circles != null)
            {
                for (int i = 0; i < _circles.Length; i++)
                {
                    Utilities.PerformActionAtMouseWorldPosition((p) =>
                    {
                        if (_circles[i].OverlapPoint(p) == true)
                        {
                            Selection.activeGameObject = _circles[i].gameObject;
                            _lastSelected = Selection.activeGameObject;
                            return;
                        }
                    });
                }
            }
            if (_boxes != null)
            {
                for (int i = 0; i < _boxes.Length; i++)
                {
                    Utilities.PerformActionAtMouseWorldPosition((p) =>
                    {
                        if (_boxes[i].OverlapPoint(p) == true)
                        {
                            Selection.activeGameObject = _boxes[i].gameObject;
                            _lastSelected = Selection.activeGameObject;
                            return;
                        }
                    });
                }
            }
        }

        if (_lastSelected != null && Selection.activeGameObject == null)
        {
            Selection.activeGameObject = _lastSelected;
            _lastSelected = null;
        }
    }
#endif

    private void init()
    {
        _debugMaterial = new Material("Shader \"Lines/Colored Blended\" {" +
               "SubShader { Pass { " +
               "    Blend SrcAlpha OneMinusSrcAlpha " +
               "    ZWrite Off Cull Off Fog { Mode Off } " +
               "    BindChannels {" +
               "      Bind \"vertex\", vertex Bind \"color\", color }" +
               "} } }");
        _debugMaterial.hideFlags = HideFlags.HideAndDontSave;
        _debugMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
        int selectedPallet = 0;
#if UNITY_EDITOR
        selectedPallet = EditorPrefs.GetInt("PolyTie_SelectedColorPallet");
#endif
        var colorPallets = Resources.LoadAll<ColorPallet>("ColorPallets");

        ColorPalletReader.Read(colorPallets[selectedPallet]);

        loadPolygons();
        loadCircles();
        loadRectangles();

        if (Application.isPlaying == true)
        {
            loadPaths();
            loadEdges();
        }

        loadJoints();

        _isInitialized = true;
    }

    private void loadPolygons()
    {
        // Triangulate all polygons
        _polygons = GameObject.FindObjectsOfType<PolygonCollider2D>();
        _vertices = new Vector2[_polygons.Length][];
        _indices = new int[_polygons.Length][];
        _polygonColours = new ColorStates[_polygons.Length];
        for (int i = 0; i < _vertices.Length; i++)
        {
            _vertices[i] = _polygons[i].GetWorldPoints().ToArray<Vector2>();
            var triangulator = new Triangulator(_vertices[i]);
            _indices[i] = triangulator.Triangulate();

            _polygonColours[i] = Utilities.DetermineColorState(_polygons[i]);
        }
    }

    private void loadCircles()
    {
        // Calcualte circle segments for each circle in the scene
        _circles = GameObject.FindObjectsOfType<CircleCollider2D>();
        _circleSegments = new Vector3[_circles.Length, _circleSegmentsCount, 3];
        _circleColours = new ColorStates[_circles.Length];
        float theta = 0.0f;
        float dTheta = (Mathf.PI * 2.0f) / _circleSegmentsCount;
        for (int i = 0; i < _circles.Length; i++)
        {
            var circle = _circles[i];
            for (int j = 0; j < _circleSegmentsCount; j++)
            {
                _circleSegments[i, j, 0] = Vector3.zero;
                _circleSegments[i, j, 1] = new Vector3(circle.radius * Mathf.Cos(theta), circle.radius * Mathf.Sin(theta), 0);
                theta += dTheta;
                _circleSegments[i, j, 2] = new Vector3(circle.radius * Mathf.Cos(theta), circle.radius * Mathf.Sin(theta), 0);
            }

            _circleColours[i] = Utilities.DetermineColorState(circle);
        }
    }

    private void loadRectangles()
    {
        _boxes = GameObject.FindObjectsOfType<BoxCollider2D>();
        _boxColours = new ColorStates[_boxes.Length];
        for (int i = 0; i < _boxes.Length; i++)
        {
            _boxColours[i] = Utilities.DetermineColorState(_boxes[i]);
        }
    }

    private void loadPaths()
    {
        _paths = GameObject.FindObjectsOfType<VectorPath2D>();
        _pathPoints = new Vector2[_paths.Length][];
        for (int i = 0; i < _paths.Length; i++)
        {
            _pathPoints[i] = _paths[i].GetWorldPoints();
        }
    }

    private void loadEdges()
    {
        _edges = GameObject.FindObjectsOfType<EdgeCollider2D>();
        _edgePoints = new Vector2[_edges.Length][];
        for (int i = 0; i < _edges.Length; i++)
        {
            _edgePoints[i] = _edges[i].GetWorldPoints();
        }
    }

    private void loadJoints()
    {
        _joints = GameObject.FindObjectsOfType<Joint2D>();
    }

    private void renderDebugData()
    {
        GL.PushMatrix();
        _debugMaterial.SetPass(0);
#if UNITY_EDITOR
        // Draw grid if necessary
        if (EditorPrefs.GetBool("PolyTie_ShowSnapGrid") == true)
        {
            float gridSize = EditorPrefs.GetFloat("PolyTie_GridSize");
            GL.Begin(GL.LINES);
            GL.Color(Color.grey);
            for (int i = -300; i < 300; i++)
            {
                GL.Vertex3(-300, i * gridSize, -10);
                GL.Vertex3(300, i * gridSize, -10);
            }
            for (int i = -300; i < 300; i++)
            {
                GL.Vertex3(i * gridSize, -300, -10);
                GL.Vertex3(i * gridSize, 300, -10);
            }
            GL.End();
        }
#endif

        // Draw 2D polygon debug information
        if (_vertices != null)
        {
            for (int i = 0; i < _vertices.Length; i++)
            {
                Vector2[] vertices = _vertices[i];
                int[] indices = _indices[i];

                GL.Begin(GL.TRIANGLES);

                var colorState = _polygonColours[i];
#if UNITY_EDITOR
                if (_polygons[i].gameObject == Selection.activeGameObject)
                    colorState = ColorStates.SELECTED;
#endif

                GL.Color(ColorPalletReader.GetFillColor(colorState));

                for (int j = 0; j < indices.Length; j++)
                {
                    var pos = vertices[indices[j]];

                    GL.Vertex3(pos.x, pos.y, 0);
                }
                GL.End();

                // Draw outline
                GL.Begin(GL.LINES);
                GL.Color(ColorPalletReader.GetLineColor(colorState));

                for (int j = 0; j < vertices.Length; j++)
                {
                    var pos1 = vertices[j];
                    var pos2 = vertices[(j + 1) % vertices.Length];
                    GL.Vertex3(pos1.x, pos1.y, 0);
                    GL.Vertex3(pos2.x, pos2.y, 0);
                }
                GL.End();
            }
        }

        // Draw 2D circle debug information
        if (_circles != null)
        {
            GL.Begin(GL.TRIANGLES);
            for (int i = 0; i < _circles.Length; i++)
            {
                var colorState = _circleColours[i];
#if UNITY_EDITOR
                if (_circles[i].gameObject == Selection.activeGameObject)
                    colorState = ColorStates.SELECTED;
#endif
                GL.Color(ColorPalletReader.GetFillColor(colorState));                

                for (int j = 0; j < _circleSegmentsCount; j++)
                {
                    GL.Vertex(_circles[i].GetWorldCenter() + _circleSegments[i, j, 0]);
                    GL.Vertex(_circles[i].GetWorldCenter() + _circleSegments[i, j, 1]);
                    GL.Vertex(_circles[i].GetWorldCenter() + _circleSegments[i, j, 2]);
                }
            }
            GL.End();
            GL.Begin(GL.LINES);
            for (int i = 0; i < _circles.Length; i++)
            {
                var colorState = _circleColours[i];
#if UNITY_EDITOR
                if (_circles[i].gameObject == Selection.activeGameObject)
                    colorState = ColorStates.SELECTED;
#endif
                GL.Color(ColorPalletReader.GetLineColor(colorState));   

                for (int j = 0; j < _circleSegmentsCount; j++)
                {
                    GL.Vertex(_circles[i].GetWorldCenter() + _circleSegments[i, j, 1]);
                    GL.Vertex(_circles[i].GetWorldCenter() + _circleSegments[i, j, 2]);
                }
            }
            GL.End();
        }

        // Render rectangles
        if (_boxes != null)
        {
            GL.Begin(GL.TRIANGLES);
            for (int i = 0; i < _boxes.Length; i++)
            {
                var box = _boxes[i];
                var colorState = _boxColours[i];
#if UNITY_EDITOR
                if (box.gameObject == Selection.activeGameObject)
                    colorState = ColorStates.SELECTED;
#endif
                GL.Color(ColorPalletReader.GetFillColor(colorState));
                var corners = box.GetWorldCorners();
                GL.Vertex3(corners[0].x, corners[0].y, 0f);
                GL.Vertex3(corners[1].x, corners[1].y, 0f);
                GL.Vertex3(corners[3].x, corners[3].y, 0f);
                GL.Vertex3(corners[1].x, corners[1].y, 0f);
                GL.Vertex3(corners[2].x, corners[2].y, 0f);
                GL.Vertex3(corners[3].x, corners[3].y, 0f);
            }
            GL.End();
            GL.Begin(GL.LINES); 
            for (int i = 0; i < _boxes.Length; i++)
            {
                var box = _boxes[i];
                var colorState = _boxColours[i];
#if UNITY_EDITOR
                if (box.gameObject == Selection.activeGameObject)
                    colorState = ColorStates.SELECTED;
#endif
                GL.Color(ColorPalletReader.GetFillColor(colorState));
                var corners = box.GetWorldCorners();
                GL.Vertex3(corners[0].x, corners[0].y, 0f);
                GL.Vertex3(corners[1].x, corners[1].y, 0f);
                GL.Vertex3(corners[1].x, corners[1].y, 0f);
                GL.Vertex3(corners[2].x, corners[2].y, 0f);
                GL.Vertex3(corners[2].x, corners[2].y, 0f);
                GL.Vertex3(corners[3].x, corners[3].y, 0f);
                GL.Vertex3(corners[3].x, corners[3].y, 0f);
                GL.Vertex3(corners[0].x, corners[0].y, 0f);
            }
            GL.End();
        }

        // Render paths
        if (_pathPoints != null && Application.isPlaying == true)
        {
            GL.Begin(GL.LINES);
            GL.Color(ColorPalletReader.GetLineColor(ColorStates.PATH));
            for (int i = 0; i < _pathPoints.Length; i++)
            {
                for (int j = 0; j < _pathPoints[i].Length - 1; j++)
                {
                    Vector2 pos0 = _pathPoints[i][j];
                    Vector2 pos1 = _pathPoints[i][j + 1];
                    GL.Vertex3(pos0.x, pos0.y, 0);
                    GL.Vertex3(pos1.x, pos1.y, 0);
                }
                
            }
            GL.End();
        }

        // Render edges
        if (_edgePoints != null && Application.isPlaying == true)
        {
            GL.Begin(GL.LINES);
            GL.Color(ColorPalletReader.GetLineColor(ColorStates.PATH));
            for (int i = 0; i < _edgePoints.Length; i++)
            {
                for (int j = 0; j < _edgePoints[i].Length - 1; j++)
                {
                    Vector2 pos0 = _edgePoints[i][j];
                    Vector2 pos1 = _edgePoints[i][j + 1];
                    GL.Vertex3(pos0.x, pos0.y, 0);
                    GL.Vertex3(pos1.x, pos1.y, 0);
                }
            }
            GL.End();
        }

        // Render joints
        if (_joints != null)
        {
            GL.Begin(GL.LINES);
            for (int i = 0; i < _joints.Length; i++)
            {
                if (_joints[i].connectedBody == null)
                    continue;

                if (_joints[i].hingeJoint != null)
                {
                    GL.Vertex(_joints[i].connectedBody.transform.position + new Vector3(_joints[i].hingeJoint.connectedAnchor.x, _joints[i].hingeJoint.connectedAnchor.y, 0));
                    GL.Vertex(_joints[i].transform.position + new Vector3(_joints[i].hingeJoint.anchor.x, _joints[i].hingeJoint.anchor.y, 0));
                }
            }
            GL.End();
        }

        GL.PopMatrix();
    }
}
