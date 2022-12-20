using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class DebugBeatmapPlayer : MonoBehaviour
{
    // Inspector
    public float PreviewAheadTime = 1f;
    public float LingerTime = .5f;

    private TextMeshPro beatsDebugText;
    private TextMeshPro additionalInfoText;
    private InMemoryBeatmap beatmap;
    private Conductor conductor => Conductor.Instance;
    private int beatmapCursor;
    private List<BeatSpawnData> visibleBeats = new List<BeatSpawnData>();

    // Start is called before the first frame update
    void Start()
    {
        beatsDebugText = GetComponent<TextMeshPro>();
        string songName = SongLoader.instance.activeSong.songType.ToString();
        var beatmapCSVFile = Resources.Load<TextAsset>("Beatmaps/" + songName);
        string csvText = beatmapCSVFile.text;
        beatmap = new InMemoryBeatmap(csvText);
    }

    // Update is called once per frame
    void Update()
    {
        if (Conductor.paused) return;
        TrySpawnNext();
        RemoveCompleted();
        UpdateVisuals();

        if (additionalInfoText != null)
        {
            additionalInfoText.text = "" + conductor.songPosition;
        }
    }

    private void TrySpawnNext()
    {
        if (!beatmap.TryGet(beatmapCursor, out BeatSpawnData nextSpawn)) return;
        float nextSpawnStart = nextSpawn.StartTimeDSP;
        float now = conductor.songPosition;
        if (now + PreviewAheadTime < nextSpawnStart) return;
        visibleBeats.Add(nextSpawn);
        beatmapCursor++;
        TrySpawnNext(); // keep going until out of valid beats
    }

    private void RemoveCompleted()
    {
        float now = conductor.songPosition;
        for (int i = visibleBeats.Count - 1; i >= 0; i--)
        {
            BeatSpawnData visibleBeat = visibleBeats[i];
            float end = visibleBeat.RawEndTimeDSP;
            if (now > end + LingerTime)
            {
                visibleBeats.RemoveAt(i);
            }
        }
    }

    private void UpdateVisuals()
    {
        float now = conductor.songPosition;
        string combined = "";
        for (int i = 0; i < visibleBeats.Count; i++)
        {
            BeatSpawnData item = visibleBeats[i];
            string buttonsString = item.ToCSV().Split(',')[2];
            if (now < item.StartTimeDSP)
            {
                float t = 1 - ((item.StartTimeDSP - now) / PreviewAheadTime);
                combined += $"<color=#ffffff{PercentToHex(t)}>{buttonsString}</color>";
            }
            else if (now > item.RawEndTimeDSP)
            {
                combined += $"<color=red>{buttonsString}</color>";
            }
            else
            {
                combined += $"<color=green>{buttonsString}</color>";
            }
            if (i < visibleBeats.Count - 1) combined += " ";
        }
        beatsDebugText.text = combined;
    }

    private static string PercentToHex(float percent01)
    {
        string str = Mathf.FloorToInt(percent01 * 256).ToString("x");
        return str.Length > 1 ? str : "0" + str;
    }
}
