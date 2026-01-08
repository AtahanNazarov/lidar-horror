using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))] // Automatically adds an AudioSource if missing
public class LaserSystem : MonoBehaviour
{
    [Header("Targeting")]
    public Transform muzzlePoint;
    public LayerMask hitLayers = -1;

    [Header("Audio")]
    public AudioClip scanSound;
    [Range(0f, 1f)] public float scanVolume = 0.5f;
    private AudioSource audioSource;

    [Header("Color Cycle Settings")]
    public float cycleSpeed = 0.02f;
    [Range(0f, 1f)] public float globalSaturation = 0.8f;
    [Range(0f, 1f)] public float globalBrightness = 1.0f;
    [Range(0f, 255f)] public float rayAlphaBase = 30f;

    [Header("Dots (Persistent Scan)")]
    public Material dotMaterial;
    public int dotsPerPulse = 40;
    public float dotSpreadAngle = 25f;
    public float maxDistance = 50f;
    [Range(0.001f, 0.1f)] public float dotScale = 0.005f;

    [Header("Dot Optimization")]
    public int maxPersistentDots = 3000;
    public float minDotDistance = 0.15f;

    [Header("Rays (Visual Only)")]
    public Material rayMaterial;
    [Range(0.0005f, 0.01f)] public float rayWidth = 0.001f;
    public int rayCount = 8;
    public float raySpreadAngle = 10f;
    public float rotationPerPulse = 15f;

    [Header("Ray Animation")]
    public AnimationCurve fadeCurve = AnimationCurve.Linear(0, 1, 1, 0);
    public float fadeDuration = 0.1f;
    public float rayPulseInterval = 0.1f;
    public float wallCheckDistance = 40f;

    [Header("Controls")]
    public bool scanning = true;

    private Queue<Vector3> dotPositions;
    private Queue<Color> dotColors;
    private Mesh sphereMesh;
    private Vector3[] rayDirs;
    private float[] rayTimes;
    private LineRenderer[] rays;
    private float nextPulseTime;
    private float pulseRotation;
    private float currentHue = 0f;
    private MaterialPropertyBlock propBlock;

    void Start()
    {
        dotPositions = new Queue<Vector3>(maxPersistentDots);
        dotColors = new Queue<Color>(maxPersistentDots);
        propBlock = new MaterialPropertyBlock();

        // Setup AudioSource
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = scanSound;
        audioSource.loop = true; // Make it continuous
        audioSource.playOnAwake = false;
        audioSource.volume = scanVolume;

        GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphereMesh = tempSphere.GetComponent<MeshFilter>().sharedMesh;
        Destroy(tempSphere);

        if (muzzlePoint == null) muzzlePoint = transform;

        InitRays();
    }

    void Update()
    {
        currentHue += Time.deltaTime * cycleSpeed;
        if (currentHue > 1f) currentHue -= 1f;

        // Handle Audio and Input
        HandleInputAndSound();
    }

