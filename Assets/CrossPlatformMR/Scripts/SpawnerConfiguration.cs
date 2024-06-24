using Fusion.XRShared.Demo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerConfiguration : MonoBehaviour
{
    [SerializeField] Mesh visual;
    [SerializeField] GameObject prefab;
    [SerializeField] GrabbablePrefabSpawner spawner;
    public void OnSelected()
    {
        if (spawner == null)
            spawner = gameObject.transform.parent.GetComponentInChildren<GrabbablePrefabSpawner>();
        if (spawner == null)
            Debug.LogError("GrabbablePrefabSpawner not found");

        spawner.prefab = prefab;
        spawner.spawnerGrabbableReference.GetComponentInChildren<MeshFilter>().mesh = visual;
    }

}
