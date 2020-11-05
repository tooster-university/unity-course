using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using System.Linq;
using UnityEditor;

public class GameManager : MonoBehaviour {
    #region singleton

    public static GameManager Instance { get; private set; }

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(this);
        }
    }

    #endregion singleton

    public int tiles;

    [SerializeField] private Sprite[]   sprites;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Transform  tilesParent;
    [SerializeField] private Vector2    distanceBetweenTiles;

    public GameObject menu;
    public Timer      timer;

    [NonSerialized] public Timer bestTimer;

    private Tile _selectedTile = null;


    public bool CanFlip { get; private set; }


    private void Start() {
        CanFlip     = true;
        menu        = GameObject.FindWithTag("Menu");
        timer.Count = 0;
    }

    public void shuffle(int rows, int columns, Vector3 offset) {
        menu.SetActive(false);

        var shuffledIndices = Enumerable.Range(0, rows * columns)
                                        .Select(x => x % (rows * columns / 2))
                                        .OrderBy(_ => Guid.NewGuid());
        var indexEnumerator = shuffledIndices.GetEnumerator();

        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < columns; j++) {
                GameObject clone = Instantiate(tilePrefab, transform.position, Quaternion.identity, tilesParent);
                clone.transform.localPosition = offset +
                                                new Vector3(distanceBetweenTiles.x * j, 0f,
                                                            distanceBetweenTiles.y * i);
                ;
                indexEnumerator.MoveNext();
                clone.GetComponentInChildren<SpriteRenderer>().sprite = sprites[indexEnumerator.Current];
                clone.GetComponentInChildren<Tile>().index            = indexEnumerator.Current;
            }
        }

        timer.Reset();
        timer.running = true;
        indexEnumerator.Dispose();
    }

    public void selectTile(Tile tile) {
        if (_selectedTile == null) {
            _selectedTile = tile;
            return;
        }

        CanFlip = false;
        if (_selectedTile.index == tile.index)
            StartCoroutine(delay(0.2f, () => {
                tile.destroy();
                _selectedTile.destroy();
                _selectedTile =  null;
                if (tilesParent.childCount <= 0)
                    end();
            }));
        else {
            StartCoroutine(delay(1f, () => {
                tile.unflip();
                _selectedTile.unflip();
                _selectedTile = null;
            }));
        }
    }

    private IEnumerator delay(float delay, Action action) {
        yield return new WaitForSeconds(delay);

        CanFlip = true;
        action.Invoke();
    }

    public void end() {
        menu.SetActive(true);
        timer.running   = false;
        if(bestTimer.Count == 0 || timer.Count < bestTimer.Count) 
            bestTimer.Count = timer.Count;
    }
}
