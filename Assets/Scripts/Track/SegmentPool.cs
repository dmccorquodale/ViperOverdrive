using System.Collections.Generic;
using UnityEngine;

public class SegmentPool : MonoBehaviour
{
    public GameObject segmentPrefab;
    public int initial = 128;
    private readonly Queue<GameObject> pool = new();

    void Awake()
    {
        for (int i = 0; i < initial; i++)
            pool.Enqueue(Create());
    }

    GameObject Create()
    {
        var go = Instantiate(segmentPrefab, transform);
        go.SetActive(false);
        return go;
    }

    public GameObject Get()
    {
        if (pool.Count == 0) pool.Enqueue(Create());
        var go = pool.Dequeue();
        go.SetActive(true);
        return go;
    }

    public void Release(GameObject go)
    {
        go.SetActive(false);
        pool.Enqueue(go);
    }
}
