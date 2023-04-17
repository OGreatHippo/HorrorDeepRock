using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeMeshGenerator : MonoBehaviour
{
    public CubeGrid cubeGrid;

    List<Vector3> vertices;
    List<int> triangles;

    public void GenerateMesh(int[,,] map, float cubeSize)
    {
        cubeGrid = new CubeGrid(map, cubeSize);

        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int x = 0; x < cubeGrid.cubes.GetLength(0); x++)
        {
            for (int y = 0; y < cubeGrid.cubes.GetLength(1); y++)
            {
                for (int z = 0; z < cubeGrid.cubes.GetLength(2); z++)
                {
                    TriangulateCube(cubeGrid.cubes[x, y, z]);
                }
            }
        }

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();
    }

    void TriangulateCube(Cube cube)
    {
        switch (cube.configuration)
        {
            case 0:
                MeshFromPoints(/* front face */ cube.v2, cube.v1, cube.v0, cube.v2, cube.v0, cube.v3, /* back face */ cube.v2, cube.v1, cube.v0, cube.v2, cube.v0, cube.v3);
                break;

            // 1 point:
            case 1:
                MeshFromPoints(cube.e3, cube.e0, cube.v0);
                break;
            case 2:
                MeshFromPoints(cube.v1, cube.e0, cube.e1);
                break;
            case 4:
                MeshFromPoints(cube.v2, cube.e1, cube.e2);
                break;
            case 8:
                MeshFromPoints(cube.v0, cube.v1, cube.v3, cube.v0, cube.v3, cube.v4, cube.v2, cube.v3, cube.v7);
                break;

            // 2 points:
            case 3:
                MeshFromPoints(cube.v0, cube.v2, cube.v3, cube.v0, cube.v1, cube.v3, cube.v4, cube.v5, cube.v7);
                break;
            case 6:
                MeshFromPoints(cube.v0, cube.v1, cube.v2, cube.v0, cube.v2, cube.v4, cube.v2, cube.v3, cube.v7, cube.v1, cube.v3, cube.v5);
                break;
            case 9:
                MeshFromPoints(cube.v0, cube.v1, cube.v2, cube.v0, cube.v2, cube.v4, cube.v2, cube.v3, cube.v7, cube.v1, cube.v3, cube.v5);
                break;
            case 12:
                MeshFromPoints(cube.v0, cube.v1, cube.v2, cube.v0, cube.v2, cube.v4, cube.v2, cube.v3, cube.v7, cube.v1, cube.v3, cube.v5);
                break;
            case 5:
                MeshFromPoints(cube.v0, cube.v2, cube.v3, cube.v0, cube.v1, cube.v3, cube.v4, cube.v5, cube.v7);
                break;
            case 10:
                MeshFromPoints(cube.v0, cube.v1, cube.v2, cube.v2, cube.v3, cube.v7);
                break;

            // 3 points:
            case 7:
                MeshFromPoints(cube.v0, cube.v1, cube.v2, cube.v2, cube.v3, cube.v7);
                break;
            case 11:
                MeshFromPoints(cube.v0, cube.v1, cube.v3, cube.v0, cube.v3, cube.v4, cube.v2, cube.v3, cube.v7);
                break;
            case 13:
                MeshFromPoints(cube.v0, cube.v1, cube.v2, cube.v2, cube.v3, cube.v7);
                break;
            case 14:
                MeshFromPoints(cube.v0, cube.v2, cube.v3, cube.v0, cube.v1, cube.v3, cube.v4, cube.v5, cube.v6);
                break;

            // 4 points:
            case 15:
                MeshFromPoints(cube.v2, cube.v1, cube.v0, cube.v2, cube.v0, cube.v3);
                break;
        }
    }

    void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);

        if (points.Length >= 3)
        {
            CreateTrigangle(points[0], points[1], points[2]);
        }

        if (points.Length >= 4)
        {
            CreateTrigangle(points[0], points[2], points[3]);
        }

        if (points.Length >= 5)
        {
            CreateTrigangle(points[0], points[3], points[4]);
        }

        if (points.Length >= 6)
        {
            CreateTrigangle(points[0], points[4], points[5]);
        }
    }

    void AssignVertices(Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].vertexIndex == -1)
            {
                points[i].vertexIndex = vertices.Count;

                vertices.Add(points[i].position);
            }
        }
    }

    void CreateTrigangle(Node a, Node b, Node c)
    {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);
    }

    public class CubeGrid
    {
        public Cube[,,] cubes;

        public CubeGrid(int[,,] map, float cubeSize)
        {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            int nodeCountZ = map.GetLength(2);

            float mapWidth = nodeCountX * cubeSize;
            float mapHeight = nodeCountY * cubeSize;
            float mapLength = nodeCountZ * cubeSize;

            ControlNode[,,] controlNodes = new ControlNode[nodeCountX, nodeCountY, nodeCountZ];

            for (int x = 0; x < nodeCountX; x++)
            {
                for (int y = 0; y < nodeCountY; y++)
                {
                    for (int z = 0; z < nodeCountZ; z++)
                    {
                        Vector3 pos = new Vector3(-mapWidth / 2 + x * cubeSize + cubeSize / 2, -mapHeight / 2 + y * cubeSize + cubeSize / 2, -mapLength / 2 + z * cubeSize + cubeSize / 2);

                        controlNodes[x, y, z] = new ControlNode(pos, map[x, y, z] == 1, cubeSize);
                    }
                }
            }

            cubes = new Cube[nodeCountX - 1, nodeCountY - 1, nodeCountZ - 1];

            for (int x = 0; x < nodeCountX - 1; x++)
            {
                for (int y = 0; y < nodeCountY - 1; y++)
                {
                    for (int z = 0; z < nodeCountZ - 1; z++)
                    {
                        cubes[x, y, z] = new Cube(controlNodes[x, y + 1, z], controlNodes[x + 1, y + 1, z], controlNodes[x + 1, y + 1, z + 1], controlNodes[x, y + 1, z + 1], controlNodes[x, y, z + 1], controlNodes[x, y, z], controlNodes[x + 1, y, z], controlNodes[x + 1, y, z + 1]);
                    }
                }
            }
        }
    }

    public class Cube
    {
        public ControlNode v4, v5, v1, v0;
        public ControlNode v7, v6, v2, v3;
        public Node e2, e1, e0, e3;
        public Node e6, e5, e4, e7;

        public int configuration;

        public Cube(ControlNode _frontTopLeft, ControlNode _frontTopRight, ControlNode _frontBottomRight, ControlNode _frontBottomLeft, ControlNode _backTopLeft, ControlNode _backTopRight, ControlNode _backBottomRight, ControlNode _backBottomLeft)
        {
            v4 = _frontTopLeft;
            v5 = _frontTopRight;
            v1 = _frontBottomRight;
            v0 = _frontBottomLeft;

            v7 = _backTopLeft;
            v6 = _backTopRight;
            v2 = _backBottomRight;
            v3 = _backBottomLeft;

            e2 = v4.right;
            e1 = v1.above;
            e0 = v0.right;
            e3 = v0.above;

            e6 = v7.right;
            e5 = v2.above;
            e4 = v3.right;
            e7 = v3.above;

            if (v4.active)
            {
                configuration += 128;
            }

            if (v5.active)
            {
                configuration += 64;
            }

            if (v1.active)
            {
                configuration += 32;
            }

            if (v0.active)
            {
                configuration += 16;
            }

            if (v7.active)
            {
                configuration += 8;
            }

            if (v6.active)
            {
                configuration += 4;
            }

            if (v2.active)
            {
                configuration += 2;
            }

            if (v3.active)
            {
                configuration += 1;
            }
        }
    }

    public class Node
    {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 _pos)
        {
            position = _pos;
        }
    }

    public class ControlNode : Node
    {
        public bool active;
        public Node above, right, up;

        public ControlNode(Vector3 _pos, bool _active, float squareSize) : base(_pos)
        {
            active = _active;

            above = new Node(position + Vector3.forward * squareSize / 2f);
            right = new Node(position + Vector3.right * squareSize / 2f);
            up = new Node(position + Vector3.up * squareSize / 2f);
        }
    }
}
