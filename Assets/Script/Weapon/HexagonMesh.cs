using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexagonMesh : MonoBehaviour
{
    void Start()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // 1. Tạo 7 đỉnh (1 tâm + 6 đỉnh ngoài)
        Vector3[] vertices = new Vector3[7];
        Vector2[] uv = new Vector2[7]; // <--- THÊM DÒNG NÀY

        vertices[0] = Vector3.zero; // Tâm
        uv[0] = new Vector2(0.5f, 0.5f); // UV tâm là chính giữa texture

        for (int i = 0; i < 6; i++)
        {
            float angle_deg = (60 * i) + 90f;
            float angle_rad = Mathf.Deg2Rad * angle_deg;

            float x = Mathf.Cos(angle_rad);
            float y = Mathf.Sin(angle_rad);

            vertices[i + 1] = new Vector3(x, y, 0);

            // Map tọa độ từ (-1, 1) về (0, 1) cho UV
            uv[i + 1] = new Vector2(x * 0.5f + 0.5f, y * 0.5f + 0.5f);
        }

        mesh.vertices = vertices;
        mesh.uv = uv; // <--- CỰC KỲ QUAN TRỌNG

        mesh.triangles = new int[] {
            0, 1, 2,
            0, 2, 3,
            0, 3, 4,
            0, 4, 5,
            0, 5, 6,
            0, 6, 1
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds(); // Giúp Unity tính toán vùng hiển thị của Mesh
    }
}