using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.HoleMarket3D
{
    public class VoidMesh : MonoBehaviour
    {
        private static VoidMesh instance;

        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        private Mesh mesh;

        private List<Vector3> refPoints = new List<Vector3>();

        private int pointsCount;

        private void Awake()
        {
            instance = this;

            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshFilter = gameObject.AddComponent<MeshFilter>();
            
            mesh = new Mesh();
            mesh.MarkDynamic();

            meshFilter.sharedMesh = mesh;

            gameObject.layer = 10;

            PlayerBehavior.OnRadiusChanged += OnRadiusChanged;
        }

        public void Init(int holeResolution, Material material)
        {
            pointsCount = (holeResolution - 1) * 4 + 4;

            OnRadiusChanged(PlayerBehavior.Radius);

            meshRenderer.sharedMaterial = material;
        }

        private void OnRadiusChanged(float radius)
        {
            refPoints = new List<Vector3>(pointsCount);

            for (int i = 0; i < pointsCount; i++)
            {
                var angle = Mathf.Lerp(0, 360, (float)i / pointsCount);

                refPoints.Add(Quaternion.Euler(0, angle, 0) * Vector3.forward * radius);
            }

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            var down = Vector3.down * 10;

            vertices.Add(refPoints[0]);
            vertices.Add(refPoints[0] + down);

            for(int i = 1, j = 0; i < pointsCount; j = i++)
            {
                int ii = i * 2;
                int jj = j * 2;

                vertices.Add(refPoints[i]);
                vertices.Add(refPoints[i] + down);

                triangles.Add(jj + 1);
                triangles.Add(jj);
                triangles.Add(ii + 1);

                triangles.Add(ii + 1);
                triangles.Add(jj);  
                triangles.Add(ii);
            }

            var iLast = (pointsCount - 1) * 2;

            triangles.Add(iLast + 1);
            triangles.Add(iLast);
            triangles.Add(1);

            triangles.Add(1);
            triangles.Add(iLast);
            triangles.Add(0);

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);

            mesh.MarkModified();
        }

        public static void SetTexture(Texture texture)
        {
            instance.meshRenderer.sharedMaterial.SetTexture("_Gradient_Texture", texture);
        }

        private void Update()
        {
            transform.position = PlayerBehavior.Position;
        }

        private void OnDestroy()
        {
            PlayerBehavior.OnRadiusChanged -= OnRadiusChanged;
        }
    }
}