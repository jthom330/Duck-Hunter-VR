using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FireGun : MonoBehaviour
{

    public Transform rayOrigin;
    public Animator gunAnimator;
    public ParticleSystem gunFireParticles;
    public AudioSource shotSound;
    public ParticleSystem hitParticle;
    public UIController uiController;
    public GameObject floatingScoreCanvas;

    public float maxDistanceRay = 100f;
    private int layerMask;

    void Start()
    {
        //used so that ray casts can only hit objects in "Target" layer
        layerMask = LayerMask.GetMask("Target");
    }

    // Update is called once per frame
    void Update()
    {
        //fire gun
        if (GvrControllerInput.GetDevice(GvrControllerHand.Dominant).GetButtonDown(GvrControllerButton.TouchPadButton) && Globals.canFireGun && (Globals.shotsLeft > 0 || uiController.isAtSelectionMenu()))
        {
            //reduce shot count
            if (!uiController.isAtSelectionMenu())
            {
                Globals.shotsLeft--;
                uiController.StrikeShell(Mathf.Abs(Globals.shotsLeft - 2));
            }

            //cast ray from gun
            FireRayCast();

            //play FX
            gunAnimator.SetTrigger("Fire");
            gunFireParticles.Play();
            shotSound.Play();

            //disable firing again until animation finishes
            Globals.canFireGun = false;            
        }
    }

    /**
     * Handles Ray cast and collisons associated  
     */
    void FireRayCast()
    {
        //create ray from a provided point in space
        Vector3 rayStartVector = new Vector3(rayOrigin.position.x, rayOrigin.position.y, rayOrigin.position.z);
        Ray ray = new Ray(rayStartVector, rayOrigin.forward);

        RaycastHit hit;

        //cast ray forward a given distance and only register hits with the specified layer
        if (Physics.SphereCast(rayStartVector, 1f, rayOrigin.forward, out hit, 100f, layerMask))
        {
            //create and play particle system at hit position 
            ParticleSystem hitSystem = Instantiate(hitParticle, hit.point, Quaternion.identity);
            StartCoroutine(PlayHitEffect(hitSystem));

            //call die function on hit object
            hit.collider.gameObject.GetComponentInChildren<DuckBehavior>().Die();

            //handles score and kill changes
            Globals.roundKills++;
            int plusScore = Globals.IncreaseScore();
            GameObject floatingTextCanvas = Instantiate(floatingScoreCanvas, hit.point, Quaternion.identity);
            floatingTextCanvas.transform.LookAt(2 * floatingTextCanvas.transform.position - Camera.main.transform.position);
            TextMeshProUGUI scoreText = floatingTextCanvas.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            scoreText.text = plusScore.ToString();

            uiController.UpdateSignScore();
        }
    }

    /**
     * Play particle system, then destroy it after the animation finishes
     */ 
    private IEnumerator PlayHitEffect(ParticleSystem system)
    {
        system.Play();
        yield return new WaitForSeconds(system.main.duration);
        Destroy(system.gameObject);
    }

}
