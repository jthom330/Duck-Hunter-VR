using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuckBehavior : MonoBehaviour {

    public GameObject target;
    public AudioSource audioSource;
    public Animator animator;
    public AudioClip hitSound;
    private UIController uiController;

    public float xMin = -20;
    public float xMax = 20;
    public float yMin = 12;
    public float yMax = 20;
    public float zMin = -10;
    public float zMax = 1;

    private float speed;
    private float timePassed = 0;
    private float timeToQuack = 2;
    private bool alive = true;
    private int duckNum;


    // Use this for initialization
    void Start () {
        Globals.ducksSpawnedThisRound++;
        uiController = GameObject.Find("/GameController").GetComponent<UIController>();
        duckNum = Globals.ducksSpawnedThisRound;
        Random.InitState((int)System.DateTime.Now.Ticks);
        target.transform.position = new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), Random.Range(zMin, zMax));
        speed = Random.Range(Globals.duckSpeed - 15f, Globals.duckSpeed);
    }

    // Update is called once per frame
    void Update () {
        if (alive)
        {
            transform.LookAt(target.transform);
            transform.Translate(0, 0, speed * Time.deltaTime);

            timePassed += Time.deltaTime;

            if (timePassed >= timeToQuack)
            {
                Random.InitState((int)System.DateTime.Now.Ticks);
                audioSource.pitch = Random.Range(0.8f, 1.2f);
                audioSource.Play();

                Random.InitState((int)System.DateTime.Now.Ticks);
                timeToQuack = Random.Range(1f, 4f);
                timePassed = 0;
            }
        }
        else
        {
            transform.Rotate(0, 0, 360 * Time.deltaTime);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.GetInstanceID() == target.gameObject.GetInstanceID())
        {
            Random.InitState((int)System.DateTime.Now.Ticks);
            target.transform.position = new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), Random.Range(zMin, zMax));
        }        
    }

    public void Die()
    {
        Globals.aliveDucks--;
        alive = false;
        uiController.StrikeDuck(getDuckNum()-1);
        Destroy(target);

        StartCoroutine(DeathBehavior());        
    }

    private IEnumerator DeathBehavior()
    {
        audioSource.clip = hitSound;
        audioSource.Play();

        animator.SetTrigger("Die");
        gameObject.GetComponent<Rigidbody>().useGravity = true;
        transform.LookAt(Vector3.down);

        yield return new WaitForSeconds(5f);
        Destroy(transform.parent.gameObject);
    }

    public IEnumerator FlyAway()
    {
        if (target != null)
        {
            gameObject.GetComponent<BoxCollider>().enabled = false;
            target.transform.position = new Vector3(0, 75, 160);

            float timeTaken = 3f;
            float currentTime = 0.0f;

            do
            {
                transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(0, 0, 0), currentTime / timeTaken);
                currentTime += Time.deltaTime;
                yield return null;
            } while (currentTime <= timeTaken);

            Globals.aliveDucks--;
            Destroy(transform.parent.gameObject);
        }

    }

    public int getDuckNum()
    {
        return duckNum;
    }
}
