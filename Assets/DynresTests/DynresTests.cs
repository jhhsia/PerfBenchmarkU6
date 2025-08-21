using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Profiling.Memory;
using System;
using System.Runtime.CompilerServices;
using System.IO;

public class DynresTests : MonoBehaviour
{
    public enum Mode { Increase, Decrease }

    public GameObject prefab;
    public int totalObjects = 1000; // M
    public float duration = 10f;    // N (seconds)
    public int seed = 12345;

    private System.Random prng;
    private Camera cam;
    private Coroutine spawnRoutine;
    private List<GameObject> spawnedObjects = new List<GameObject>();

    void Awake()
    {
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("No main camera found!");
            enabled = false;
        }
    }

    public void StartSequence(Mode mode)
    {
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);

        prng = new System.Random(seed);

        if (mode == Mode.Increase)
            spawnRoutine = StartCoroutine(SpawnOverTime());
        else
            spawnRoutine = StartCoroutine(DespawnOverTime());
    }

    private IEnumerator SpawnOverTime()
    {
        int spawned = 0;
        float interval = duration / totalObjects;

        while (spawned < totalObjects)
        {
            SpawnObject();
            spawned++;
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator DespawnOverTime()
    {
        // Spawn all objects at once
        for (int i = 0; i < totalObjects; i++)
            SpawnObject();

        int remaining = totalObjects;
        int removePerSecond = Mathf.CeilToInt(totalObjects / duration);
        float interval = 1f;

        while (remaining > 0)
        {
            int toRemove = Mathf.Min(removePerSecond, remaining);
            for (int i = 0; i < toRemove; i++)
            {
                if (spawnedObjects.Count > 0)
                {
                    GameObject obj = spawnedObjects[0];
                    spawnedObjects.RemoveAt(0);
                    Destroy(obj);
                }
            }
            remaining -= toRemove;
            yield return new WaitForSeconds(interval);
        }
    }

    private void SpawnObject()
    {
        float x = (float)prng.NextDouble();
        float y = (float)prng.NextDouble();
        float z = Mathf.Lerp(5f, 20f, (float)prng.NextDouble());

        Vector3 viewportPos = new Vector3(x, y, z);
        Vector3 worldPos = cam.ViewportToWorldPoint(viewportPos);

        Quaternion rot = RandomRotation(prng);

        GameObject obj = Instantiate(prefab, worldPos, rot);
        spawnedObjects.Add(obj);
    }

    private Quaternion RandomRotation(System.Random prng)
    {
        float x = (float)prng.NextDouble();
        float y = (float)prng.NextDouble();
        float z = (float)prng.NextDouble();
        float w = (float)prng.NextDouble();

        Quaternion q = new Quaternion(x, y, z, w);
        q.Normalize();
        return q;
    }

    public void StartDynresSequence(int sequence)
    {
        Mode mode = sequence == 0 ? Mode.Increase : Mode.Decrease;
        StartSequence(mode);
    }
}
