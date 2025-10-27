using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.HoleMarket3D
{
    public class GroundMesh : MonoBehaviour
    {
        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        private Mesh mesh;
        private MeshCollider meshCollider;
        private Mesh colliderMesh;

        private void Awake()
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshCollider = gameObject.AddComponent<MeshCollider>();

            mesh = new Mesh();
            colliderMesh = new Mesh();

            meshFilter.sharedMesh = mesh;
            meshCollider.sharedMesh = colliderMesh;

            gameObject.layer = 6;
        }

        public void Init(bool dynamic, Material material)
        {
            if (dynamic)
            {
                mesh.MarkDynamic();
                colliderMesh.MarkDynamic();
            }

            meshRenderer.sharedMaterial = material;
        }

        public void Set(List<Vector3> vertices, List<int> triangles)
        {
            mesh.Clear();

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);

            mesh.MarkModified();

            List<Vector3> points = new List<Vector3>();

            points.AddRange(vertices);

            for (int i = 0; i < vertices.Count; i++)
            {
                points.Add(vertices[i] + Vector3.down * 10);
            }

            List<int > indices = new List<int>();

            indices.AddRange(triangles);

            for(int i = 0; i < triangles.Count; i++)
            {
                indices.Add(triangles[i] + vertices.Count);
            }

            for (int i = 0; i < vertices.Count; i++)
            {
                int ii = i + vertices.Count;
                int j = i == 0 ? vertices.Count - 1 : i - 1;
                int jj = j + vertices.Count;

                indices.Add(j);
                indices.Add(jj);
                indices.Add(ii);

                indices.Add(j);
                indices.Add(ii);
                indices.Add(i);
            }

            colliderMesh.Clear();
            colliderMesh.SetVertices(points);
            colliderMesh.SetTriangles(indices, 0);
            colliderMesh.MarkModified();

            meshCollider.convex = true;
            meshCollider.convex = false;
        }
    }
}