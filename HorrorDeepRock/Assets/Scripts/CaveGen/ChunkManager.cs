using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public GameObject chunk;

    // The size of each chunk, in units
    private int chunkSize = 80;
    public int chunkDistance = 1;

    public List<GameObject> chunks;

    public bool spawnPlayer;

    void Start()
    {
        for(int x = 0; x < chunkDistance; x++)
        {
            for (int z = 0; z < chunkDistance; z++)
            {
                Vector2 pos = new Vector2(x, z);
                Vector2 nXPos = new Vector2(x * -1, z);
                Vector2 nZPos = new Vector2(x, z * -1);
                Vector2 nZXPos = new Vector2(x * -1, z * -1);

                if (x == 0 && z == 0)
                {
                    CreateChunk(pos);
                    continue;
                }

                if (x == 0)
                {
                    CreateChunk(pos);
                    CreateChunk(nZPos);
                }
                else if (z == 0)
                {
                    CreateChunk(pos);
                    CreateChunk(nXPos);
                }
                else
                {
                    CreateChunk(pos);
                    CreateChunk(nXPos);
                    CreateChunk(nZPos);
                    CreateChunk(nZXPos);
                }
            }
        }
    }

    private void CreateChunk(Vector2 pos)
    {
        var chunkyboi = Instantiate(chunk, new Vector3(transform.position.x + (pos.x * chunkSize), transform.position.y, transform.position.z + (pos.y * chunkSize) ), Quaternion.identity);
        chunkyboi.name = "Chunk[" + pos.x + ", " + pos.y + "]";

        if(chunkyboi.name == "Chunk[0, 0]")
        {
            chunkyboi.GetComponent<CaveGenerator>().SetSpawnPlayer(spawnPlayer);
        }

        chunkyboi.transform.SetParent(gameObject.transform);
        chunkyboi.GetComponent<CaveGenerator>().SetSize(chunkSize);
        chunks.Add(chunkyboi);
    }

    public int GetSize()
    {
        return chunkSize;
    }
}
