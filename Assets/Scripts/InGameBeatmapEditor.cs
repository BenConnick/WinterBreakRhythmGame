using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class InGameBeatmapEditor : MonoBehaviour
{
    // inspector
    public bool Listening;
    public bool LogDebug;
    public bool Flush;
    public string FileOutputPath = "Assets/Resources/Beatmaps/Default.txt";

    public InMemoryBeatmap Beatmap; // exposed to inspector for debugging

    private Conductor conductor => Conductor.Instance;

    private float now => conductor.songPosition;

    private readonly List<BeatSpawnData> partialBeatRecordings = new List<BeatSpawnData>();

    // Start is called before the first frame update
    void Start()
    {
        if (!Application.isPlaying) return;
        var input = InputManager.Instance;
        input.RhythmButtonDownEvent.AddListener(RecordKeyDown);
        input.RhythmButtonUpEvent.AddListener(RecordKeyUp);
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying || !Listening) return;
        if (Flush) FlushToFile();
    }

    private void OnDestroy()
    {
        var input = InputManager.Instance;
        input.RhythmButtonDownEvent.RemoveListener(RecordKeyDown);
        input.RhythmButtonUpEvent.RemoveListener(RecordKeyUp);
    }

    private void RecordKeyDown(KeyCode code)
    {
        if (!Application.isPlaying) return;
        if (!Listening) return;

        // possible simultaneous input
        if (partialBeatRecordings.Count > 0 && now - partialBeatRecordings[partialBeatRecordings.Count - 1].StartTimeDSP < BeatSpawnData.MaxTimeForSimultaneousInput)
        {
            if (LogDebug) Debug.Log("Add simultaneous input: " + (Beatmap.Count + partialBeatRecordings.Count) + " " + code);
            var inputs = partialBeatRecordings[partialBeatRecordings.Count - 1].Tracks;
            inputs.Add(GetIndex(code));
            return;
        }

        // new input
        if (LogDebug) Debug.Log("Begin new input: " + (Beatmap.Count + partialBeatRecordings.Count+1) + " " + code);
        partialBeatRecordings.Add(new BeatSpawnData()
        {
            StartTimeDSP = now,
            Tracks = new List<int>
            {
                GetIndex(code)
            }
        });
    }

    private void RecordKeyUp(KeyCode code)
    {
        if (!Application.isPlaying) return;
        if (!Listening) return;

        if (partialBeatRecordings.Count == 0) return;

        // find matching,
        // finish them
        int codeIndex = GetIndex(code);
        for (int i = partialBeatRecordings.Count-1; i >= 0; i--)
        {
            var beatEntry = partialBeatRecordings[i];
            foreach (var idx in beatEntry.Tracks)
            {
                if (idx == codeIndex)
                {
                    SaveBeatSpawn(beatEntry);
                    partialBeatRecordings.RemoveAt(i);
                    break;
                }
            }
        }
    }

    private void SaveBeatSpawn(BeatSpawnData b)
    {
        if (LogDebug) Debug.Log("Lock input: " + (Beatmap.Count+1));
        b.RawEndTimeDSP = now;
        Beatmap.Add(b);
    }

    private int GetIndex(KeyCode code)
    {
        return InputManager.GetTrack(code);
    }

    private void FlushToFile()
    {
        Flush = false;
        if (string.IsNullOrEmpty(FileOutputPath))
        {
            Debug.LogError("Output file path missing!");
            return;
        }
        string fileText = Beatmap.ToCSV();
        try
        {
            File.WriteAllText(FileOutputPath, fileText);
        }
        catch (IOException e)
        {
            Debug.LogError("Failed to write beatmap: " + e.Message + "\n" + e.StackTrace);
            return;
        }
        Beatmap = new InMemoryBeatmap();
    }
}