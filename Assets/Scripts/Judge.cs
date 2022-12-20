using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;

public class Judge : MonoBehaviour, InputManager.ISubscriber
{
    public interface ISubscriber
    {
        public void OnDownHit(int index, BeatSpawnData danmu);
        public void OnUpHit(int index, BeatSpawnData danmu);
    }

    public static void Subscribe(ISubscriber subscriber)
    {
        Inst.DownHit.AddListener(subscriber.OnDownHit);
        Inst.UpHit.AddListener(subscriber.OnUpHit);
    }

    public static void Unsubscribe(ISubscriber subscriber)
    {
        Inst.DownHit.RemoveListener(subscriber.OnDownHit);
        Inst.UpHit.RemoveListener(subscriber.OnUpHit);
    }

    public static Judge Inst { get; private set; }

    private InMemoryBeatmap beatmap;
    private Conductor conductor => Conductor.Instance;

    public UnityEvent<int,BeatSpawnData> DownHit;
    public UnityEvent<int, BeatSpawnData> UpHit;

    private void Awake()
    {
        Inst = this;
        beatmap = InMemoryBeatmap.LoadSync(SongType.Default);
        InputManager.Subscribe(this);
    }

    void OnDestroy()
    {
        InputManager.Unsubscribe(this);
    }

    public BeatSpawnData? DownInThreshold(int track)
    {
        float threshold = GlobalGameConfig.Inst.HitTimeWindow;
        float now = Conductor.Instance.songPosition;
        string debugString = "Down no hits.\nNow: " + now + " +-" + threshold;
        for (int i = 0; i < beatmap.Count; i++)
        {
            if (beatmap.TryGet(i, out BeatSpawnData beat))
            {
                debugString += ($"\nbeatStart:{beat.StartTimeDSP}");
                if (beat.StartTimeDSP < now - threshold) continue; // danmu hasn't come
                if (beat.StartTimeDSP > now + threshold) continue; // danmu start just passed
                debugString += " (hit time)";
                if (beat.Tracks.Contains(track))
                {
                    Debug.Log("Down hit: " + beat.StartTimeDSP);
                    return beat;
                }
            }
        }
        Debug.Log(debugString);
        return null;
    }

    public BeatSpawnData? UpInThreshold(int track)
    {
        float threshold = GlobalGameConfig.Inst.HitTimeWindow;
        float now = Conductor.Instance.songPosition;
        string debugString = "Up no hits.\nNow: " + now + " +-" + threshold;
        for (int i = 0; i < beatmap.Count; i++)
        {
            if (beatmap.TryGet(i, out BeatSpawnData beat))
            {
                debugString += ($"\nbeatEnd:{beat.EndTimeDSP}");
                if (beat.IsTap)
                {
                    if (beat.StartTimeDSP < now - threshold) continue; // tap danmu hasn't started
                    if (beat.StartTimeDSP > now + threshold) continue; // tap danmu passed
                }
                else
                {
                    if (beat.StartTimeDSP < now - threshold) continue; // long danmu hasn't started
                    if (beat.EndTimeDSP > now + threshold) continue; // long danmu passed
                    if (beat.EndTimeDSP < now + threshold) continue; // too early to finish long note
                }
                debugString += " (hit time)";
                if (beat.Tracks.Contains(track))
                {
                    Debug.Log("Up hit: " + beat.EndTimeDSP);
                    return beat;
                }
            }
        }
        Debug.Log(debugString);
        return null;
    }

    List<int> partialDownHits = new List<int>();

    public void OnButtonDown(KeyCode key)
    {
        int i = InputManager.GetTrack(key);
        var danmu = DownInThreshold(i);
        if (danmu != null)
        {
            partialDownHits.Add(i);
            DownHit?.Invoke(i, danmu.Value);
        }
    }

    public void OnButtonUp(KeyCode key)
    {
        int i = InputManager.GetTrack(key);
        var danmu = UpInThreshold(i);

        // if success, play success
        if (danmu != null && (danmu.Value.IsTap || partialDownHits.Contains(i)))
        {
            UpHit?.Invoke(i, danmu.Value);
        }

        partialDownHits.Remove(i);
    }
}
