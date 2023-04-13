using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
	public SquareGrid squareGrid;
	public MeshFilter walls;
	public MeshFilter caveM;
	public MeshCollider wallCollider;

	List<Vector3> vertices;
	List<int> triangles;

	Dictionary<int, List<MeshTriangle>> triangleDictionary = new Dictionary<int, List<MeshTriangle>>();
	List<List<int>> edges = new List<List<int>>();
	HashSet<int> checkedVertices = new HashSet<int>();

	public float wallHeight = 10;

    public void GenerateMesh(int[,,] cave, float squareSize)
	{
		edges.Clear();
		checkedVertices.Clear();
		triangleDictionary.Clear();

		squareGrid = new SquareGrid(cave, squareSize);

		vertices = new List<Vector3>();
		triangles = new List<int>();

		for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
		{
			for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
			{
				for (int z = 0; z < squareGrid.squares.GetLength(2); z++)
				{
					TriangulateSquare(squareGrid.squares[x, y, z]);
				}
			}
		}

		Mesh mesh = new Mesh();
		caveM.mesh = mesh;

		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals();

		Vector2[] uvs = new Vector2[vertices.Count];

		int tileAmount = 20;
		for (int i = 0; i < vertices.Count; i++)
		{
			float u = Mathf.InverseLerp(-cave.GetLength(0) / 2 * squareSize, cave.GetLength(0) / 2 * squareSize, vertices[i].x) * tileAmount;
			float v = Mathf.InverseLerp(-cave.GetLength(1) / 2 * squareSize, cave.GetLength(1) / 2 * squareSize, vertices[i].z) * tileAmount;

			float y = vertices[i].y;
			v += y * 0.1f;

			uvs[i] = new Vector2(u, v);
		}


		mesh.uv = uvs;

		CreateWallMesh();
	}

	private void CreateWallMesh()
    {
		CalculateMeshEdge();

		List<Vector3> wallVertices = new List<Vector3>();

		List<int> wallTriangles = new List<int>();

		Mesh wallMesh = new Mesh();

		foreach(List<int> edge in edges)
        {
			for (int i = 0; i < edge.Count - 1; i++)
			{
				int startIndex = wallVertices.Count;
				wallVertices.Add(vertices[edge[i]]);
				wallVertices.Add(vertices[edge[i + 1]]);
				wallVertices.Add(vertices[edge[i]] - Vector3.up * wallHeight);
				wallVertices.Add(vertices[edge[i + 1]] - Vector3.up * wallHeight);

				wallTriangles.Add(startIndex + 0);
				wallTriangles.Add(startIndex + 2);
				wallTriangles.Add(startIndex + 3);

				wallTriangles.Add(startIndex + 3);
				wallTriangles.Add(startIndex + 1);
				wallTriangles.Add(startIndex + 0);
			}
		}

		wallMesh.vertices = wallVertices.ToArray();
		wallMesh.triangles = wallTriangles.ToArray();
		walls.mesh = wallMesh;

		wallCollider.sharedMesh = walls.mesh;
    }

	void TriangulateSquare(SquareConfiguration square)
	{
		switch (square.configuration)
		{
			case 0:
				break;

			// 1 point in square
			case 1:
				MeshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft);
				break;
			case 2:
				MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
				break;
			case 4:
				MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
				break;
			case 8:
				MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
				break;

			// 2 points in square
			case 3:
				MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
				break;
			case 6:
				MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
				break;
			case 9:
				MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
				break;
			case 12:
				MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
				break;
			case 5:
				MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
				break;
			case 10:
				MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
				break;

			// 3 points in square
			case 7:
				MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
				break;
			case 11:
				MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
				break;
			case 13:
				MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
				break;
			case 14:
				MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
				break;

			// All points in square
			case 15:
				MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
				checkedVertices.Add(square.topLeft.vertexIndex);
				checkedVertices.Add(square.topRight.vertexIndex);
				checkedVertices.Add(square.bottomRight.vertexIndex);
				checkedVertices.Add(square.bottomLeft.vertexIndex);
				break;
		}

	}

	void MeshFromPoints(params CentreNode[] points)
	{
		AssignVertices(points);

		if (points.Length >= 3)
        {
			CreateTriangle(points[0], points[1], points[2]);
		}
			
		if (points.Length >= 4)
        {
			CreateTriangle(points[0], points[2], points[3]);
		}
			
		if (points.Length >= 5)
        {
			CreateTriangle(points[0], points[3], points[4]);
		}
			
		if (points.Length >= 6)
        {
			CreateTriangle(points[0], points[4], points[5]);
		}
	}

	void AssignVertices(CentreNode[] points)
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

	void CreateTriangle(CentreNode a, CentreNode b, CentreNode c)
	{
		triangles.Add(a.vertexIndex);
		triangles.Add(b.vertexIndex);
		triangles.Add(c.vertexIndex);

		MeshTriangle triangle = new MeshTriangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
		AddTriangleToDictionary(triangle.vertextIndexA, triangle);
		AddTriangleToDictionary(triangle.vertextIndexB, triangle);
		AddTriangleToDictionary(triangle.vertextIndexC, triangle);
	}

	private void AddTriangleToDictionary(int vertexIndexKey, MeshTriangle triangle)
    {
		if(triangleDictionary.ContainsKey(vertexIndexKey))
        {
			triangleDictionary[vertexIndexKey].Add(triangle);
        }

		else
        {
			List<MeshTriangle> triangleList = new List<MeshTriangle>();
			triangleList.Add(triangle);
			triangleDictionary.Add(vertexIndexKey, triangleList);
        }
    }

	private void CalculateMeshEdge()
    {
		for(int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
        {
			if(!checkedVertices.Contains(vertexIndex))
            {
				int newEdgeVertex = GetConnectedVertexEdge(vertexIndex);

				if(newEdgeVertex != -1)
                {
					checkedVertices.Add(vertexIndex);

					List<int> newEdge = new List<int>();
					newEdge.Add(vertexIndex);
					edges.Add(newEdge);
					FollowEdge(newEdgeVertex, edges.Count - 1);
					edges[edges.Count - 1].Add(vertexIndex);
                }
            }
        }
    }

	private void FollowEdge(int vertexIndex, int edgeIndex)
    {
		edges[edgeIndex].Add(vertexIndex);
		checkedVertices.Add(vertexIndex);

		int nextVertexIndex = GetConnectedVertexEdge(vertexIndex);

		if(nextVertexIndex != -1)
        {
			FollowEdge(nextVertexIndex, edgeIndex);
        }
    }		

	private int GetConnectedVertexEdge(int vertexIndex)
    {
		List<MeshTriangle> trianglesContainingVertex = triangleDictionary[vertexIndex];

		for(int i = 0; i < trianglesContainingVertex.Count; i++)
        {
			MeshTriangle triangle = trianglesContainingVertex[i];

			for(int j = 0; j < 3; j++)
            {
				int vertexB = triangle[j];

				if(vertexB != vertexIndex && !checkedVertices.Contains(vertexB))
                {
					if (IsTriangleVertexEdge(vertexIndex, vertexB))
					{
						return vertexB;
					}
				}
            }
        }

		return -1;
    }

	private bool IsTriangleVertexEdge(int vertexA, int vertexB)
    {
		List<MeshTriangle> trianglesContainingVertexA = triangleDictionary[vertexA];
		int sharedTriangleCount = 0;

		for(int i = 0; i< trianglesContainingVertexA.Count; i++)
        {
			if(trianglesContainingVertexA[i].Contains(vertexB))
            {
				sharedTriangleCount++;
				if(sharedTriangleCount > 1)
                {
					break;
                }
            }
        }

		return sharedTriangleCount == 1;
    }
}

