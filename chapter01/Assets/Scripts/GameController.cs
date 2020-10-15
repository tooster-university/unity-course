using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour {
    public MapController    mapController;
    public PlayerController playerController;
    

    public static GameController Instance { get; private set; }

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Debug.Log("Warning: multiple " + this + " in scene!");
        }
    }

    // Start is called before the first frame update
    void Start() {
        playerController.PlayerDied += HandlePlayerDied;
    }

    private void HandlePlayerDied(PlayerController playercontroller) {
        
    }


    public void PlayerDied() { mapController.scrollSpeed = 0; }
}
