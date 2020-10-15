using System;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapController : MonoBehaviour {
    public float      scrollSpeed = 1f;
    public float      spawnDelay  = 3f;
    public int        difficulty  = 1;
    public GameObject obstacles;


    [SerializeField] private TextMeshProUGUI scoreText       = null;
    [SerializeField] private BoxCollider     mapBounds       = null;
    [SerializeField] private GameObject      spawnpoints     = null;
    [SerializeField] private GameObject      ground          = null;
    [SerializeField] private GameObject[]    obstaclePrefabs = null;


    private Material _groundMaterial;
    private float    _spawnTimer;

    private static readonly Color NORMAL_COLOR = new Color(1, 1, 1);
    private static readonly Color GOLD_COLOR   = new Color(0xFF / 255f, 0xA9 / 255f, 0x00 / 255f);

    private float _distanceTravelled = 0f;
    private int   _highscore         = 0;

    private static readonly int     MAIN_TEX      = Shader.PropertyToID("_MainTex");
    private static readonly Vector3 MOTION_VECTOR = Vector3.back;

    
    void OnEnable() {
        scoreText.color = NORMAL_COLOR;

        if (_groundMaterial == null) _groundMaterial = ground.GetComponent<Renderer>().material;
        
        _distanceTravelled = 0f;
        _spawnTimer        = spawnDelay; // to make it spawn in first frame

        foreach (Transform child in obstacles.transform) // cleanup
            Destroy(child.gameObject);        
    }

    void updateScore() {
        var score = (int) _distanceTravelled;
        scoreText.text = score.ToString();
        if ((int) _distanceTravelled > _highscore) {
            _highscore      = score;
            scoreText.color = GOLD_COLOR;
        }
    }

    void Update() {
        spawnBarrels();
        
        scrollMap();

        updateScore();

    }

    private void scrollMap() {
        _distanceTravelled += Time.deltaTime * scrollSpeed;
        _groundMaterial.SetTextureOffset(MAIN_TEX, new Vector2(0, _distanceTravelled));

        // move obstacles and destroy if needed
        foreach (Transform obstacle in obstacles.transform) {
            obstacle.position += MOTION_VECTOR * (scrollSpeed * Time.deltaTime);

            if (!mapBounds.bounds.Contains(obstacle.position))
                Destroy(obstacle.gameObject);
        }
    }

    private void spawnBarrels() {
        _spawnTimer += Time.deltaTime;
        if (_spawnTimer < spawnDelay)
            return;

        _spawnTimer -= spawnDelay;

        var childCount = spawnpoints.transform.childCount;

        // roll maximum number of barrels scaled with difficulty
        int obstaclesToSpawn = Random.Range(1, Math.Min(difficulty, childCount - 1));
        if (difficulty > 1)
            obstaclesToSpawn = Math.Max(obstaclesToSpawn, Random.Range(1, Math.Min(difficulty, childCount - 1)));
        if (difficulty > 2)
            obstaclesToSpawn = Math.Max(obstaclesToSpawn, Random.Range(1, Math.Min(difficulty, childCount - 1)));


        // for random indexes in spawnpoints spawn obstacles
        foreach (var idx in Enumerable.Range(0, childCount).OrderBy(x => Guid.NewGuid()).Take(obstaclesToSpawn)) {
            var prefabIdx = Random.Range(0, obstaclePrefabs.Length);
            var obstacle = Instantiate(obstaclePrefabs[prefabIdx],
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
