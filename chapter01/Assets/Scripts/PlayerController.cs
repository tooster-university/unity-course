using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;


public class PlayerController : MonoBehaviour {
    [SerializeField] private bool            godmode   = false;
    [SerializeField] private TextMeshProUGUI lifesText = null;
    [SerializeField] private GameObject      lanes     = null; // keeps children with lanes to swap onto

    public new Rigidbody                  rigidbody;
    public     float                      dashDuration = 0.1f;
    public event Action<PlayerController> PlayerDied;


    private float _dashTimer = 0.0f;
    private int   _lifes     = 3;
    private int   _currentLane;
    private int   _targetLane;

    private int Lifes {
        get => _lifes;
        set {
            _lifes         = Math.Max(0, value);
            lifesText.text = new string('#', _lifes);
        }
    }

    public void reset() {
        transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        _dashTimer   = 0f;
        Lifes        = 3;
        _currentLane = _targetLane = lanes.transform.childCount / 2; // middle lane
    }

    private void Awake() { reset(); }

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
        rigidbody.MovePosition(Vector3.Lerp(startPos, endPos, _dashTimer / dashDuration));
    }

    public void takeDamage(int damage) {
        if (godmode) return;

        Lifes -= damage;
        if (Lifes == 0)
            PlayerDied?.Invoke(this);
    }
}
