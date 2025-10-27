using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Watermelon;

namespace Watermelon.HoleMarket3D
{

    public class GroundGenerator : MonoBehaviour
    {
        private static GroundGenerator instance;
        public static bool stopVerticalMeshGen = false;

        public const float PLAYGROUND_WIDTH = 5.5f;
        public const float PLAYGROUND_HEIGHT = 9.5f;
        public const float GROUNDS_OFFSET = 6f;
        public const float HOLE_INITIAL_OFFSET_Z = 0.6f;
        public const float HOLE_DIAMETER = 1f;

        [Header("Settings")]
        public Vector2 backSeaSize = new Vector2(10, 30);
        public float seaEdgeSize = 1.5f;
        public float seaSmooth;
        public float seaSmoothScale;
        public float seaAnimationSpeed;

        [Header("References")]
        public MeshFilter firstGroundMeshFilter;
        public MeshCollider firstGroundMeshCollider;
        //public MeshRenderer mr;
        public MeshFilter secondGroundMeshFilter;
        public MeshCollider secondGroundMeshCollider;
        public MeshFilter connectingPathMeshFilter;
        public MeshCollider connectingPathMeshCollider;
        public MeshFilter backSeaMeshFilter;

        [Space(5)]
        public Mesh holeMesh;
        public Transform holeTransform;

        private List<Vector3> bottomLeftVerts = new List<Vector3>();
        private List<Vector3> bottomRightVerts = new List<Vector3>();
        private List<Vector3> topRightVerts = new List<Vector3>();
        private List<Vector3> topLeftVerts = new List<Vector3>();
        private Vector3[] initialVertsPos;

        private bool needRecalculateNormals = true;
        private float holeHalf;
        private float seaRendererHeight;

        public static float PlaygroundCenter
        {
            get { return PLAYGROUND_WIDTH * 0.5f; }
        }

        private void Awake()
        {
            instance = this;
            holeHalf = HOLE_DIAMETER * 0.5f;
            instance.InitGroundMesh();
            instance.IntitBackSea();
        }

        #region Initialization

        public static void InitPlayground()
        {
            instance.holeTransform.position = new Vector3(PLAYGROUND_WIDTH * 0.5f, 0f, HOLE_INITIAL_OFFSET_Z);

            instance.GeneratePlayground();
        }

        private void InitGroundMesh()
        {
            needRecalculateNormals = true;

            // preparing info for creating a horizontal plane around hole
            int groundsVertsCound = 16 + holeMesh.vertices.Length;

            List<Vector3> vertsSorted = new List<Vector3>();
            vertsSorted.AddRange(holeMesh.vertices);

            vertsSorted.OrderBy(v => v.x).ThenBy(v => v.z);


            for (int i = 0; i < holeMesh.vertexCount; i++)
            {
                Vector3 vector = holeMesh.vertices[i];

                if (vector.y > -0.01f)
                {
                    if (vector.x <= 0.01f && vector.z <= 0.01f)
                    {
                        if (!bottomLeftVerts.Contains(vector))
                        {
                            bottomLeftVerts.Add(vector);
                        }
                    }

                    if (vector.x >= -0.01f && vector.z <= 0.01f)
                    {
                        if (!bottomRightVerts.Contains(vector))
                        {
                            bottomRightVerts.Add(vector);
                        }
                    }

                    if (vector.x >= -0.01f && vector.z >= -0.01f)
                    {
                        if (!topRightVerts.Contains(vector))
                        {
                            topRightVerts.Add(vector);
                        }
                    }

                    if (vector.x <= 0.01f && vector.z >= -0.01f)
                    {
                        if (!topLeftVerts.Contains(vector))
                        {
                            topLeftVerts.Add(vector);
                        }
                    }
                }
            }


            bottomLeftVerts.Sort(delegate (Vector3 v1, Vector3 v2)
            {
                return v1.x.CompareTo(v2.x);
            });

            bottomRightVerts.Sort(delegate (Vector3 v1, Vector3 v2)
            {
                return v1.x.CompareTo(v2.x);
            });

            topRightVerts.Sort(delegate (Vector3 v1, Vector3 v2)
            {
                return v2.x.CompareTo(v1.x);
            });

            topLeftVerts.Sort(delegate (Vector3 v1, Vector3 v2)
            {
                return v2.x.CompareTo(v1.x);
            });

            // initializing ground mesh
            Mesh mesh = new Mesh();

            mesh.vertices = new Vector3[16 + holeMesh.vertexCount + bottomLeftVerts.Count + bottomRightVerts.Count + topRightVerts.Count + topLeftVerts.Count];
            mesh.triangles = new int[(16 + bottomLeftVerts.Count - 1 + bottomRightVerts.Count - 1 + topRightVerts.Count - 1 + topLeftVerts.Count - 1) * 3 + holeMesh.triangles.Length];

            firstGroundMeshFilter.mesh = mesh;

            mesh = new Mesh();

            mesh.vertices = new Vector3[16 + holeMesh.vertexCount + bottomLeftVerts.Count + bottomRightVerts.Count + topRightVerts.Count + topLeftVerts.Count];
            mesh.triangles = new int[(16 + bottomLeftVerts.Count - 1 + bottomRightVerts.Count - 1 + topRightVerts.Count - 1 + topLeftVerts.Count - 1) * 3 + holeMesh.triangles.Length];

            secondGroundMeshFilter.mesh = mesh;
        }

