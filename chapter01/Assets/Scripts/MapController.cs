using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapController : MonoBehaviour {
    [SerializeField] private float   windupDistance        = 300f;
    [SerializeField] private float   difficultyThreshold   = 300f;
    [SerializeField] private float   lifeInterval          = 500f;
    [SerializeField] private Vector2 mapSpeedRange         = new Vector2(4f, 9f);
    [SerializeField] private Vector2 obstacleDistanceRange = new Vector2(4f, 3f);

    [SerializeField, Space] private GameObject      obstaclesRoot   = null;
    [SerializeField]        private TextMeshProUGUI difficultyText  = null;
    [SerializeField]        private BoxCollider     mapBounds       = null;
    [SerializeField]        private GameObject      spawnpoints     = null;
    [SerializeField]        private float           appearanceDuration = 1f;
    [SerializeField]        private GameObject      ground          = null;
    [SerializeField]        private LifePickup      lifePrefab      = null;
    [SerializeField]        private Obstacle[]      obstaclePrefabs = null;
    [SerializeField]        private Color           difficultyVignetteColor;


    public TextMeshProUGUI scoreText = null;
    public float           DistanceTravelled { get; private set; }
    public bool            Running           { get; set; }

    private Material _groundMaterial        = null;
    private float    _lastSpawnDistance     = 0f;
    private float    _lastLifeSpawnDistance = 0f;
    private int      _initialDifficulty     = 1;
    private int      _difficulty            = 1;
    private float    _obstacleDistance      = 3f;
    private float    _scrollSpeed           = 0f;
    private float    _highscore             = 0f;

    private static readonly Vector3 MOTION_VECTOR = Vector3.back;
    private static readonly int     MAIN_TEX      = Shader.PropertyToID("_MainTex");
    private static readonly Color   NORMAL_COLOR  = new Color(1, 1, 1);
    private static readonly Color   GOLD_COLOR    = new Color(0xFF / 255f, 0xA9 / 255f, 0x00 / 255f);

    public int Difficulty {
        get => _difficulty;
        set {
            _difficulty         = Mathf.Clamp(value, 1, spawnpoints.transform.childCount);
            difficultyText.text = new string('!', _difficulty);
        }
    }

    public void reset() {
        DistanceTravelled      = 0f;
        _lastSpawnDistance     = -_obstacleDistance; // to make it spawn in first frame
        _lastLifeSpawnDistance = 0f;
        _scrollSpeed           = 0f;
        Difficulty             = _initialDifficulty;
        Running                = false;
        scoreText.color        = NORMAL_COLOR;
        foreach (Transform child in obstaclesRoot.transform) // cleanup
            Destroy(child.gameObject);
    }

    public void displayHighscore() {
        scoreText.text  = ((int) _highscore).ToString();
        scoreText.color = GOLD_COLOR;
    }

    void Start() {
        _groundMaterial = ground.GetComponent<Renderer>().material;
        reset();
    }

    private void FixedUpdate() {
        if (!Running) return;

        spawnNextLayer();

        scrollMap();

        adjustDifficulty();
    }

    private void Update() {
        // difficulty update - loop em'
        if (InputBuffer.pollAction(InputAction.CHANGE_DIFFICULTY) != null) {
            Difficulty = _initialDifficulty = 1 + _initialDifficulty % spawnpoints.transform.childCount;
        }

        if (!Running) return;

        if (DistanceTravelled >= _highscore) {
            _highscore = DistanceTravelled;
            displayHighscore();
        } else
            scoreText.text = ((int) DistanceTravelled).ToString();
    }

    private void adjustDifficulty() {
        //     phase 1 - windup speed
        _scrollSpeed = Mathf.Lerp(mapSpeedRange.x, mapSpeedRange.y, DistanceTravelled / windupDistance);
        //     phase 2 - windup object distance
        _obstacleDistance = Mathf.Lerp(obstacleDistanceRange.x, obstacleDistanceRange.y,
                                       DistanceTravelled / windupDistance);
        //     phase 3 - change difficulty between 1-5
        var newDifficulty = _initialDifficulty + (int) (DistanceTravelled / difficultyThreshold);
        if (newDifficulty != Difficulty) {
            Difficulty = newDifficulty;
            GameController.Instance.postProcessingController.VignetteBurst(difficultyVignetteColor);
        }
    }

    private void scrollMap() {
        DistanceTravelled += Time.deltaTime * _scrollSpeed;
        _groundMaterial.SetTextureOffset(MAIN_TEX, new Vector2(0, DistanceTravelled));

        // move obstacles and destroy if needed
        foreach (Transform obstacle in obstaclesRoot.transform) {
            obstacle.GetComponent<Rigidbody>()
                    .MovePosition(obstacle.transform.position + MOTION_VECTOR * (_scrollSpeed * Time.deltaTime));
            // obstacle.transform.position += MOTION_VECTOR * (scrollSpeed * Time.deltaTime);

            if (!mapBounds.bounds.Contains(obstacle.position))
                Destroy(obstacle.gameObject);
        }
    }

    private void spawnNextLayer() {
        if (DistanceTravelled - _lastSpawnDistance < _obstacleDistance)
            return;

        _lastSpawnDistance = DistanceTravelled;

        var lanes = spawnpoints.transform.childCount;

        // roll maximum number of barrels scaled with difficulty
        int obstaclesToSpawn                 = Random.Range(1, Mathf.Min(Difficulty + 3, lanes));
        if (Difficulty > 1) obstaclesToSpawn = Random.Range(obstaclesToSpawn, Mathf.Min(Difficulty, lanes));
        if (Difficulty > 3) obstaclesToSpawn = Random.Range(obstaclesToSpawn, Mathf.Min(Difficulty, lanes));

        // for random indexes in spawnpoints spawn obstacles
        foreach (var laneIdx in Enumerable.Range(0, lanes).OrderBy(x => Guid.NewGuid()).Take(obstaclesToSpawn)) {
            // random obstacle
            Obstacle laneElement = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            if (DistanceTravelled - _lastLifeSpawnDistance >= lifeInterval) {
                _lastLifeSpawnDistance = DistanceTravelled;
                laneElement            = lifePrefab;
            }

            var obstacle = Instantiate(laneElement.gameObject,
                                       spawnpoints.transform.GetChild(laneIdx).position,
                                       Quaternion.identity,
                                       obstaclesRoot.transform);
            var tween = obstacle.AddComponent<AppearTween>();
            tween.duration = appearanceDuration;
        }
    }
    
    // ScaleIn tween
    

    private void OnDrawGizmos() {
        foreach (Transform spawnpoint in spawnpoints.transform) {
            Gizmos.DrawSphere(spawnpoint.position, .2f);
        }
    }
}
