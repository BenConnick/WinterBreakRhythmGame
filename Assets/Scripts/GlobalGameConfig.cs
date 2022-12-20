using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "GlobalGameConfig")]
public class GlobalGameConfig : ScriptableObject
{
    private static GlobalGameConfig inst;
    public static GlobalGameConfig Inst => inst;

    [RuntimeInitializeOnLoadMethod]
    private static void Init()
    {
        inst = Resources.Load<GlobalGameConfig>("GlobalGameConfig");
    }

    // global values
    public float HitTimeWindow = .25f;
    public float NoteTimeVisualOffset = 0f;
    public KeyCode[] Player1Keys;
    public KeyCode[] Player2Keys;
}
