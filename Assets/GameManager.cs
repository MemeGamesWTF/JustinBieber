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
    private float timer = 10f; // Timer starts at 60 seconds
    private bool isTimerRunning = false;

    public Slider progressBar;

    private float lastTapTime = 0f; // Time since the last tap
    private float tapDecayDelay = 0f; // Time before value starts decreasing
    public float decayRate = 0.3f;

    public GameObject[] peopleSad;
    public GameObject[] peopleHappy;
    //public GameObject[] ;
     public Animator Playeranimators;
    public AudioSource[] PlayerAudioSource;
    [DllImport("__Internal")]
    private static extern void SendScore(int score, int game);

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
           // Debug.LogWarning("Another instance of GameManager already exists. Destroying this instance.");
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
                // Debug.Log("gameover");
                PlayerAudioSource[0].Play();
                GameOVer(); // End the game when the timer runs out
               
            }

            UpdateTimerUI();
        }


        Playeranimators.SetFloat("Val", progressBar.value / progressBar.maxValue);
        if (Input.GetMouseButtonDown(0))
        {
            IncreaseSliderValue(0.4f);
            PlayerAudioSource[2].Play();
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) // Detects only the initial tap, ignoring holds
            {
                IncreaseSliderValue(0.1f);
                PlayerAudioSource[2].Play();
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
                activePeople();
                //  Debug.Log("Slider reached 75% (Three-Quarter Way)");
            }

            if (progressBar.value >= progressBar.maxValue)
            {
                PlayerAudioSource[1].Play();
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

                deactivePeople();
                // Debug.Log("Slider dropped below 75%");
            }
            if (previousValue >= halfway && progressBar.value < halfway)
            {
          

              

                // Debug.Log("Slider dropped below 50%");
            }
            if (previousValue >= quarterWay && progressBar.value < quarterWay)
            {
               

                // Debug.Log("Slider dropped below 25%");
            }


            
        }
    }
    public void activePeople()
    {
        peopleSad[0].gameObject.SetActive(false);
        peopleSad[1].gameObject.SetActive(false);
        peopleSad[2].gameObject.SetActive(false);
        peopleSad[3].gameObject.SetActive(false);
        peopleSad[4].gameObject.SetActive(false);
        peopleSad[5].gameObject.SetActive(false);

        peopleHappy[0].gameObject.SetActive(true);
        peopleHappy[1].gameObject.SetActive(true);
        peopleHappy[2].gameObject.SetActive(true);
        peopleHappy[3].gameObject.SetActive(true);
        peopleHappy[4].gameObject.SetActive(true);
        peopleHappy[5].gameObject.SetActive(true);
    }
    public void deactivePeople()
    {
        peopleSad[0].gameObject.SetActive(true);
        peopleSad[1].gameObject.SetActive(true);
        peopleSad[2].gameObject.SetActive(true);
        peopleSad[3].gameObject.SetActive(true);
        peopleSad[4].gameObject.SetActive(true);
        peopleSad[5].gameObject.SetActive(true);

        peopleHappy[0].gameObject.SetActive(false);
        peopleHappy[1].gameObject.SetActive(false);
        peopleHappy[2].gameObject.SetActive(false);
        peopleHappy[3].gameObject.SetActive(false);
        peopleHappy[4].gameObject.SetActive(false);
        peopleHappy[5].gameObject.SetActive(false);
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timer / 10);
        int seconds = Mathf.FloorToInt(timer % 10);
        TimerText.text = $"{minutes:00}:{seconds:00}";
    }
    public void GameWin()
    {
        GameState = false;
        GameWinScreen.SetActive(true);
       // Debug.Log(currentScore);
        SendScore(currentScore, 69);
       
    }

    public void GameOVer()
    {
        GameState = false;
        //Debug.Log(currentScore);
        GameOverScreen.SetActive(true);
        SendScore(currentScore, 69);
        
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
      
        timer = 10f;
        isTimerRunning = true;
        ScoreText.text = "0";
        Score.score = 0;
        currentScore = 0;
        deactivePeople();
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
