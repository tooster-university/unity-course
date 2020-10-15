using System;
using TMPro;
using UnityEngine;


public class PlayerController : MonoBehaviour {
    public new Rigidbody rigidbody;


    public float normalSpeed  = 1f;
    public float dashSpeed    = 5f;
    public float dashDuration = 0.1f;
    public float dashCooldown = 0.5f; // total time after dash is renewed

    public event Action<PlayerController> PlayerDied;

    private float _moveX;
    private float _dashTimer; // we can compare to float 0f since it's not a numeric error
    private bool  _dashTriggered;

    [SerializeField] private bool            godmode   = false;
    [SerializeField] private TextMeshProUGUI lifesText = null;
    private                  int             _lifes;

    public int Lifes {
        get => _lifes;
        set {
            _lifes         = value;
            lifesText.text = new string('#', _lifes);
        }
    }

    public void reset() {
        transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        _dashTimer     = 0f;
        _dashTriggered = false;
        Lifes          = 3;
    }
    
    public void move(float playerMoveDirection) => _moveX = playerMoveDirection;

    public void triggerDash() => _dashTriggered = true;

    public void releaseDash() {
        if (_dashTimer >= dashCooldown) {
            _dashTriggered = false;
            _dashTimer     = 0f;
        }
    }

    private void Awake() { reset(); }

    private void FixedUpdate() {
        var speed = normalSpeed;
        if (_dashTriggered) {
            if (_dashTimer <= dashDuration)
                speed = dashSpeed;
            _dashTimer += Time.deltaTime;
        }

        var velocity = rigidbody.velocity;
        velocity.x         = _moveX * (Time.deltaTime * speed);
        rigidbody.velocity = velocity;
    }

    private void OnTriggerEnter(Collider other) {
        if(godmode) return;
        --Lifes;
        if (Lifes > 0) {
            Destroy(other.gameObject);
        } else {
            _moveX = 0;
            PlayerDied?.Invoke(this);
        }
    }
}
