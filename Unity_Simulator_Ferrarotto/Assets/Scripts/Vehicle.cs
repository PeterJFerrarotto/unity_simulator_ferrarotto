using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum VEHICLE_STATE { VEHICLE_SCANNING_REQUESTS, VEHICLE_SCANNING_DEMAND, VEHICLE_GOING_TO_PICKUP, VEHICLE_GOING_TO_DEMAND_ZONE, VEHICLE_GOING_TO_DESTINATION, VEHICLE_WAITING_AT_PICKUP, VEHICLE_DROPPING_OFF };

public class Vehicle : MonoBehaviour {
    public static RequestGrid requestGrid = null;
    public static float speed;
    public static float unitSize;
    private static int radiusToUnitSize = 12;

    public static float timeRadius;
    public static float minimumScore;
    public static float tripDistanceWeight;
    public static float minimumSearchRadius;
    public static float maximumSearchRadius;
    public static float customSearchRadiusStep;
    public static float maximumDestinationRequestCount;

    public int requestsCompleted;
    public float distanceTravelledWithPassenger;
    public float distanceTravelledWithoutPassenger;

    public Vector2 startingPosition;

    private VEHICLE_STATE state;

    private Request currentRequest;
    private Request highestScoringRequest;
    private float highestScore;

    private Vector3 currentDestination;
    private Vector3 previousLocation;
    private float timeProjectedToArriveAtDestination;
    private float currentTime;

    private List<Request> collidedRequestsRadius1;
    private List<Request> collidedRequestsRadius2;
    private List<Request> collidedRequestsRadius3;
    // Use this for initialization
    void Start () {
        collidedRequestsRadius1 = new List<Request>();
        collidedRequestsRadius2 = new List<Request>();
        collidedRequestsRadius3 = new List<Request>();
        currentTime = 0;
        previousLocation = startingPosition;
        foreach(SpriteRenderer renderer in gameObject.GetComponentsInChildren<SpriteRenderer>())
        {
            renderer.enabled = renderer.gameObject.tag == "Vehicle";
        }
	}
	
