using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Scorekeeper : MonoBehaviour, Judge.ISubscriber
{
    [SerializeField] private PlayerInputRenderer[] playerInputRenderers;
    public UnityEvent[] ScoreChangeEvents;

    private Conductor conductor => Conductor.Instance;

    private int[] perPlayerHits = new int[2];
    private int[] perPlayerNotesPassed = new int[2];

    public int GetHits(int index)
    {
        if (index < 0 || index > perPlayerHits.Length) return 0;
        return perPlayerHits[index];
    }

    public int GetTotalElapsed(int index)
    {
        if (index < 0 || index > perPlayerHits.Length) return 0;
        return perPlayerNotesPassed[index];
    }

    public int P1Score
    {
        get => perPlayerHits[0];
        set {
            perPlayerHits[0] = value;
            ScoreChangeEvents[0]?.Invoke();
        }
    }

    public int P2Score
    {
        get => perPlayerHits[1];
        set
        {
            perPlayerHits[1] = value;
            ScoreChangeEvents[1]?.Invoke();
        }
    }

    public void OnDownHit(int index, BeatSpawnData danmu)
    {
        
    }

    public void OnUpHit(int trackIndex, BeatSpawnData danmu)
    {
        perPlayerHits[GetPlayerIndex(trackIndex)]++;
    }

    private InMemoryBeatmap beatmap;
    int beatmapCursor = 0;

    // Start is called before the first frame update
    void Start()
    {
        beatmap = InMemoryBeatmap.LoadSync(SongLoader.instance.currentSongType);
        Judge.Subscribe(this);
    }

    void OnDestroy()
    {
        Judge.Unsubscribe(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (Conductor.paused) return;
        TrySpawnNext();
        RemoveCompleted();
    }

    List<BeatSpawnData> visibleBeats = new List<BeatSpawnData>();

    private void TrySpawnNext()
    {
        if (!beatmap.TryGet(beatmapCursor, out BeatSpawnData nextSpawn)) return;
        float nextSpawnStart = nextSpawn.StartTimeDSP;
        float now = conductor.songPosition;
        if (now < nextSpawnStart) return;
        visibleBeats.Add(nextSpawn);
        beatmapCursor++;
        TrySpawnNext(); // keep going until out of valid beats
    }

    private void RemoveCompleted()
    {
        float now = conductor.songPosition;
        for (int i = visibleBeats.Count - 1; i >= 0; i--)
        {
            var visibleBeat = visibleBeats[i];
            float end = visibleBeat.EndTimeDSP;
            if (now > end)
            {
                visibleBeats.RemoveAt(i);
                perPlayerNotesPassed[GetPlayerIndex(visibleBeat.SoleTrack)]++;
            }
        }
    }

    private static int GetPlayerIndex(int trackIndex)
    {
        return InputManager.Instance.TrackPlayerMapping[trackIndex];
    }
}
