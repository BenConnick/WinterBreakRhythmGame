using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Conductor : MonoBehaviour {

    //Static song information
    public float beatTempo;
    public float secPerBeat;
    public float offestInBeats = 4;
    private float offsetToFirstBeat;
    public float startingPosition;
    public AudioSource musicSource;
    public AudioListener audioListener;

    //Dynamic song information
    public float songPosition;
    public float songPosInBeats;
    public float dspSongTime;
    public float loopPosInBeats;
    public float loopPosInAnalog;
    public int completedLoops;
    public bool musicStarted = false;
    public int currentSequence;
    public int totalSequences = 1;

    //Pause information
    public static bool paused = false;
    public static float pauseTimeStamp = -1f;
    public static float pausedTime = 0;
    public GameObject PauseCanvas;

    //static note information
    public float timeSig;
    public float beatThreshold;

    // Debugging
    public bool PrintDebugLogs;

    //Instance
    public static Conductor Instance { get; private set; }
    public bool isTutorial = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartMusic(); // replace this with a player input when we have one
    }

    private void LoadParameters()
    {
        if (PrintDebugLogs) Debug.Log("Loading conductor parameters");
        beatTempo = SongLoader.instance.activeSong.bpm;
        musicSource.clip = SongLoader.instance.activeSong.track;
        timeSig = SongLoader.instance.activeSong.timeSig;
    }

    private void ComputeDerivedValues()
    {
        if (beatTempo == 0 || timeSig == 0)
        {
            Debug.LogError("Conductor computing derived values before valid inital params");
            return;
        }

        paused = false;
        pauseTimeStamp = -1f;
        //Calculate the number of seconds per beat
        secPerBeat = 60f / beatTempo;
        //Figure out 1 measure lead in
        //Set the song to n-1 measures into the loop
        offsetToFirstBeat = offestInBeats * secPerBeat;
        startingPosition = timeSig * secPerBeat - offsetToFirstBeat;
        songPosition = startingPosition;
        songPosInBeats = songPosition / secPerBeat;
        loopPosInBeats = songPosInBeats + 1;
        loopPosInAnalog = (loopPosInBeats - 1) / timeSig;
    }

	// Update is called once per frame
	void Update () {

        //Only do things if the music has started
        if (!musicStarted) return;

        if (paused)
        {
            if (pauseTimeStamp < 0f) //not managed
            {
                pauseTimeStamp = (float)AudioSettings.dspTime;
                AudioListener.pause = true;
                //Activate some UI here
                PauseCanvas.SetActive(true);
            }

            return;
        }
        else if (pauseTimeStamp > 0f) //resume not managed
        {
            AudioListener.pause = false;
            pauseTimeStamp = -1f;
        }

        //calculate the position of the song in seconds from dsp space
        songPosition = (float)(AudioSettings.dspTime - dspSongTime - pausedTime);

        //calculate the position in beats
        songPosInBeats = songPosition / secPerBeat;

        //calculate loop position in beats
        if (songPosInBeats >= (completedLoops + 1) * timeSig)
            completedLoops++;
        loopPosInBeats = songPosInBeats - completedLoops * timeSig + 1;
        loopPosInAnalog = (loopPosInBeats-1) / timeSig;

    }

    public void Resume()
    {
        PauseCanvas.SetActive(false);
        paused = false;
    }

    public void StartMusic()
    {
        LoadParameters();

        ComputeDerivedValues();

        if (PrintDebugLogs) Debug.Log("Starting Music");

        //Record the time when the audio starts
        dspSongTime = (float)AudioSettings.dspTime - startingPosition;

        //start the song
        musicSource.Play();

        musicStarted = true;
    }

    public void StopMetronome()
    {
        musicSource.volume = 0;
    }
    
}
