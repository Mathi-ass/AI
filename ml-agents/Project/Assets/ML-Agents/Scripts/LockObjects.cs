using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class LockObjects : MonoBehaviour
{
    [Header("Lock Settings")]
    public bool isLocked = false;
    public GameObject lockIconPrefab;
    private GameObject lockIconInstance;

    [Header("Progress Ring")]
    public GameObject progressRingPrefab;
    private Image progressRingImage;

    [Header("Player Settings")]
    public Transform player;
    public int maxLockedBoxes = 3; // Maksimalt antal låste objekter
    public float maxLockDistance = 4f; // Maksimal afstand for at låse/oplåse

    [Header("Cooldown")]
    public float lockCooldown = 1f; // Nedkølingsperiode mellem lås/oplås
    private float lastToggleTime = -999f;

    // Per-material instances so changes only affect this renderer
    private Material[] instanceMaterials;
    private float[] normalAlphas;
    private Renderer boxRenderer;

    private static List<LockObjects> lockedBoxes = new List<LockObjects>();
    private Rigidbody rb;

    // Progress fade control
    public float progressFadeDuration = 0.2f; // Sekunder til at fade progress ring ud
    private bool progressCompleted = false;
    private float progressCompleteTime = 0f;
    private CanvasGroup progressRingCanvasGroup;

    // Lock icon fade control
    public float iconFadeDuration = 0.2f; // Sekunder til at fade lås ikonet ud
    private bool iconFading = false;
    private float iconFadeStartTime = 0f;
    private CanvasGroup lockIconCanvasGroup;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boxRenderer = GetComponentInChildren<Renderer>();
    }

    void OnMouseDown()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (player == null) return;

        if (Time.time < lastToggleTime + lockCooldown) return;

        float dist = Vector3.Distance(player.position, transform.position);
        if (dist > maxLockDistance) return;

        if (!isLocked)
        {
            if (lockedBoxes.Count >= maxLockedBoxes)
                lockedBoxes[0].UnlockBox();

            LockBox();
        }
        else
        {
            UnlockBox();
        }

        lastToggleTime = Time.time;
    }

    public void LockBox()
    {
        isLocked = true;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        lockedBoxes.Add(this);

        if (lockIconPrefab != null)
        {
            lockIconInstance = Instantiate(lockIconPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            lockIconInstance.transform.SetParent(transform);

            // Ensure a CanvasGroup exists so we can fade the icon
            lockIconCanvasGroup = lockIconInstance.GetComponent<CanvasGroup>();
            if (lockIconCanvasGroup == null)
                lockIconCanvasGroup = lockIconInstance.AddComponent<CanvasGroup>();
            lockIconCanvasGroup.alpha = 1f;
            iconFading = false;
        }

        if (progressRingPrefab != null)
        {
            // Instantiate ring as a child of this object
            GameObject ringObj = Instantiate(progressRingPrefab, transform.position + Vector3.up * 2f, Quaternion.identity, transform);
            progressRingImage = ringObj.GetComponentInChildren<Image>();
            if (progressRingImage != null)
                progressRingImage.fillAmount = 0f;  // starter tom

            // Ensure we have a CanvasGroup on the root of the ring so we can fade the whole thing
            progressRingCanvasGroup = ringObj.GetComponent<CanvasGroup>();
            if (progressRingCanvasGroup == null)
                progressRingCanvasGroup = ringObj.AddComponent<CanvasGroup>();

            progressRingCanvasGroup.alpha = 1f;
            progressCompleted = false;
        }
    }

    public void UnlockBox()
    {
        isLocked = false;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;

        lockedBoxes.Remove(this);

        // Start fading the lock icon instead of destroying immediately
        if (lockIconInstance != null)
        {
            lockIconCanvasGroup = lockIconInstance.GetComponent<CanvasGroup>();
            if (lockIconCanvasGroup == null)
                lockIconCanvasGroup = lockIconInstance.AddComponent<CanvasGroup>();
            lockIconCanvasGroup.alpha = 1f;
            iconFading = true;
            iconFadeStartTime = Time.time;
        }

        // Ensure progress ring also appears when unlocking
        if (progressRingPrefab != null && progressRingImage == null)
        {
            GameObject ringObj = Instantiate(progressRingPrefab, transform.position + Vector3.up * 2f, Quaternion.identity, transform);
            progressRingImage = ringObj.GetComponentInChildren<Image>();
            if (progressRingImage != null)
                progressRingImage.fillAmount = 0f;

            progressRingCanvasGroup = ringObj.GetComponent<CanvasGroup>();
            if (progressRingCanvasGroup == null)
                progressRingCanvasGroup = ringObj.AddComponent<CanvasGroup>();

            progressRingCanvasGroup.alpha = 1f;
            progressCompleted = false;
            // progressCompleteTime will be set when fill completes in Update
        }
    }

    void Update()
    {

        // Rotation af ikon + progress ring
        if (player != null)
        {
            if (lockIconInstance != null)
                lockIconInstance.transform.LookAt(player);

            if (progressRingImage != null && progressRingImage.transform.parent != null)
                progressRingImage.transform.parent.LookAt(player);
        }

        // Handle fading lock icon (when unlocking)
        if (iconFading && lockIconInstance != null)
        {
            float elapsed = Time.time - iconFadeStartTime;
            float t = Mathf.Clamp01(elapsed / iconFadeDuration);

            if (lockIconCanvasGroup != null)
                lockIconCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            // optional slight shrink
            lockIconInstance.transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.9f, t) / 2f;

            if (t >= 1f)
            {
                Destroy(lockIconInstance);
                lockIconInstance = null;
                iconFading = false;
                lockIconCanvasGroup = null;
            }
        }

        // Opdater progress ring
        if (progressRingImage != null)
        {
            // If progress hasn't completed yet, update fill based on cooldown
            if (!progressCompleted)
            {
                float t = (Time.time - lastToggleTime) / lockCooldown;
                progressRingImage.fillAmount = Mathf.Clamp01(t);

                // When filled, start fade timer
                if (progressRingImage.fillAmount >= 1f - Mathf.Epsilon)
                {
                    progressCompleted = true;
                    progressCompleteTime = Time.time;
                }
            }
            else
            {
                // Fade out over progressFadeDuration seconds
                float elapsed = Time.time - progressCompleteTime;
                float fadeT = Mathf.Clamp01(elapsed / progressFadeDuration);

                if (progressRingCanvasGroup != null)
                    progressRingCanvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeT);

                // Slight shrink effect during fade
                if (progressRingImage.transform.parent != null)
                {
                    float scale = Mathf.Lerp(1f, 0.9f, fadeT);
                    progressRingImage.transform.parent.localScale = Vector3.one * scale / 333.333f;
                }

                // Destroy ring when fade complete
                if (fadeT >= 1f)
                {
                    Destroy(progressRingImage.transform.parent.gameObject);
                    progressRingImage = null;
                    progressRingCanvasGroup = null;
                    progressCompleted = false;
                }
            }
        }
    }
}
