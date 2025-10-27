using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Watermelon.HoleMarket3D
{
    public class GroundBehavior : MonoBehaviour
    {
        private GroundMesh topGround;
        private GroundMesh bottomGround;
        private GroundMesh leftGround;
        private GroundMesh rightGround;

        private GroundMesh topLeftGround;
        private GroundMesh topRightGround;
        private GroundMesh bottomLeftGround;
        private GroundMesh bottomRightGround;

        private int holeResolution;

        private Vector2 halfSize;

        private VoidMesh voidMesh;

        public void Awake()
        {
            topGround = new GameObject("Top Ground").AddComponent<GroundMesh>();
            bottomGround = new GameObject("Bottom Ground").AddComponent<GroundMesh>();
            leftGround = new GameObject("Left Ground").AddComponent<GroundMesh>();
            rightGround = new GameObject("Right Ground").AddComponent<GroundMesh>();

            topLeftGround = new GameObject("Top Left Ground").AddComponent<GroundMesh>();
            topRightGround = new GameObject("Top Right Ground").AddComponent<GroundMesh>();
            bottomLeftGround = new GameObject("Bottom Left Ground").AddComponent<GroundMesh>();
            bottomRightGround = new GameObject("Bottom Right Ground").AddComponent<GroundMesh>();

            voidMesh = new GameObject("Void").AddComponent<VoidMesh>();
        }


        public void Init(int holeResolution, Material groundMaterial, Material voidMaterial) 
        {
            this.holeResolution = holeResolution >= 4 ? holeResolution : 4;

            topGround.Init(false, groundMaterial);
            bottomGround.Init(false, groundMaterial);
            leftGround.Init(false, groundMaterial);
            rightGround.Init(false, groundMaterial);

            topLeftGround.Init(true, groundMaterial);
            topRightGround.Init(true, groundMaterial);
            bottomLeftGround.Init(true, groundMaterial);
            bottomRightGround.Init(true, groundMaterial);

            voidMesh.Init(holeResolution, voidMaterial);

            halfSize = Vector3.one * LevelBehavior.LevelBounds.MaxAbsCoord() * 1.5f;

            CalculateTop();
            CalculateBottom();
            CalculateRight();
            CalculateLeft();
        }

        private void Update()
        {
            CalculateTopLeft();
            CalculateTopRight();
            CalculateBottomRight();
            CalculateBottomLeft();
        }

        private void CalculateTopLeft()
        {
            // TL____________T____________TR
            //   |           |           |        
            //   |           |           |
            //   |   this    |           |
            //   |          _|_          |
            //   |         /B| \         |
            //  L|________/__|__\________|R
            //   |       A\  |O /        |
            //   |         \_|_/         |
            //   |           |           |
            //   |           |           |
            //   |           |           |
            // BL|___________|___________|BR
            //               B

            Vector3 center3D = PlayerBehavior.Position.SetY(0);
            Vector2 center = center3D.xz(); // Point O

            float radius = PlayerBehavior.Radius;

            List<Vector3> vertices = new List<Vector3> { 
                new Vector3(-halfSize.x, 0, center.y), // Point L
                new Vector3(-halfSize.x, 0, halfSize.y), // Point TL
                new Vector3(center.x, 0, halfSize.y), // Point T
            };

            vertices.Add(center3D + Vector3.left * radius); // Point A

            for (int i = 1; i < holeResolution; i++)
            {
                var angle = Mathf.Lerp(0, 90, (float)i / holeResolution);

                vertices.Add(center3D + Quaternion.Euler(0, angle, 0) * Vector3.left * radius);
            }

            vertices.Add(center3D + Vector3.forward * radius); // Point B

            List<int> triangles = new List<int>
            {
                0, 1, 3, // L-Tl-A
                1, 2, vertices.Count - 1, // Tl-T-B
            };

            for (int j = 3, i = 4; i < vertices.Count; j = i++)
            {
                triangles.Add(j);
                triangles.Add(1);
                triangles.Add(i);
            }

            topLeftGround.Set(vertices, triangles);
        }

        private void CalculateTopRight()
        {
            // TL____________T____________TR
            //   |           |           |        
            //   |           |           |
            //   |           |   this    |
            //   |          _|_          |
            //   |         / |A\         |
            //  L|________/__|__\________|R
            //   |        \  |O /B       |
            //   |         \_|_/         |
            //   |           |           |
            //   |           |           |
            //   |           |           |
            // BL|___________|___________|BR
            //               B

            Vector3 center3D = PlayerBehavior.Position.SetY(0);
            Vector2 center = center3D.xz(); // Point O

            float radius = PlayerBehavior.Radius;

            List<Vector3> vertices = new List<Vector3> {
                new Vector3(center.x, 0, halfSize.y), // Point T
                new Vector3(halfSize.x, 0, halfSize.y), // Point TR
                new Vector3(halfSize.x, 0, center.y), // Point R
            };

            vertices.Add(center3D + Vector3.forward * radius); // Point A

            for (int i = 1; i < holeResolution; i++)
            {
                var angle = Mathf.Lerp(0, 90, (float)i / holeResolution);

                vertices.Add(center3D + Quaternion.Euler(0, angle, 0) * Vector3.forward * radius);
            }

            vertices.Add(center3D + Vector3.right * radius); // Point B

            List<int> triangles = new List<int>
            {
                0, 1, 3, // L-Tl-A
                1, 2, vertices.Count - 1, // Tl-T-B
            };

            for (int j = 3, i = 4; i < vertices.Count; j = i++)
            {
                triangles.Add(j);
                triangles.Add(1);
                triangles.Add(i);
            }

            topRightGround.Set(vertices, triangles);
        }

        private void CalculateBottomRight()
        {
            // TL____________T____________TR
            //   |           |           |        
            //   |           |           |
            //   |           |           |
            //   |          _|_          |
            //   |         / | \         |
            //  L|________/__|__\________|R
            //   |        \  |O /A       |
            //   |         \_|_/         |
            //   |           |B          |
            //   |           |    this   |
            //   |           |           |
            // BL|___________|___________|BR
            //               B

            Vector3 center3D = PlayerBehavior.Position.SetY(0);
            Vector2 center = center3D.xz(); // Point O

            float radius = PlayerBehavior.Radius;

            List<Vector3> vertices = new List<Vector3> {
                new Vector3(halfSize.x, 0, center.y), // Point R
                new Vector3(halfSize.x, 0, -halfSize.y), // Point BR
                new Vector3(center.x, 0, -halfSize.y), // Point B
            };

            vertices.Add(center3D + Vector3.right * radius); // Point A

            for (int i = 1; i < holeResolution; i++)
            {
                var angle = Mathf.Lerp(0, 90, (float)i / holeResolution);

                vertices.Add(center3D + Quaternion.Euler(0, angle, 0) * Vector3.right * radius);
            }

            vertices.Add(center3D + Vector3.back * radius); // Point B

            List<int> triangles = new List<int>
            {
                0, 1, 3, // L-Tl-A
                1, 2, vertices.Count - 1, // Tl-T-B
            };

            for (int j = 3, i = 4; i < vertices.Count; j = i++)
            {
                triangles.Add(j);
                triangles.Add(1);
                triangles.Add(i);
            }

            bottomRightGround.Set(vertices, triangles);
        }

        private void CalculateBottomLeft()
        {
            // TL____________T____________TR
            //   |           |           |        
            //   |           |           |
            //   |           |           |
            //   |          _|_          |
            //   |         / | \         |
            //  L|________/__|__\________|R
            //   |        \  |O /A       |
            //   |         \_|_/         |
            //   |           |B          |
            //   |           |    this   |
            //   |           |           |
            // BL|___________|___________|BR
            //               B

            Vector3 center3D = PlayerBehavior.Position.SetY(0);
            Vector2 center = center3D.xz(); // Point O

            float radius = PlayerBehavior.Radius;

            List<Vector3> vertices = new List<Vector3> {
                new Vector3(center.x, 0, -halfSize.y), // Point B
                new Vector3(-halfSize.x, 0, -halfSize.y), // Point BL
                new Vector3(-halfSize.x, 0, center.y), // Point B
            };

            vertices.Add(center3D + Vector3.back * radius); // Point A

            for (int i = 1; i < holeResolution; i++)
            {
                var angle = Mathf.Lerp(0, 90, (float)i / holeResolution);

                vertices.Add(center3D + Quaternion.Euler(0, angle, 0) * Vector3.back * radius);
            }

            vertices.Add(center3D + Vector3.left * radius); // Point B

            List<int> triangles = new List<int>
            {
                0, 1, 3, // L-Tl-A
                1, 2, vertices.Count - 1, // Tl-T-B
            };

            for (int j = 3, i = 4; i < vertices.Count; j = i++)
            {
                triangles.Add(j);
                triangles.Add(1);
                triangles.Add(i);
            }

            bottomLeftGround.Set(vertices, triangles);
        }

        #region Bottom, Top, Left, Right

        private void CalculateTop()
        {

            //  ____T1      T2   
            //  L1  | this  |‾‾‾‾R1
            //      |_______|
            //      |A     B|    
            //      |       |
            //      |       |
            //      |_______|
            //      |D     C|   
            //  ____|       |____
            //  L2  B1      B2   R2

            List<Vector3> vertices = new List<Vector3> 
            { 
                new Vector3(-halfSize.x, 0, halfSize.y), // Point A
                new Vector3(-halfSize.x, 0, halfSize.y + 100), // Point T1
                new Vector3(halfSize.x, 0, halfSize.y + 100), // Point T2
                new Vector3(halfSize.x, 0, halfSize.y), // Point A
            };

            List<int> triangles = new List<int>
            {
                0, 1, 2, // A-T1-T2
                0, 2, 3, // A-T2-B
            };

            topGround.Set(vertices, triangles);
        }

        private void CalculateBottom()
        {

            //  ____T1      T2   
            //  L1  |       |‾‾‾‾R1
            //      |_______|
            //      |A     B|    
            //      |       |
            //      |       |
            //      |_______|
            //      |D     C|   
            //  ____|  this |____
            //  L2  B1      B2   R2

            List<Vector3> vertices = new List<Vector3>
            {
                new Vector3(-halfSize.x, 0, -halfSize.y - 100), // Point B1
                new Vector3(-halfSize.x, 0, -halfSize.y), // Point D
                new Vector3(halfSize.x, 0, -halfSize.y), // Point C
                new Vector3(halfSize.x, 0, -halfSize.y - 100), // Point B2
            };

            List<int> triangles = new List<int>
            {
                0, 1, 2, // B1-D-C
                0, 2, 3, // B1-C-B2
            };

            bottomGround.Set(vertices, triangles);
        }

        private void CalculateLeft()
        {

            //  ____T1      T2   
            //  L1  |       |‾‾‾‾R1
            //      |_______|
            //      |A     B|    
            //      |       |
            // this |       |
            //      |_______|
            //      |D     C|   
            //  ____|       |____
            //  L2  B1      B2   R2

            List<Vector3> vertices = new List<Vector3>
            {
                new Vector3(-halfSize.x - 100, 0, -halfSize.y - 100), // Point L2
                new Vector3(-halfSize.x - 100, 0, halfSize.y + 100), // Point L1
                new Vector3(-halfSize.x, 0, halfSize.y + 100), // Point T1
                new Vector3(-halfSize.x, 0, -halfSize.y - 100), // Point B1
            };

            List<int> triangles = new List<int>
            {
                0, 1, 2, // L2-L1-T1
                0, 2, 3, // L2-T1-B1
            };

            leftGround.Set(vertices, triangles);
        }

        private void CalculateRight()
        {

            //  ____T1      T2   
            //  L1  |       |‾‾‾‾R1
            //      |_______|
            //      |A     B|    
            //      |       |
            //      |       | this
            //      |_______|
            //      |D     C|   
            //  ____|       |____
            //  L2  B1      B2   R2

            List<Vector3> vertices = new List<Vector3>
            {
                new Vector3(halfSize.x, 0, -halfSize.y - 100), // Point B2
                new Vector3(halfSize.x, 0, halfSize.y + 100), // Point T2
                new Vector3(halfSize.x + 100, 0, halfSize.y + 100), // Point R1
                new Vector3(halfSize.x + 100, 0, -halfSize.y - 100), // Point R2
            };

            List<int> triangles = new List<int>
            {
                0, 1, 2, // B2-T2-R1
                0, 2, 3, // B2-R1-R2
            };

            rightGround.Set(vertices, triangles);
        }

        #endregion

        private void OnDestroy()
        {
            if (topGround) Destroy(topGround.gameObject);
            if (bottomGround) Destroy(bottomGround.gameObject);
            if (leftGround) Destroy(leftGround.gameObject);
            if (rightGround) Destroy(rightGround.gameObject);

            if (topLeftGround) Destroy(topLeftGround.gameObject);
            if (topRightGround) Destroy(topRightGround.gameObject);
            if (bottomLeftGround) Destroy(bottomLeftGround.gameObject);
            if (bottomRightGround) Destroy(bottomRightGround.gameObject);

            if (voidMesh) Destroy(voidMesh.gameObject);
        }
    }
}