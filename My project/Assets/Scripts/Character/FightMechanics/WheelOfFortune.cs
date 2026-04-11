using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class WheelOfFortune : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform wheelRoot; // то, что вращаем    

    [Header("Spin")]
    [SerializeField] private int minFullTurns = 4;
    [SerializeField] private int maxFullTurns = 8;
    [SerializeField] private float spinDurationMin = 2.8f;
    [SerializeField] private float spinDurationMax = 4.5f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Events")]
    public UnityEvent<int> OnSpinFinished;
    
    [SerializeField] private List<float> weights = new List<float>()
    {
        1,1,1,1,1,1,1,1,1,1
    };
    [SerializeField] private int segmentCount = 10;

    public bool IsSpinning { get; private set; }
    
    private void OnValidate()
    {
        if (weights.Count != segmentCount)
        {
            weights = new List<float>();

            for (int i = 0; i < segmentCount; i++)
                weights.Add(1f);
        }
    }

    public void Spin()
    {
        if (IsSpinning) return;

        int winnerIndex = PickWeightedIndex();
        StartCoroutine(SpinToIndexRoutine(winnerIndex));
    }

    public void SpinToIndex(int index)
    {
        if (IsSpinning) return;
        if (segmentCount < 2) return;

        index = Mathf.Clamp(index, 0, segmentCount - 1);

        StartCoroutine(SpinToIndexRoutine(index));
    }

    private IEnumerator SpinToIndexRoutine(int index)
    {
        IsSpinning = true;
        
        int n = segmentCount;
        float sliceAngle = 360f / n;
        
        float centerAngleClockwise = index * sliceAngle + sliceAngle * 0.5f;

        float startZ = wheelRoot.eulerAngles.z;
        
        float targetZModulo = centerAngleClockwise;

        float deltaForward = Repeat360(targetZModulo - startZ);

        int turns = Random.Range(minFullTurns, maxFullTurns + 1);
        float endZ = startZ + turns * 360f + deltaForward;

        float duration = Random.Range(spinDurationMin, spinDurationMax);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.001f, duration);
            float k = ease.Evaluate(Mathf.Clamp01(t));
            float z = Mathf.LerpUnclamped(startZ, endZ, k);

            wheelRoot.rotation = Quaternion.Euler(0, 0, z);

            yield return null;
        }

        wheelRoot.rotation = Quaternion.Euler(0, 0, endZ);

        IsSpinning = false;

        OnSpinFinished?.Invoke(index);
    }

    private int PickWeightedIndex()
    {
        if (weights == null || weights.Count == 0)
            return Random.Range(0, segmentCount);

        if (weights.Count != segmentCount)
        {
            Debug.LogWarning("Weights count != segment count!");
            return Random.Range(0, segmentCount);
        }

        float totalWeight = 0f;

        for (int i = 0; i < weights.Count; i++)
        {
            totalWeight += Mathf.Max(0f, weights[i]);
        }

        if (totalWeight <= 0f)
            return Random.Range(0, segmentCount);

        float random = Random.value * totalWeight;

        float cumulative = 0f;

        for (int i = 0; i < weights.Count; i++)
        {
            cumulative += Mathf.Max(0f, weights[i]);

            if (random <= cumulative)
                return i;
        }

        return segmentCount - 1;
    }

    private static float Repeat360(float x)
    {
        x %= 360f;
        if (x < 0f) x += 360f;
        return x;
    }
}

