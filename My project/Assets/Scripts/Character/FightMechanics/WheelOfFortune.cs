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
    [SerializeField] private RectTransform wheelRoot;   
    [SerializeField] private TMP_Text labelPrefab;
    [SerializeField] private GameObject wheel;
    [SerializeField] private GameObject wheelHolder;
    private TurnManager turnManager;
    
    [Header("Labels")]
    [SerializeField] private float labelRadius = 140f;

    [Header("Spin")] 
    [SerializeField] private float secondsTillHideWheel;
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
    
    [SerializeField] private List<int> segmentValueIndices = new List<int>()
    {
        0,0,0,0,
        1,1,1,
        2,2,
        3
    };
    [SerializeField] private List<string> valueTexts = new List<string>()
    {
        "0", "1", "2", "3"
    };
    [SerializeField] private bool shuffleOnStart = true;

    public bool IsSpinning { get; private set; }
    private readonly List<TMP_Text> spawnedLabels = new List<TMP_Text>();

    void Awake()
    {
        ValidateData();

        if (shuffleOnStart)
            ShuffleSegments();
        
        turnManager = FindFirstObjectByType<TurnManager>();
        RebuildLabels();
    }

    private void ShowWheel()
    {
        wheelHolder.SetActive(true);
        wheel.SetActive(true);
    }

    private void HideWheel()
    {
        if (turnManager != null)
            turnManager.EndAction();
        
        wheel.SetActive(false);
        wheelHolder.SetActive(false);
    }
    
    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(secondsTillHideWheel);
        HideWheel();
    }

    private void OnValidate()
    {
        ValidateData();
    }
    
    private void ValidateData()
    {
        if (segmentCount < 2)
            segmentCount = 2;

        if (weights == null)
            weights = new List<float>();

        if (weights.Count != segmentCount)
        {
            weights.Clear();
            for (int i = 0; i < segmentCount; i++)
                weights.Add(1f);
        }

        if (segmentValueIndices == null)
            segmentValueIndices = new List<int>();

        if (segmentValueIndices.Count != segmentCount)
        {
            segmentValueIndices = new List<int>()
            {
                0,0,0,0,
                1,1,1,
                2,2,
                3
            };

            while (segmentValueIndices.Count < segmentCount)
                segmentValueIndices.Add(0);

            if (segmentValueIndices.Count > segmentCount)
                segmentValueIndices.RemoveRange(segmentCount, segmentValueIndices.Count - segmentCount);
        }

        if (valueTexts == null || valueTexts.Count == 0)
        {
            valueTexts = new List<string>() { "0", "1", "2", "3" };
        }
    }
    
    public void ShuffleSegments()
    {
        for (int i = 0; i < segmentValueIndices.Count; i++)
        {
            int rand = Random.Range(i, segmentValueIndices.Count);

            int temp = segmentValueIndices[i];
            segmentValueIndices[i] = segmentValueIndices[rand];
            segmentValueIndices[rand] = temp;
        }
    }
    
    public void RebuildLabels()
    {
        ClearLabels();

        if (wheelRoot == null || labelPrefab == null)
            return;

        float sliceAngle = 360f / segmentCount;

        for (int i = 0; i < segmentCount; i++)
        {
            TMP_Text label = Instantiate(labelPrefab, wheelRoot);
            spawnedLabels.Add(label);

            int valueIndex = segmentValueIndices[i];
            string labelText = GetLabelTextForValueIndex(valueIndex);

            label.text = labelText;
            label.name = $"Label_{i}_{labelText}";

            RectTransform rt = label.rectTransform;

            float centerAngleClockwise = i * sliceAngle + sliceAngle * 0.5f;
            float rad = centerAngleClockwise * Mathf.Deg2Rad;

            float x = Mathf.Sin(rad) * labelRadius;
            float y = Mathf.Cos(rad) * labelRadius;

            rt.anchoredPosition = new Vector2(x, y);
            rt.localRotation = Quaternion.identity;
        }
    }
    
    private string GetLabelTextForValueIndex(int valueIndex)
    {
        if (valueIndex < 0)
            valueIndex = 0;

        if (valueIndex >= valueTexts.Count)
            valueIndex = valueTexts.Count - 1;

        return valueTexts[valueIndex];
    }
    
    private void ClearLabels()
    {
        for (int i = spawnedLabels.Count - 1; i >= 0; i--)
        {
            if (spawnedLabels[i] != null)
                Destroy(spawnedLabels[i].gameObject);
        }

        spawnedLabels.Clear();
    }

    public void Spin()
    {
        if (IsSpinning) return;
    
        ShowWheel();
        
        int visualSegmentIndex = PickWeightedIndex();
        StartCoroutine(SpinToIndexRoutine(visualSegmentIndex));
    }
    
    public void SpinToIndex(int visualSegmentIndex)
    {
        if (IsSpinning) return;
        if (segmentCount < 2) return;

        visualSegmentIndex = Mathf.Clamp(visualSegmentIndex, 0, segmentCount - 1);
        StartCoroutine(SpinToIndexRoutine(visualSegmentIndex));
    }

    private IEnumerator SpinToIndexRoutine(int visualSegmentIndex)
    {
        IsSpinning = true;
        
        float sliceAngle = 360f / segmentCount;
        
        float centerAngleClockwise = visualSegmentIndex * sliceAngle + sliceAngle * 0.5f;

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

        int effectIndex = segmentValueIndices[visualSegmentIndex];
        OnSpinFinished?.Invoke(effectIndex);
        StartCoroutine(HideAfterDelay());
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