	// Update is called once per frame
	void Update () {
        foreach(SpriteRenderer sprite in gameObject.GetComponentsInChildren<SpriteRenderer>())
        {
            if(sprite.gameObject.tag == "Thinking_Sprite" || sprite.gameObject.tag == "Waiting_Sprite")
            {
                sprite.gameObject.transform.localPosition = new Vector3(9 * Mathf.Sin(gameObject.transform.eulerAngles.z * Mathf.Deg2Rad), 16 * Mathf.Cos(gameObject.transform.eulerAngles.z * Mathf.Deg2Rad));
                sprite.gameObject.transform.rotation = Quaternion.identity;
            }
        }
        currentTime = GameObject.FindGameObjectWithTag("Simulation_Controller").GetComponent<SimulationController>().currentTime;
        gameObject.GetComponent<CircleCollider2D>().radius = maximumSearchRadius * radiusToUnitSize;
        switch (state)
        {
            case VEHICLE_STATE.VEHICLE_SCANNING_REQUESTS:
                foreach (SpriteRenderer sprite in gameObject.GetComponentsInChildren<SpriteRenderer>())
                {
                    sprite.enabled = sprite.gameObject.tag == "Thinking_Sprite" || sprite.gameObject.tag == "Vehicle";
                }
                ScanRequests();
                break;
            case VEHICLE_STATE.VEHICLE_GOING_TO_PICKUP:
                foreach (SpriteRenderer renderer in gameObject.GetComponentsInChildren<SpriteRenderer>())
                {
                    renderer.enabled = renderer.gameObject.tag == "Vehicle";
                }
                if (gameObject.transform.position == currentDestination)
                {
                    if(currentTime < currentRequest.requestTime)
                    {
                        state = VEHICLE_STATE.VEHICLE_WAITING_AT_PICKUP;
                    }
                    else
                    {
                        PickUpRequest();
                    }
                    distanceTravelledWithPassenger += currentRequest.tripLength;
                }
                else
                {
                    // 3
                    Vector3 vectorToTarget = currentDestination - transform.position;
                    // 4
                    vectorToTarget.z = 0;
                    vectorToTarget.Normalize();
                    float targetAngle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
                    targetAngle -= 90;
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, targetAngle), 1.0f);
                    gameObject.transform.position = Vector3.Lerp(previousLocation, currentDestination, currentTime / timeProjectedToArriveAtDestination);
                }
                break;
            case VEHICLE_STATE.VEHICLE_WAITING_AT_PICKUP:
                if(currentTime >= currentRequest.requestTime)
                {
                    PickUpRequest();
                }
                else
                {
                    foreach(SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
                    {
                        renderer.enabled = renderer.gameObject.tag == "Waiting_Sprite" || renderer.gameObject.tag == "Vehicle";
                    }
                }
                break;
            case VEHICLE_STATE.VEHICLE_GOING_TO_DESTINATION:
                foreach (SpriteRenderer renderer in gameObject.GetComponentsInChildren<SpriteRenderer>())
                {
                    renderer.enabled = renderer.gameObject.tag == "Vehicle";
                }
                if (gameObject.transform.position == currentDestination)
                {
                    DropOffRequest();
                }
                else
                {
                    // 3
                    Vector3 vectorToTarget = currentDestination - transform.position;
                    // 4
                    vectorToTarget.z = 0;
                    vectorToTarget.Normalize();
                    float targetAngle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
                    targetAngle -= 90;
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, targetAngle), 1.0f);
                    gameObject.transform.position = Vector3.Lerp(previousLocation, currentDestination, currentTime/timeProjectedToArriveAtDestination);
                }
                break;
            default:
                state = VEHICLE_STATE.VEHICLE_SCANNING_REQUESTS;
                break;
        }
	}

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Request")
        {
            if(other.gameObject.GetComponent<Request>().matchedToVehicle == false)
            {
                if(maximumSearchRadius == minimumSearchRadius)
                {
                    if (!collidedRequestsRadius1.Contains(other.gameObject.GetComponent<Request>()))
                    {
                        collidedRequestsRadius1.Add(other.gameObject.GetComponent<Request>());
                    }
                }
                else
                {
                    float dist = Vector2.Distance(gameObject.transform.position, other.gameObject.transform.position);
                    if (dist >= minimumSearchRadius * radiusToUnitSize && dist < minimumSearchRadius * radiusToUnitSize + radiusToUnitSize * customSearchRadiusStep)
                    {
                        if (!collidedRequestsRadius1.Contains(other.gameObject.GetComponent<Request>()))
                        {
                            collidedRequestsRadius1.Add(other.gameObject.GetComponent<Request>());
                        }
                    }
                    else if (dist >= minimumSearchRadius * radiusToUnitSize + radiusToUnitSize * customSearchRadiusStep && dist < maximumSearchRadius * radiusToUnitSize)
                    {
                        if (!collidedRequestsRadius2.Contains(other.gameObject.GetComponent<Request>()))
                        {
                            collidedRequestsRadius2.Add(other.gameObject.GetComponent<Request>());
                        }
                    }
                    else
                    {
                        if (!collidedRequestsRadius3.Contains(other.gameObject.GetComponent<Request>()))
                        {
                            collidedRequestsRadius3.Add(other.gameObject.GetComponent<Request>());
                        }
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Request")
        {
            if (collidedRequestsRadius1.Contains(other.gameObject.GetComponent<Request>()))
            {
                collidedRequestsRadius1.Remove(other.gameObject.GetComponent<Request>());
            }
            else if (collidedRequestsRadius2.Contains(other.gameObject.GetComponent<Request>()))
            {
                collidedRequestsRadius2.Remove(other.gameObject.GetComponent<Request>());
            }
            else if (collidedRequestsRadius3.Contains(other.gameObject.GetComponent<Request>()))
            {
                collidedRequestsRadius3.Remove(other.gameObject.GetComponent<Request>());
            }
        }
    }

    private float ScoreRequest(Request request)
    {
        request.beingEvaled = true;
        float currentTime = Time.time;
        float score = 0.0f;
        float lenToRequest = Vector3.Distance(gameObject.transform.position, request.requestLocation);
        float percentUtilization = (request.tripLength / (request.tripLength + lenToRequest)) * 10;
        float requestLengthValue = request.tripLength * tripDistanceWeight * 10;

        if(request.requestTime + timeRadius < (lenToRequest * unitSize) / speed)
        {
            return -1;
        }

        int requestsAtDestination = requestGrid.getRequestsAtLocation(request.destination);
        float destinationPenalty = 0;
        if (requestsAtDestination < maximumDestinationRequestCount)
        {
            destinationPenalty = 3 - (requestsAtDestination * (3 / maximumDestinationRequestCount));
        }

        score = percentUtilization + requestLengthValue - destinationPenalty;
        return score * 10;
    }

    private void ScanRequestsFirstRadius()
    {
        float tmpScore = 0.0f;
        foreach (Request request in collidedRequestsRadius1)
        {
            tmpScore = ScoreRequest(request);
            if (tmpScore > highestScore)
            {
                if (highestScoringRequest != null)
                {
                    highestScoringRequest.beingEvaled = false;
                }
                highestScoringRequest = request;
                highestScore = tmpScore;
            }
            else
            {
                request.beingEvaled = false;
            }
        }
    }

    private void ScanRequestsSecondRadius()
    {
        float tmpScore = 0.0f;
        foreach (Request request in collidedRequestsRadius2)
        {
            tmpScore = ScoreRequest(request);
            if (tmpScore > highestScore)
            {
                if (highestScoringRequest != null)
                {
                    highestScoringRequest.beingEvaled = false;
                }
                highestScoringRequest = request;
                highestScore = tmpScore;
            }
            else
            {
                request.beingEvaled = false;
            }
        }
    }

    private void ScanRequestsThirdRadius()
    {
        float tmpScore = 0.0f;
        foreach (Request request in collidedRequestsRadius3)
        {
            tmpScore = ScoreRequest(request);
            if (tmpScore > highestScore)
            {
                if (highestScoringRequest != null)
                {
                    highestScoringRequest.beingEvaled = false;
                }
                highestScoringRequest = request;
                highestScore = tmpScore;
            }
            else
            {
                request.beingEvaled = false;
            }
        }
    }

    private void ScanRequests()
    {
        collidedRequestsRadius1.RemoveAll(request => request.matchedToVehicle);
        collidedRequestsRadius2.RemoveAll(request => request.matchedToVehicle);
        collidedRequestsRadius3.RemoveAll(request => request.matchedToVehicle);

        collidedRequestsRadius1.RemoveAll(request => request.beingEvaled);
        collidedRequestsRadius2.RemoveAll(request => request.beingEvaled);
        collidedRequestsRadius3.RemoveAll(request => request.beingEvaled);

        collidedRequestsRadius1.RemoveAll(request => request.requestTime < currentTime - timeRadius);
        collidedRequestsRadius2.RemoveAll(request => request.requestTime < currentTime - timeRadius);
        collidedRequestsRadius3.RemoveAll(request => request.requestTime < currentTime - timeRadius);

        if (collidedRequestsRadius1.Count > 0)
        {
            ScanRequestsFirstRadius();
            if(highestScoringRequest == null)
            {
                ScanRequestsSecondRadius();
            }
            if(highestScoringRequest == null)
            {
                ScanRequestsThirdRadius();
            }
        }
        else if (collidedRequestsRadius2.Count > 0)
        {
            ScanRequestsSecondRadius();
            if (highestScoringRequest == null)
            {
                ScanRequestsThirdRadius();
            }
        }
        else if (collidedRequestsRadius3.Count > 0)
        {
            ScanRequestsThirdRadius();
        }
        if(highestScoringRequest == null)
        {
            StartScanningDemand();
        }
        else
        {
            currentRequest = highestScoringRequest;
            state = VEHICLE_STATE.VEHICLE_GOING_TO_PICKUP;
            currentDestination = currentRequest.requestLocation;
            currentRequest.matchedToVehicle = true;
            timeProjectedToArriveAtDestination = currentTime + (Vector3.Distance(gameObject.transform.position, currentDestination) * unitSize) / 30;
            foreach (SpriteRenderer sprite in gameObject.GetComponentsInChildren<SpriteRenderer>())
            {
                sprite.enabled = sprite.gameObject.tag == "Vehicle";
            }
        }
    }
    
    private void StartScanningRequests()
    {
        state = VEHICLE_STATE.VEHICLE_SCANNING_REQUESTS;
        highestScore = minimumScore;
        highestScoringRequest = null;
        currentRequest = null;
    }

    private void StartScanningDemand()
    {
        state = VEHICLE_STATE.VEHICLE_SCANNING_DEMAND;
    }

    private void DropOffRequest()
    {
        distanceTravelledWithPassenger += currentRequest.tripLength;
        previousLocation = currentRequest.destination;
        currentDestination = currentRequest.destination;
        currentRequest.droppedOff = true;
        //currentRequest = null;
        StartScanningRequests();
    }


    private void PickUpRequest()
    {
        currentRequest.pickedUp = true;
        state = VEHICLE_STATE.VEHICLE_GOING_TO_DESTINATION;
        distanceTravelledWithoutPassenger += Vector2.Distance(previousLocation, currentDestination);
        currentDestination = currentRequest.destination;
        timeProjectedToArriveAtDestination = currentTime + (currentRequest.tripLength * unitSize)/30;
        previousLocation = gameObject.transform.position;
    }
}
