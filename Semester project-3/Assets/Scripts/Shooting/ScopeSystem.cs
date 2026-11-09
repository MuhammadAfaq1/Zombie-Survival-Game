
using System.Collections;
using UnityEngine;

public class ScopeSystem : MonoBehaviour
{
    [Header("Scope Settings")]
    public GameObject scopeImage;
    private CanvasGroup scopeCanvasGroup;
    public float fadeDuration = 0.15f;

    [Tooltip("Set the zoom multiplier: 2 = 2x, 4 = 4x, 8 = 8x")]
    [Range(1f, 10f)]
    public float scopeZoom = 2f;

    [Header("Camera Settings")]
    public Camera scopeCamera;
    public Camera mainCamera;

    [Tooltip("Starting Field of View before zoom is applied.")]
    public float baseFOV = 60f;

    [Header("Scroll Zoom Settings")]
    public float scrollZoomSpeed = 10f;
    public float minFOV = 5f;
    public float maxFOV = 80f;

    private bool isScoped = false;
    private float currentFOV;
    private Coroutine fadeRoutine;

    public bool IsScoped
    {
        get { return isScoped; }
    }

    void Start()
    {
        // Automatically find or add CanvasGroup.
        if (scopeImage != null)
        {
            scopeCanvasGroup = scopeImage.GetComponent<CanvasGroup>();

            if (scopeCanvasGroup == null)
            {
                scopeCanvasGroup = scopeImage.AddComponent<CanvasGroup>();
            }

            scopeCanvasGroup.alpha = 0f;
            scopeImage.SetActive(false);
        }

        ForceMainCameraActive();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (isScoped)
                CloseScope();
            else
                OpenScope();
        }

        // Scroll-wheel zoom.
        if (isScoped && scopeCamera != null)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (scroll != 0f)
            {
                currentFOV -= scroll * scrollZoomSpeed;

                currentFOV = Mathf.Clamp(
                    currentFOV,
                    minFOV,
                    maxFOV
                );

                scopeCamera.fieldOfView = currentFOV;
            }
        }
    }

    // ---------------- OPEN SCOPE ----------------

    private void OpenScope()
    {
        if (mainCamera == null || scopeCamera == null)
        {
            Debug.LogError("Main Camera or Scope Camera is missing!");
            return;
        }

        // Synchronize scope camera position and rotation.
        scopeCamera.transform.position =
            mainCamera.transform.position;

        scopeCamera.transform.rotation =
            mainCamera.transform.rotation;

        // Synchronize ScopeCameraController.
        ScopeCameraController scopeController =
            scopeCamera.GetComponent<ScopeCameraController>();

        if (scopeController != null)
        {
            scopeController.SyncWithMainCamera();
        }

        // Enable scope mode.
        isScoped = true;

        mainCamera.enabled = false;

        scopeCamera.gameObject.SetActive(true);
        scopeCamera.enabled = true;

        // Apply initial zoom.
        currentFOV = baseFOV / Mathf.Max(scopeZoom, 1f);

        currentFOV = Mathf.Clamp(
            currentFOV,
            minFOV,
            maxFOV
        );

        scopeCamera.fieldOfView = currentFOV;

        // Fade in scope image.
        if (scopeImage != null && scopeCanvasGroup != null)
        {
            scopeImage.SetActive(true);

            if (fadeRoutine != null)
                StopCoroutine(fadeRoutine);

            fadeRoutine = StartCoroutine(
                FadeScope(scopeCanvasGroup.alpha, 1f)
            );
        }
    }

    // ---------------- CLOSE SCOPE ----------------

    public void CloseScope()
    {
        isScoped = false;

        // Restore main camera.
        ForceMainCameraActive();

        // Fade out scope image.
        if (scopeImage != null && scopeCanvasGroup != null)
        {
            if (fadeRoutine != null)
                StopCoroutine(fadeRoutine);

            fadeRoutine = StartCoroutine(
                FadeScope(scopeCanvasGroup.alpha, 0f, true)
            );
        }
    }

    // ---------------- FORCE MAIN CAMERA ACTIVE ----------------

    private void ForceMainCameraActive()
    {
        if (scopeCamera != null)
        {
            scopeCamera.enabled = false;
            scopeCamera.gameObject.SetActive(false);
            scopeCamera.fieldOfView = baseFOV;
        }

        if (mainCamera != null)
        {
            mainCamera.enabled = true;
        }
    }

    // ---------------- FADE COROUTINE ----------------

    private IEnumerator FadeScope(
        float startAlpha,
        float targetAlpha,
        bool disableOnComplete = false)
    {
        if (scopeCanvasGroup == null)
            yield break;

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress = fadeDuration > 0f
                ? elapsedTime / fadeDuration
                : 1f;

            scopeCanvasGroup.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                progress
            );

            yield return null;
        }

        scopeCanvasGroup.alpha = targetAlpha;

        if (disableOnComplete && scopeImage != null)
        {
            scopeImage.SetActive(false);
        }

        fadeRoutine = null;
    }

    // ---------------- WEAPON SWITCHING / DISABLE ----------------

    private void OnDisable()
    {
        isScoped = false;

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        if (scopeCanvasGroup != null)
        {
            scopeCanvasGroup.alpha = 0f;
        }

        if (scopeImage != null)
        {
            scopeImage.SetActive(false);
        }

        ForceMainCameraActive();
    }
}
