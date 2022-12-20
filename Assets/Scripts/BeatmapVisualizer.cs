using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class BeatmapVisualizer : MonoBehaviour, IPoolProvider<BeatmapVisualizer.BeatObject, BeatSpawnData>
{
    // Inspector
    [Header("Config")]
    public float PreviewAheadTime = 1f;
    public float LingerTime = .5f;

    [Header("Refs")]
    public Transform[] SpawnPoints;

    [Header("Resources")]
    [SerializeField] private SpriteRenderer beatPrefab;

    public RectTransform RT => GetComponent<RectTransform>();
    private InMemoryBeatmap beatmap;
    private Conductor conductor => Conductor.Instance;
    private int beatmapCursor;
    private List<BeatObject> visibleBeats = new List<BeatObject>();
    private BeatObjectPool beatObjectPool;

    // Start is called before the first frame update
    void Start()
    {
        beatObjectPool = new BeatObjectPool(this);
        string songName = SongLoader.instance.activeSong.songType.ToString();
        beatmap = InMemoryBeatmap.LoadSync(SongType.Default);
    }

    // Update is called once per frame
    void Update()
    {
        if (Conductor.paused) return;
        TrySpawnNext();
        RemoveCompleted();
        UpdateVisuals();
    }

    private void TrySpawnNext()
    {
        if (!beatmap.TryGet(beatmapCursor, out BeatSpawnData nextSpawn)) return;
        float nextSpawnStart = nextSpawn.StartTimeDSP;
        float now = conductor.songPosition;
        if (now + PreviewAheadTime < nextSpawnStart) return;
        visibleBeats.Add(GetPool().Get(nextSpawn));
        beatmapCursor++;
        TrySpawnNext(); // keep going until out of valid beats
    }

    private void RemoveCompleted()
    {
        float now = conductor.songPosition;
        for (int i = visibleBeats.Count - 1; i >= 0; i--)
        {
            var visibleBeat = visibleBeats[i];
            float end = visibleBeat.Data.EndTimeDSP;
            if (now > end + LingerTime)
            {
                visibleBeats.RemoveAt(i);
                visibleBeat.Free();
            }
        }
    }

    private void UpdateVisuals()
    {
        for (int i = 0; i < visibleBeats.Count; i++)
        {
            BeatObject item = visibleBeats[i];
            item.Update(conductor.songPosition);
        }
    }

    public IPool<BeatObject, BeatSpawnData> GetPool()
    {
        return beatObjectPool;
    }

    public class BeatObject : IPoolable<BeatSpawnData>
    {
        public BeatObject(BeatmapVisualizer visualizer, BeatSpawnData spawnData)
        {
            Owner = visualizer;
            Renderers = new SpriteRenderer[spawnData.Tracks.Count];
            for (int i = 0; i < Renderers.Length; i++)
            {
                Renderers[i] = Instantiate(Owner.beatPrefab, Owner.transform);
            }
            ReInitialize(spawnData);
        }

        public BeatmapVisualizer Owner;
        public BeatSpawnData Data;
        public SpriteRenderer[] Renderers;

        public bool CanUseFor(BeatSpawnData data)
        {
            return Data.Tracks.Count == data.Tracks.Count;
        }

        public void ReInitialize(BeatSpawnData spawnData)
        {
            Data = spawnData;
            Reset();
            VisDo(r =>
            {
                r.GetComponent<BeatRenderer>().Initialize(Data);
                r.gameObject.SetActive(true);
            });
        }

        // do something to each renderer
        public delegate void RendererPropSetter(SpriteRenderer spriteRenderer);
        private void VisDo(RendererPropSetter propSetter)
        {
            if (Renderers == null) return; // not initialized
            foreach (var r in Renderers) propSetter(r);
        }

        private void Reset()
        {
            // add all modified properties here
            static void resetRenderer(SpriteRenderer r)
            {
                // color
                r.color = Color.white;
                // scale
                //r.transform.localScale = Vector3.one;
                // sprite scale
                r.size = new Vector2(1, 1);
            }
            VisDo(resetRenderer);
        }

        public void Update(float now)
        {
            if (Renderers == null) return; // not initialized

            for (int i = 0; i < Renderers.Length; i++)
            {
                SpriteRenderer r = Renderers[i];

                // size
                // total preview time width ÷ full preview time = width / sec
                float timeWindow = Data.IsTap ? GlobalGameConfig.Inst.HitTimeWindow * .5f : (Data.EndTimeDSP - Data.StartTimeDSP);
                float width = timeWindow * Owner.RT.rect.width / Owner.PreviewAheadTime;
                float height = 1f;
                r.enabled = !Data.IsTap;
                r.size = new Vector2(width, height);

                // position
                r.transform.localPosition = new Vector3(
                    GetRendererX(now, width),
                    GetTrackY(Data.Tracks[i]));
            }
            if (now < Data.StartTimeDSP)
            {

            }
            else if (now > Data.EndTimeDSP)
            {
                // fade out
                float timeSinceEndNormalized = (now - Data.EndTimeDSP) / Owner.LingerTime;
                VisDo(r => r.color = new Color(1, 1, 1, .5f - timeSinceEndNormalized * .5f));
            }
            else
            {

            }
        }

        private float GetRendererX(float now, float worldWidth)
        {
            // if
            // preview ahead time = 5
            // startTime = 5
            // now = 0
            // normalized = (5 - 0) / 5 = 1
            // if
            // preview ahead time - 5
            // startTime = 5
            // now = 5
            // normalized = (5 - 5) / 5 = 0
            float adjustedNow = now + GlobalGameConfig.Inst.NoteTimeVisualOffset + GlobalGameConfig.Inst.HitTimeWindow * .5f;
            float normalizedBeatPos = (Data.StartTimeDSP - adjustedNow) / Owner.PreviewAheadTime;
            float w = Owner.RT.rect.width;
            return w * normalizedBeatPos - w * .5f + worldWidth * .5f;
        }

        private float GetTrackY(int track)
        {
            if (track >= 0 && track < Owner.SpawnPoints.Length)
            {
                Vector3 p = Owner.RT.InverseTransformPoint(Owner.SpawnPoints[track].position);
                return p.y;
            }
            // fallback
            Rect ownerRect = Owner.RT.rect;
            float normalized = track * .1667f;
            return (normalized - .5f) * ownerRect.height;
        }

        // convenient shorthand for freeing from pool
        // never call this method from the pool's free method (inf loop)
        // just a wrapper around the pool's free method
        // never add any additioonal cleanup here, do that in
        // reset or in the pool
        public void Free()
        {
            Owner.GetPool().Free(this);
        }

        public void Hide()
        {
            VisDo(r => r.gameObject.SetActive(false));
        }
    }

    private class BeatObjectPool : IPool<BeatObject, BeatSpawnData>
    {
        private BeatmapVisualizer visualizer;

        public BeatObjectPool(BeatmapVisualizer beatmapVisualizer)
        {
            visualizer = beatmapVisualizer;
        }

        private readonly List<BeatObject> inUse = new List<BeatObject>();
        private readonly List<BeatObject> freePool = new List<BeatObject>();

        public BeatObject Get(BeatSpawnData spawnData)
        {
            if (CanReuseFromPool(spawnData, out BeatObject obtained))
            {
                freePool.Remove(obtained);
                obtained.ReInitialize(spawnData);
            }
            else
            {
                // make a new one
                obtained = new BeatObject(visualizer, spawnData);
            }
            inUse.Add(obtained);
            return obtained;
        }

        private bool CanReuseFromPool(BeatSpawnData spawnData, out BeatObject usable)
        {
            usable = null;
            foreach (var b in freePool)
            {
                if (b.CanUseFor(spawnData))
                {
                    usable = b;
                    return true;
                }
            }
            return false;
        }

        public void Free(BeatObject beatObject)
        {
            if (inUse.Contains(beatObject)) inUse.Remove(beatObject);
            beatObject.Hide();
            freePool.Add(beatObject);
        }

        public void FreeAll()
        {
            foreach (var item in inUse)
                Free(item);
        }
    }
}
