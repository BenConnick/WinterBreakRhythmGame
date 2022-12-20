using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputManager : MonoBehaviour {

    // inspector
    public KeyCode[] Player1Buttons;
    public KeyCode[] Player2Buttons;

    public KeyCode PauseKey;

    public UnityEvent<KeyCode> RhythmButtonDownEvent;
    public UnityEvent<KeyCode> RhythmButtonUpEvent;

    // track maps
    public Dictionary<int, int> TrackPlayerMapping { get; private set; }
    public Dictionary<KeyCode, int> KeyTrackMapping { get; private set; }

    public interface ISubscriber
    {
        public void OnButtonDown(KeyCode key);
        public void OnButtonUp(KeyCode key);
    }

    public static void Subscribe(ISubscriber subscriber)
    {
        Instance.RhythmButtonDownEvent.AddListener(subscriber.OnButtonDown);
        Instance.RhythmButtonUpEvent.AddListener(subscriber.OnButtonUp);
    }

    public static void Unsubscribe(ISubscriber subscriber)
    {
        Instance.RhythmButtonDownEvent.RemoveListener(subscriber.OnButtonDown);
        Instance.RhythmButtonUpEvent.RemoveListener(subscriber.OnButtonUp);
    }

    // shorthand
    public static int GetTrack(KeyCode key)
    {
        return Instance.GetTrackIndex(key);
    }

    public int GetTrackIndex(KeyCode key)
    {
        if (KeyTrackMapping.ContainsKey(key))
            return KeyTrackMapping[key];

        return -1; // error
    }

    public static InputManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple input managers");
            Destroy(gameObject);
        }
        FillDerivedTrackStructures();
    }

    private void FillDerivedTrackStructures()
    {
        KeyTrackMapping = new Dictionary<KeyCode, int>();
        TrackPlayerMapping = new Dictionary<int, int>();
        int count = 0;
        foreach (KeyCode code in Player1Buttons)
        {
            KeyTrackMapping.Add(code, count);
            TrackPlayerMapping.Add(count, 0);
            count++;
        }
        foreach (KeyCode code in Player2Buttons)
        {
            KeyTrackMapping.Add(code, count);
            TrackPlayerMapping.Add(count, 1);
            count++;
        }
    }

    private void Update()
    {
        if (!Conductor.paused)
        {
            foreach (var code in Player1Buttons)
            {
                if (Input.GetKeyDown(code))
                    RhythmButtonDownEvent?.Invoke(code);
                if (Input.GetKeyUp(code))
                    RhythmButtonUpEvent?.Invoke(code);
            }

            foreach (var code in Player2Buttons)
            {
                if (Input.GetKeyDown(code))
                    RhythmButtonDownEvent?.Invoke(code);
                if (Input.GetKeyUp(code))
                    RhythmButtonUpEvent?.Invoke(code);
            }
        }

        if (Input.GetKeyDown(PauseKey))
        {
            
            if (!Conductor.paused)
            {
                Conductor.paused = true;
            }
            else
            {
                Conductor.Instance.Resume();
            }
        }
    }
}
