using UnityEngine;

public class BarrelObstacle : Obstacle {
    [SerializeField] private int damage = 1;

    public BarrelObstacle() { onCollide = other => { other.GetComponent<PlayerController>().takeDamage(damage); }; }
}
