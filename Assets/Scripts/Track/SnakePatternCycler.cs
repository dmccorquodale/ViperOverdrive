using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SnakePatternPreset
{
    public string name;
    public float speed;
    public float lateralAmp;
    public float lateralWavelengthM;
    public float verticalAmp;
    public float verticalWavelengthM;
}

public class SnakePatternCycler : MonoBehaviour
{
    [Header("Target")]
    public SnakeHeadController target;

    private SnakePatternPreset starterPattern = new SnakePatternPreset { name = "Starter", speed = 1f, lateralAmp = 1f, lateralWavelengthM = 77f, verticalAmp = 1f, verticalWavelengthM = 162f };


    [Header("Presets (ordered)")]
    List<SnakePatternPreset> patterns = new()
    {
        new SnakePatternPreset{ name="C", speed=1f, lateralAmp=20f, lateralWavelengthM=77f, verticalAmp=2f, verticalWavelengthM=162f },
        new SnakePatternPreset{ name="B", speed=1f, lateralAmp=10f, lateralWavelengthM=77f, verticalAmp=5f, verticalWavelengthM=162f },
        new SnakePatternPreset{ name="A", speed=1f, lateralAmp=0f, lateralWavelengthM=77f, verticalAmp=8f, verticalWavelengthM=162f },
        //new SnakePatternPreset{ name="B", speed=15f, lateralAmp=8f, lateralWavelengthM=200f, verticalAmp=11f, verticalWavelengthM=300f },
        //new SnakePatternPreset{ name="C", speed=15f, lateralAmp=24f, lateralWavelengthM=200f, verticalAmp=10f, verticalWavelengthM=300f },
        //new SnakePatternPreset{ name="C", speed=15f, lateralAmp=48f, lateralWavelengthM=200f, verticalAmp=8f, verticalWavelengthM=300f },
        // Add a second preset to see cycling happen:
        // new SnakePatternPreset{ name="B", speed=16f, lateralAmp=16f, lateralWavelengthM=200f, verticalAmp=8f, verticalWavelengthM=300f },
    };

    [Header("Timing")]
    public float intervalSeconds = 5f;
    public float blendSeconds = 1.5f;

    [Header("Order")]
    public bool randomOrder = false;
    public int startIndex = 0;

    [Header("Playback")]
    public bool playOnStart = true;
    [Tooltip("If true and you only have one preset, it will still re-apply it every interval (mostly for testing).")]
    public bool loopSinglePreset = false;

    private bool firstIndex = true;

    int _index = 0;
    Coroutine _runner;

    void OnValidate()
    {
        if (target == null) target = GetComponent<SnakeHeadController>();
        if (intervalSeconds < 0.1f) intervalSeconds = 0.1f;
        if (blendSeconds < 0f) blendSeconds = 0f;
    }

    void Awake()
    {
        // Extra safety: if still null, try parent/children
        if (target == null) target = GetComponentInChildren<SnakeHeadController>();
        if (target == null) target = GetComponentInParent<SnakeHeadController>();
    }

    void Start()
    {
        if (!target || patterns.Count == 0) return;
        _index = Mathf.Clamp(startIndex, 0, patterns.Count - 1);

        // ✅ Apply initial preset once so values on SnakeHeadController update immediately
        // ApplyToTarget(patterns[_index]);
        ApplyToTarget(starterPattern);

        if (!playOnStart) return;

        // Start runner if we have multiple presets, or if we explicitly want to loop a single preset
        if (patterns.Count > 1 || loopSinglePreset)
            _runner = StartCoroutine(RunCycle());
    }

    public void Play()
    {
        if (_runner == null && target && (patterns.Count > 1 || loopSinglePreset))
            _runner = StartCoroutine(RunCycle());
    }

    public void Stop()
    {
        if (_runner != null)
        {
            StopCoroutine(_runner);
            _runner = null;
        }
    }

    public void NextImmediate()
    {
        int next = GetNextIndex();
        if (next == _index) return;
        _index = next;
        ApplyToTarget(patterns[_index]);
    }

    IEnumerator RunCycle()
    {
        var waitPostBlend = Mathf.Max(0f, intervalSeconds - blendSeconds);

        while (true)
        {
            int next = GetNextIndex();

            var from = patterns[_index];
            var to   = patterns[next];
            if (firstIndex)
            {
                from = starterPattern;
                // to   = starterPattern;
                firstIndex = false;
            }

            // If single preset & not looping single, just wait and continue
            if (next == _index && patterns.Count == 1 && !loopSinglePreset)
            {
                yield return new WaitForSeconds(intervalSeconds);
                continue;
            }

            if (blendSeconds > 0.0001f && !PresetsApproximatelyEqual(from, to))
            {
                float t = 0f;
                while (t < blendSeconds)
                {
                    float a = t / blendSeconds;
                    a = a * a * (3f - 2f * a); // smoothstep

                    ApplyToTarget(new SnakePatternPreset
                    {
                        speed               = Mathf.Lerp(from.speed,               to.speed,               a),
                        lateralAmp          = Mathf.Lerp(from.lateralAmp,          to.lateralAmp,          a),
                        lateralWavelengthM  = Mathf.Lerp(from.lateralWavelengthM,  to.lateralWavelengthM,  a),
                        verticalAmp         = Mathf.Lerp(from.verticalAmp,         to.verticalAmp,         a),
                        verticalWavelengthM = Mathf.Lerp(from.verticalWavelengthM, to.verticalWavelengthM, a),
                    });

                    t += Time.deltaTime;
                    yield return null;
                }
            }

            // Snap to exact 'to' values and commit
            ApplyToTarget(to);
            _index = next;

            if (waitPostBlend > 0f)
                yield return new WaitForSeconds(waitPostBlend);
            else
                yield return null;
        }
    }

    int GetNextIndex()
    {
        if (patterns.Count <= 1)
            return _index; // stays on same (unless loopSinglePreset is enabled)

        if (!randomOrder) return (_index + 1) % patterns.Count;

        int pick;
        do { pick = Random.Range(0, patterns.Count); }
        while (pick == _index && patterns.Count > 1);
        return pick;
    }

    void ApplyToTarget(SnakePatternPreset p)
    {
        // Debug.Log($"[Cycler] Applying preset: {p.name} " +
        //         $"speed={p.speed} latAmp={p.lateralAmp} latWave={p.lateralWavelengthM} " +
        //         $"vertAmp={p.verticalAmp} vertWave={p.verticalWavelengthM}", this);

        target.ApplyPattern(p.speed, p.lateralAmp, p.lateralWavelengthM, p.verticalAmp, p.verticalWavelengthM);

        // Debug.Log($"[Head] Now: " +
        //         $"speed={target.forwardSpeed} latAmp={target.lateralAmp} latWave={target.lateralWavelengthM} " +
        //         $"vertAmp={target.verticalAmp} vertWave={target.verticalWavelengthM}", target);
    }

    bool PresetsApproximatelyEqual(SnakePatternPreset a, SnakePatternPreset b, float eps = 1e-3f)
    {
        return Mathf.Abs(a.speed - b.speed) < eps
            && Mathf.Abs(a.lateralAmp - b.lateralAmp) < eps
            && Mathf.Abs(a.lateralWavelengthM - b.lateralWavelengthM) < eps
            && Mathf.Abs(a.verticalAmp - b.verticalAmp) < eps
            && Mathf.Abs(a.verticalWavelengthM - b.verticalWavelengthM) < eps;
    }
}
