using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public struct InMemoryBeatmap
{
    [SerializeField]
    private List<BeatSpawnData> Data;

    public void Add(BeatSpawnData newData)
    {
        if (Data == null) Data = new List<BeatSpawnData>();
        Data.Add(newData);
    }

    public bool TryGet(int index, out BeatSpawnData beat)
    {
        beat = default;
        if (Data == null || index < 0) return false;
        if (index >= Data.Count) return false;
        beat = Data[index];
        return true;
    }

    public int Count => Data?.Count ?? 0;

    public string ToCSV()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var b in Data)
        {
            sb.AppendLine(b.ToCSV());
        }
        return sb.ToString();
    }

    public InMemoryBeatmap(string csv)
    {
        Data = new List<BeatSpawnData>();
        string[] lines = csv.Split('\n');
        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line)) continue;
            Add(BeatSpawnData.Parse(line));
        }
    }

    public static InMemoryBeatmap LoadSync(SongType song)
    {
        var beatmapCSVFile = Resources.Load<TextAsset>("Beatmaps/" + song.ToString());
        string csvText = beatmapCSVFile.text;
        return new InMemoryBeatmap(csvText);
    }
}

[Serializable]
public struct BeatSpawnData
{
    // constants
    public const float MaxTimeForSimultaneousInput = 0.4f;
    public const float MinTimeForStretchedBeat = 1.5f;
    private const int SerializationIndexInput = 2;

    [SerializeField]
    public List<int> Tracks; // must be single-digit integers 0-9
    public float StartTimeDSP;
    public float RawEndTimeDSP;
    public float EndTimeDSP => IsTap ? StartTimeDSP + GlobalGameConfig.Inst.HitTimeWindow : RawEndTimeDSP;

    // convenience methods
    public bool IsValid => Tracks.Count > 0 && StartTimeDSP > 0;
    public bool IsSingle => Tracks.Count == 1;
    public int SoleTrack => Tracks[0];
    public bool IsTap => RawEndTimeDSP - StartTimeDSP < MinTimeForStretchedBeat;

    // serialize as CSV string
    // example format:
    // 23.2354351,25.124123,024
    public string ToCSV()
    {
        string combinedInputsString = "";
        foreach (int inputIndex in Tracks)
        {
            combinedInputsString += inputIndex;
        }
        return $"{StartTimeDSP},{RawEndTimeDSP},{combinedInputsString}";
    }

    // deserialize CSV string
    public static BeatSpawnData Parse(string csv)
    {
        string[] split = csv.Split(',');
        var inputs = new List<int>();
        for (int i = 0; i < split[SerializationIndexInput].Length; i++)
        {
            inputs.Add(int.Parse("" + split[SerializationIndexInput][i]));
        }
        return new BeatSpawnData()
        {
            StartTimeDSP = float.Parse(split[0]),
            RawEndTimeDSP = float.Parse(split[1]),
            Tracks = inputs
        };
    }
}