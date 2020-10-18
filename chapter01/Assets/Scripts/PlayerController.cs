using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour {
    [SerializeField] private bool            godMode    = false;
    [SerializeField] private TextMeshProUGUI lifesText  = null;
    [SerializeField] private GameObject      lanes      = null; // keeps children with lanes to swap onto
    [SerializeField] private AudioClip       dashSound  = null;
    [SerializeField] private AudioClip       jumpSound  = null;
    [SerializeField] private AudioClip       crashSound = null;
    [SerializeField] private AudioClip       healSound  = null;

    [NonSerialized] public Animator    animator;
    [NonSerialized] public AudioSource audioSource;
    private                Rigidbody   _rigidbody;


    public float                          dashDuration = 0.1f;
    public event Action<PlayerController> PlayerDied;

    private static readonly int START_LIFES = 3;
    private static readonly int MAX_LIFES   = 6;

    private float _dashTimer = 0.0f;
    private int   _lifes     = START_LIFES;
    private int   _currentLane;
    private int   _targetLane;

    private void Awake() {
        animator    = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        _rigidbody  = GetComponent<Rigidbody>();
        reset();
    }

    private int Lifes {
        get => _lifes;
        set {
            _lifes         = Mathf.Clamp(value, 0, MAX_LIFES);
            lifesText.text = new string('#', _lifes);
        }
    }

    public void reset() {
        transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        _dashTimer   = 0f;
        Lifes        = START_LIFES;
        _currentLane = _targetLane = lanes.transform.childCount / 2; // middle lane
    }

    private void FixedUpdate() {
        // if dashing
        if (_dashTimer > 0f) {
            _dashTimer += Time.fixedDeltaTime;
            // if dash ended
            if (_dashTimer >= dashDuration) {
                _dashTimer   = 0f;
                _currentLane = _targetLane;
            }
        } else if (_dashTimer == 0f && InputBuffer.pollAction(InputAction.DASH)) {
            audioSource.PlayOneShot(dashSound);
            _dashTimer += Time.fixedDeltaTime;
            // if not dashing, and polling resulted in dash
            var direction = (MoveDirection) InputBuffer.getData(InputAction.DASH);
            if (direction is MoveDirection.Left)
                --_targetLane;
            else if (direction is MoveDirection.Right)
                ++_targetLane;
            else Debug.LogError("DASH action data invalid.");

            _targetLane = Mathf.Clamp(_targetLane, 0, lanes.transform.childCount - 1); // lane arg in bounds 
        }

        // lerp positions with elastic bounce out
        var startPos = transform.position;
        var endPos   = startPos;
        startPos.x = lanes.transform.GetChild(_currentLane).transform.position.x;
        endPos.x   = lanes.transform.GetChild(_targetLane).transform.position.x;
        _rigidbody.MovePosition(Vector3.Lerp(startPos, endPos, Mathf.Clamp(_dashTimer / dashDuration, 0f, 1f)));
    }

    public void takeDamage(int damage) {
        if (!godMode) Lifes -= damage;
        audioSource.PlayOneShot(damage > 0 ? crashSound : healSound);
        if (Lifes == 0)
            PlayerDied?.Invoke(this);
    }

    public void playJumpSound() {
        audioSource.PlayOneShot(jumpSound);
    }
}
