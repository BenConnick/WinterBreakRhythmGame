using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SongLoader : MonoBehaviour
{
    public int songIndex;
    public SongObject[] songObjects;
    public SongType currentSongType;
    public SongObject activeSong;

    public static SongLoader instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Debug.Log("Double songloader instance");
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        // load first song
        var s = songObjects[0];
        currentSongType = s.songType;
        activeSong = s;
    }
}
