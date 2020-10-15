using System;
using TMPro;
using UnityEngine;


public class PlayerController : MonoBehaviour {
    public new Rigidbody rigidbody;


    public float normalSpeed  = 1f;
    public float dashSpeed    = 5f;
    public float dashDuration = 0.1f;
    public float dashCooldown = 0.5f; // total time after dash is renewed

    private float _moveX         = 0f;
    private float _dashTimer     = 0f; // we can compare to float 0f since it's not a numeric error
    private bool  _dashTriggered = false;

    [SerializeField] private TextMeshProUGUI lifesText;
    private                  int             _lifes;

    public int Lifes {
        get => _lifes;
        set {
            _lifes         = value;
            lifesText.text = new string('#', _lifes);
        }
    }


    private void setLifes(int lifes) { _lifes = lifes; }

    private void Awake() { setLifes(3); }

    private void Update() {
        // determine move direction
        _moveX = 0f
               + (Input.GetKey(KeyCode.RightArrow) ? 1f : 0f)
               - (Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);

        // reset dash after cooldown and shift release
        if (Input.GetKeyUp(KeyCode.LeftShift) && _dashTimer >= dashCooldown) {
            _dashTriggered = false;
            _dashTimer     = 0f;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && !_dashTriggered)
            _dashTriggered = true;
    }

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

    public event Action<PlayerController> PlayerDied;

    private void OnTriggerEnter(Collider other) {
        if (--Lifes > 0) {
            Destroy(other.gameObject);
        } else
            PlayerDied?.Invoke(this);
    }
}
