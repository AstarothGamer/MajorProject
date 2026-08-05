using System.Collections;
using UnityEngine;

public class DamageShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float duration = 0.25f;
    [SerializeField] private float strength = 0.18f;
    [SerializeField] private int vibrations = 12;

    private Vector3 originalLocalPosition;
    private Coroutine shakeRoutine;

    private void Awake()
    {
        if (target == null)
            target = transform;

        originalLocalPosition = target.localPosition;
    }

    public void Shake()
    {
        if (target == null)
            return;

        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            target.localPosition = originalLocalPosition;
        }

        shakeRoutine = StartCoroutine(ShakeRoutine(strength, duration));
    }

    public void Shake(float customStrength)
    {
        if (target == null)
            return;

        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            target.localPosition = originalLocalPosition;
        }

        shakeRoutine = StartCoroutine(ShakeRoutine(customStrength, duration));
    }

    private IEnumerator ShakeRoutine(float shakeStrength, float shakeDuration)
    {
        float timer = 0f;

        while (timer < shakeDuration)
        {
            timer += Time.deltaTime;

            float progress = timer / shakeDuration;
            float fade = 1f - progress;

            float x = Random.Range(-1f, 1f) * shakeStrength * fade;
            float y = Random.Range(-1f, 1f) * shakeStrength * fade;

            target.localPosition = originalLocalPosition + new Vector3(x, y, 0f);

            yield return null;
        }

        target.localPosition = originalLocalPosition;
        shakeRoutine = null;
    }
}
