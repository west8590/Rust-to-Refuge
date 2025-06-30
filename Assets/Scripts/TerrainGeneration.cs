using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class TerrainGeneration : MonoBehaviour
{
    public GameObject tree;
    public GameObject grass;
    public GameObject boarder;
    public GameObject sand;
    public int mapArea = 30;
    public int mapWashing = 4;
    public int scale = 20;
    int offsetX;
    int offsetY;
    void Start()
    {
        offsetX = Random.Range(0, 100000);
        offsetY = Random.Range(0, 100000);
        GenerateTerrain();
        StartCoroutine(CombineAtEndOfFrame());
        GenerateResources();
        GenerateExtraBoarders();
        StitchBoarderColliders();
        removeCloneName();
    }
    void removeCloneName()
    {
        foreach(Transform child in gameObject.transform)
        {
            if (child.name.Substring(child.name.Length-7) == "(Clone)")
            {
                child.name = child.name.Substring(0, child.name.Length - 7);
            }
        }
    }
    void GenerateResources()
    {
        for (int y = 0; y < mapArea-15; y+=2)
        {
            for (int x = 0; x < mapArea-15; x++)
            {
                if (Mathf.PerlinNoise((float)x / mapArea * scale + offsetX, (float)y / mapArea * scale + offsetY) >= 0.5)
                {
                    Instantiate(tree, new Vector3(x-mapArea/2+8,y + 0.5f - mapArea / 2 + 8, -0.1f),Quaternion.identity, gameObject.transform);
                }
            }
        }
    }
    void GenerateExtraBoarders()
    {
        for (int y = -1; y < mapArea+1; y++)
        {
            for (int x = -1; x < mapArea+1; x++)
            {
                if (x == -1 || y == -1 || x == mapArea || y == mapArea)
                {
                    Vector3 pos = new Vector3(x - (mapArea / 2), y - (mapArea / 2), 0f);
                    Instantiate(boarder, pos, Quaternion.identity, transform);
                }
            }
        }
    }
    void GenerateTerrain()
    {
        for (int y = 0; y < mapArea; y++)
        {
            for (int x = 0; x < mapArea; x++)
            {
                int edgeDistance = Mathf.Min(x, y, mapArea - 1 - x, mapArea - 1 - y);
                bool generateBlock = true;
                if (edgeDistance < mapWashing)
                {
                    float chance = (edgeDistance + 1f) / (mapWashing + 1f);
                    if (Random.value > chance) generateBlock = false;
                }
                Vector3 pos = new Vector3(x - (mapArea / 2), y - (mapArea / 2), 0.2f);
                if (generateBlock)
                {
                    if (y < mapWashing + 2 || y > mapArea-(mapWashing + 2) || x < mapWashing + 2 || x > mapArea - (mapWashing + 2))
                    {
                        Instantiate(sand, pos, Quaternion.identity, transform);
                    }
                    else
                    {
                        if (y < mapWashing + 3 || y > mapArea-(mapWashing + 3) || x < mapWashing + 3 || x > mapArea - (mapWashing + 3))
                        {
                            if (Random.Range(0,2) == 0)
                            {
                                Instantiate(grass, pos, Quaternion.identity, transform);
                            }
                            else
                            {
                                Instantiate(sand, pos, Quaternion.identity, transform);
                            }
                        }
                        else
                        {
                            Instantiate(grass, pos, Quaternion.identity, transform);
                        }
                    }
                }
                else
                {
                    Instantiate(boarder, pos, Quaternion.identity, transform);
                }
            }
        }
    }
    System.Collections.IEnumerator CombineAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        CombineByMaterial();
    }
    void CombineByMaterial()
    {
        var materialToFilters = new Dictionary<Material, List<CombineInstance>>();

        foreach (MeshFilter mf in GetComponentsInChildren<MeshFilter>())
        {
            if (mf.gameObject.name == "Tree") continue;
            if (mf.sharedMesh == null || mf == GetComponent<MeshFilter>()) continue;
            Material mat = mf.GetComponent<MeshRenderer>()?.sharedMaterial;
            if (mat == null) continue;

            if (!materialToFilters.ContainsKey(mat))
                materialToFilters[mat] = new List<CombineInstance>();

            CombineInstance ci = new CombineInstance();
            ci.mesh = mf.sharedMesh;
            ci.transform = mf.transform.localToWorldMatrix;
            materialToFilters[mat].Add(ci);
            Destroy(mf.gameObject);
        }

        int index = 0;
        foreach (var kvp in materialToFilters)
        {
            GameObject chunk = new GameObject("CombinedMesh_" + index++);
            chunk.transform.parent = transform;

            Mesh combined = new Mesh();
            combined.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            combined.CombineMeshes(kvp.Value.ToArray());

            chunk.AddComponent<MeshFilter>().mesh = combined;
            chunk.AddComponent<MeshRenderer>().sharedMaterial = kvp.Key;
        }
    }
    public void StitchBoarderColliders()
    {
        List<Vector2[]> allPaths = new List<Vector2[]>();

        List<GameObject> toDestroy = new List<GameObject>();

        foreach (Transform child in transform)
        {
            if (child.name == "Boarder(Clone)")
            {
                BoxCollider2D box = child.GetComponent<BoxCollider2D>();
                if (box != null)
                {
                    Vector2 offset = box.offset;
                    Vector2 size = box.size;
                    Vector2 pos = (Vector2)child.transform.position;

                    Vector2[] corners = new Vector2[4];
                    corners[0] = pos + offset + new Vector2(-size.x, -size.y) * 0.5f;
                    corners[1] = pos + offset + new Vector2(-size.x, size.y) * 0.5f;
                    corners[2] = pos + offset + new Vector2(size.x, size.y) * 0.5f;
                    corners[3] = pos + offset + new Vector2(size.x, -size.y) * 0.5f;

                    allPaths.Add(corners);
                    toDestroy.Add(child.gameObject);
                }
            }
        }

        if (allPaths.Count == 0)
        {
            Debug.LogWarning("No valid BoxCollider2D objects found for stitching.");
            return;
        }

        GameObject mergedObj = new GameObject("MergedBorderCollider");
        mergedObj.transform.parent = transform;
        mergedObj.transform.position = Vector3.zero;

        var poly = mergedObj.AddComponent<PolygonCollider2D>();
        poly.pathCount = allPaths.Count;
        for (int i = 0; i < allPaths.Count; i++)
        {
            poly.SetPath(i, allPaths[i]);
        }

        foreach (GameObject obj in toDestroy)
        {
            DestroyImmediate(obj);
        }
    }
}