        private void GeneratePlayground()
        {
            needRecalculateNormals = true;
            GenerateFirstPlayground(false);

            needRecalculateNormals = true;
            GenerateConnectingPath();

            needRecalculateNormals = true;
            GenerateSecondPlayground(true);

            needRecalculateNormals = true;
        }

        #endregion

        #region Playground Generation

        public static void UpdateGround()
        {
            /*if (GameController.CurrentState == GameController.LevelState.First)
            {
                instance.GenerateFirstPlayground(false);
            }
            else if (GameController.CurrentState == GameController.LevelState.Second)
            {
                instance.GenerateSecondPlayground(false);
            }
            else
            {
                instance.GenerateFirstPlayground(false);
                instance.needRecalculateNormals = true;
                instance.GenerateSecondPlayground(false);
            }*/
        }

        public static void UpdateConnectingPath()
        {
            instance.GenerateConnectingPath();
        }

        public static void RecalculateNormals()
        {
            instance.needRecalculateNormals = true;
        }

        private void GenerateFirstPlayground(bool initialGeneration)
        {
            Mesh mesh = GeneratePlaygroundPart(firstGroundMeshFilter.mesh, 0f, initialGeneration);

            firstGroundMeshFilter.mesh = mesh;

            firstGroundMeshCollider.sharedMesh = firstGroundMeshFilter.mesh;
        }

        private void GenerateSecondPlayground(bool initialGeneration)
        {
            Mesh mesh = GeneratePlaygroundPart(secondGroundMeshFilter.mesh, PLAYGROUND_HEIGHT + GROUNDS_OFFSET, initialGeneration);

            secondGroundMeshFilter.mesh = mesh;

            secondGroundMeshCollider.sharedMesh = secondGroundMeshFilter.mesh;
        }

