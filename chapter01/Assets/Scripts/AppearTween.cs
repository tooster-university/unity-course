using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppearTween : MonoBehaviour {
    // Start is called before the first frame update
    public float duration = 1f;
    void         Start() { StartCoroutine(ScaleIn()); }

    private IEnumerator ScaleIn() {
        var scale = transform.localScale;
        var t = 0f;
        while (t < duration) {
            t += Time.deltaTime;
            transform.localScale = scale * (t / duration);
            yield return null;
        }

        transform.localScale = Vector3.one;
    }
}