    void HandleInputAndSound()
    {
        // Only scan if LMB is held down AND scanning is enabled
        bool isHoldingClick = Input.GetMouseButton(0);

        if (isHoldingClick && scanning)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    void LateUpdate()
    {
        DrawDots();

        // Updated logic: Don't show rays or add dots unless LMB is held
        if (!scanning || !Input.GetMouseButton(0))
        {
            DisableAllRays();
            return;
        }

        bool nearWall = Physics.Raycast(muzzlePoint.position, muzzlePoint.forward, out _, wallCheckDistance, hitLayers);

        if (nearWall && Time.time >= nextPulseTime)
        {
            AddDots();
            PulseRays();
            nextPulseTime = Time.time + rayPulseInterval;
        }

        AnimateRays();
    }

    // --- REMAINDER OF YOUR SCRIPT (AddDots, DrawDots, etc.) REMAINS THE SAME ---
    // [I've omitted the rest for brevity, but keep your original methods below]
    
    void AddDots()
    {
        Color spawnColor = Color.HSVToRGB(currentHue, globalSaturation, globalBrightness);

        for (int i = 0; i < dotsPerPulse; i++)
        {
            Vector3 localDir = RandomLocalDir(dotSpreadAngle);
            Vector3 worldDir = muzzlePoint.TransformDirection(localDir);

            if (Physics.Raycast(muzzlePoint.position, worldDir, out RaycastHit hit, maxDistance, hitLayers))
            {
                if (IsTooClose(hit.point)) continue;

                dotPositions.Enqueue(hit.point);
                dotColors.Enqueue(spawnColor);

                if (dotPositions.Count > maxPersistentDots)
                {
                    dotPositions.Dequeue();
                    dotColors.Dequeue();
                }
            }
        }
    }

    bool IsTooClose(Vector3 p)
    {
        foreach (var d in dotPositions)
        {
            if ((d - p).sqrMagnitude < (minDotDistance * minDotDistance)) return true;
        }
        return false;
    }

    void DrawDots()
    {
        if (!sphereMesh || !dotMaterial) return;

        Vector3 scale = Vector3.one * dotScale;
        
        var posEnum = dotPositions.GetEnumerator();
        var colEnum = dotColors.GetEnumerator();

        while (posEnum.MoveNext() && colEnum.MoveNext())
        {
            propBlock.SetColor("_Color", colEnum.Current);
            
            Graphics.DrawMesh(
                sphereMesh,
                Matrix4x4.TRS(posEnum.Current, Quaternion.identity, scale),
                dotMaterial,
                gameObject.layer,
                null, 0, propBlock
            );
        }
    }

    void InitRays()
    {
        rayDirs = new Vector3[rayCount];
        rayTimes = new float[rayCount];
        rays = new LineRenderer[rayCount];

        for (int i = 0; i < rayCount; i++)
        {
            rayDirs[i] = ConeDir(i, rayCount, raySpreadAngle);
            
            GameObject g = new GameObject($"Lidar_Ray_{i}");
            g.transform.SetParent(muzzlePoint);
            g.transform.localPosition = Vector3.zero;
            g.transform.localRotation = Quaternion.identity;

            LineRenderer lr = g.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.positionCount = 2;
            if (rayMaterial != null) lr.material = new Material(rayMaterial);
            lr.startWidth = rayWidth;
            lr.endWidth = rayWidth;
            lr.enabled = false;
            rays[i] = lr;
            rayTimes[i] = -999f;
        }
    }

    void PulseRays()
    {
        pulseRotation += rotationPerPulse;
        for (int i = 0; i < rayCount; i++)
        {
            rayTimes[i] = Time.time;
            rays[i].enabled = true;
            
            Quaternion spin = Quaternion.AngleAxis(pulseRotation, Vector3.forward);
            Vector3 rotatedLocalDir = spin * rayDirs[i];
            
            rays[i].SetPosition(0, Vector3.zero);
            rays[i].SetPosition(1, rotatedLocalDir * maxDistance);
        }
    }

    void AnimateRays()
    {
        Color beamColor = Color.HSVToRGB(currentHue, globalSaturation, globalBrightness);

        for (int i = 0; i < rayCount; i++)
        {
            if (!rays[i].enabled) continue;
            
            float t = (Time.time - rayTimes[i]) / fadeDuration;
            if (t >= 1f) { rays[i].enabled = false; continue; }
            
            float alphaMultiplier = fadeCurve.Evaluate(Mathf.Clamp01(t));
            Color finalRayColor = beamColor;
            finalRayColor.a = (rayAlphaBase / 255f) * alphaMultiplier;

            Material m = rays[i].material;
            if (m.HasProperty("_Color")) m.SetColor("_Color", finalRayColor);
        }
    }

    Vector3 RandomLocalDir(float angle)
    {
        Vector3 r = Random.insideUnitSphere.normalized;
        return Vector3.Slerp(Vector3.forward, r, angle / 90f).normalized;
    }

    void DisableAllRays() { for (int i = 0; i < rayCount; i++) if(rays[i] != null) rays[i].enabled = false; }

    Vector3 ConeDir(int i, int total, float spread)
    {
        float anglePerStep = (i / (float)total) * Mathf.PI * 2f;
        float radSpread = spread * Mathf.Deg2Rad;
        
        float x = Mathf.Sin(radSpread) * Mathf.Cos(anglePerStep);
        float y = Mathf.Sin(radSpread) * Mathf.Sin(anglePerStep);
        float z = Mathf.Cos(radSpread);
        
        return new Vector3(x, y, z);
    }
}