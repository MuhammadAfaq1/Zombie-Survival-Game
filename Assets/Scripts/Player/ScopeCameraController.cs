
using UnityEngine;

public class ScopeCameraController : MonoBehaviour
{
    public Transform playerTransform;
    public float sensitivity = 5f;

    public float minXAngle = -30f;
    public float maxXAngle = 30f;

    public float minYAngle = -360f;
    public float maxYAngle = 360f;

    public float smoothSpeed = 10f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        sensitivity = PlayerPrefs.GetFloat("Sensitivity", 2f);

        SyncWithMainCamera();
    }

    void Update()
    {
        // Only process mouse movement when this camera is active.
        if (!enabled || !gameObject.activeInHierarchy)
            return;

        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        rotationX -= mouseY;
        rotationY += mouseX;

        rotationX = Mathf.Clamp(rotationX, minXAngle, maxXAngle);
        rotationY = Mathf.Clamp(rotationY, minYAngle, maxYAngle);

        Quaternion targetRotation =
            Quaternion.Euler(rotationX, rotationY, 0f);

        if (playerTransform != null)
        {
            playerTransform.rotation = Quaternion.Slerp(
                playerTransform.rotation,
                targetRotation,
                smoothSpeed * Time.deltaTime
            );
        }

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            smoothSpeed * Time.deltaTime
        );
    }

    // Synchronize internal rotation values with the Main Camera.
    public void SyncWithMainCamera()
    {
        Vector3 angles = transform.eulerAngles;

        rotationX = angles.x;

        if (rotationX > 180f)
            rotationX -= 360f;

        rotationY = angles.y;

        rotationX = Mathf.Clamp(rotationX, minXAngle, maxXAngle);
        rotationY = Mathf.Clamp(rotationY, minYAngle, maxYAngle);
    }
}
