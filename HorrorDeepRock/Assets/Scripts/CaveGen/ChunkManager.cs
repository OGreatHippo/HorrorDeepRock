using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public GameObject chunk;

    public int chunkSize = 40;
    public int chunkDistance = 1;

    public List<GameObject> chunks;

    public bool spawnPlayer;

    private Transform camera;

    private int unloadDistance = 1;

    void Start()
    {
        camera = GameObject.Find("Camera").transform;

        int startX = Mathf.RoundToInt(camera.position.x / chunkSize);
        int startZ = Mathf.RoundToInt(camera.position.z / chunkSize);

        for (int x = startX - chunkDistance; x < startX + chunkDistance; x++)
        {
            for (int z = startZ - chunkDistance; z < startZ + chunkDistance; z++)
            {
                CreateChunk(new Vector2(x, z));
            }
        }

        StartCoroutine(GenerateChunks());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            foreach(GameObject _chunk in chunks)
            {
                _chunk.GetComponent<CaveGenerator>().GenerateCave(chunkSize);
            }
        }

        
    }

    private IEnumerator GenerateChunks()
    {
        while (true)
        {
            int playerX = Mathf.RoundToInt(camera.position.x / chunkSize);
            int playerZ = Mathf.RoundToInt(camera.position.z / chunkSize);

            // Load new chunks
            for (int x = playerX - chunkDistance; x < playerX + chunkDistance; x++)
            {
                for (int z = playerZ - chunkDistance; z < playerZ + chunkDistance; z++)
                {
                    if (!ChunkExists(new Vector2(x, z)))
                    {
                        CreateChunk(new Vector2(x, z));
                    }
                }
            }

            // Unload old chunks
            for (int i = chunks.Count - 1; i >= 0; i--)
            {
                GameObject chunk = chunks[i];
                Vector2 chunkPos = GetChunkPos(chunk.transform.position);

                if (Mathf.Abs(chunkPos.x - playerX) > unloadDistance || Mathf.Abs(chunkPos.y - playerZ) > unloadDistance)
                {
                    chunks.RemoveAt(i);
                    Destroy(chunk);
                }
            }

            yield return null;
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

    private bool ChunkExists(Vector2 pos)
    {
        foreach (GameObject chunk in chunks)
        {
            if (GetChunkPos(chunk.transform.position) == pos)
            {
                return true;
            }
        }

        return false;
    }

    private Vector2 GetChunkPos(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / chunkSize);
        int z = Mathf.RoundToInt(worldPos.z / chunkSize);
        return new Vector2(x, z);
    }

    public int GetSize()
    {
        return chunkSize;
    }
}
