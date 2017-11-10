using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerController : MonoBehaviour
{
    public GameObject NpcParent;
    public GameObject TemplateNpc;

    public float SpawnRadius;
    public int SpawnCount;
    public bool Respawn;

    private int _id = 0;
    private System.Random rd;

    // Use this for initialization
    void Start()
    {
        rd = new System.Random(name.GetHashCode() + System.DateTime.Now.Millisecond);
        TemplateNpc.SetActive(false);

        for (int i = 0; i < SpawnCount; ++i)
        {
            NewNpc();
        }
    }

    // Update is called once per frame
    void Update()
    {
        while (Respawn && NpcParent.transform.childCount < SpawnCount)
        {
            NewNpc();
        }
    }

    void NewNpc()
    {
        Vector3 ela = new Vector3(rd.Next(0, 360), rd.Next(0, 360), rd.Next(0, 360));
        Quaternion q = Quaternion.Euler(ela);
        Vector3 ela2 = new Vector3(rd.Next(0, 360), rd.Next(0, 360), rd.Next(0, 360));
        Quaternion q2 = Quaternion.Euler(ela2);
        Vector3 pos = q2 * Vector3.up * (((float)rd.NextDouble() * 0.7f + 0.3f) * SpawnRadius);

        GameObject newNpc = Instantiate(TemplateNpc, pos, q);
        newNpc.name = TemplateNpc.tag + _id;
        ++_id;
        newNpc.transform.SetParent(NpcParent.transform, true);
        newNpc.SetActive(true);
    }
}
