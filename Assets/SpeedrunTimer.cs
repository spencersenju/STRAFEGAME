using UnityEngine;
using TMPro;

public class SpeedrunTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    private float timer;
    private bool isRunning = true;

    void Start()
    {
        timer = 0f;
        isRunning = true;
    }

    void Update()
    {
        if (!isRunning) return;

        timer += Time.deltaTime;
        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);
        int milliseconds = Mathf.FloorToInt((timer * 100f) % 100);

        timerText.text = $"{minutes:00}:{seconds:00}.{milliseconds:00}";
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public float GetTime()
    {
        return timer;
    }
}
