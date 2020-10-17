using System;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour {
    public MapController    mapController    = null;
    public PlayerController playerController = null;
    public TextMeshProUGUI  jumboText        = null;

    [SerializeField] private float   windupDistance       = 300;
    [SerializeField] private Vector2 mapSpeed             = new Vector2(2f, 10f);
    [SerializeField] private Vector2 obstacleDistance     = new Vector2(10f, 2f);
    [SerializeField] private int     difficulty           = 1;
    [SerializeField] private float   fastForwardDistance  = 30f;
    [SerializeField] private float   fastForwardTimescale = 10f;

    public static GameController Instance { get; private set; }

    public enum State { WELCOME, PLAYING, DIED }

    public static readonly string[] DEAD_MESSAGES = {"DIEDED", "DED", "X-X", "KAPUT", "OOF", " :( ", "N00B"};

    // it ain't good, but leave it for now. Remember not to override directly.
    private State _gameState;

    // state switch and cleanup logic
    public State GameState {
        get => _gameState;
        private set {
            _gameState = value;
            switch (_gameState) {
                case State.WELCOME:
                    InputBuffer.enableActions(InputAction.RESTART);
                    InputBuffer.disableActions(InputAction.DASH);
                    playerController.animator.enabled = false;
                    playerController.reset();
                    mapController.reset();
                    mapController.scrollSpeed = 0f;
                    jumboText.enabled         = true;
                    jumboText.text            = " <-R-> ";
                    Time.timeScale            = playerController.audioSource.pitch = 1f;
                    break;
                case State.PLAYING:
                    InputBuffer.enableActions(InputAction.DASH);
                    InputBuffer.enableActions(InputAction.EXIT);
                    playerController.animator.enabled = true;
                    playerController.reset();
                    mapController.reset();
                    jumboText.enabled = false;
                    break;
                case State.DIED:
                    InputBuffer.disableActions(InputAction.DASH);
                    playerController.animator.enabled = false;
                    mapController.scrollSpeed         = 0f;
                    jumboText.enabled                 = true;
                    jumboText.text                    = DEAD_MESSAGES.OrderBy(_ => Guid.NewGuid()).First();
                    Time.timeScale = playerController.audioSource.pitch = 1f;
                    break;
            }
        }
    }

    void Update() {
        switch (GameState) {
            case State.WELCOME: goto restart;
            case State.DIED:
                restart:
                if (InputBuffer.pollAction(InputAction.RESTART))
                    GameState = State.PLAYING;
                break;
            case State.PLAYING:
                // todo: phases should be grouped by difficulty after distance and speed + interval windup
                if (mapController.DistanceTravelled < fastForwardDistance || Input.GetKey(KeyCode.UpArrow)) {
                    Time.timeScale                     = fastForwardTimescale;
                    playerController.audioSource.pitch = 1.2f;
                } else {
                    Time.timeScale = playerController.audioSource.pitch = 1f;
                }

                // exit during game
                if (InputBuffer.pollAction(InputAction.EXIT)) {
                    GameState = State.WELCOME;
                    return;
                }

                // phase 1 - windup speed
                mapController.scrollSpeed = Mathf.Lerp(mapSpeed.x, mapSpeed.y,
                                                       mapController.DistanceTravelled / windupDistance);
                // phase 2 - windup object distance
                mapController.obstacleDistance = Mathf.Lerp(obstacleDistance.x, obstacleDistance.y,
                                                            mapController.DistanceTravelled / windupDistance - 1f);
                mapController.difficulty = difficulty;

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
