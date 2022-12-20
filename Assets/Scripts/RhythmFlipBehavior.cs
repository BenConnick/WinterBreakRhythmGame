using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmFlipBehavior : MonoBehaviour
{
    public float FlipTimer;
    public float NextFlip;

    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        FlipTimer = Conductor.Instance.songPosInBeats;
        if (FlipTimer > NextFlip)
        {
            NextFlip = FlipTimer + 1;
            spriteRenderer.flipY = !spriteRenderer.flipY;
        }
    }
}
