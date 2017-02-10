using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Request : MonoBehaviour {
    public static float timeRadius;
    public bool pickedUp;
    public float requestTime;
    public Vector2 destination;
    public Vector2 requestLocation;
    public float tripLength;
    public bool matchedToVehicle;
    public bool droppedOff;

    private float currentTime;

    //Check for if it is currently being scored by a separate vehicle.
    public bool beingEvaled;
	// Use this for initialization
	void Start () {
        beingEvaled = false;
        matchedToVehicle = false;
        pickedUp = false;
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        foreach(SpriteRenderer renderer in gameObject.GetComponentsInChildren<SpriteRenderer>())
        {
            renderer.enabled = false;
        }
	}

    // Update is called once per frame
    void Update()
    {
        currentTime = GameObject.FindGameObjectWithTag("Simulation_Controller").GetComponent<SimulationController>().currentTime;
        if (!matchedToVehicle)
        {
            if (currentTime >= requestTime && currentTime <= requestTime + timeRadius)
            {

                gameObject.GetComponent<BoxCollider2D>().enabled = true;
                foreach (SpriteRenderer renderer in gameObject.GetComponentsInChildren<SpriteRenderer>())
                {
                    if(renderer.gameObject.tag == "Request_Dest")
                    {
                        renderer.enabled = false;
                    }
                    else
                    {
                        renderer.enabled = true;
                    }
                }
            }
            else if (currentTime < requestTime)
            {
                gameObject.GetComponent<BoxCollider2D>().enabled = false;
                foreach(SpriteRenderer renderer in gameObject.GetComponentsInChildren<SpriteRenderer>())
                {
                    renderer.enabled = false;
                }
            }
            else if (currentTime > requestTime + timeRadius && !beingEvaled)
            {
                gameObject.SetActive(false);
            }
        }
        else if (matchedToVehicle && !pickedUp && !droppedOff)
        {
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
            foreach(SpriteRenderer renderer in gameObject.GetComponentsInChildren<SpriteRenderer>())
            {
                if(renderer.gameObject.tag == "Request_Dest")
                {
                    renderer.enabled = false;
                }
                else
                {
                    renderer.enabled = true;
                }
            }
        }
        else if (matchedToVehicle && pickedUp && !droppedOff)
        {
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
            foreach (SpriteRenderer renderer in gameObject.GetComponentsInChildren<SpriteRenderer>())
            {
                if (renderer.gameObject.tag == "Request_Dest")
                {
                    renderer.enabled = true;
                    renderer.gameObject.transform.position = destination;
                }
                else
                {
                    renderer.enabled = false;
                }
            }
        }
        else if (droppedOff)
        {
            gameObject.SetActive(false);
        }
    }
}
