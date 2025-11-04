using UnityEngine;
using UnityEngine.UI;
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
    public int maxLockedBoxes = 3;
    public float maxLockDistance = 5f;

    [Header("Cooldown")]
    public float lockCooldown = 1f;
    private float lastToggleTime = -999f;

    [Header("Visual Feedback")]
    public float fadeDistance = 3f;        // Hvis spiller er tættere end dette → fade
    public float fadedAlpha = 0.35f;       // Hvor gennemsigtig kassen bliver

    private float normalAlpha = 1f;
    private Renderer boxRenderer;

    private static List<LockObjects> lockedBoxes = new List<LockObjects>();
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boxRenderer = GetComponentInChildren<Renderer>();

        if (boxRenderer != null)
            normalAlpha = boxRenderer.material.color.a;
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

    void LockBox()
    {
        isLocked = true;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        lockedBoxes.Add(this);

        if (lockIconPrefab != null)
        {
            lockIconInstance = Instantiate(lockIconPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            lockIconInstance.transform.SetParent(transform);
        }

        if (progressRingPrefab != null)
        {
            GameObject ringObj = Instantiate(progressRingPrefab, transform.position + Vector3.up * 2.2f, Quaternion.identity, transform);
            progressRingImage = ringObj.GetComponentInChildren<Image>();
            progressRingImage.fillAmount = 0f;  // starter tom
        }
    }

    public void UnlockBox()
    {
        isLocked = false;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;

        lockedBoxes.Remove(this);

        if (lockIconInstance != null)
            Destroy(lockIconInstance);

        if (progressRingImage != null)
            Destroy(progressRingImage.transform.parent.gameObject);
    }

    void Update()
    {
        // Fade, når spiller er tæt på
        if (player != null && boxRenderer != null)
        {
            float dist = Vector3.Distance(player.position, transform.position);
            float targetAlpha = dist < fadeDistance ? fadedAlpha : normalAlpha;

            Color c = boxRenderer.material.color;
            c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * 10f);
            boxRenderer.material.color = c;
        }

        // Rotation af ikon + progress ring
        if (player != null)
        {
            if (lockIconInstance != null)
                lockIconInstance.transform.LookAt(player);

            if (progressRingImage != null)
                progressRingImage.transform.parent.LookAt(player);
        }

        // Opdater progress ring
        if (progressRingImage != null)
        {
            float t = (Time.time - lastToggleTime) / lockCooldown;
            progressRingImage.fillAmount = Mathf.Clamp01(t);
        }
    }
}
