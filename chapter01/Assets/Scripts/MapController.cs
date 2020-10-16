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
    [SerializeField] private Obstacle[]      obstaclePrefabs = null;

    [NonSerialized] public float scrollSpeed      = 0f;
    [NonSerialized] public float obstacleDistance = 3f;
    [NonSerialized] public int   difficulty       = 1;

    public float DistanceTravelled { get; private set; }

    private Material _groundMaterial    = null;
    private float    _lastSpawnDistance = 0f;
    private int      _highscore         = 0;


    private static readonly Vector3 MOTION_VECTOR = Vector3.back;
    private static readonly int     MAIN_TEX      = Shader.PropertyToID("_MainTex");
    private static readonly Color   NORMAL_COLOR  = new Color(1, 1, 1);
    private static readonly Color   GOLD_COLOR    = new Color(0xFF / 255f, 0xA9 / 255f, 0x00 / 255f);

    public void reset() {
        DistanceTravelled  = 0f;
        _lastSpawnDistance = -obstacleDistance;          // to make it spawn in first frame
        foreach (Transform child in obstacles.transform) // cleanup
            Destroy(child.gameObject);
        scoreText.color = NORMAL_COLOR;
    }

    void Start() {
        _groundMaterial = ground.GetComponent<Renderer>().material;

        reset();
    }

    void Update() {
        spawnBarrels();

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
            obstacle.position += MOTION_VECTOR * (scrollSpeed * Time.deltaTime);

            if (!mapBounds.bounds.Contains(obstacle.position))
                Destroy(obstacle.gameObject);
        }
    }

    private void spawnBarrels() {
        if (DistanceTravelled - _lastSpawnDistance < obstacleDistance)
            return;

        _lastSpawnDistance = DistanceTravelled;

        var childCount = spawnpoints.transform.childCount;

        // roll maximum number of barrels scaled with difficulty
        int obstaclesToSpawn = Random.Range(0, Math.Min(difficulty, childCount - 1));
        if (difficulty > 1)
            obstaclesToSpawn = Math.Max(obstaclesToSpawn, Random.Range(0, Math.Min(difficulty, childCount - 1)));
        if (difficulty > 2)
            obstaclesToSpawn = Math.Max(obstaclesToSpawn, Random.Range(0, Math.Min(difficulty, childCount - 1)));


        // for random indexes in spawnpoints spawn obstacles
        foreach (var idx in Enumerable.Range(0, childCount).OrderBy(x => Guid.NewGuid()).Take(obstaclesToSpawn)) {
            var prefabIdx = Random.Range(0, obstaclePrefabs.Length);
            var obstacle = Instantiate(obstaclePrefabs[prefabIdx].gameObject,
                                       spawnpoints.transform.GetChild(idx).position,
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
