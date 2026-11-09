
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Button normalDifficultyButton;
    public Button hardDifficultyButton;

    public Slider smoothSpeedSlider;
    public Slider sensitivitySlider;

    public Button SaveButton;

    [SerializeField] private float smoothSpeed;
    [SerializeField] private float sensitivity;
    [SerializeField] private float timeBetweenWaves;
    [SerializeField] private int zombiePerWave;

    public Button map1Button;
    public Button map2Button;
    public Button continueGameButton;

    [SerializeField] private string selectedMap;

    void Start()
    {
        smoothSpeedSlider.minValue = 1f;
        smoothSpeedSlider.maxValue = 10f;

        sensitivitySlider.minValue = 1f;
        sensitivitySlider.maxValue = 5f;

        // Default settings
        timeBetweenWaves = 10f;
        zombiePerWave = 2;
        smoothSpeed = 10f;
        sensitivity = 2f;

        LoadSettings();

        normalDifficultyButton.onClick.AddListener(SetNormalDifficulty);
        hardDifficultyButton.onClick.AddListener(SetHardDifficulty);

        map1Button.onClick.AddListener(() => SelectMap("Map1"));
        map2Button.onClick.AddListener(() => SelectMap("Map2"));

        continueGameButton.onClick.AddListener(LoadSelectedMap);
        SaveButton.onClick.AddListener(SaveSettings);
    }

    private void SetNormalDifficulty()
    {
        timeBetweenWaves = 10f;
        zombiePerWave = 2;
    }

    private void SetHardDifficulty()
    {
        timeBetweenWaves = 7f;
        zombiePerWave = 4;
    }

    public void SaveSettings()
    {
        smoothSpeed = smoothSpeedSlider.value;
        sensitivity = sensitivitySlider.value;

        PlayerPrefs.SetFloat("TimeBetweenWaves", timeBetweenWaves);
        PlayerPrefs.SetInt("ZombiePerWave", zombiePerWave);

        PlayerPrefs.SetFloat("SmoothSpeed", smoothSpeed);
        PlayerPrefs.SetFloat("Sensitivity", sensitivity);

        PlayerPrefs.Save();

        Debug.Log("Settings saved!");
        Debug.Log("Sensitivity: " + sensitivity);
        Debug.Log("Smooth Speed: " + smoothSpeed);
    }

    public void LoadSettings()
    {
        timeBetweenWaves = PlayerPrefs.GetFloat("TimeBetweenWaves", 10f);
        zombiePerWave = PlayerPrefs.GetInt("ZombiePerWave", 2);

        smoothSpeed = PlayerPrefs.GetFloat("SmoothSpeed", 10f);
        sensitivity = PlayerPrefs.GetFloat("Sensitivity", 2f);

        smoothSpeedSlider.value = smoothSpeed;
        sensitivitySlider.value = sensitivity;

        selectedMap = PlayerPrefs.GetString("SelectedMap", "");
    }

    private void SelectMap(string mapName)
    {
        selectedMap = mapName;

        PlayerPrefs.SetString("SelectedMap", selectedMap);
        PlayerPrefs.Save();
    }

    public void LoadSelectedMap()
    {
        if (string.IsNullOrEmpty(selectedMap))
        {
            Debug.LogWarning("Please select a map first!");
            return;
        }

        SceneManager.LoadScene(selectedMap);
    }
}
