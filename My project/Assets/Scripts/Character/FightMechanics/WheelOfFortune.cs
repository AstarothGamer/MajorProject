using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[ExecuteAlways]
public class WheelOfFortune : MonoBehaviour
{
    [Serializable]
    public class WheelSegment
    {
        public string label = "Item";
        public Color color = Color.white;

        [Min(0f)]
        public float weight = 1f; // шанс (1 = обычный). Можно делать разные вероятности.
    }

    [Serializable]
    public class SpinResultEvent : UnityEvent<int, string> { }

    [Header("References")]
    [SerializeField] private RectTransform wheelRoot; // то, что вращаем
    [SerializeField] private Image slicePrefab;       // Image (Filled Radial360)
    [SerializeField] private TMP_Text labelPrefab;    // TextMeshProUGUI (опционально)

    [Header("Layout")]
    [SerializeField] private float labelRadius = 140f;
    [SerializeField] private bool keepLabelsUpright = true; // чтобы текст не крутился вверх ногами

    [Header("Spin")]
    [SerializeField] private int minFullTurns = 4;
    [SerializeField] private int maxFullTurns = 8;
    [SerializeField] private float spinDurationMin = 2.8f;
    [SerializeField] private float spinDurationMax = 4.5f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Segments (editable)")]
    [SerializeField] private List<WheelSegment> segments = new List<WheelSegment>(8);

    [Header("Events")]
    public SpinResultEvent OnSpinFinished;

    private readonly List<GameObject> spawned = new();
    public bool IsSpinning { get; private set; }

    void Start()
    {
        Spin();
    }

    private void Reset()
    {
        // База на 8 секций
        segments = new List<WheelSegment>
        {
            new WheelSegment{ label="1", color=new Color(0.95f,0.35f,0.35f), weight=1 },
            new WheelSegment{ label="2", color=new Color(0.35f,0.65f,0.95f), weight=1 },
            new WheelSegment{ label="3", color=new Color(0.45f,0.90f,0.55f), weight=1 },
            new WheelSegment{ label="4", color=new Color(0.95f,0.85f,0.35f), weight=1 },
            new WheelSegment{ label="5", color=new Color(0.75f,0.45f,0.95f), weight=1 },
            new WheelSegment{ label="6", color=new Color(0.35f,0.90f,0.85f), weight=1 },
            new WheelSegment{ label="7", color=new Color(0.95f,0.55f,0.75f), weight=1 },
            new WheelSegment{ label="8", color=new Color(0.65f,0.65f,0.65f), weight=1 },
        };

        if (wheelRoot == null) wheelRoot = GetComponent<RectTransform>();
    }

    private void OnEnable() => RebuildWheel();

    private void OnValidate()
    {
        if (!isActiveAndEnabled) return;
        RebuildWheel();
    }

    // ===== Public API: добавлять/удалять секции =====

    public IReadOnlyList<WheelSegment> Segments => segments;

    public void AddSegment(string label, Color color, float weight = 1f)
    {
        segments.Add(new WheelSegment { label = label, color = color, weight = Mathf.Max(0f, weight) });
        RebuildWheel();
    }

    public void RemoveSegmentAt(int index)
    {
        if (index < 0 || index >= segments.Count) return;
        segments.RemoveAt(index);
        RebuildWheel();
    }

    public bool RemoveSegmentByLabel(string label)
    {
        int idx = segments.FindIndex(s => string.Equals(s.label, label, StringComparison.OrdinalIgnoreCase));
        if (idx < 0) return false;
        segments.RemoveAt(idx);
        RebuildWheel();
        return true;
    }

    public void ClearSegments()
    {
        segments.Clear();
        RebuildWheel();
    }

    // ===== Wheel build =====

    public void RebuildWheel()
    {
        if (wheelRoot == null || slicePrefab == null) return;

        // Минимум 2 секции, иначе вращать бессмысленно
        int n = Mathf.Max(segments.Count, 0);

        CleanupSpawned();

        if (n < 2) return;

        float sliceAngle = 360f / n;

        for (int i = 0; i < n; i++)
        {
            // --- Slice ---
            var slice = InstantiateSafe(slicePrefab, wheelRoot);
            slice.rectTransform.localPosition = new Vector3(0,0,0);
            slice.name = $"Slice_{i}_{segments[i].label}";
            slice.fillAmount = 1f / n;
            slice.color = segments[i].color;

            // Важно: prefab должен быть Filled/Radial360/Top/Clockwise
            // Поворачиваем каждый сектор на нужный угол
            slice.rectTransform.localRotation = Quaternion.Euler(0, 0, -i * sliceAngle);

            spawned.Add(slice.gameObject);

            // --- Label (optional) ---
            if (labelPrefab != null)
            {
                float centerAngleClockwise = i * sliceAngle + sliceAngle * 0.5f;
                float rad = centerAngleClockwise * Mathf.Deg2Rad;

                // 0° = верх (0, +R), по часовой:
                float x = Mathf.Sin(rad) * labelRadius;
                float y = Mathf.Cos(rad) * labelRadius;

                var label = InstantiateSafe(labelPrefab, wheelRoot);
                label.name = $"Label_{i}_{segments[i].label}";
                label.text = segments[i].label;

                var rt = (RectTransform)label.transform;
                rt.anchoredPosition = new Vector2(x, y);

                // Разворот текста "по радиусу"
                rt.localRotation = Quaternion.Euler(0, 0, -centerAngleClockwise);

                spawned.Add(label.gameObject);
            }
        }
    }

