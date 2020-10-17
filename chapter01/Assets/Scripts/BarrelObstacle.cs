using UnityEngine;

public class BarrelObstacle : Obstacle {
    [SerializeField] private int damage = 1;

    protected override void onCollide(Collider other) => other.GetComponent<PlayerController>().takeDamage(damage);
}
