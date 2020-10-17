using UnityEngine;

public class LifePickup : Obstacle {
    [SerializeField] private int HP = 1;

    protected override void onCollide(Collider other) => other.GetComponent<PlayerController>().takeDamage(-HP);
}
