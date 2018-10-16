using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

    public GameObject gameOverPanel;
    public TextMeshProUGUI scoreText;

    public GameObject flyAwayPanel;

    public GameObject roundPanel;
    public TextMeshProUGUI roundNumText;
    public TextMeshProUGUI killReqText;

    public TextMeshProUGUI signRound;
    public TextMeshProUGUI signScore;
    public Text[] shellStrikes;
    public Text[] duckStrikes;

    private bool isAtMenu = false;

    public void UpdateSignScore()
    {
        signScore.text = Globals.score.ToString();
    }

    public void UpdateSignRound()
    {
        signRound.text = Globals.round.ToString();
    }

    public void StrikeShell(int index)
    {
        shellStrikes[index].text = "/";
    }

    public void ResetStrikeShells()
    {
        for(int i=0; i < shellStrikes.Length; i++)
        {
            shellStrikes[i].text = "";
        }
    }

    public void StrikeDuck(int index)
    {
        duckStrikes[index].text = "/";
    }

    public void ResetStrikeDucks()
    {
        for (int i = 0; i < duckStrikes.Length; i++)
        {
            duckStrikes[i].text = "";
        }
    }

    public void ResetSign()
    {
        signScore.text = "0";
        signRound.text = "1";
        ResetStrikeDucks();
        ResetStrikeShells();
    }

    public void ShowFlyAway()
    {
        flyAwayPanel.SetActive(true);
    }

    public void HideFlyAway()
    {
        flyAwayPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        isAtMenu = true;
        scoreText.text = Globals.score.ToString();
        gameOverPanel.SetActive(true);
    }

    public void HideGameOver()
    {
        isAtMenu = false;
        gameOverPanel.SetActive(false);
    }

    public void ShowNewRound()
    {
        roundNumText.text = Globals.round.ToString();
        killReqText.text = Globals.roundKillRequirment.ToString();
        roundPanel.SetActive(true);
    }

    public void HideNewRound()
    {
        roundPanel.SetActive(false);
    }

    public bool isAtSelectionMenu()
    {
        return isAtMenu;
    }

    public void setIsAtSelectedMenu(bool val)
    {
        isAtMenu = val;
    }


}
