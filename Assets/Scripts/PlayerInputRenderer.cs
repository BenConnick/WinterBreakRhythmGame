using UnityEngine;

public class PlayerInputRenderer : MonoBehaviour, InputManager.ISubscriber, Judge.ISubscriber
{
    [Header("Config")]
    public int playerIndex;
    public int localButtonIndex;
    public float pressAnimSpeed = 10f;

    [Header("References")]
    public Animator primaryAnimator;
    public Animation successAnimation;

    bool pressed;
    bool hitNoteStart;
    float pressLerp;

    public KeyCode keyCode
    {
        get
        {
            var btns = playerIndex == 1 ? InputManager.Instance.Player2Buttons : InputManager.Instance.Player1Buttons;
            return btns[localButtonIndex];
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        InputManager.Subscribe(this);
        Judge.Subscribe(this);
    }

    private void OnDestroy()
    {
        InputManager.Unsubscribe(this);
        Judge.Unsubscribe(this);
    }

    // Update is called once per frame
    void Update()
    {
        pressLerp = Mathf.Clamp01(pressLerp + pressAnimSpeed * Time.deltaTime * (pressed ? 1 : -1));
        primaryAnimator.SetFloat("Pressed", pressLerp);
    }

    public void OnButtonDown(KeyCode key)
    {
        if (key != keyCode) return;
        pressed = true;
    }

    public void OnButtonUp(KeyCode key)
    {
        if (key != keyCode) return;
        pressed = false;
    }

    public void OnDownHit(int index, BeatSpawnData danmu)
    {
        // unused
    }

    public void OnUpHit(int index, BeatSpawnData danmu)
    {
        int selfIndex = InputManager.GetTrack(keyCode);
        if (index != selfIndex) return;

        Debug.Log("hit");
        successAnimation.Stop();
        successAnimation.Play();
    }
}
