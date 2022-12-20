using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BeatRenderer : MonoBehaviour
{
    public BeatSpawnData spawnData;

    public void Initialize(BeatSpawnData data)
    {
        spawnData = data;
    }
}