        private Mesh GeneratePlaygroundPart(Mesh mesh, float zOffset, bool initialGeneration)
        {
            Vector3[] verts = mesh.vertices;
            int[] tris = mesh.triangles;

            // surrounding ground
            // 0-3
            verts[0] = new Vector3(0f, 0f, zOffset);
            verts[1] = new Vector3(0f, 0f, Mathf.Clamp(holeTransform.position.z - holeHalf, zOffset, PLAYGROUND_HEIGHT + zOffset));
            verts[2] = new Vector3(0f, 0f, Mathf.Clamp(holeTransform.position.z + holeHalf, zOffset, PLAYGROUND_HEIGHT + zOffset));
            verts[3] = new Vector3(0f, 0f, PLAYGROUND_HEIGHT + zOffset);

            // 4-7
            verts[4] = new Vector3(holeTransform.position.x - holeHalf, 0f, zOffset);
            verts[5] = new Vector3(holeTransform.position.x - holeHalf, 0f, Mathf.Clamp(holeTransform.position.z - holeHalf, zOffset, PLAYGROUND_HEIGHT + zOffset));
            verts[6] = new Vector3(holeTransform.position.x - holeHalf, 0f, Mathf.Clamp(holeTransform.position.z + holeHalf, zOffset, PLAYGROUND_HEIGHT + zOffset));
            verts[7] = new Vector3(holeTransform.position.x - holeHalf, 0f, PLAYGROUND_HEIGHT + zOffset);

            // 8-11
            verts[8] = new Vector3(holeTransform.position.x + holeHalf, 0f, zOffset);
            verts[9] = new Vector3(holeTransform.position.x + holeHalf, 0f, Mathf.Clamp(holeTransform.position.z - holeHalf, zOffset, PLAYGROUND_HEIGHT + zOffset));
            verts[10] = new Vector3(holeTransform.position.x + holeHalf, 0f, Mathf.Clamp(holeTransform.position.z + holeHalf, zOffset, PLAYGROUND_HEIGHT + zOffset));
            verts[11] = new Vector3(holeTransform.position.x + holeHalf, 0f, PLAYGROUND_HEIGHT + zOffset);

            // 12-15
            verts[12] = new Vector3(PLAYGROUND_WIDTH, 0f, zOffset);
            verts[13] = new Vector3(PLAYGROUND_WIDTH, 0f, Mathf.Clamp(holeTransform.position.z - holeHalf, zOffset, PLAYGROUND_HEIGHT + zOffset));
            verts[14] = new Vector3(PLAYGROUND_WIDTH, 0f, Mathf.Clamp(holeTransform.position.z + holeHalf, zOffset, PLAYGROUND_HEIGHT + zOffset));
            verts[15] = new Vector3(PLAYGROUND_WIDTH, 0f, PLAYGROUND_HEIGHT + zOffset);

            // 0-5
            tris[0] = 0;
            tris[1] = 1;
            tris[2] = 4;

            tris[3] = 1;
            tris[4] = 5;
            tris[5] = 4;

            tris[6] = 1;
            tris[7] = 2;
            tris[8] = 5;

            tris[9] = 2;
            tris[10] = 6;
            tris[11] = 5;

            tris[12] = 2;
            tris[13] = 3;
            tris[14] = 6;

            tris[15] = 3;
            tris[16] = 7;
            tris[17] = 6;


            // 6-9
            tris[18] = 4;
            tris[19] = 5;
            tris[20] = 8;

            tris[21] = 5;
            tris[22] = 9;
            tris[23] = 8;

            tris[24] = 6;
            tris[25] = 7;
            tris[26] = 10;

            tris[27] = 7;
            tris[28] = 11;
            tris[29] = 10;


            // 10-15
            tris[30] = 8;
            tris[31] = 9;
            tris[32] = 12;

            tris[33] = 9;
            tris[34] = 13;
            tris[35] = 12;

            tris[36] = 9;
            tris[37] = 10;
            tris[38] = 13;

            tris[39] = 10;
            tris[40] = 14;
            tris[41] = 13;

            tris[42] = 10;
            tris[43] = 11;
            tris[44] = 14;

            tris[45] = 11;
            tris[46] = 15;
            tris[47] = 14;


            // hole vertical planes
            int nextVertexIndex = 16;
            int nextTriangleIndex = 48;

            if (!initialGeneration && !stopVerticalMeshGen)
            {
                for (int i = 0; i < holeMesh.vertexCount; i++)
                {
                    verts[nextVertexIndex + i] = holeMesh.vertices[i] + holeTransform.position;
                }

                int triangleIndex = 0;
                while (triangleIndex < holeMesh.triangles.Length)
                {
                    tris[nextTriangleIndex + triangleIndex] = holeMesh.triangles[triangleIndex] + nextVertexIndex;
                    tris[nextTriangleIndex + triangleIndex + 1] = holeMesh.triangles[triangleIndex + 1] + nextVertexIndex;
                    tris[nextTriangleIndex + triangleIndex + 2] = holeMesh.triangles[triangleIndex + 2] + nextVertexIndex;

                    triangleIndex += 3;
                }
            }
            else if (stopVerticalMeshGen)
            {
                for (int i = 0; i < holeMesh.vertexCount; i++)
                {
                    verts[nextVertexIndex + i] = Vector3.zero;
                }

                int triangleIndex = 0;
                while (triangleIndex < holeMesh.triangles.Length)
                {
                    tris[nextTriangleIndex + triangleIndex] = holeMesh.triangles[triangleIndex] + nextVertexIndex;
                    tris[nextTriangleIndex + triangleIndex + 1] = holeMesh.triangles[triangleIndex + 1] + nextVertexIndex;
                    tris[nextTriangleIndex + triangleIndex + 2] = holeMesh.triangles[triangleIndex + 2] + nextVertexIndex;

                    triangleIndex += 3;
                }
            }

            // horizontal plane around hole
            nextVertexIndex += holeMesh.vertexCount;
            nextTriangleIndex += holeMesh.triangles.Length;

            Vector3 vertex = bottomLeftVerts[0] + holeTransform.position;
            vertex = vertex.SetZ(Mathf.Clamp(vertex.z, zOffset, PLAYGROUND_HEIGHT + zOffset));
            verts[nextVertexIndex++] = vertex;

            for (int i = 0; i < bottomLeftVerts.Count - 1; i++)
            {
                vertex = bottomLeftVerts[i + 1] + holeTransform.position;
                vertex = vertex.SetZ(Mathf.Clamp(vertex.z, zOffset, PLAYGROUND_HEIGHT + zOffset));

                verts[nextVertexIndex++] = vertex;

                tris[nextTriangleIndex++] = nextVertexIndex - 2;
                tris[nextTriangleIndex++] = nextVertexIndex - 1;
                tris[nextTriangleIndex++] = 5;
            }

            vertex = bottomRightVerts[0] + holeTransform.position;
            vertex = vertex.SetZ(Mathf.Clamp(vertex.z, zOffset, PLAYGROUND_HEIGHT + zOffset));
            verts[nextVertexIndex++] = vertex;

            for (int i = 0; i < bottomRightVerts.Count - 1; i++)
            {
                vertex = bottomRightVerts[i + 1] + holeTransform.position;
                vertex = vertex.SetZ(Mathf.Clamp(vertex.z, zOffset, PLAYGROUND_HEIGHT + zOffset));
                verts[nextVertexIndex++] = vertex;

                tris[nextTriangleIndex++] = nextVertexIndex - 2;
                tris[nextTriangleIndex++] = nextVertexIndex - 1;
                tris[nextTriangleIndex++] = 9;
            }

            vertex = topRightVerts[0] + holeTransform.position;
            vertex = vertex.SetZ(Mathf.Clamp(vertex.z, zOffset, PLAYGROUND_HEIGHT + zOffset));
            verts[nextVertexIndex++] = vertex;

            for (int i = 0; i < topRightVerts.Count - 1; i++)
            {
                vertex = topRightVerts[i + 1] + holeTransform.position;
                vertex = vertex.SetZ(Mathf.Clamp(vertex.z, zOffset, PLAYGROUND_HEIGHT + zOffset));
                verts[nextVertexIndex++] = vertex;

                tris[nextTriangleIndex++] = nextVertexIndex - 2;
                tris[nextTriangleIndex++] = nextVertexIndex - 1;
                tris[nextTriangleIndex++] = 10;
            }

            vertex = topLeftVerts[0] + holeTransform.position;
            vertex = vertex.SetZ(Mathf.Clamp(vertex.z, zOffset, PLAYGROUND_HEIGHT + zOffset));
            verts[nextVertexIndex++] = vertex;

            for (int i = 0; i < topLeftVerts.Count - 1; i++)
            {
                vertex = topLeftVerts[i + 1] + holeTransform.position;
                vertex = vertex.SetZ(Mathf.Clamp(vertex.z, zOffset, PLAYGROUND_HEIGHT + zOffset));
                verts[nextVertexIndex++] = vertex;

                tris[nextTriangleIndex++] = nextVertexIndex - 2;
                tris[nextTriangleIndex++] = nextVertexIndex - 1;
                tris[nextTriangleIndex++] = 6;
            }

            mesh.vertices = verts;
            mesh.triangles = tris;

            if (needRecalculateNormals)
            {
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                needRecalculateNormals = false;
            }


            return mesh;
        }

