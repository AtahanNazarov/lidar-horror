using System.Collections.Generic;
using UnityEngine;

public class LaserSystem : MonoBehaviour
{
    [Header("Dots (Static Hit Points)")]
    public Material dotMaterial; 
    public int dotCount = 50;
    public float dotSpreadAngle = 20f;
    public float maxDistance = 50f;
    public float dotScale = 0.1f;

    [Header("Rays (Animated Pulses)")]
    public Material rayMaterial;
    public float rayWidth = 0.04f;
    public int rayCount = 12;
    public float raySpreadAngle = 12f;
    public float rotationPerPulse = 10f; // Degrees to rotate per pulse

    [Header("Ray Animation")]
    public AnimationCurve fadeCurve;
    public float fadeDuration = 0.25f;
    public float rayPulseInterval = 0.08f;
    public float wallCheckDistance = 30f; 

    [Header("Camera / Origin")]
    public Transform cameraTransform;
    
    [Tooltip("If assigned, rays will fire from this transform instead of the camera")]
    public Transform gunTransform;

    // Dot drawing state
    private List<Vector3> dotPositions; 
    private Mesh sphereMesh; 
    private Material dotDrawMaterialInstance; 

    // Deterministic ray buffers
    private Vector3[] rayDirections;

    // Ray fading state
    private float[] rayFadeTimes;
    private LineRenderer[] rayLines;

    private float nextPulseTime;
    private Vector3 lastCamPos; 
    private Quaternion lastCamRot; 
    
    private float currentPulseRotation = 0f;

    void Start()
    {
        // --- DOT SETUP ---
        dotPositions = new List<Vector3>(dotCount);
        sphereMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
        
        if (dotMaterial != null)
        {
            dotDrawMaterialInstance = new Material(dotMaterial);
        }

        InitRays();
        
        lastCamPos = cameraTransform.position;
        lastCamRot = cameraTransform.rotation;
        
        UpdateDotCollisions(); 
    }

    void Update()
    {
        // Dots: Update ONLY when the camera/laser origin moves or rotates.
        if (cameraTransform.position != lastCamPos || cameraTransform.rotation != lastCamRot)
        {
            UpdateDotCollisions();
            lastCamPos = cameraTransform.position;
            lastCamRot = cameraTransform.rotation;
        }

        // --- RAY ANIMATION LOGIC ---
        
        // Check for a nearby wall
        bool nearWall = Physics.Raycast(
            cameraTransform.position,
            cameraTransform.forward,
            out _,
            wallCheckDistance
        );

        // Pulse rays on timer if near a wall
        if (nearWall && Time.time >= nextPulseTime)
        {
            PulseRays();
            nextPulseTime = Time.time + rayPulseInterval;
        }

        // Animate the ray fades
        AnimateRayFades();

        // Draw the sphere dots
        DrawSphereDots();
    }

    // ----------------------------------------------------------
    // Dots (Static Hit Points)
    // ----------------------------------------------------------

    void UpdateDotCollisions()
    {
        dotPositions.Clear();
        for (int i = 0; i < dotCount; i++)
        {
            Vector3 dir = RandomDirection(dotSpreadAngle);
            if (Physics.Raycast(cameraTransform.position, dir, out RaycastHit hit, maxDistance))
            {
                dotPositions.Add(hit.point);
            }
        }
    }

    void DrawSphereDots()
    {
        if (sphereMesh == null || dotDrawMaterialInstance == null) return;
        Quaternion rotation = Quaternion.identity; 
        foreach (Vector3 position in dotPositions)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(
                position,
                rotation,
                Vector3.one * dotScale
            );
            Graphics.DrawMesh(sphereMesh, matrix, dotDrawMaterialInstance, gameObject.layer, null, 0);
        }
    }

    Vector3 RandomDirection(float angle)
    {
        Vector3 rand = Random.insideUnitSphere.normalized;
        return Vector3.Slerp(cameraTransform.forward, rand, angle / 90f).normalized;
    }

    // ----------------------------------------------------------
    // Rays (Animated Pulses) - CONE BASED
    // ----------------------------------------------------------
    
    void InitRays()
    {
        rayDirections = new Vector3[rayCount];
        rayFadeTimes = new float[rayCount];
        rayLines = new LineRenderer[rayCount];

        for (int i = 0; i < rayCount; i++)
        {
            // Create rays in a circle around the forward axis
            rayDirections[i] = ConeDirection(i, rayCount, raySpreadAngle);

            GameObject g = new GameObject("Ray_" + i);
            g.transform.SetParent(transform);

            LineRenderer lr = g.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.material = new Material(rayMaterial);
            lr.startWidth = rayWidth;
            lr.endWidth = rayWidth;
            lr.enabled = false;

            rayLines[i] = lr;
            rayFadeTimes[i] = -999f;
        }
    }

    void PulseRays()
    {
        // Use gun position if assigned, otherwise use camera position
        Transform firingTransform = gunTransform != null ? gunTransform : cameraTransform;
        Vector3 origin = firingTransform.position + firingTransform.forward * 0.05f;
        
        // Accumulate rotation around the cone axis (forward direction)
        currentPulseRotation += rotationPerPulse;
        
        for (int i = 0; i < rayCount; i++)
        {
            rayFadeTimes[i] = Time.time;
            rayLines[i].enabled = true;

            // Rotate the local cone direction around the forward axis
            Quaternion rotationOffset = Quaternion.AngleAxis(currentPulseRotation, Vector3.forward);
            Vector3 rotatedLocalDir = rotationOffset * rayDirections[i];
            
            // Transform to world space using firing transform rotation
            Vector3 worldDir = firingTransform.rotation * rotatedLocalDir;

            Vector3 end;
            if (Physics.Raycast(origin, worldDir, out RaycastHit hit, maxDistance))
                end = hit.point;
            else
                end = origin + worldDir * maxDistance;

            rayLines[i].SetPosition(0, origin);
            rayLines[i].SetPosition(1, end);
        }
    }

    void AnimateRayFades()
    {
        for (int i = 0; i < rayCount; i++)
        {
            float t = (Time.time - rayFadeTimes[i]) / fadeDuration;

            if (t >= 1f)
            {
                rayLines[i].enabled = false;
                continue;
            }

            float alpha = fadeCurve.Evaluate(Mathf.Clamp01(t));

            Color c = rayLines[i].material.color;
            c.a = alpha;
            rayLines[i].material.color = c;
        }
    }

    Vector3 ConeDirection(int index, int total, float spreadAngle)
    {
        // Create rays in a circle pattern around the forward axis
        float angleAroundCone = (index / (float)total) * 360f * Mathf.Deg2Rad;
        
        // Convert spread angle to radians
        float spreadRad = spreadAngle * Mathf.Deg2Rad;
        
        // Create a direction at the specified angle from the cone axis
        // X and Y form the circle, Z is forward
        float x = Mathf.Sin(spreadRad) * Mathf.Cos(angleAroundCone);
        float y = Mathf.Sin(spreadRad) * Mathf.Sin(angleAroundCone);
        float z = Mathf.Cos(spreadRad);
        
        return new Vector3(x, y, z).normalized;
    }
}