using System;
using UnityEngine;
using UnityEngine.Events;

// Use layer collisions
[RequireComponent(typeof(Collider))]
public class Obstacle : MonoBehaviour {
    [SerializeField] private new Collider       collider       = null;
    [SerializeField] private     GameObject     model          = null;
    [SerializeField] private new ParticleSystem particleSystem = null;

    [SerializeField] protected Action<Collider> onCollide = null;

    private void OnTriggerEnter(Collider other) {
        onCollide?.Invoke(other);
        collider.enabled = false;
        if (model) model.SetActive(false);
        if (particleSystem) particleSystem.Play();
    }
}
