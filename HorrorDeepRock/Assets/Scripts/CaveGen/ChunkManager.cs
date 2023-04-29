using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public CaveGenerator caveData;

    // The size of each chunk, in units
    public int chunkSize = 80;

    void Start()
    {
        GenerateMesh();
    }

    void GenerateMesh()
    {
        // Generate the cave data for this chunk
        caveData.GenerateCave(transform.position, chunkSize);
    }
}
