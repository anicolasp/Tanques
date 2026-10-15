using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelTimer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float timeLimit = 60f;
    [SerializeField] private string failScene = "GameOver";
    [SerializeField] private string nextLevelScene = "Level2";

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private float warningThreshold = 10f;

    private float _timeRemaining;
    private bool _isRunning = false;
    private bool _levelComplete = false;
    private bool _isPaused = false;

    void Start()
    {
        _timeRemaining = timeLimit;
        _isRunning = true;
        UpdateTimerUI();
    }

    void Update()
    {
        if (!_isRunning || _isPaused || _levelComplete) return;

        _timeRemaining -= Time.deltaTime;

        if (_timeRemaining <= 0f)
        {
            _timeRemaining = 0f;
            _isRunning = false;
            OnTimeExpired();
        }

        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(_timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(_timeRemaining % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";

        // Flash red when time is low
        if (_timeRemaining <= warningThreshold)
            timerText.color = warningColor;
        else
            timerText.color = Color.white;
    }

    /// Called by your level-completion logic (e.g. reaching the goal trigger)
    public void CompleteLevel()
    {
        
        _levelComplete = true;
        _isRunning = false;
        SceneManager.LoadScene(nextLevelScene);
    }

    void OnTimeExpired()
    {
        // Player failed to finish in time
        SceneManager.LoadScene(failScene);
    }

    /// Pause the timer (e.g. when opening a pause menu)
    public void PauseTimer() => _isPaused = true;
    public void ResumeTimer() => _isPaused = false;

    /// Modify time (e.g. collect a time-boost pickup)
    public void AddTime(float amount)
    {
        if (!_isRunning) return;
        _timeRemaining = Mathf.Max(0f, _timeRemaining + amount);
    }
}   