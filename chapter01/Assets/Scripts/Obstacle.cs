using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour {
    [SerializeField] private Collider       collider;
    [SerializeField] private GameObject     model;
    [SerializeField] private ParticleSystem particleSystem;
    [SerializeField] private AudioSource    sound;

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Player")) {
            other.gameObject.GetComponent<PlayerController>().takeDamage(1);
            collider.enabled = false;
            model.SetActive(false);
            particleSystem.Play();
            sound.Play();
        }
    }
}
