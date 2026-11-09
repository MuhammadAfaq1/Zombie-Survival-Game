
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Weapons")]
    public GameObject[] weapons;

    [Header("Shooting Controller")]
    public ShootingController shootingController;

    [Header("Weapon Settings")]
    public GameObject[] muzzleFlashes;
    public Transform[] firePoints;

    [SerializeField] private int currentWeaponIndex = 0;

    [Header("Score Settings")]
    public int currentScore = 0;
    public int highScore = 0;

    [Header("Zombie Statistics")]
    public int totalZombiesKilled = 0;
    public int currentWave = 1;

    [Header("UI Text References")]
    public TMP_Text currentScoreText;
    public TMP_Text highScoreText;
    public TMP_Text totalZombiesKilledText;
    public TMP_Text waveNumberText;
[   Header("Main Menu")]
    public string mainMenuSceneName = "MainMenu";
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    private void Start()
    {
        // Initialize weapons.
        if (weapons == null || weapons.Length == 0)
        {
            Debug.LogError("No weapons assigned to GameManager!");
        }
        else
        {
            currentWeaponIndex = Mathf.Clamp(
                currentWeaponIndex, 0, weapons.Length - 1
            );

            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] != null)
                    weapons[i].SetActive(i == currentWeaponIndex);
            }

            UpdateWeaponSettings(currentWeaponIndex);
        }

        UpdateUI();
    }

   private void Update()
{
    // Return to Main Menu when Escape is pressed.
    if (Input.GetKeyDown(KeyCode.Escape))
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(mainMenuSceneName);
        return;
    }

    if (Input.GetKeyDown(KeyCode.Alpha1))
        SwitchWeapon(0);
    else if (Input.GetKeyDown(KeyCode.Alpha2))
        SwitchWeapon(1);
    else if (Input.GetKeyDown(KeyCode.Alpha3))
        SwitchWeapon(2);
    else if (Input.GetKeyDown(KeyCode.Alpha4))
        SwitchWeapon(3);
}
    // WEAPON SWITCHING

    private void SwitchWeapon(int newIndex)
    {
        if (weapons == null ||
            newIndex < 0 ||
            newIndex >= weapons.Length)
        {
            Debug.LogWarning("Invalid weapon index: " + newIndex);
            return;
        }

        if (weapons[newIndex] == null)
        {
            Debug.LogWarning("Weapon " + newIndex + " is not assigned!");
            return;
        }

        if (newIndex == currentWeaponIndex)
            return;

        // Hide previous weapon.
        if (weapons[currentWeaponIndex] != null)
            weapons[currentWeaponIndex].SetActive(false);

        // Activate selected weapon.
        weapons[newIndex].SetActive(true);

        currentWeaponIndex = newIndex;

        // Update shooting references.
        UpdateWeaponSettings(currentWeaponIndex);

        Debug.Log("Switched to weapon: " +
                  weapons[currentWeaponIndex].name);
    }

    private void UpdateWeaponSettings(int weaponIndex)
    {
        if (shootingController == null)
        {
            Debug.LogError("ShootingController is not assigned!");
            return;
        }

        // Update muzzle flash.
        if (muzzleFlashes != null &&
            weaponIndex < muzzleFlashes.Length)
        {
            shootingController.SetMuzzleFlash(
                muzzleFlashes[weaponIndex]
            );
        }

        // Update fire point.
        if (firePoints != null &&
            weaponIndex < firePoints.Length)
        {
            shootingController.firePoint = firePoints[weaponIndex];
        }
    }

    // SCORE SYSTEM

    public void AddScore(int amount)
    {
        currentScore += amount;

        // Increase kills whenever positive score is awarded.
        if (amount > 0)
        {
            totalZombiesKilled += amount;
        }

        // Update high score.
        if (currentScore > highScore)
        {
            highScore = currentScore;

            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        UpdateUI();

        Debug.Log("Score: " + currentScore +
                  " | Kills: " + totalZombiesKilled);
    }

    // KILL COUNTER
    // Do not call this separately when AddScore() handles kills.

    public void ZombieKilled()
    {
        // Intentionally empty to prevent double counting.
    }

    // WAVE COUNTER

    public void SetCurrentWave(int wave)
    {
        currentWave = wave;
        UpdateUI();
    }

    // UPDATE UI

    private void UpdateUI()
    {
        if (currentScoreText != null)
            currentScoreText.text = currentScore.ToString();

        if (highScoreText != null)
            highScoreText.text = highScore.ToString();

        if (totalZombiesKilledText != null)
            totalZombiesKilledText.text = "Kills:" +
                totalZombiesKilled.ToString();

        if (waveNumberText != null)
            waveNumberText.text = currentWave.ToString();
    }

    // RESET SCORE AND STATISTICS

    public void ResetCurrentScore()
    {
        currentScore = 0;
        totalZombiesKilled = 0;
        currentWave = 1;

        UpdateUI();
    }
}