        private void GenerateConnectingPath()
        {
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();

            verts.Add(new Vector3(PlaygroundCenter - holeHalf, 0f, PLAYGROUND_HEIGHT));
            verts.Add(new Vector3(PlaygroundCenter - holeHalf, 0f, Mathf.Clamp(holeTransform.position.z - holeHalf, PLAYGROUND_HEIGHT, PLAYGROUND_HEIGHT + GROUNDS_OFFSET)));
            verts.Add(new Vector3(PlaygroundCenter - holeHalf, 0f, Mathf.Clamp(holeTransform.position.z + holeHalf, PLAYGROUND_HEIGHT, PLAYGROUND_HEIGHT + GROUNDS_OFFSET)));
            verts.Add(new Vector3(PlaygroundCenter - holeHalf, 0f, PLAYGROUND_HEIGHT + GROUNDS_OFFSET));

            verts.Add(new Vector3(PlaygroundCenter + holeHalf, 0f, PLAYGROUND_HEIGHT));
            verts.Add(new Vector3(PlaygroundCenter + holeHalf, 0f, Mathf.Clamp(holeTransform.position.z - holeHalf, PLAYGROUND_HEIGHT, PLAYGROUND_HEIGHT + GROUNDS_OFFSET)));
            verts.Add(new Vector3(PlaygroundCenter + holeHalf, 0f, Mathf.Clamp(holeTransform.position.z + holeHalf, PLAYGROUND_HEIGHT, PLAYGROUND_HEIGHT + GROUNDS_OFFSET)));
            verts.Add(new Vector3(PlaygroundCenter + holeHalf, 0f, PLAYGROUND_HEIGHT + GROUNDS_OFFSET));

            tris.AddRange(new int[] { 0, 1, 4 });
            tris.AddRange(new int[] { 1, 5, 4 });
            tris.AddRange(new int[] { 2, 3, 6 });
            tris.AddRange(new int[] { 3, 7, 6 });

            Mesh mesh = new Mesh();
            mesh.vertices = verts.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.RecalculateNormals();

            connectingPathMeshFilter.mesh = mesh;
            connectingPathMeshCollider.sharedMesh = mesh;
        }

