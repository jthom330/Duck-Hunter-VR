using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellBehavior : MonoBehaviour {

    public float fadeSpeed = 1f;
    private Transform shellModel;

	// Use this for initialization
	void Start () {
        shellModel = transform.GetChild(0);
        StartCoroutine(AlphaFade());
    }
	
	// Update is called once per frame
	void Update () {
       
    }

    // This method fades only the alpha.
    IEnumerator AlphaFade()
    {
        // Alpha start value.
        float alpha = 1.0f;

        // Loop until aplha is below zero (completely invisalbe)
        while (alpha > 0.0f)
        {
            // Reduce alpha by fadeSpeed amount.
            alpha -= fadeSpeed * Time.deltaTime;

            //update alpha values and set it back
            for (int m = 0; m < shellModel.GetComponent<MeshRenderer>().materials.Length; m++)
            {
                Color color = shellModel.GetComponent<MeshRenderer>().materials[m].color;
                color.a = alpha;
                shellModel.GetComponent<MeshRenderer>().materials[m].color = color;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
