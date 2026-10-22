using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CrystalPartyMode : MonoBehaviour
{
    [Header("Riferimenti")]
    [Tooltip("Il renderer del cristallo (se vuoto lo cerca nei figli)")]
    [SerializeField] private Renderer crystalRenderer;
    [Tooltip("Global Volume con override Bloom (opzionale, per cambiare il bloom di tutta la scena)")]
    [SerializeField] private Volume globalVolume;

    [Header("Rotazione su se stesso")]
    [SerializeField] private float spinSpeed = 90f;
    [Tooltip("Quanto la velocità di rotazione oscilla (effetto 'trottola impazzita')")]
    [SerializeField] private float spinWobbleAmount = 60f;
    [SerializeField] private float spinWobbleSpeed = 2f;

    [Header("Orbita attorno al piedistallo")]
    [SerializeField] private float orbitRadius = 1.2f;
    [SerializeField] private float orbitSpeed = 60f;
    [Tooltip("Il raggio pulsa avanti e indietro")]
    [SerializeField] private float orbitRadiusPulse = 0.4f;

    [Header("Saltelli")]
    [SerializeField] private float bounceHeight = 0.8f;
    [SerializeField] private float bounceFrequency = 2.5f;
    [Tooltip("Squash & stretch: schiaccia a terra, si allunga in aria")]
    [SerializeField] private float squashAmount = 0.25f;

    [Header("Barcollamento")]
    [SerializeField] private float tiltAngle = 20f;
    [SerializeField] private float tiltSpeed = 3f;

    [Header("Cambio colore + bloom")]
    [SerializeField] private float colorChangeInterval = 3f;
    [SerializeField] private float colorBlendSpeed = 4f;
    [SerializeField] private Vector2 emissionIntensityRange = new Vector2(2f, 6f);
    [Tooltip("Il bloom 'pulsa' a ritmo del salto")]
    [SerializeField] private float emissionPulse = 1.5f;
    [Tooltip("Botta di bloom e di scala quando cambia colore")]
    [SerializeField] private float changePunch = 3f;
    [SerializeField] private Vector2 bloomIntensityRange = new Vector2(2f, 8f);

    private Vector3 startLocalPos;
    private Vector3 baseScale;
    private float spinAngle;
    private float orbitAngle;
    private float colorTimer;
    private float punch;

    private Material mat;
    private Color currentColor;
    private Color targetColor;
    private float currentIntensity;
    private float targetIntensity;
    private float targetBloom;
    private float currentBloom;
    private Bloom bloom;

    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private void Start()
    {
        startLocalPos = transform.localPosition;
        baseScale = transform.localScale;

        if (crystalRenderer == null)
            crystalRenderer = GetComponentInChildren<Renderer>();

        // .material crea un'istanza: non modifica il material nel progetto
        mat = crystalRenderer.material;
        mat.EnableKeyword("_EMISSION");
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;

        if (globalVolume != null && globalVolume.profile.TryGet(out bloom))
        {
            bloom.intensity.overrideState = true;
            bloom.tint.overrideState = true;
            currentBloom = targetBloom = bloom.intensity.value;
        }

        currentColor = targetColor = mat.GetColor(BaseColorId);
        currentIntensity = targetIntensity = emissionIntensityRange.x;
        PickNewColor();
    }

    private void Update()
    {
        float t = Time.time;
        float dt = Time.deltaTime;

        // --- Orbita (il raggio pulsa) ---
        orbitAngle += orbitSpeed * dt;
        float radius = orbitRadius + Mathf.Sin(t * 1.7f) * orbitRadiusPulse;
        float rad = orbitAngle * Mathf.Deg2Rad;
        Vector3 orbitOffset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;

        // --- Saltelli (abs(sin) = rimbalzo vero, con pausa a terra) ---
        float bounceWave = Mathf.Abs(Mathf.Sin(t * bounceFrequency * Mathf.PI));
        float height = bounceWave * bounceHeight;
        transform.localPosition = startLocalPos + orbitOffset + Vector3.up * height;

        // --- Rotazione con velocità ballerina + barcollamento ---
        spinAngle += (spinSpeed + Mathf.Sin(t * spinWobbleSpeed) * spinWobbleAmount) * dt;
        float tiltX = Mathf.Sin(t * tiltSpeed) * tiltAngle;
        float tiltZ = Mathf.Cos(t * tiltSpeed * 1.3f) * tiltAngle;
        transform.localRotation = Quaternion.Euler(tiltX, spinAngle, tiltZ);

        // --- Squash & stretch: schiacciato a terra, allungato in aria ---
        float stretch = (bounceWave - 0.5f) * 2f * squashAmount;
        punch = Mathf.Lerp(punch, 0f, dt * 5f);
        Vector3 s = new Vector3(1f - stretch * 0.5f, 1f + stretch, 1f - stretch * 0.5f);
        transform.localScale = Vector3.Scale(baseScale, s) * (1f + punch * 0.1f);

        // --- Cambio colore periodico ---
        colorTimer += dt;
        if (colorTimer >= colorChangeInterval)
        {
            colorTimer = 0f;
            PickNewColor();
        }

        UpdateColorAndBloom(bounceWave, dt);
    }

    private void PickNewColor()
    {
        // Colori vivaci: tinta casuale, saturazione e luminosità al massimo
        targetColor = Color.HSVToRGB(Random.value, Random.Range(0.6f, 1f), 1f);
        targetIntensity = Random.Range(emissionIntensityRange.x, emissionIntensityRange.y);
        targetBloom = Random.Range(bloomIntensityRange.x, bloomIntensityRange.y);
        punch = changePunch;
    }

    private void UpdateColorAndBloom(float bounceWave, float dt)
    {
        float k = dt * colorBlendSpeed;
        currentColor = Color.Lerp(currentColor, targetColor, k);
        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, k);
    }
}

        // Il bloom pulsa a ritmo di salto + botta al