        private int GetGroundPointForAroundHoleTriange(int triangleIndex)
        {
            int allTrissCount = bottomLeftVerts.Count - 1;

            if (triangleIndex < allTrissCount)
            {
                return 5; // index of closest corner to BOTTOM LEFT points
            }

            allTrissCount += bottomRightVerts.Count - 1;

            if (triangleIndex < allTrissCount)
            {
                return 9; // index of clesest corner to BOTTOM RIGHT points
            }

            allTrissCount += topRightVerts.Count - 1;

            if (triangleIndex < allTrissCount)
            {
                return 10; // index of clesest corner to TOP RIGHT points
            }

            return 6; // index of clesest corner to TOP LEFT points
        }

        #endregion

        #region Back Sea Generation


        private void IntitBackSea()
        {
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            seaRendererHeight = backSeaMeshFilter.transform.position.y;

            float heightOffset = Mathf.Sqrt(seaEdgeSize * seaEdgeSize - seaEdgeSize * seaEdgeSize * 0.5f * 0.5f);

            int sign = 1;
            Vector3 currentStartPoint = new Vector3(0f, 0f, 0f);

            for (int i = 0; i < backSeaSize.y; i++)
            {
                for (int j = 0; j < backSeaSize.x; j++)
                {

                    if (i < backSeaSize.y - 1 && j < backSeaSize.x - 1)
                    {
                        if (sign > 0)
                        {
                            verts.Add(currentStartPoint.AddToX(j * seaEdgeSize));
                            verts.Add(currentStartPoint.AddToX(j * seaEdgeSize + seaEdgeSize * 0.5f).AddToZ(heightOffset));
                            verts.Add(currentStartPoint.AddToX(j * seaEdgeSize + seaEdgeSize));

                            tris.AddRange(new int[] { verts.Count - 3, verts.Count - 2, verts.Count - 1 });

                            verts.Add(currentStartPoint.AddToX(j * seaEdgeSize + seaEdgeSize));
                            verts.Add(currentStartPoint.AddToX(j * seaEdgeSize + seaEdgeSize * 0.5f).AddToZ(heightOffset));
                            verts.Add(currentStartPoint.AddToX(j * seaEdgeSize + seaEdgeSize * 0.5f + seaEdgeSize).AddToZ(heightOffset));

                            tris.AddRange(new int[] { verts.Count - 3, verts.Count - 2, verts.Count - 1 });
                        }
                        else
                        {
                            verts.Add(currentStartPoint.AddToX(j * seaEdgeSize));
                            verts.Add(currentStartPoint.AddToX(j * seaEdgeSize - seaEdgeSize * 0.5f).AddToZ(heightOffset));
                            verts.Add(currentStartPoint.AddToX(j * seaEdgeSize + seaEdgeSize * 0.5f).AddToZ(heightOffset));

                            tris.AddRange(new int[] { verts.Count - 3, verts.Count - 2, verts.Count - 1 });

                            verts.Add(currentStartPoint.AddToX(j * seaEdgeSize));
                            verts.Add(currentStartPoint.AddToX(j * seaEdgeSize + seaEdgeSize * 0.5f).AddToZ(heightOffset));
                            verts.Add(currentStartPoint.AddToX(j * seaEdgeSize + seaEdgeSize));

                            tris.AddRange(new int[] { verts.Count - 3, verts.Count - 2, verts.Count - 1 });
                        }
                    }
                }

                currentStartPoint = currentStartPoint.AddToZ(heightOffset).AddToX(seaEdgeSize * 0.5f * sign);
                sign *= -1;
            }

            Mesh mesh = new Mesh();

            initialVertsPos = verts.ToArray();
            mesh.vertices = initialVertsPos;
            mesh.triangles = tris.ToArray();

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            backSeaMeshFilter.mesh = mesh;

            StartCoroutine(BackSeaAnimation());
        }

