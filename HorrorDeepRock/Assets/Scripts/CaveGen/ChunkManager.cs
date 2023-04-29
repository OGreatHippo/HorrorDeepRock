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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            GenerateMesh();
        }
    }

    void GenerateMesh()
    {
        StartCoroutine(delay());
    }

    IEnumerator delay()
    {
        yield return new WaitForSeconds(0.01f);

        // Generate the cave data for this chunk
        caveData.GenerateCave(chunkSize);
    }
}
