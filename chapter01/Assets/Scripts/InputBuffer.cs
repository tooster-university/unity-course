using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum InputAction {
    DASH,
    RESTART,
}

// http://kpulv.com/106/Jump_Input_Buffering/
public class InputBuffer : MonoBehaviour {
    public double earlyInputForgiveness = 0.1f;

    private static double _now;

    private struct ActionStruct {
        public double? timing;
        public object data;
    }

    private static ActionStruct[] _actions = new ActionStruct[Enum.GetNames(typeof(InputAction)).Length];

    // singleton
    protected static InputBuffer Instance { get; private set; }

    // returns true if action was read
    public static bool peekAction(InputAction action)
        => _actions[(int)action].timing != null &&  _now - _actions[(int) action].timing <= Instance.earlyInputForgiveness;

    // returns true if action was read and consumes the action
    public static bool pollAction(InputAction action) {
        var detected                                = peekAction(action);
        if (detected) _actions[(int) action].timing = null;
        return detected;
    }

    public static object getData(InputAction action) => _actions[(int) action].data;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            registerKeyDetectors();
        } else {
            Destroy(this);
        }
    }

    private void registerKeyDetectors() {
        KeyDetectors += ClickDetector(KeyCode.LeftArrow, () => { register(InputAction.DASH, MoveDirection.Left); });
        KeyDetectors += ClickDetector(KeyCode.RightArrow, () => { register(InputAction.DASH, MoveDirection.Right); });
        KeyDetectors += ClickDetector(KeyCode.R, () => { register(InputAction.RESTART, MoveDirection.Left); });
    }

    private Action ClickDetector(KeyCode keyCode, Action actionOnDetection) {
        var isPressed = false;
        return () => {
            if (Input.GetKeyUp(keyCode)) isPressed = false;
            if (Input.GetKeyDown(keyCode) && !isPressed) {
                isPressed = true;
                actionOnDetection.Invoke();
            }
        };
    }

    private void register(InputAction action, object data) {
        _actions[(int) action].timing = _now;
        _actions[(int) action].data   = data;
    }

    private void register(InputAction action) => register(action, null);

    private event Action KeyDetectors = () => { };

    private void Update() {
        _now += Time.unscaledTime;
        KeyDetectors.Invoke();
    }
}
