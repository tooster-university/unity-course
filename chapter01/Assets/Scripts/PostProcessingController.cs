using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessingController : MonoBehaviour {
    [SerializeField] private float vignetteTime = 2.0f;

    public  PostProcessVolume  volume;
    private Vignette           _vignette;
    private Color              vigneteOriginalColor;

    // Start is called before the first frame update
    void Start() {
        if(volume.profile == null);
        volume.sharedProfile.TryGetSettings(out _vignette);
        vigneteOriginalColor = _vignette.color.value;
    }

    public void VignetteBurst(Color color) { StartCoroutine(_vignetteBurst(color)); }

    private IEnumerator _vignetteBurst(Color color) {
        float timer = vignetteTime;
        while (timer > 0.0f) 
        {
            _vignette.color.Interp(vigneteOriginalColor, color, timer / vignetteTime);
            yield return null;
            timer -= Time.fixedDeltaTime;
        }

        _vignette.color.value = vigneteOriginalColor;
    }
}
