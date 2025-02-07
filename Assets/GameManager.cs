using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Static instance for the singleton
    public static GameManager Instance { get; private set; }

    public int GameID = 0;

    public GameObject GameOverScreen, GameWinScreen, InfoScreen;
    public bool GameState = false;
    public BasePlayer Player;

    public Text ScoreText;
    private int currentScore;

    private ScoreObj Score;

    public Text TimerText;  // Assign this in the Unity Inspector
    private float timer = 30f; // Timer starts at 60 seconds
    private bool isTimerRunning = false;

    public Slider progressBar;

    private float lastTapTime = 0f; // Time since the last tap
    private float tapDecayDelay = 0f; // Time before value starts decreasing
    public float decayRate = 0.3f;

   // public GameObject[] people;
    //public GameObject[] ;
     public Animator Playeranimators;

    [DllImport("__Internal")]
    private static extern void SendScore(int score, int game);

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Another instance of GameManager already exists. Destroying this instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }

    private void Start()
    {
        InfoScreen.SetActive(true);
        if (progressBar != null)
            progressBar.value = 5;
    }

    void Update()
    {
        if (!GameState)
            return;

        if (GameState && isTimerRunning)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                timer = 0;
                Debug.Log("gameover");

                GameOVer(); // End the game when the timer runs out
            }

            UpdateTimerUI();
        }


        Playeranimators.SetFloat("Val", progressBar.value / progressBar.maxValue);
        if (Input.GetMouseButtonDown(0))
        {
            IncreaseSliderValue(0.3f);
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) // Detects only the initial tap, ignoring holds
            {
                IncreaseSliderValue(0.1f);
            }
        }


        if (Time.time - lastTapTime > tapDecayDelay)
        {
            DecreaseSliderValue();
        }
        //GAME LOGIC

    }

    private void IncreaseSliderValue(float increment)
    {
        if (progressBar != null)
        {
            float previousValue = progressBar.value;
            progressBar.value += increment; // Increase slider value
            progressBar.value = Mathf.Clamp(progressBar.value, 0, progressBar.maxValue);
            AddScore();

            float quarterWay = progressBar.maxValue * 0.25f;
            float halfway = progressBar.maxValue * 0.5f;
            float threeQuarterWay = progressBar.maxValue * 0.75f;
           

            if (previousValue < quarterWay && progressBar.value >= quarterWay)
            {

                

                //Debug.Log("Slider reached 25% (Quarter Way)");
            }
            if (previousValue < halfway && progressBar.value >= halfway)
            {
              
              //  Debug.Log("Slider reached 50% (Halfway)");
            }
            if (previousValue < threeQuarterWay && progressBar.value >= threeQuarterWay)
            {
              //  Debug.Log("Slider reached 75% (Three-Quarter Way)");
            }

            if (progressBar.value >= progressBar.maxValue)
            {
                GameWin();
            }

        }
        lastTapTime = Time.time;
    }
    private void DecreaseSliderValue()
    {
        float previousValue = progressBar.value;
        if (progressBar != null && progressBar.value > 0)
        {
            progressBar.value -= decayRate * Time.deltaTime; // Decrease over time
            progressBar.value = Mathf.Clamp(progressBar.value, 0, progressBar.maxValue);

            float quarterWay = progressBar.maxValue * 0.25f;
            float halfway = progressBar.maxValue * 0.5f;
            float threeQuarterWay = progressBar.maxValue * 0.75f;

            if (previousValue >= threeQuarterWay && progressBar.value < threeQuarterWay)
            {
               // Playeranimators.SetBool("isEnd", false);
              //

                // Debug.Log("Slider dropped below 75%");
            }
            if (previousValue >= halfway && progressBar.value < halfway)
            {
               // Playeranimators.SetBool("isMiddle", false);
              //  Playeranimators.SetBool("isStart", true);
               

                // Debug.Log("Slider dropped below 50%");
            }
            if (previousValue >= quarterWay && progressBar.value < quarterWay)
            {
               // Playeranimators.SetBool("isStart", false);
                // Reset people state or apply other logic
                activePeople();

                // Debug.Log("Slider dropped below 25%");
            }


            // ReduceScore();
        }
    }
    public void activePeople()
    {
       // people[0].gameObject.SetActive(true);
       // people[1].gameObject.SetActive(true);
       // people[2].gameObject.SetActive(true);
       // people[3].gameObject.SetActive(true);
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timer / 30);
        int seconds = Mathf.FloorToInt(timer % 30);
        TimerText.text = $"{minutes:00}:{seconds:00}";
    }
    public void GameWin()
    {
        GameState = false;
        GameWinScreen.SetActive(true);
        Debug.Log(currentScore);
        SendScore((int)currentScore, GameID);
       
    }

    public void GameOVer()
    {
        GameState = false;
        Debug.Log(currentScore);
        GameOverScreen.SetActive(true);
        SendScore((int)currentScore, GameID);
        
    }
    public void AddScore()
    {


        if (int.TryParse(ScoreText.text, out currentScore))
        {
            currentScore += 10;
            ScoreText.text = currentScore.ToString();
        }
        else
        {

            ScoreText.text = "0";
        }
    }
   

    public void GameResetScreen()
    {
       
        timer = 30f;
        isTimerRunning = true;
        ScoreText.text = "0";
        Score.score = 0;
        currentScore = 0;
        activePeople();
        UpdateTimerUI();
        Playeranimators.SetBool("isEnd", false);
        InfoScreen.SetActive(false);
        GameOverScreen.SetActive(false);
        GameWinScreen.SetActive(false);
        GameState = true;
        if (progressBar != null)
            progressBar.value = 3;

        Player.Reset();
    }

    public void AddScore(float f)
    {
        Score.score += f;
    }



    //HELPER FUNTION TO GET SPAWN POINT
    public Vector2 GetRandomPointInsideSprite(SpriteRenderer SpawnBounds)
    {
        if (SpawnBounds == null || SpawnBounds.sprite == null)
        {
            Debug.LogWarning("Invalid sprite renderer or sprite.");
            return Vector2.zero;
        }

        Bounds bounds = SpawnBounds.sprite.bounds;
        Vector2 randomPoint = new Vector2(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y)
        );

        // Transform local point to world space
        return SpawnBounds.transform.TransformPoint(randomPoint);
    }


    public struct ScoreObj
    {
        public float score;
    }
}