    private void CleanupSpawned()
    {
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i] == null) continue;

            if (Application.isPlaying)
                Destroy(spawned[i]);
            else
                DestroyImmediate(spawned[i]);
        }
        spawned.Clear();
    }

    private T InstantiateSafe<T>(T prefab, RectTransform parent) where T : Component
    {
        if (Application.isPlaying)
            return Instantiate(prefab, parent);
        else
        {
            // чтобы работало и в Edit Mode
            var obj = (T)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, parent);
            return obj;
        }
    }

    // ===== Spin =====

    public void Spin()
    {
        if (IsSpinning) return;
        if (wheelRoot == null) return;
        if (segments == null || segments.Count < 2) return;

        int winnerIndex = PickWeightedIndex();
        StartCoroutine(SpinToIndexRoutine(winnerIndex));
    }

    public void SpinToIndex(int index)
    {
        if (IsSpinning) return;
        if (segments == null || segments.Count < 2) return;
        index = Mathf.Clamp(index, 0, segments.Count - 1);
        StartCoroutine(SpinToIndexRoutine(index));
    }

    private IEnumerator SpinToIndexRoutine(int index)
    {
        IsSpinning = true;

        int n = segments.Count;
        float sliceAngle = 360f / n;

        // Центр сектора (в градусах, по часовой от "верх")
        float centerAngleClockwise = index * sliceAngle + sliceAngle * 0.5f;

        // Текущий угол колеса (0..360)
        float startZ = wheelRoot.eulerAngles.z;

        // Хотим прийти к такому углу, чтобы centerAngleClockwise оказался наверху.
        // В Unity +Z = против часовой, а мы считаем clockwise, поэтому используем centerAngleClockwise как "целевой Z"
        // и добираем по кругу в одном направлении.
        float targetZModulo = centerAngleClockwise;

        // Сколько докрутить (только вперед, 0..360)
        float deltaForward = Repeat360(targetZModulo - startZ);

        int turns = UnityEngine.Random.Range(minFullTurns, maxFullTurns + 1);
        float endZ = startZ + turns * 360f + deltaForward;

        float duration = UnityEngine.Random.Range(spinDurationMin, spinDurationMax);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.001f, duration);
            float k = ease.Evaluate(Mathf.Clamp01(t));
            float z = Mathf.LerpUnclamped(startZ, endZ, k);

            wheelRoot.rotation = Quaternion.Euler(0, 0, z);

            // если хотим, чтобы текст был всегда "ровный"
            if (keepLabelsUpright && labelPrefab != null)
            {
                // разворачиваем все labels обратно
                for (int i = 0; i < spawned.Count; i++)
                {
                    if (spawned[i] == null) continue;
                    if (!spawned[i].name.StartsWith("Label_")) continue;

                    var rt = spawned[i].GetComponent<RectTransform>();
                    if (rt == null) continue;

                    // "убираем" вращение колеса
                    rt.rotation = Quaternion.identity;
                }
            }

            yield return null;
        }

        // Фиксируем финал ровно
        wheelRoot.rotation = Quaternion.Euler(0, 0, endZ);

        IsSpinning = false;

        OnSpinFinished?.Invoke(index, segments[index].label);
    }

    private int PickWeightedIndex()
    {
        float total = 0f;
        for (int i = 0; i < segments.Count; i++)
            total += Mathf.Max(0f, segments[i].weight);

        // Если все веса нулевые — выбираем равновероятно
        if (total <= 0f)
            return UnityEngine.Random.Range(0, segments.Count);

        float r = UnityEngine.Random.value * total;
        float acc = 0f;

        for (int i = 0; i < segments.Count; i++)
        {
            acc += Mathf.Max(0f, segments[i].weight);
            if (r <= acc) return i;
        }

        return segments.Count - 1;
    }

    private static float Repeat360(float x)
    {
        x %= 360f;
        if (x < 0f) x += 360f;
        return x;
    }
}

