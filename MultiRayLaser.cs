using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LaserSystem : MonoBehaviour
{
    [Header("Dots (Persistent Scan)")]
    public Material dotMaterial;
    public int dotsPerPulse = 50;
    public float dotSpreadAngle = 20f;
    public float maxDistance = 50f;
    public float dotScale = 0.1f;

    [Header("Dot Optimization")]
    public int maxPersistentDots = 2000;
    public float minDotDistance = 0.15f;

    [Header("Rays (Visual Only)")]
    public Material rayMaterial;
    public float rayWidth = 0.04f;
    public int rayCount = 12;
    public float raySpreadAngle = 12f;
    public float rotationPerPulse = 10f;

    [Header("Ray Animation")]
    public AnimationCurve fadeCurve;
    public float fadeDuration = 0.25f;
    public float rayPulseInterval = 0.08f;
    public float wallCheckDistance = 30f;

    // -------------------------

    private PlayerControls controls;
    private bool scanning;

    private Queue<Vector3> dotPositions;
    private Mesh sphereMesh;
    private Material dotMatInstance;

    private Vector3[] rayDirs;
    private float[] rayTimes;
    private LineRenderer[] rays;

    private float nextPulseTime;
    private float pulseRotation;

    void Awake()
    {
        controls = new PlayerControls();
        controls.Player.Attack.performed += _ => scanning = true;
        controls.Player.Attack.canceled += _ => scanning = false;
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Start()
    {
        dotPositions = new Queue<Vector3>(maxPersistentDots);
        sphereMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
        dotMatInstance = dotMaterial ? new Material(dotMaterial) : null;

        InitRays();
    }

    void LateUpdate()
    {
        // Always draw revealed areas
        DrawDots();

        if (!scanning)
        {
            DisableAllRays();
            return;
        }

        bool nearWall = Physics.Raycast(
            transform.position,
            transform.forward,
            out _,
            wallCheckDistance
        );

        if (nearWall && Time.time >= nextPulseTime)
        {
            AddDots();
            PulseRays();
            nextPulseTime = Time.time + rayPulseInterval;
        }

        AnimateRays();
    }


    // ==================================================
    // DOTS
    // ==================================================

    void AddDots()
    {
        for (int i = 0; i < dotsPerPulse; i++)
        {
            Vector3 localDir = RandomLocalDir(dotSpreadAngle);
            Vector3 worldDir = transform.TransformDirection(localDir);

            if (Physics.Raycast(transform.position, worldDir, out RaycastHit hit, maxDistance))
            {
                if (IsTooClose(hit.point))
                    continue;

                dotPositions.Enqueue(hit.point);

                if (dotPositions.Count > maxPersistentDots)
                    dotPositions.Dequeue();
            }
        }
    }

    bool IsTooClose(Vector3 p)
    {
        foreach (var d in dotPositions)
        {
            if (Vector3.Distance(d, p) < minDotDistance)
                return true;
        }
        return false;
    }

    void DrawDots()
    {
        if (!sphereMesh || !dotMatInstance)
            return;

        foreach (var p in dotPositions)
        {
            Graphics.DrawMesh(
                sphereMesh,
                Matrix4x4.TRS(p, Quaternion.identity, Vector3.one * dotScale),
                dotMatInstance,
                gameObject.layer
            );
        }
    }

    Vector3 RandomLocalDir(float angle)
    {
        Vector3 r = Random.insideUnitSphere.normalized;
        return Vector3.Slerp(Vector3.forward, r, angle / 90f).normalized;
    }

    // ==================================================
    // RAYS
    // ==================================================

    void InitRays()
    {
        rayDirs = new Vector3[rayCount];
        rayTimes = new float[rayCount];
        rays = new LineRenderer[rayCount];

        for (int i = 0; i < rayCount; i++)
        {
            rayDirs[i] = ConeDir(i, rayCount, raySpreadAngle);

            GameObject g = new GameObject($"Ray_{i}");
            g.transform.SetParent(transform);
            g.transform.localPosition = Vector3.zero;
            g.transform.localRotation = Quaternion.identity;

            LineRenderer lr = g.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.positionCount = 2;
            lr.material = new Material(rayMaterial);
            lr.startWidth = rayWidth;
            lr.endWidth = rayWidth;
            lr.enabled = false;

            rays[i] = lr;
            rayTimes[i] = -999f;
        }
    }

    void DisableAllRays()
    {
        for (int i = 0; i < rayCount; i++)
            rays[i].enabled = false;
    }

    void PulseRays()
    {
        pulseRotation += rotationPerPulse;

        for (int i = 0; i < rayCount; i++)
        {
            rayTimes[i] = Time.time;
            rays[i].enabled = true;

            Quaternion spin = Quaternion.AngleAxis(pulseRotation, Vector3.forward);
            Vector3 localDir = spin * rayDirs[i];

            rays[i].SetPosition(0, Vector3.zero);
            rays[i].SetPosition(1, localDir * maxDistance);
        }
    }

    void AnimateRays()
    {
        for (int i = 0; i < rayCount; i++)
        {
            float t = (Time.time - rayTimes[i]) / fadeDuration;

            if (t >= 1f)
            {
                rays[i].enabled = false;
                continue;
            }

            Color c = rays[i].material.color;
            c.a = fadeCurve.Evaluate(Mathf.Clamp01(t));
            rays[i].material.color = c;
        }
    }

    Vector3 ConeDir(int i, int total, float spread)
    {
        float a = (i / (float)total) * Mathf.PI * 2f;
        float s = spread * Mathf.Deg2Rad;

        return new Vector3(
            Mathf.Sin(s) * Mathf.Cos(a),
            Mathf.Sin(s) * Mathf.Sin(a),
            Mathf.Cos(s)
        ).normalized;
    }
}
