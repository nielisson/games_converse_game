
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SquashAndStretch : MonoBehaviour
{
    [SerializeField] private float squashAmount = 0.8f;
    [SerializeField] private float stretchAmount = 1.2f;

    private Vector3 initialScale;
    private bool isSquashing = false;
    private bool isStretching = false;

    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        if (!isSquashing && !isStretching && Input.GetMouseButtonDown(0) && IsMouseOver())
        {
            Debug.Log("Update called");
            StartCoroutine(SquashAndStretchCoroutine());
        }
    }

    IEnumerator SquashAndStretchCoroutine()
    {
        float duration = 0.1f;
        float elapsedTime = 0f;

        isSquashing = true;

        while (elapsedTime < duration)
        {
            transform.localScale = Vector3.Lerp(initialScale, initialScale * squashAmount, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;

        isSquashing = false;
        isStretching = true;

        while (elapsedTime < duration)
        {
            transform.localScale = Vector3.Lerp(initialScale * squashAmount, initialScale * stretchAmount, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;

        isStretching = false;

        while (elapsedTime < duration)
        {
            transform.localScale = Vector3.Lerp(initialScale * stretchAmount, initialScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = initialScale;
    }

    private bool IsMouseOver()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 localMousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out localMousePosition);
        return rectTransform.rect.Contains(localMousePosition);
    }
}
