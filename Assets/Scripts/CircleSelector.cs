using System;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
public class CircleMesh : MonoBehaviour
{
    private float _circleRadius = 5f;

    private Mesh _mesh;
    
    [SerializeField]
    private int rayCount = 360;
    
    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;

    public delegate void MouseDownAction(Vector3 hitPoint);
    private MouseDownAction _onMouseDownAction;

    private static float maxX;
    private static float maxZ;
    
    #region Setters

    public void SetOnMouseDownAction(MouseDownAction action)
    {
        _onMouseDownAction = action;
    }
    
    public void SetCircleRadius(float radius)
    {
        _circleRadius = radius;
    }
    
    public static void SetMaxValues(float x, float z)
    {
        maxX = x;
        maxZ = z;
    }
    #endregion

    private void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
        
        _mesh = new Mesh();
        
        _meshFilter.mesh = _mesh;
        _meshCollider.sharedMesh = _mesh;

        HideMesh();
    }

    public void HideMesh()
    {
        gameObject.SetActive(false);
    }

    private void OnMouseUp()
    {
        if (Camera.main == null) return;
        
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var layerMask = LayerMask.GetMask("Ground");
        
        if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, layerMask)) return;
        
        _onMouseDownAction?.Invoke(hit.point);
    }

    public void DrawMesh()
    {
        float angle = 0f;
        float angleIncrease = 360 / rayCount;
        var origin = Vector3.zero;

        Vector3[] vertices = new Vector3[rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = origin;

        int vertexIndex = 1;
        int triangleIndex = 0;

        for (int i = 0; i <= rayCount; i++)
        {
            
            Vector3 localVertex = origin + GetVectorFromAngle(angle) * _circleRadius;
            Vector3 worldVertex = transform.TransformPoint(localVertex); // Convert to world coordinates
            worldVertex = AdjustVertexToMapLimits(worldVertex); // Adjust vertex in world coordinates
            localVertex = transform.InverseTransformPoint(worldVertex); // Convert back to local coordinates
            
            vertices[vertexIndex] = localVertex;

            if (i > 0)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }
            vertexIndex++;

            angle -= angleIncrease;
        }

        _mesh.vertices = vertices;
        _mesh.uv = uv;
        _mesh.triangles = triangles;
        
        gameObject.SetActive(true);
    }

    private Vector3 GetVectorFromAngle(float angle)
    {
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }
    
    private Vector3 AdjustVertexToMapLimits(Vector3 vertex)
    {
        vertex.x = Mathf.Clamp(vertex.x, -maxX, maxX);
        vertex.z = Mathf.Clamp(vertex.z, -maxZ, maxZ);
        return vertex;
    }
}
