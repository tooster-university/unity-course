using UnityEngine;

public class Tile : MonoBehaviour {
    public int        index;
    public GameObject parent;

    [SerializeField] private GameObject particles;

    private Animator _animator;
    private bool     _flipped;

    private static readonly int SHOWN = Animator.StringToHash("shown");

    void Start() { _animator = GetComponent<Animator>(); }

    private void OnMouseDown() {
        if (!GameManager.Instance.CanFlip || _flipped) return;

        _flipped = true;

        _animator.SetBool(SHOWN, true);

        GameManager.Instance.selectTile(this);
    }

    public void destroy() {
        Instantiate(particles, transform.position, Quaternion.identity);
        DestroyImmediate(parent);
    }

    public void unflip() {
        _animator.SetBool(SHOWN, false);
        _flipped = false;
    }
}
