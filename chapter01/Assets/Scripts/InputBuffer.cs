using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = System.Object;

public enum InputAction {
    DASH,
    RESTART,
    EXIT,
    CHANGE_DIFFICULTY,
}

// http://kpulv.com/106/Jump_Input_Buffering/
public class InputBuffer : MonoBehaviour {
    public double earlyInputForgiveness = 0.1f;

    private static readonly int ACTIONS_CNT = Enum.GetNames(typeof(InputAction)).Length;

    private static double _now = 0;

    // separated for better locality
    private static double?[] _actionTimers   = new double?[ACTIONS_CNT];
    private static object[]  _actionData     = new Object[ACTIONS_CNT];
    private static BitArray  _enabledActions = new BitArray(ACTIONS_CNT);

    // singleton
    protected static InputBuffer Instance { get; private set; }

    // todo: fix InputSystem package not working on Linux 
    // returns true if action was read
    internal static bool peekAction(InputAction action) =>
        _enabledActions[(int) action]
     && _actionTimers[(int) action] != null
     && _now - _actionTimers[(int) action] <= Instance.earlyInputForgiveness;

    // returns true if action was read and consumes the action
    public static bool pollAction(InputAction action) {
        var detected = peekAction(action);

        if (detected) _actionTimers[(int) action] = null;
        return detected;
    }

    public static object getData(InputAction action) => _actionData[(int) action];

    public static void disableActions(params InputAction[] actions) => setActions(false, actions);

    public static void enableActions(params  InputAction[] actions) => setActions(true, actions);

    private static void setActions(bool value, params InputAction[] actions) {
        if (actions.Length == 0) _enabledActions.SetAll(value);
        else
            foreach (var action in actions)
                _enabledActions[(int) action] = value;
    }

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
        KeyDetectors += actionStartedDetector(KeyCode.LeftArrow,  () => { markActionRegistered(InputAction.DASH, MoveDirection.Left); });
        KeyDetectors += actionStartedDetector(KeyCode.RightArrow, () => { markActionRegistered(InputAction.DASH, MoveDirection.Right); });
        KeyDetectors += actionStartedDetector(KeyCode.Space, () => { markActionRegistered(InputAction.RESTART); });
        KeyDetectors += actionStartedDetector(KeyCode.Escape, () => { markActionRegistered(InputAction.EXIT); });
        KeyDetectors += actionStartedDetector(KeyCode.Return, () => { markActionRegistered(InputAction.CHANGE_DIFFICULTY); });
        KeyDetectors += actionStartedDetector(KeyCode.Q, () => { Application.Quit(0); });
    }

    private Action actionStartedDetector(KeyCode keyCode, Action actionOnDetection) {
        var isPressed = false;
        return () => {
            if (Input.GetKeyUp(keyCode)) isPressed = false;
            if (Input.GetKeyDown(keyCode) && !isPressed) {
                isPressed = true;
                actionOnDetection.Invoke();
            }
        };
    }

    private void markActionRegistered(InputAction action, object data = null) {
        _actionTimers[(int) action] = _now;
        _actionData[(int) action]   = data;
    }

    private event Action KeyDetectors = () => { };

    private void Update() {
        _now = Time.unscaledTime;
        KeyDetectors.Invoke();
    }
}
