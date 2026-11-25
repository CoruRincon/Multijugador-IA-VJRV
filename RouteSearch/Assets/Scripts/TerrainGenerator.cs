using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField]
    private GameObject voxelPrefab;

    private List<GameObject> voxels;

    [SerializeField]
    private int mapWidth, mapHeight, mapLarge, seed;

    [SerializeField]
    private float detail;
    
    public void Start() {
        voxels = new List<GameObject>();
        seed = Random.Range(100000, 999999);
        GenerateTerrain();
    }

    public void GenerateTerrain() {
        if(mapHeight <= 0 || mapWidth <= 0 || mapLarge <= 0) { return; }

        for (int x = 0; x < mapWidth; x++) {
            for (int z = 0; z < mapLarge; z++) {
                mapHeight = (int)(Mathf.PerlinNoise((x / 2 + seed) / detail, (z / 2 + seed) / detail) * detail);
                for (int y = 0; y < mapHeight; y++) {
                    Vector3 voxelPos = new Vector3(x * voxelPrefab.transform.localScale.x, y * voxelPrefab.transform.localScale.y, z * voxelPrefab.transform.localScale.z);
                    GameObject thisVoxel = Instantiate(voxelPrefab, voxelPos, Quaternion.identity, transform);
                    thisVoxel.GetComponent<VoxelVisibility>().height = voxelPos.y;
                    thisVoxel.GetComponent<VoxelVisibility>().maxHeight = mapHeight;

                    voxels.Add(thisVoxel);
                }
            }
        }
    }
}
