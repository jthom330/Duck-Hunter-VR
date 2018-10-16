using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IntroController : MonoBehaviour {

    public GameObject title;
    public Animator buttonAnimator;
    public GameObject highScorePanel;
    public UIController uiController;
    public Animator fadeAnimator;
    public Transform player;
    public Transform gamePosition;
    public GameController gameController;
    public TextMeshProUGUI highScoreText;

	// Use this for initialization
	void Start () {
        uiController.setIsAtSelectedMenu(true);
        StartCoroutine(SetTitleScreen());
    }

    public IEnumerator SetTitleScreen()
    {
        yield return new WaitForSeconds(2);
        title.SetActive(true);
        yield return new WaitForSeconds(1);
        buttonAnimator.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        buttonAnimator.gameObject.SetActive(true);

        //high score stuff
        highScoreText.text = "1st: "+PlayerPrefs.GetInt("First",0).ToString()+"\n"+ "2nd: " + PlayerPrefs.GetInt("Second", 0).ToString() + "\n" + "3rd: " + PlayerPrefs.GetInt("Third", 0).ToString();

        yield return new WaitForSeconds(1);
        highScorePanel.SetActive(true);
    }

    public void StartGame()
    {
        //play button animation
        buttonAnimator.SetTrigger("Clicked");

        StartCoroutine(StartGameCoroutine());
    }

    private IEnumerator StartGameCoroutine()
    {
        
        //pause for 1ish seconds
        yield return new WaitForSeconds(1);
        //fade to black
        fadeAnimator.SetTrigger("Fade");
        yield return new WaitForSeconds(2);

        //move camera
        player.position = gamePosition.position;
        player.rotation = gamePosition.rotation;

        //fade in
        yield return new WaitForSeconds(2);

        //enable game controller
        gameController.enabled = true;

        uiController.setIsAtSelectedMenu(false);

        //disable fade canvas
        Destroy(fadeAnimator.transform.parent.gameObject);

        //disable this canvas
        Destroy(gameObject);
    }
}
