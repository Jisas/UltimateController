using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[ExecuteInEditMode]
public class PointsGenerator : MonoBehaviour
{
    public MeshFilter meshFilter;
    public float pointSpacing = 0.5f;
    public LayerMask obstacleLayers;
    public List<Vector3> SurfacePoints = new();

    public bool IsGenerating { get; set; }
    public float Progress { get; set; }

    private int triangleIndex;
    private Vector3[] vertices;
    private int[] triangles;
    private int totalTriangles;
    private string pointsFilePath;

    private void Awake()
    {
        LoadSurfacePoints();
    }
    private void OnEnable()
    {
        if (!File.Exists(pointsFilePath))
        {
            pointsFilePath = Path.Combine(Application.persistentDataPath, $"{gameObject.name}_surface_points.json");
        }
    }

    public void GenerateSurfacePoints()
    {
        IsGenerating = true;
        SurfacePoints.Clear();
        triangleIndex = 0;
        Mesh mesh = meshFilter.sharedMesh;
        vertices = mesh.vertices;
        triangles = mesh.triangles;
        totalTriangles = triangles.Length / 3;
        Progress = 0f;

        EditorApplication.update += EditorUpdate;
    }
    public void CancelGeneration()
    {
        IsGenerating = false;
        EditorApplication.update -= EditorUpdate;
        SurfacePoints.Clear();
        Progress = 0.0f;
        Debug.Log("Generation cancelled and memory cleared.");
    }
    private void EditorUpdate()
    {
        if (!IsGenerating)
        {
#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
#endif
            return;
        }

        for (int i = 0; i < 100 && triangleIndex < totalTriangles; i++, triangleIndex++)
        {
            GenerateGridPointsOnTriangle(triangleIndex);
        }

        if (triangleIndex >= totalTriangles)
        {
            SaveSurfacePoints();
            IsGenerating = false;
            Progress = 1.0f;
#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
#endif
        }
        else
        {
            Progress = (float)triangleIndex / totalTriangles;
        }

        RepaintInspector();
    }
    private void GenerateGridPointsOnTriangle(int index)
    {
        int i = index * 3;
        Vector3 v0 = vertices[triangles[i]];
        Vector3 v1 = vertices[triangles[i + 1]];
        Vector3 v2 = vertices[triangles[i + 2]];

        Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;

        if (Mathf.Abs(normal.y) < 1.1f) // Ajustar el umbral según sea necesario
        {
            float area = Vector3.Cross(v1 - v0, v2 - v0).magnitude * 0.5f;
            int pointCount = Mathf.CeilToInt(area / (pointSpacing * pointSpacing));

            for (int uIndex = 0; uIndex < pointCount; uIndex++)
            {
                for (int vIndex = 0; vIndex < pointCount; vIndex++)
                {
                    float u = (float)uIndex / pointCount;
                    float v = (float)vIndex / pointCount;

                    if (u + v <= 1.0f)
                    {
                        Vector3 gridPoint = v0 * (1 - u - v) + v1 * u + v2 * v;
                        Vector3 worldPoint = transform.TransformPoint(gridPoint);

                        if (IsPointVisible(worldPoint))
                        {
                            SurfacePoints.Add(worldPoint);
                        }
                    }
                }
            }
        }
    }
    private bool IsPointVisible(Vector3 point)
    {
        // Verificar si el punto está dentro de otro mesh usando OverlapSphere
        Collider[] hitColliders = Physics.OverlapSphere(point, pointSpacing / 2, obstacleLayers);
        return hitColliders.Length == 0;
    }
    private void SaveSurfacePoints()
    {
        if (string.IsNullOrEmpty(pointsFilePath))
        {
            Debug.LogError("Points file path is not set.");
            return;
        }

        string json = JsonUtility.ToJson(new SurfacePointsData { points = SurfacePoints }, true);
        File.WriteAllText(pointsFilePath, json);
        Debug.Log("Surface points saved to " + pointsFilePath);
    }
    private void LoadSurfacePoints()
    {
        if (!string.IsNullOrEmpty(pointsFilePath) && File.Exists(pointsFilePath))
        {
            string json = File.ReadAllText(pointsFilePath);
            SurfacePoints = JsonUtility.FromJson<SurfacePointsData>(json).points;
            Debug.Log("Surface points loaded from " + pointsFilePath);
        }
        else
        {
            Debug.LogWarning("Points file not found: " + pointsFilePath);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (SurfacePoints == null) return;

        Gizmos.color = Color.red;
        foreach (Vector3 point in SurfacePoints)
        {
            Gizmos.DrawSphere(point, 0.05f);
        }
    }
    private void RepaintInspector()
    {
        EditorUtility.SetDirty(this);
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }
#endif

    [System.Serializable]
    private class SurfacePointsData
    {
        public List<Vector3> points;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PointsGenerator))]
public class SurfacePointsGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(10);

        PointsGenerator generator = (PointsGenerator)target;
        if (!generator.IsGenerating)
        {
            if (GUILayout.Button("Generate Surface Points"))
            {
                generator.GenerateSurfacePoints();
            }
        }

        if (generator.IsGenerating)
        {
            GUILayout.Space(10);
            Rect rect = EditorGUILayout.GetControlRect(false, 20);
            EditorGUI.ProgressBar(rect, generator.Progress, "Generating points, please wait...");
            GUILayout.Space(10);

            if (GUILayout.Button("Cancel Generation"))
            {
                generator.CancelGeneration();
            }
        }
    }
}
#endif
