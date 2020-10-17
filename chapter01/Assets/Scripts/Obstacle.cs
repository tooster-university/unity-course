using System;
using UnityEngine;

// Use layer collisions
[RequireComponent(typeof(Rigidbody))]
public class Obstacle : MonoBehaviour {
    [SerializeField] private     GameObject     model          = null;
    [SerializeField] private new ParticleSystem particleSystem = null;

    protected virtual void onCollide(Collider other) { }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            onCollide(other);
            if (model) model.SetActive(false);
            if (particleSystem) particleSystem.Play();
        }
    }
}
