
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShootingController : MonoBehaviour
{
    [Header("Gun Animator")]
    public Animator animator;

    [Header("Shooting")]
    public Transform firePoint;
    public float fireRate = 10f;
    public float fireRange = 10f;
    public bool isAutoFire = false;
    public int damagePerShot = 20;

    [Header("Camera Aiming")]
    public Camera mainCamera;
    public Camera scopeCamera;
    public ScopeSystem scopeSystem;

    private float nextFireTime;

    [Header("Ammo")]
    public int maxAmmo = 30;
    public int currentAmmo = 30;
    public float reloadTime = 1.5f;

    [Header("Ammo UI")]
    public TMP_Text currentAmmoText;
    public TMP_Text maxAmmoText;

    private bool isReloading;

    public bool IsReloading
    {
        get { return isReloading; }
    }

    [Header("Muzzle Flash")]
    public GameObject muzzleFlash;
    public float flashDuration = 0.04f;

    private Light[] muzzleFlashLights;
    private Coroutine muzzleFlashCoroutine;

    [Header("Blood Impact")]
    public Material bloodMaterial;
    public float bloodMarkSize = 0.015f;
    public float bloodSurfaceOffset = 0.003f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip shootSound;
    public AudioClip reloadSound;

    // ---------------- INITIALIZATION ----------------

    private void Awake()
    {
        currentAmmo = Mathf.Clamp(currentAmmo, 0, maxAmmo);

        DisableMuzzleFlash();
        UpdateAmmoUI();
    }

    private void Start()
    {
        UpdateAmmoUI();
    }

    private void Update()
    {
        if (isReloading)
            return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (currentAmmo < maxAmmo)
                StartCoroutine(Reload());

            return;
        }

        if (currentAmmo <= 0)
        {
            if (Input.GetButtonDown("Fire1"))
                StartCoroutine(Reload());

            return;
        }

        bool firePressed = isAutoFire
            ? Input.GetButton("Fire1")
            : Input.GetButtonDown("Fire1");

        if (firePressed && Time.time >= nextFireTime)
        {
            nextFireTime =
                Time.time + 1f / Mathf.Max(fireRate, 0.01f);

            Shoot();
        }
    }

    // ---------------- AMMO UI ----------------

    private void UpdateAmmoUI()
    {
        if (currentAmmoText != null)
            currentAmmoText.text = currentAmmo.ToString();

        if (maxAmmoText != null)
            maxAmmoText.text = maxAmmo.ToString();
    }

    // ---------------- WEAPON SWITCHING ----------------

    public void SetWeaponAnimator(Animator newAnimator)
    {
        animator = newAnimator;

        if (animator != null)
        {
            animator.ResetTrigger("Shoot");
            animator.ResetTrigger("Reload");

            Debug.Log(
                "Active weapon Animator: " + animator.gameObject.name
            );
        }
        else
        {
            Debug.LogError("Current weapon Animator is missing!");
        }
    }

    public void SetFirePoint(Transform newFirePoint)
    {
        firePoint = newFirePoint;
    }

    public void SetMuzzleFlash(GameObject newMuzzleFlash)
    {
        DisableMuzzleFlash();

        muzzleFlash = newMuzzleFlash;

        if (muzzleFlash != null)
        {
            muzzleFlashLights =
                muzzleFlash.GetComponentsInChildren<Light>(true);

            foreach (Light lightComponent in muzzleFlashLights)
            {
                if (lightComponent != null)
                    lightComponent.enabled = false;
            }

            muzzleFlash.SetActive(false);
        }
        else
        {
            muzzleFlashLights = null;
        }
    }

    // ---------------- DISABLE MUZZLE FLASH ----------------

    private void DisableMuzzleFlash()
    {
        if (muzzleFlashCoroutine != null)
        {
            StopCoroutine(muzzleFlashCoroutine);
            muzzleFlashCoroutine = null;
        }

        if (muzzleFlash != null)
        {
            muzzleFlashLights =
                muzzleFlash.GetComponentsInChildren<Light>(true);

            foreach (Light lightComponent in muzzleFlashLights)
            {
                if (lightComponent != null)
                    lightComponent.enabled = false;
            }

            muzzleFlash.SetActive(false);
        }
    }

    // ---------------- GET ACTIVE CAMERA ----------------

    private Camera GetActiveCamera()
    {
        if (scopeSystem != null && scopeSystem.IsScoped)
            return scopeCamera;

        return mainCamera;
    }

    // ---------------- SHOOTING ----------------

    private void Shoot()
    {
        if (firePoint == null)
        {
            Debug.LogError("Fire Point is not assigned!");
            return;
        }

        if (currentAmmo <= 0 || isReloading)
            return;

        Camera activeCamera = GetActiveCamera();

        if (activeCamera == null)
        {
            Debug.LogError("Active shooting camera is missing!");
            return;
        }

        currentAmmo--;

        UpdateAmmoUI();

        Debug.Log("Current Ammo: " + currentAmmo);

        if (animator != null)
        {
            animator.SetTrigger("Shoot");

            Debug.Log(
                "Shoot animation triggered on: " +
                animator.gameObject.name
            );
        }
        else
        {
            Debug.LogError("Current weapon Animator is missing!");
        }

        TriggerMuzzleFlash();

        if (audioSource != null && shootSound != null)
            audioSource.PlayOneShot(shootSound);

        // Get the center point of the active camera's view.
        Ray cameraRay = activeCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        Vector3 targetPoint;

        if (Physics.Raycast(
            cameraRay,
            out RaycastHit cameraHit,
            fireRange))
        {
            targetPoint = cameraHit.point;
        }
        else
        {
            targetPoint = cameraRay.GetPoint(fireRange);
        }

        Vector3 shootingDirection =
            (targetPoint - firePoint.position).normalized;

        Ray ray = new Ray(
            firePoint.position,
            shootingDirection
        );

        List<Component> zombies = new List<Component>();

        zombies.AddRange(FindObjectsByType<Zombie_AI>());

        zombies.AddRange(
            FindObjectsByType<WayPointZombieAI>()
        );

        Component closestZombie = null;
        SkinnedMeshRenderer closestRenderer = null;

        Vector3 closestPoint = Vector3.zero;
        Vector3 closestNormal = Vector3.zero;

        float closestDistance = fireRange;

        foreach (Component zombie in zombies)
        {
            if (zombie == null)
                continue;

            SkinnedMeshRenderer[] renderers =
                zombie.GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach (SkinnedMeshRenderer skin in renderers)
            {
                if (skin == null ||
                    !skin.enabled ||
                    skin.sharedMesh == null)
                    continue;

                Mesh bakedMesh = new Mesh();

                try
                {
                    skin.BakeMesh(bakedMesh);

                    Vector3[] vertices = bakedMesh.vertices;
                    int[] triangles = bakedMesh.triangles;

                    for (int i = 0; i < triangles.Length; i += 3)
                    {
                        Vector3 a = skin.transform.TransformPoint(
                            vertices[triangles[i]]
                        );

                        Vector3 b = skin.transform.TransformPoint(
                            vertices[triangles[i + 1]]
                        );

                        Vector3 c = skin.transform.TransformPoint(
                            vertices[triangles[i + 2]]
                        );

                        if (RayIntersectsTriangle(
                            ray, a, b, c,
                            out float distance,
                            out Vector3 normal))
                        {
                            if (distance >= 0f &&
                                distance < closestDistance)
                            {
                                closestDistance = distance;
                                closestZombie = zombie;
                                closestRenderer = skin;
                                closestPoint = ray.GetPoint(distance);

                                if (Vector3.Dot(
                                    normal, ray.direction) > 0f)
                                {
                                    normal = -normal;
                                }

                                closestNormal = normal;
                            }
                        }
                    }
                }
                finally
                {
                    Destroy(bakedMesh);
                }
            }
        }

        if (closestZombie == null)
        {
            Debug.Log("Shot did not hit any zombie.");
            return;
        }

        if (closestZombie is Zombie_AI normalZombie)
        {
            normalZombie.TakeDamage(damagePerShot);

            Debug.Log(
                "Zombie_AI hit: " + normalZombie.name +
                " | Damage: " + damagePerShot
            );
        }
        else if (closestZombie is WayPointZombieAI waypointZombie)
        {
            waypointZombie.TakeDamage(damagePerShot);

            Debug.Log(
                "WayPointZombieAI hit: " + waypointZombie.name +
                " | Damage: " + damagePerShot
            );
        }

        ShowBloodMarkOnBone(
            closestRenderer,
            closestPoint,
            closestNormal
        );
    }

    // ---------------- UNIVERSAL MUZZLE FLASH ----------------

    private void TriggerMuzzleFlash()
    {
        if (muzzleFlash == null)
        {
            Debug.LogWarning(
                "Muzzle flash is not assigned to the current weapon!"
            );
            return;
        }

        if (muzzleFlashCoroutine != null)
        {
            StopCoroutine(muzzleFlashCoroutine);
            muzzleFlashCoroutine = null;
        }

        muzzleFlash.transform.position = firePoint.position;
        muzzleFlash.transform.rotation = firePoint.rotation;

        muzzleFlashLights =
            muzzleFlash.GetComponentsInChildren<Light>(true);

        muzzleFlash.SetActive(true);

        foreach (Light lightComponent in muzzleFlashLights)
        {
            if (lightComponent != null)
                lightComponent.enabled = true;
        }

        muzzleFlashCoroutine =
            StartCoroutine(HideMuzzleFlash());
    }

    private IEnumerator HideMuzzleFlash()
    {
        yield return new WaitForSeconds(flashDuration);

        if (muzzleFlashLights != null)
        {
            foreach (Light lightComponent in muzzleFlashLights)
            {
                if (lightComponent != null)
                    lightComponent.enabled = false;
            }
        }

        if (muzzleFlash != null)
            muzzleFlash.SetActive(false);

        muzzleFlashCoroutine = null;
    }

    // ---------------- RELOAD ----------------

    private IEnumerator Reload()
    {
        if (isReloading || currentAmmo >= maxAmmo)
            yield break;

        // Automatically close scope if player reloads while scoped.
        if (scopeSystem != null && scopeSystem.IsScoped)
        {
            scopeSystem.CloseScope();
        }

        isReloading = true;

        if (animator != null)
            animator.SetTrigger("Reload");

        if (audioSource != null && reloadSound != null)
            audioSource.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;

        UpdateAmmoUI();

        isReloading = false;

        Debug.Log("Reload complete. Ammo: " + currentAmmo);
    }

    // ---------------- BLOOD MARK ----------------

    private void ShowBloodMarkOnBone(
        SkinnedMeshRenderer skin,
        Vector3 hitPoint,
        Vector3 hitNormal)
    {
        if (bloodMaterial == null)
        {
            Debug.LogWarning(
                "Blood Material is not assigned!"
            );
            return;
        }

        if (skin == null)
            return;

        Transform[] bones = skin.bones;

        if (bones == null || bones.Length == 0)
        {
            Debug.LogWarning("No bones found on zombie mesh.");
            return;
        }

        Transform closestBone = null;
        float closestDistance = float.MaxValue;

        foreach (Transform bone in bones)
        {
            if (bone == null)
                continue;

            float distance =
                (bone.position - hitPoint).sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestBone = bone;
            }
        }

        if (closestBone == null)
            return;

        GameObject mark =
            GameObject.CreatePrimitive(PrimitiveType.Quad);

        mark.name = "BloodImpactMark";

        Collider markCollider = mark.GetComponent<Collider>();

        if (markCollider != null)
            Destroy(markCollider);

        mark.transform.position =
            hitPoint + hitNormal * bloodSurfaceOffset;

        mark.transform.rotation =
            Quaternion.LookRotation(hitNormal);

        mark.transform.localScale =
            new Vector3(bloodMarkSize, bloodMarkSize, 1f);

        Renderer markRenderer = mark.GetComponent<Renderer>();

        markRenderer.material = bloodMaterial;

        markRenderer.shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.Off;

        markRenderer.receiveShadows = false;

        mark.transform.SetParent(closestBone, true);
    }

    // ---------------- RAY-TRIANGLE DETECTION ----------------

    private bool RayIntersectsTriangle(
        Ray ray,
        Vector3 a,
        Vector3 b,
        Vector3 c,
        out float distance,
        out Vector3 normal)
    {
        distance = 0f;
        normal = Vector3.zero;

        Vector3 edge1 = b - a;
        Vector3 edge2 = c - a;

        Vector3 p = Vector3.Cross(ray.direction, edge2);
        float determinant = Vector3.Dot(edge1, p);

        if (Mathf.Abs(determinant) < 0.000001f)
            return false;

        float inverseDeterminant = 1f / determinant;

        Vector3 t = ray.origin - a;
        float u = Vector3.Dot(t, p) * inverseDeterminant;

        if (u < 0f || u > 1f)
            return false;

        Vector3 q = Vector3.Cross(t, edge1);
        float v = Vector3.Dot(ray.direction, q) * inverseDeterminant;

        if (v < 0f || u + v > 1f)
            return false;

        float hitDistance =
            Vector3.Dot(edge2, q) * inverseDeterminant;

        if (hitDistance < 0f)
            return false;

        distance = hitDistance;
        normal = Vector3.Cross(edge1, edge2).normalized;

        return true;
    }
}
