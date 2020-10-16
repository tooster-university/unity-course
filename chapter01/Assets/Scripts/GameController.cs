using System;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour {
    public MapController    mapController    = null;
    public PlayerController playerController = null;
    public TextMeshProUGUI  jumboText        = null;

    [SerializeField] private float       windupTime           = 60f;
    [SerializeField] private Vector2     mapSpeed             = new Vector2(2f, 10f);
    [SerializeField] private Vector2     obstacleDistance     = new Vector2(10f, 2f);
    [SerializeField] private int         difficulty           = 1;
    [SerializeField] private float       fastForwardDistance  = 30f;
    [SerializeField] private float       fastForwardTimescale = 10f;

    public static GameController Instance { get; private set; }

    public enum State { WELCOME, PLAYING, DIED }

    // it ain't good, but leave it for now
    private State _gameState;
    private float _playingTimer;

    public State GameState {
        get => _gameState;
        private set {
            _gameState = value;
            switch (_gameState) {
                case State.WELCOME:
                    jumboText.enabled = true;
                    jumboText.text    = "PRESS R";
                    break;
                case State.PLAYING:
                    _playingTimer                  = 0f;
                    jumboText.enabled              = false;
                    mapController.scrollSpeed      = mapSpeed.x;
                    mapController.obstacleDistance = obstacleDistance.x;
                    mapController.difficulty       = difficulty;
                    mapController.reset();
                    playerController.reset();
                    break;
                case State.DIED:
                    mapController.scrollSpeed = 0f;
                    jumboText.enabled         = true;
                    jumboText.text            = "DIEDED";
                    break;
            }
        }
    }

    // input processor;

    void Update() {
        switch (GameState) {
            case State.WELCOME: goto restart;
            case State.DIED:
                restart:
                if (InputBuffer.pollAction(InputAction.RESTART))
                    GameState = State.PLAYING;
                break;
            case State.PLAYING:
                // fast forward

                _playingTimer += Time.deltaTime;

                // phase 1 - windup speed
                mapController.scrollSpeed
                    = Mathf.Lerp(mapSpeed.x, mapSpeed.y, _playingTimer / windupTime);
                // phase 2 - windup object distance
                mapController.obstacleDistance =
                    Mathf.Lerp(obstacleDistance.x, obstacleDistance.y, (_playingTimer - windupTime) / windupTime);
                mapController.difficulty = difficulty;

                if (mapController.DistanceTravelled < fastForwardDistance) {
                    Time.timeScale = fastForwardTimescale;
                } else {
                    Time.timeScale = 1f;
                }

                break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(this);
        }
    }

    // Start is called before the first frame update
    void Start() {
        playerController.PlayerDied += pc => GameState = State.DIED;
        GameState                   =  State.WELCOME;
    }
}