        private IEnumerator BackSeaAnimation()
        {
            float offset = 0f;

            while (true)
            {
                offset += seaAnimationSpeed * Time.deltaTime;
                GenerateBackSea(offset);

                yield return null;
            }
        }

        private void GenerateBackSea(float offset)
        {
            Vector3[] verts = backSeaMeshFilter.mesh.vertices;

            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 vert = verts[i];
                vert = vert.SetY(Mathf.PerlinNoise(vert.x * seaSmooth + offset, vert.z * seaSmooth + offset) * seaSmoothScale);
                float horizontalOffset = Mathf.PerlinNoise(vert.x * seaSmooth - offset * 0.3f, vert.z * seaSmooth - offset * 0.3f) * 0.6f;
                vert = initialVertsPos[i].SetY(vert.y/*Mathf.Clamp( vert.y + seaRendererHeight, -10f, -0.41f)*/).AddToX(horizontalOffset).AddToZ(-horizontalOffset);

                verts[i] = vert;
            }

            backSeaMeshFilter.mesh.RecalculateNormals();
            backSeaMeshFilter.mesh.vertices = verts;
        }

        #endregion

        //[Button("Test")]
        //public void Test1()
        //{
        //    ObjExporter.MeshToFile(connectingPathMeshFilter, mr, "D:/Mesh.obj");
        //}

    }

}
