using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MeshGenerator : MonoBehaviour
{
	public SquareGrid squareGrid;
	public MeshFilter walls;
	public MeshFilter floor;
	public MeshFilter roof;
	public MeshFilter caveM;
	public MeshCollider wallCollider;
	public MeshCollider floorCollider;
	public MeshCollider roofCollider;

	public NavMeshSurface navMesh;

	private List<Vector3> vertices;
	private List<int> triangles;

	private Dictionary<int, List<MeshTriangle>> triangleDictionary = new Dictionary<int, List<MeshTriangle>>();
	private List<List<int>> edges = new List<List<int>>();
	private HashSet<int> checkedVertices = new HashSet<int>();

	private float wallHeight = 10;

	public Material caveMat;
	private int tileAmount = 50;

	public void GenerateMesh(int[,] cave, float squareSize)
	{
		edges.Clear();
		checkedVertices.Clear();
		triangleDictionary.Clear();

		squareGrid = new SquareGrid(cave, squareSize);

		vertices = new List<Vector3>();
		triangles = new List<int>();

		for (int x = 0; x < squareGrid.GetAmountInGrid(0); x++)
		{
			for (int z = 0; z < squareGrid.GetAmountInGrid(1); z++)
			{
				TriangulateSquare(squareGrid.GetSquareAtCoords(x, z));
			}
		}

		Mesh mesh = new Mesh();
		caveM.mesh = mesh;

		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals();

		Vector2[] uvs = new Vector2[vertices.Count];

        for (int i = 0; i < vertices.Count; i++)
        {
            float percentX = Mathf.InverseLerp(-cave.GetLength(0) / 2 * squareSize, cave.GetLength(0) / 2 * squareSize, vertices[i].x) * tileAmount;
            float percentZ = Mathf.InverseLerp(-cave.GetLength(1) / 2 * squareSize, cave.GetLength(1) / 2 * squareSize, vertices[i].z) * tileAmount;

            uvs[i] = new Vector2(percentX, percentZ);
        }

        mesh.uv = uvs;

		CreateWallMesh(cave, 1);
		FloorMesh(cave, 1);
		CreateRoofMesh(cave, 1);
	}

	private void CreateWallMesh(int[,] cave, float squareSize)
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

        Vector2[] uvs = new Vector2[wallVertices.Count];

        float textureScale = caveMat.mainTextureScale.x;
        float increment = (textureScale / cave.GetLength(0));

        float[] uvEntries = new float[] { 0.5f, increment };

        for (int i = 0; i < wallVertices.Count; i++)
        {
            float percentY = Mathf.InverseLerp((-wallHeight) * squareSize, 0, wallMesh.vertices[i].y) * (wallHeight / cave.GetLength(0)) * tileAmount;
            uvs[i] = new Vector2(uvEntries[i % 2], percentY);
        }

        wallMesh.uv = uvs;

        wallMesh.RecalculateNormals();

		walls.mesh = wallMesh;

		wallCollider.sharedMesh = walls.mesh;

		walls.GetComponent<MeshRenderer>().material = caveMat;
	}

	private void FloorMesh(int[,] cave, float squareSize)
    {
		List<Vector3> floorVertices = new List<Vector3>();
		List<int> floorTriangles = new List<int>();
		Mesh floorMesh = new Mesh();

		floorVertices.Clear();
		floorTriangles.Clear();
		floorMesh.Clear();
		floor.mesh.Clear();

		Vector3 positionOffset = new Vector3(-(cave.GetLength(0) / 2f), 0, -(cave.GetLength(1) / 2f));

		for (int x = 0; x < cave.GetLength(0); x++)
        {
			for (int z = 0; z < cave.GetLength(1); z++)
			{
                if (x == cave.GetLength(0) - 1 || z == cave.GetLength(1) - 1)
                {
                    continue;
                }

                // Calculate the position of the vertices for this square
                Vector3 bottomLeft = new Vector3(x, 0, z + squareSize) + positionOffset;
                Vector3 bottomRight = new Vector3(x + squareSize, 0, z + squareSize) + positionOffset;
                Vector3 topLeft = new Vector3(x, 0, z) + positionOffset;
                Vector3 topRight = new Vector3(x + squareSize, 0, z) + positionOffset;

                // Add the vertices to the list
                int vertexIndex = floorVertices.Count;

                floorVertices.Add(bottomLeft);
                floorVertices.Add(topLeft);
                floorVertices.Add(bottomRight);
                floorVertices.Add(topRight);

                // Add the triangles to the list
                floorTriangles.Add(vertexIndex + 0);
                floorTriangles.Add(vertexIndex + 2);
                floorTriangles.Add(vertexIndex + 1);

                floorTriangles.Add(vertexIndex + 1);
                floorTriangles.Add(vertexIndex + 2);
                floorTriangles.Add(vertexIndex + 3);
            }
        }

		floorMesh.vertices = floorVertices.ToArray();
		floorMesh.triangles = floorTriangles.ToArray();

        Vector2[] uvs = new Vector2[floorVertices.Count];

		int tileAmount = 50;
		for (int i = 0; i < floorVertices.Count; i++)
        {
            float percentX = Mathf.InverseLerp(-cave.GetLength(0) / 2 * squareSize, cave.GetLength(0) / 2 * squareSize, floorVertices[i].x) * tileAmount;
            float percentY = Mathf.InverseLerp(-cave.GetLength(1) / 2 * squareSize, cave.GetLength(1) / 2 * squareSize, floorVertices[i].z) * tileAmount;

            uvs[i] = new Vector2(percentX, percentY);
        }

        floorMesh.uv = uvs;

		floor.mesh = floorMesh;

		AddNoise(floor, cave);

		floorCollider.sharedMesh = floor.mesh;

		// Update the nav mesh
		navMesh.BuildNavMesh();
	}

	private void AddNoise(MeshFilter mf, int[,] cave)
{
    Vector3[] vertices = mf.mesh.vertices;

    float noiseScale = 0.1f;
    float noiseAmplitude = 1f;

    // Calculate the bounds of the mesh
    Bounds bounds = mf.mesh.bounds;

    // Iterate over all vertices and apply noise if the vertex is not on the edge
    for (int i = 0; i < vertices.Length; i++)
    {
        // Check if the vertex is on the edge of the mesh
        bool isOnEdge = Mathf.Approximately(vertices[i].x, bounds.min.x) ||
                        Mathf.Approximately(vertices[i].x, bounds.max.x) ||
                        Mathf.Approximately(vertices[i].z, bounds.min.z) ||
                        Mathf.Approximately(vertices[i].z, bounds.max.z);

        // Apply noise only if the vertex is not on the edge
        if (!isOnEdge)
        {
            float xCoord = (vertices[i].x + cave.GetLength(0) / 2f) * noiseScale;
            float zCoord = (vertices[i].z + cave.GetLength(1) / 2f) * noiseScale;

            float noiseValue = Mathf.PerlinNoise(xCoord, zCoord);

            // Scale the noise value by the amplitude
            noiseValue *= noiseAmplitude;

            // Apply the noise to the vertex
            vertices[i] += Vector3.up * 1f / (1.02f - noiseValue);
        }
    }

    // Update the mesh
    mf.mesh.vertices = vertices;
    mf.mesh.RecalculateBounds();
    mf.mesh.RecalculateNormals();
}

	private void CreateRoofMesh(int[,] cave, float squareSize)
	{
		List<Vector3> roofVertices = new List<Vector3>();
		List<int> roofTriangles = new List<int>();
		Mesh roofMesh = new Mesh();

		roofVertices.Clear();
		roofTriangles.Clear();
		roofMesh.Clear();
		roof.mesh.Clear();

		Vector3 positionOffset = new Vector3(-(cave.GetLength(0) / 2f), 0, -(cave.GetLength(1) / 2f));

		for (int x = 0; x < cave.GetLength(0); x++)
		{
			for (int z = 0; z < cave.GetLength(1); z++)
			{
				if (x == cave.GetLength(0) - 1 || z == cave.GetLength(1) - 1)
				{
					continue;
				}

				// Calculate the position of the vertices for this square
				Vector3 bottomLeft = new Vector3(x, 0, z + squareSize) + positionOffset;
				Vector3 bottomRight = new Vector3(x + squareSize, 0, z + squareSize) + positionOffset;
				Vector3 topLeft = new Vector3(x, 0, z) + positionOffset;
				Vector3 topRight = new Vector3(x + squareSize, 0, z) + positionOffset;

				// Add the vertices to the list
				int vertexIndex = roofVertices.Count;

				roofVertices.Add(bottomLeft);
				roofVertices.Add(topLeft);
				roofVertices.Add(bottomRight);
				roofVertices.Add(topRight);

				// Add the triangles to the list
				roofTriangles.Add(vertexIndex + 0);
				roofTriangles.Add(vertexIndex + 2);
				roofTriangles.Add(vertexIndex + 1);

				roofTriangles.Add(vertexIndex + 1);
				roofTriangles.Add(vertexIndex + 2);
				roofTriangles.Add(vertexIndex + 3);
			}
		}

		roofMesh.vertices = roofVertices.ToArray();
		roofMesh.triangles = roofTriangles.ToArray();

		Vector2[] uvs = new Vector2[roofVertices.Count];

		int tileAmount = 50;
		for (int i = 0; i < roofVertices.Count; i++)
		{
			float percentX = Mathf.InverseLerp(-cave.GetLength(0) / 2 * squareSize, cave.GetLength(0) / 2 * squareSize, roofVertices[i].x) * tileAmount;
			float percentY = Mathf.InverseLerp(-cave.GetLength(1) / 2 * squareSize, cave.GetLength(1) / 2 * squareSize, roofVertices[i].z) * tileAmount;

			uvs[i] = new Vector2(percentX, percentY);
		}

		roofMesh.uv = uvs;

		roof.mesh = roofMesh;

		AddNoise(roof, cave);

		roofCollider.sharedMesh = roof.mesh;
	}

	private void TriangulateSquare(SquareConfiguration square)
	{
		switch (square.GetConfiguration())
		{
			case 0:
				break;

			// 1 point in square
			case 1:
				MeshFromPoints(square.GetCentreLeft(), square.GetCentreBottom(), square.GetBottomLeft());
				break;
			case 2:
				MeshFromPoints(square.GetBottomRight(), square.GetCentreBottom(), square.GetCentreRight());
				break;
			case 4:
				MeshFromPoints(square.GetTopRight(), square.GetCentreRight(), square.GetCentreTop());
				break;
			case 8:
				MeshFromPoints(square.GetTopLeft(), square.GetCentreTop(), square.GetCentreLeft());
				break;

			// 2 points in square
			case 3:
				MeshFromPoints(square.GetCentreRight(), square.GetBottomRight(), square.GetBottomLeft(), square.GetCentreLeft());
				break;
			case 6:
				MeshFromPoints(square.GetCentreTop(), square.GetTopRight(), square.GetBottomRight(), square.GetCentreBottom());
				break;
			case 9:
				MeshFromPoints(square.GetTopLeft(), square.GetCentreTop(), square.GetCentreBottom(), square.GetBottomLeft());
				break;
			case 12:
				MeshFromPoints(square.GetTopLeft(), square.GetTopRight(), square.GetCentreRight(), square.GetCentreLeft());
				break;
			case 5:
				MeshFromPoints(square.GetCentreTop(), square.GetTopRight(), square.GetCentreRight(), square.GetCentreBottom(), square.GetBottomLeft(), square.GetCentreLeft());
				break;
			case 10:
				MeshFromPoints(square.GetTopLeft(), square.GetCentreTop(), square.GetCentreRight(), square.GetBottomRight(), square.GetCentreBottom(), square.GetCentreLeft());
				break;

			// 3 points in square
			case 7:
				MeshFromPoints(square.GetCentreTop(), square.GetTopRight(), square.GetBottomRight(), square.GetBottomLeft(), square.GetCentreLeft());
				break;
			case 11:
				MeshFromPoints(square.GetTopLeft(), square.GetCentreTop(), square.GetCentreRight(), square.GetBottomRight(), square.GetBottomLeft());
				break;
			case 13:
				MeshFromPoints(square.GetTopLeft(), square.GetTopRight(), square.GetCentreRight(), square.GetCentreBottom(), square.GetBottomLeft());
				break;
			case 14:
				MeshFromPoints(square.GetTopLeft(), square.GetTopRight(), square.GetBottomRight(), square.GetCentreBottom(), square.GetCentreLeft());
				break;

			// All points in square
			case 15:
				MeshFromPoints(square.GetTopLeft(), square.GetTopRight(), square.GetBottomRight(), square.GetBottomLeft());
				checkedVertices.Add(square.GetTopLeft().GetVertexIndex());
				checkedVertices.Add(square.GetTopRight().GetVertexIndex());
				checkedVertices.Add(square.GetBottomRight().GetVertexIndex());
				checkedVertices.Add(square.GetBottomLeft().GetVertexIndex());
				break;
		}

	}

	private void MeshFromPoints(params CentreNode[] points)
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

	private void AssignVertices(CentreNode[] points)
	{
		for (int i = 0; i < points.Length; i++)
		{
			if (points[i].GetVertexIndex() == -1)
			{
				points[i].SetVertexIndex(vertices.Count);
				vertices.Add(points[i].GetPos());
			}
		}
	}

	private void CreateTriangle(CentreNode a, CentreNode b, CentreNode c)
	{
		triangles.Add(a.GetVertexIndex());
		triangles.Add(b.GetVertexIndex());
		triangles.Add(c.GetVertexIndex());

		MeshTriangle triangle = new MeshTriangle(a.GetVertexIndex(), b.GetVertexIndex(), c.GetVertexIndex());
		AddTriangleToDictionary(triangle.GetVertexA(), triangle);
		AddTriangleToDictionary(triangle.GetVertexB(), triangle);
		AddTriangleToDictionary(triangle.GetVertexC(), triangle);
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
		List<MeshTriangle> trianglesWithVertex = triangleDictionary[vertexIndex];

		for(int i = 0; i < trianglesWithVertex.Count; i++)
        {
			MeshTriangle triangle = trianglesWithVertex[i];

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
		List<MeshTriangle> trianglesWithVertexA = triangleDictionary[vertexA];
		int sharedTriangleCount = 0;

		for(int i = 0; i< trianglesWithVertexA.Count; i++)
        {
			if(trianglesWithVertexA[i].Contains(vertexB))
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

