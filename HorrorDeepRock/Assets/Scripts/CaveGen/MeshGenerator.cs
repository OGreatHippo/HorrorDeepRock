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

	List<Vector3> vertices;
	List<int> triangles;

	Dictionary<int, List<MeshTriangle>> triangleDictionary = new Dictionary<int, List<MeshTriangle>>();
	List<List<int>> edges = new List<List<int>>();
	HashSet<int> checkedVertices = new HashSet<int>();

	public float wallHeight = 10;

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

		for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
		{
			for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
			{
				TriangulateSquare(squareGrid.squares[x, y]);
			}
		}

		Mesh mesh = new Mesh();
		caveM.mesh = mesh;

		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals();

		Vector2[] uvs = new Vector2[vertices.Count];

		for(int i = 0; i < vertices.Count; i++)
        {
			float percentX = Mathf.InverseLerp(-cave.GetLength(0) / 2 * squareSize, cave.GetLength(0) / 2 * squareSize, vertices[i].x) * tileAmount;
			float percentY = Mathf.InverseLerp(-cave.GetLength(1) / 2 * squareSize, cave.GetLength(1) / 2 * squareSize, vertices[i].z) * tileAmount;

			uvs[i] = new Vector2(percentX, percentY);
		}

		mesh.uv = uvs;

		CreateWallMesh(cave, 1);
	}

	private void CreateWallMesh(int[,] cave, float squareSize)
    {
		CalculateMeshEdge();

		List<Vector3> wallVertices = new List<Vector3>();

		List<int> wallTriangles = new List<int>();

		Mesh wallMesh = new Mesh();

		float minY = 0.1f;
		float maxY = 0.5f;

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

		/* this part slows down generation due to it looping thorugh all vertices and adding the texture to it, could be improved by grouping each length of wall together and pasting one big texture over it instead, also needs texture to tile on the y and not just x */

        //Vector2[] uvs = new Vector2[wallVertices.Count];

        //float textureScale = caveMat.mainTextureScale.x;
        //float increment = (textureScale / cave.GetLength(0));

        //float[] uvEntries = new float[] { 0.5f, increment };

        //for (int i = 0; i < wallVertices.Count; i++)
        //{
        //    float percentY = Mathf.InverseLerp((-wallHeight) * squareSize, 0, wallMesh.vertices[i].y) * (wallHeight / cave.GetLength(0)) * tileAmount;
        //    uvs[i] = new Vector2(uvEntries[i % 2], percentY);
        //}

        //wallMesh.uv = uvs;

		/* end of part that needs improvement */

        wallMesh.RecalculateNormals();

		walls.mesh = wallMesh;

		wallCollider.sharedMesh = walls.mesh;

		walls.GetComponent<MeshRenderer>().material = caveMat;

		//CreateFloorMesh(cave, minY, 1);
		FloorMesh(cave, minY, 1);
		CreateRoofMesh(cave, maxY, 1);
	}

	private void FloorMesh(int[,] cave, float _minY, float squareSize)
    {
		List<Vector3> floorVertices = new List<Vector3>();
		List<int> floorTriangles = new List<int>();
		Mesh floorMesh = new Mesh();

		Vector3 positionOffset = new Vector3(-(cave.GetLength(0) / 2f), 0, -(cave.GetLength(1) / 2f));

		for (int x = 0; x < cave.GetLength(0); x++)
        {
			for (int z = 0; z < cave.GetLength(1); z++)
			{
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

		floorMesh.RecalculateNormals();
		floorMesh.RecalculateBounds();

		floor.mesh = floorMesh;
	}

    private void CreateFloorMesh(int[,] cave, float _minY, float squareSize)
    {
		List<Vector3> floorVertices = new List<Vector3>();

		List<int> floorTriangles = new List<int>();

		Mesh floorMesh = new Mesh();

		//Vector3 positionOffset = new Vector3(-(cave.GetLength(0) / 2f), 0, -(cave.GetLength(1) / 2f));

		//int blockSize = 50;
		//int numBlocksX = cave.GetLength(0) / blockSize;
		//int numBlocksZ = cave.GetLength(1) / blockSize;

		//// Loop through each block and generate vertices and triangles
		//for (int blockX = 0; blockX < numBlocksX; blockX++)
		//{
		//	for (int blockZ = 0; blockZ < numBlocksZ; blockZ++)
		//	{
		//		int lastBlockSizeX = cave.GetLength(0) % blockSize;
		//		int lastBlockSizeZ = cave.GetLength(1) % blockSize;
		//		if (lastBlockSizeX != 0)
		//		{
		//			// Adjust the width of the last block
		//			int startX = numBlocksX * blockSize;
		//			int endX = startX + lastBlockSizeX;
		//			for (int x = startX; x <= endX; x++)
		//			{
		//				for (int z = 0; z <= blockSize; z++)
		//				{
		//					Vector3 vertex = new Vector3(x * squareSize, _minY, (blockZ * blockSize + z) * squareSize) + positionOffset;
		//					floorVertices.Add(vertex);
		//				}
		//			}
		//		}
		//		if (lastBlockSizeZ != 0)
		//		{
		//			// Adjust the depth of the last block
		//			int startZ = numBlocksZ * blockSize;
		//			int endZ = startZ + lastBlockSizeZ;
		//			for (int x = 0; x <= blockSize; x++)
		//			{
		//				for (int z = startZ; z <= endZ; z++)
		//				{
		//					Vector3 vertex = new Vector3((blockX * blockSize + x) * squareSize, _minY, z * squareSize) + positionOffset;
		//					floorVertices.Add(vertex);
		//				}
		//			}
		//		}

		//		int startIndex = floorVertices.Count;

		//              // Generate block vertices
		//              for (int x = 0; x <= blockSize; x++)
		//              {
		//                  for (int z = 0; z <= blockSize; z++)
		//                  {
		//                      Vector3 vertex = new Vector3((blockX * blockSize + x) * squareSize, _minY, (blockZ * blockSize + z) * squareSize) + positionOffset;
		//                      floorVertices.Add(vertex);
		//                  }
		//              }

		Vector3 positionOffset = new Vector3(-(cave.GetLength(0) / 2f), 0, -(cave.GetLength(1) / 2f));

		int blockSize = 50;
		int numBlocksX = cave.GetLength(0) / blockSize;
		int numBlocksZ = cave.GetLength(1) / blockSize;

		// Loop through each block and generate vertices and triangles
		for (int blockX = 0; blockX < numBlocksX; blockX++)
		{
			for (int blockZ = 0; blockZ < numBlocksZ; blockZ++)
			{
				int startIndex = floorVertices.Count;

				// Generate block vertices
				for (int x = 0; x <= blockSize; x++)
				{
					for (int z = 0; z <= blockSize; z++)
					{
						Vector3 vertex = new Vector3((blockX * blockSize + x) * squareSize, _minY, (blockZ * blockSize + z) * squareSize) + positionOffset;
						floorVertices.Add(vertex);
					}
				}

				int lastBlockSizeX = cave.GetLength(0) % blockSize;
				int lastBlockSizeZ = cave.GetLength(1) % blockSize;
				if (lastBlockSizeX != 0 && blockX == numBlocksX - 1)
				{
					// Adjust the width of the last block
					int startX = numBlocksX * blockSize;
					int endX = startX + lastBlockSizeX;
					for (int x = startX; x <= endX; x++)
					{
						for (int z = 0; z <= blockSize; z++)
						{
							Vector3 vertex = new Vector3(x * squareSize, _minY, (blockZ * blockSize + z) * squareSize) + positionOffset;
							floorVertices.Add(vertex);
						}
					}
				}
				if (lastBlockSizeZ != 0 && blockZ == numBlocksZ - 1)
				{
					// Adjust the depth of the last block
					int startZ = numBlocksZ * blockSize;
					int endZ = startZ + lastBlockSizeZ;
					for (int x = 0; x <= blockSize; x++)
					{
						for (int z = startZ; z <= endZ; z++)
						{
							Vector3 vertex = new Vector3((blockX * blockSize + x) * squareSize, _minY, z * squareSize) + positionOffset;
							floorVertices.Add(vertex);
						}
					}
				}

				// Generate block triangles
				for (int x = 0; x < blockSize; x++)
				{
					for (int z = 0; z < blockSize; z++)
					{
						int vertexIndex = (x * (blockSize + 1)) + z + startIndex;
						int nextVertexIndex = vertexIndex + blockSize + 1;

						// First triangle
						floorTriangles.Add(vertexIndex + 1);
						floorTriangles.Add(nextVertexIndex);
						floorTriangles.Add(vertexIndex);

                        // Second triangle
                        floorTriangles.Add(nextVertexIndex + 1);
						floorTriangles.Add(nextVertexIndex);
						floorTriangles.Add(vertexIndex + 1);
					}
				}
			}
		}

		//floorTriangles.Add(vertexIndex + 0);
		//floorTriangles.Add(vertexIndex + 2);
		//floorTriangles.Add(vertexIndex + 1);

		//floorTriangles.Add(vertexIndex + 1);
		//floorTriangles.Add(vertexIndex + 2);
		//floorTriangles.Add(vertexIndex + 1);

		floorMesh.vertices = floorVertices.ToArray();
		floorMesh.triangles = floorTriangles.ToArray();

        //Vector2[] uvs = new Vector2[floorVertices.Count];

        //for (int i = 0; i < floorVertices.Count; i++)
        //{
        //    float percentX = Mathf.InverseLerp(-cave.GetLength(0) / 2 * squareSize, cave.GetLength(0) / 2 * squareSize, floorVertices[i].x);
        //    float percentY = Mathf.InverseLerp(-cave.GetLength(1) / 2 * squareSize, cave.GetLength(1) / 2 * squareSize, floorVertices[i].z);

        //    uvs[i] = new Vector2(percentX, percentY);
        //}

        //floorMesh.uv = uvs;

        floorMesh.RecalculateBounds();
        floorMesh.RecalculateNormals();

		floor.mesh = floorMesh;

		//AddNoise(floor, cave);

		//navMesh.BuildNavMesh();

		floorCollider.sharedMesh = floor.mesh;
	}

	private void AddNoise(MeshFilter mf, int[,] cave)
    {
		Vector3[] vertices = mf.mesh.vertices;

		float noiseScale = 0.1f;

		for (int i = 0; i < vertices.Length; i++)
        {
			float xCoord = (vertices[i].x + cave.GetLength(0) / 2f) * noiseScale;
			float yCoord = (vertices[i].z + cave.GetLength(1) / 2f) * noiseScale;

			float noiseValue = Mathf.PerlinNoise(xCoord, yCoord);

			vertices[i] += Vector3.up * noiseValue * 2f;
		}

		mf.mesh.vertices = vertices;
		mf.mesh.RecalculateBounds();
		mf.mesh.RecalculateNormals();
	}

	private void CreateRoofMesh(int[,] cave, float _maxY, float squareSize)
	{
		List<Vector3> roofVertices = new List<Vector3>();

		List<int> roofTriangles = new List<int>();

		Mesh roofMesh = new Mesh();

		Vector3 positionOffset = new Vector3(-(cave.GetLength(0) / 2f), 0, -(cave.GetLength(1) / 2f));

        int startIndex = roofVertices.Count;
        roofVertices.Add(new Vector3(0f, _maxY, 0f) + positionOffset);
		roofVertices.Add(new Vector3(cave.GetLength(0), _maxY, 0f) + positionOffset);
		roofVertices.Add(new Vector3(0f, _maxY, cave.GetLength(1)) + positionOffset);
		roofVertices.Add(new Vector3(cave.GetLength(0), _maxY, cave.GetLength(1)) + positionOffset);

		roofTriangles.Add(startIndex + 0);
		roofTriangles.Add(startIndex + 2);
		roofTriangles.Add(startIndex + 1);

		roofTriangles.Add(startIndex + 1);
		roofTriangles.Add(startIndex + 2);
		roofTriangles.Add(startIndex + 3);

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

		roofMesh.RecalculateNormals();

		roof.mesh = roofMesh;

		roofCollider.sharedMesh = roof.mesh;	
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


	//int startIndex = floorVertices.Count;
	//floorVertices.Add(new Vector3(0f, _minY, 0f) + positionOffset);
	//floorVertices.Add(new Vector3(cave.GetLength(0), _minY, 0f) + positionOffset);
	//floorVertices.Add(new Vector3(0f, _minY, cave.GetLength(1)) + positionOffset);
	//floorVertices.Add(new Vector3(cave.GetLength(0), _minY, cave.GetLength(1)) + positionOffset);

	//floorTriangles.Add(startIndex + 0);
	//floorTriangles.Add(startIndex + 2);
	//floorTriangles.Add(startIndex + 1);

	//floorTriangles.Add(startIndex + 1);
	//floorTriangles.Add(startIndex + 2);
	//floorTriangles.Add(startIndex + 3);

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

