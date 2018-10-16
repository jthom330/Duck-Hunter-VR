using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuckSpawner : MonoBehaviour {

	public DuckBehavior Spawn(GameObject duck)
    {
        Globals.aliveDucks++;        
        Vector3 spawnPosition = new Vector3(transform.position.x + Random.Range(-20.0f, 20.0f), transform.position.y, transform.position.x + Random.Range(-9.0f, 9.0f));
        GameObject prefab = Instantiate(duck, spawnPosition, Quaternion.identity);        

        return prefab.GetComponentInChildren<DuckBehavior>();
    }
}
