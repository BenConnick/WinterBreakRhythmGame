using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Song Object")]
public class SongObject : ScriptableObject
{
    //This holds everything about a song.
    //It gets loaded into the main scene when a song is selected from the main menu
    [Header("Conductor info")]
    public SongType songType;
    public float bpm;
    public AudioClip track;
    public float timeSig;
    public float beatThreshold = 0.45f;
}

public enum SongType
{
    Default
}
