using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour , InputManager.ISubscriber
{
    [Header("Config")]
    [SerializeField] private int playerIndex = 0;
    [SerializeField] private float defaultAttackAnimDuration = 1f;

    [Header("References")]
    [SerializeField] private SpriteRenderer characterRenderer;

    [Header("Assets")]
    [SerializeField] private Sprite idle;
    [SerializeField] private Sprite upAttack;
    [SerializeField] private Sprite middleAttack;
    [SerializeField] private Sprite downAttack;

    public int PlayerIndex => playerIndex;

    private Sprite[] attackSprites;

    private float animCooldownTimer;

    void Awake()
    {
         attackSprites = new[] { upAttack, middleAttack, downAttack };
    }

    public void OnButtonDown(KeyCode key)
    {
        int track = InputManager.GetTrack(key);
        int player = InputManager.Instance.TrackPlayerMapping[track];
        if (player == playerIndex)
            PlayAttackAnim(track - InputManager.Instance.Player1Buttons.Length * playerIndex);
    }

    public void OnButtonUp(KeyCode key)
    {
        int track = InputManager.GetTrack(key);
        int player = InputManager.Instance.TrackPlayerMapping[track];
        if (player == playerIndex)
            animCooldownTimer = -1;
    }

    private void PlayAttackAnim(int index)
    {
        index = Mathf.Clamp(index, 0, attackSprites.Length-1);
        characterRenderer.sprite = attackSprites[index];
        animCooldownTimer = defaultAttackAnimDuration;
    }

    // Start is called before the first frame update
    void Start()
    {
        InputManager.Subscribe(this);
    }

    void Update()
    {
        //animCooldownTimer -= Time.deltaTime;
        if (animCooldownTimer < 0) characterRenderer.sprite = idle;
    }
}
