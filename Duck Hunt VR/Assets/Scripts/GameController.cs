using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public GameObject target;
    public DuckSpawner spawner;
    public float flyAwayTimer = 15f;
    public UIController uiController;
    public AudioSource audioSource;
    public AudioClip subWin;
    public AudioClip roundWin;
    public AudioClip subLose;
    public AudioClip roundLose;


    private float currentTime = 0;
    private List<DuckBehavior> ducks;
    private bool paused = false;
    private bool didFlyAway = false;

	// Use this for initialization
	void Start () {
        ducks = new List<DuckBehavior>();

        Globals.ResetAll();
        uiController.ResetSign();

        paused = true;
        StartCoroutine(NewRound(true));
	}
	
	// Update is called once per frame
	void Update () {

        if (!paused && Globals.aliveDucks > 0 && (currentTime >= flyAwayTimer || (Globals.shotsLeft <= 0 && currentTime > 4f)))
        {
            paused = true;
            StartCoroutine(FlyAwayActions());
        }
        else if (!paused && Globals.aliveDucks <= 0 && Globals.ducksSpawnedThisRound >= 10)
        {
            paused = true;
            if (Globals.roundKills >= Globals.roundKillRequirment)
            {
                StartCoroutine(NewRound(false));
            }
            else
            {
                GameOverUI();
            }

        }
        else if (!paused && Globals.aliveDucks <= 0 && Globals.ducksSpawnedThisRound < 10)
        {
            paused = true;
            StartCoroutine(BreifPauseandSpawn());
        }
        currentTime += Time.deltaTime;
	}

    private IEnumerator NewRound(bool first)
    {    

        if (!first)
        {
            Globals.canFireGun = false;
            Globals.ducksSpawnedThisRound = 0;
            Globals.round++;
            Globals.IncreaseDoubleSpawnRate();
            Globals.IncrementSpeed();
            Globals.UpdateKillRequirment();
            Globals.roundKills = 0;
        }

        yield return new WaitForSeconds(2);

        //play sfx
        audioSource.clip = roundWin;
        audioSource.Play();

        //Show UI
        uiController.ShowNewRound();
        uiController.UpdateSignRound();
        uiController.ResetStrikeDucks();


        yield return new WaitForSeconds(3);
        Globals.ResetShots();

        //remove UI
        uiController.HideNewRound();

        yield return new WaitForSeconds(1);

        paused = false;
    }

    private IEnumerator BreifPauseandSpawn()
    {
        Globals.canFireGun = false;
      
        yield return new WaitForSeconds(2);

        //play sfx if flyaway didn't happen
        if (!didFlyAway && Globals.ducksSpawnedThisRound > 0)
        {
            audioSource.clip = subWin;
            audioSource.Play();
        }
        Globals.ResetShots();
        uiController.ResetStrikeShells();

        yield return new WaitForSeconds(1);

        Globals.canFireGun = true;

        StartCoroutine(Spawn());

        didFlyAway = false;
        currentTime = 0;
        paused = false;
    }

    private IEnumerator FlyAwayActions()
    {
        Globals.canFireGun = false;
        didFlyAway = true;

        //TODO: play flyaway action
        for (int i = 0; i < ducks.Count; i++)
        {
            if (ducks[i] != null)
            {
                StartCoroutine(ducks[i].FlyAway());
            }
        }

        yield return new WaitForSeconds(2);

        //play sfx
        audioSource.clip = subLose;
        audioSource.Play();

        //Show UI
        uiController.ShowFlyAway();

        yield return new WaitForSeconds(3);
        Globals.ResetShots();

        //remove UI
        uiController.HideFlyAway();

        yield return new WaitForSeconds(1);
        paused = false;
        currentTime = 0;
    }

    private void GameOverUI()
    {
        SetHighScores();

        //play sfx
        audioSource.clip = roundLose;
        audioSource.Play();

        //Show UI
        uiController.ShowGameOver();        
    }

    private IEnumerator Spawn()
    {
        ducks.Clear();

        //Spawn 1 or 2 ducks
        ducks.Add(spawner.Spawn(target));

        yield return new WaitForSeconds(1);
        Random.InitState((int)System.DateTime.Now.Ticks);
        if (Globals.doubleDuckSpawnChance >= Random.Range(0.0f, 1.0f) && Globals.ducksSpawnedThisRound < 10)
        {
            ducks.Add(spawner.Spawn(target));
        }
    }

    public void Restart()
    {
        Globals.ResetAll();
        uiController.ResetSign();

        paused = true;
        StartCoroutine(NewRound(true));
        uiController.HideGameOver();
    }

    public void Quit()
    {
        Application.Quit();
    }

    private void SetHighScores()
    {
        if(Globals.score > PlayerPrefs.GetInt("First", 0))
        {
            PlayerPrefs.SetInt("Third", PlayerPrefs.GetInt("Second", 0));
            PlayerPrefs.SetInt("Second", PlayerPrefs.GetInt("First", 0));
            PlayerPrefs.SetInt("First", Globals.score);
        }
        else if (Globals.score > PlayerPrefs.GetInt("Second", 0))
        {
            PlayerPrefs.SetInt("Third", PlayerPrefs.GetInt("Second", 0));
            PlayerPrefs.SetInt("Second", Globals.score);
        }
        else if (Globals.score > PlayerPrefs.GetInt("Third", 0))
        {
            PlayerPrefs.SetInt("Third", Globals.score);
        }
    }
}
