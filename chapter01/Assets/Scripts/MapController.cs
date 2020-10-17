using System;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapController : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI scoreText       = null;
    [SerializeField] private BoxCollider     mapBounds       = null;
    [SerializeField] private GameObject      spawnpoints     = null;
    [SerializeField] private GameObject      ground          = null;
    [SerializeField] private GameObject      obstacles       = null;
    [SerializeField] private LifePickup      lifePrefab      = null;
    [SerializeField] private float           lifeInterval    = 500f;
    [SerializeField] private Obstacle[]      obstaclePrefabs = null;

    [NonSerialized] public float scrollSpeed      = 0f;
    [NonSerialized] public float obstacleDistance = 3f;
    [NonSerialized] public int   difficulty       = 1;

    public float DistanceTravelled { get; private set; }

    private Material _groundMaterial        = null;
    private float    _lastSpawnDistance     = 0f;
    private float    _lastLifeSpawnDistance = 0f;
    private int      _highscore             = 0;


    private static readonly Vector3 MOTION_VECTOR = Vector3.back;
    private static readonly int     MAIN_TEX      = Shader.PropertyToID("_MainTex");
    private static readonly Color   NORMAL_COLOR  = new Color(1, 1, 1);
    private static readonly Color   GOLD_COLOR    = new Color(0xFF / 255f, 0xA9 / 255f, 0x00 / 255f);

    public void reset() {
        DistanceTravelled      = 0f;
        _lastSpawnDistance     = -obstacleDistance; // to make it spawn in first frame
        _lastLifeSpawnDistance = 0f;
        foreach (Transform child in obstacles.transform) // cleanup
            Destroy(child.gameObject);
        scoreText.color = NORMAL_COLOR;
    }

    void Start() {
        _groundMaterial = ground.GetComponent<Renderer>().material;

        reset();
    }

    void Update() {
        spawnNextLayer();

        scrollMap();

        updateScore();
    }

    private void updateScore() {
        var score = (int) DistanceTravelled;
        scoreText.text = score.ToString();
        if ((int) DistanceTravelled > _highscore) {
            _highscore      = score;
            scoreText.color = GOLD_COLOR;
        }
    }

    private void scrollMap() {
        DistanceTravelled += Time.deltaTime * scrollSpeed;
        _groundMaterial.SetTextureOffset(MAIN_TEX, new Vector2(0, DistanceTravelled));

        // move obstacles and destroy if needed
        foreach (Transform obstacle in obstacles.transform) {
            obstacle.GetComponent<Rigidbody>().MovePosition(obstacle.transform.position + MOTION_VECTOR * (scrollSpeed * Time.deltaTime));
            // obstacle.transform.position += MOTION_VECTOR * (scrollSpeed * Time.deltaTime);

            if (!mapBounds.bounds.Contains(obstacle.position))
                Destroy(obstacle.gameObject);
        }
    }

    private void spawnNextLayer() {
        if (DistanceTravelled - _lastSpawnDistance < obstacleDistance)
            return;

        _lastSpawnDistance = DistanceTravelled;

        var lanes = spawnpoints.transform.childCount;

        // roll maximum number of barrels scaled with difficulty
        int obstaclesToSpawn = Random.Range(1, lanes);
        if (difficulty > 1)
            obstaclesToSpawn = Random.Range(obstaclesToSpawn, difficulty);
        if (difficulty > 2)
            obstaclesToSpawn = Random.Range(obstaclesToSpawn, difficulty);

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
                                       obstacles.transform);
        }
    }

    private void OnDrawGizmos() {
        foreach (Transform spawnpoint in spawnpoints.transform) {
            Gizmos.DrawSphere(spawnpoint.position, .2f);
        }
    }
}
