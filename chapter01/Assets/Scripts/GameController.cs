using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public enum State { WELCOME, PLAYING, DIED }


public class GameController : MonoBehaviour {
    public MapController            mapController            = null;
    public PlayerController         playerController         = null;
    public TextMeshProUGUI          jumboText                = null;
    public TextMeshProUGUI          fastForwardIndicator     = null;
    public PostProcessingController postProcessingController = null;

    [SerializeField] private float     fastForwardDistance  = 30f;
    [SerializeField] private float     fastForwardTimescale = 10f;
    [SerializeField] private float     fastForwardPitch     = 1.2f;
    [SerializeField] private bool      canSlowDown          = false;
    [SerializeField] private AudioClip gameOverSound        = null;

    public static GameController Instance { get; private set; }

    public static readonly string[] DEAD_MESSAGES =
    {
        "DIEDED", "DED", "x_x", "KAPUT", "OOF", " :( ", "GIT GUD", "...", "XD", "U MAD?", "FACEPALM", "R.I.P", "K.I.A", "LOL", "KEKW", "DUDE...", "._."
    };


    // it ain't good, but leave it for now. Remember not to override directly.
    private State _gameState;

    // state switch and cleanup logic
    public State GameState {
        get => _gameState;
        private set {
            _gameState = value;
            switch (_gameState) {
                case State.WELCOME:
                    InputBuffer.enableActions(InputAction.RESTART, InputAction.CHANGE_DIFFICULTY);
                    InputBuffer.disableActions(InputAction.DASH, InputAction.CHANGE_LANE);
                    playerController.reset();
                    playerController.PlayerStop();
                    mapController.reset();
                    mapController.displayHighscore();
                    jumboText.enabled            = true;
                    jumboText.text               = "SPACE";
                    fastForwardIndicator.enabled = false;
                    Time.timeScale               = playerController.audioSource.pitch = 1f;
                    break;
                case State.PLAYING:
                    InputBuffer.enableActions(InputAction.DASH, InputAction.EXIT, InputAction.CHANGE_LANE);
                    InputBuffer.disableActions(InputAction.CHANGE_DIFFICULTY);
                    playerController.reset();
                    playerController.PlayerStart();
                    mapController.reset();
                    mapController.Running             = true;
                    jumboText.enabled = false;
                    break;
                case State.DIED:
                    InputBuffer.enableActions(InputAction.RESTART, InputAction.CHANGE_DIFFICULTY);
                    InputBuffer.disableActions(InputAction.DASH, InputAction.CHANGE_LANE);
                    playerController.PlayerStop();
                    mapController.Running        = false;
                    jumboText.enabled            = true;
                    jumboText.text               = DEAD_MESSAGES.OrderBy(_ => Guid.NewGuid()).First();
                    fastForwardIndicator.enabled = false;
                    Time.timeScale               = playerController.audioSource.pitch = 1f;
                    playerController.audioSource.PlayOneShot(gameOverSound);
                    break;
            }
        }
    }

    void Update() {
        switch (GameState) {
            case State.WELCOME:
                goto pauseScreen;
            case State.DIED:
                // exit during game
                if (InputBuffer.pollAction(InputAction.EXIT) != null) GameState = State.WELCOME;

                pauseScreen:

                if (InputBuffer.pollAction(InputAction.RESTART) != null)
                    GameState = State.PLAYING;
                break;

            case State.PLAYING:

                // timescale manipulation
                // yeah, the input handling should be separated somewhere but this project is too small to bother
                if (mapController.DistanceTravelled < fastForwardDistance || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.L)) {
                    fastForwardIndicator.enabled       = true;
                    Time.timeScale                     = fastForwardTimescale;
                    playerController.audioSource.pitch = fastForwardPitch;
                } else if (canSlowDown && (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.L))) {
                    Time.timeScale = playerController.audioSource.pitch = 0.5f; // WIP 
                } else {
                    fastForwardIndicator.enabled = false;
                    Time.timeScale               = playerController.audioSource.pitch = 1f;
                }

                // exit during game
                if (InputBuffer.pollAction(InputAction.EXIT) != null) GameState = State.WELCOME;

                break;
        }
    }

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(this);
        }

        Cursor.visible   = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    // Start is called before the first frame update
    void Start() {
        playerController.PlayerDied += pc => { GameState = State.DIED; };
        GameState                   =  State.WELCOME;
    }
}
