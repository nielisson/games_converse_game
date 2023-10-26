using UnityEngine;

public class StretchAndSquash : MonoBehaviour
{
    public Vector3 stretchAmount = new Vector3(0.2f, -0.2f, 0f);
    public float duration = 0.1f;

    private Vector3 originalScale;
    private bool isClicked = false;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void OnMouseDown()
    {
        if (!isClicked)
        {
            // Start the stretch and squash animation
            StartCoroutine(AnimateStretchAndSquash());
            isClicked = true;
        }
    }

    private System.Collections.IEnumerator AnimateStretchAndSquash()
    {
        Vector3 targetScale = originalScale + stretchAmount;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        // Ensure the scale returns to the original size
        transform.localScale = originalScale;
        isClicked = false;
    }
}