using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.PostProcessing;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour {
    [SerializeField] private bool            godMode       = false;
    [SerializeField] private TextMeshProUGUI lifesText     = null;
    [SerializeField] private GameObject      lanes         = null; // keeps children with lanes to swap onto
    [SerializeField] private AudioClip       dashSound     = null;
    [SerializeField] private AudioClip       jumpSound     = null;
    [SerializeField] private AudioClip       crashSound    = null;
    [SerializeField] private AudioClip       healSound     = null;
    [SerializeField] private ParticleSystem  dashParticles = null;

    [SerializeField] private Color vignetteDamage = Color.red;
    [SerializeField] private Color vignetteHeal   = Color.green;

    [NonSerialized] public Animator    animator;
    [NonSerialized] public AudioSource audioSource;
    private                Rigidbody   _rigidbody;


    public float                          dashDuration = 0.1f;
    public event Action<PlayerController> PlayerDied;

    private static readonly int START_LIFES = 3;
    private static readonly int MAX_LIFES   = 6;

    private float          _dashTimer = 0.0f;
    private int            _lifes     = START_LIFES;
    private int            _currentLane;
    private int            _targetLane;
    private ParticleSystem _dashParticles;

    private void Awake() {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        _rigidbody = GetComponent<Rigidbody>();
        reset();
    }

    private int Lifes {
        get => _lifes;
        set {
            _lifes = Mathf.Clamp(value, 0, MAX_LIFES);
            lifesText.text = new string('#', _lifes);
        }
    }

    public void reset() {
        transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        _dashTimer = 0f;
        Lifes = START_LIFES;
        _currentLane = _targetLane = lanes.transform.childCount / 2; // middle lane
    }

    public void PlayerStart() {
        animator.enabled = true;
        audioSource.Play();
        _dashParticles = Instantiate(dashParticles, gameObject.transform);
    }

    public void PlayerStop() {
        animator.enabled = false;
        audioSource.Stop();
        if (_dashParticles != null)
            Destroy(_dashParticles);
    }

    private void FixedUpdate() {
        // if dashing
        if (_dashTimer > 0f) {
            _dashTimer += Time.fixedDeltaTime;
            // if dash ended
            if (_dashTimer >= dashDuration) {
                _dashTimer = 0f;
                _currentLane = _targetLane;
            }

        } else if (_dashTimer == 0f) {
            var action = InputBuffer.pollAction(InputAction.DASH) ?? InputBuffer.pollAction(InputAction.CHANGE_LANE);
            if (action != null) {
                audioSource.PlayOneShot(dashSound, 0.2f);
                _dashTimer += Time.fixedDeltaTime;
                if (action == InputAction.CHANGE_LANE) {
                    _targetLane = (int)InputBuffer.getData(InputAction.CHANGE_LANE);
                } else {
                    var direction = (MoveDirection)InputBuffer.getData(InputAction.DASH);
                    if (direction is MoveDirection.Left)
                        --_targetLane;
                    else if (direction is MoveDirection.Right)
                        ++_targetLane;
                    else Debug.LogError("DASH action data invalid.");

                    _targetLane = Mathf.Clamp(_targetLane, 0, lanes.transform.childCount - 1);
                }
            }
        }

        // lerp positions with elastic bounce out
        var startPos = transform.position;
        var endPos = startPos;
        startPos.x = lanes.transform.GetChild(_currentLane).transform.position.x;
        endPos.x = lanes.transform.GetChild(_targetLane).transform.position.x;
        _rigidbody.MovePosition(Vector3.Lerp(startPos, endPos, Mathf.Clamp(_dashTimer / dashDuration, 0f, 1f)));
    }

    public void takeDamage(int damage) {
        if (!godMode) Lifes -= damage;
        var sound = damage > 0 ? crashSound : healSound;
        var volume = damage > 0 ? 1f : 0.5f;
        var vignetteColor = damage > 0 ? vignetteDamage : vignetteHeal;
        audioSource.PlayOneShot(sound, volume);
        GameController.Instance.postProcessingController.VignetteBurst(vignetteColor);
        if (Lifes == 0) {
            PlayerStop();
            PlayerDied?.Invoke(this);
        }
    }

    public void playJumpSound() { audioSource.PlayOneShot(jumpSound, 0.1f); }
}
