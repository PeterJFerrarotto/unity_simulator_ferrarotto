using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum VEHICLE_STATE { VEHICLE_SCANNING_REQUESTS, VEHICLE_SCANNING_DEMAND, VEHICLE_GOING_TO_PICKUP, VEHICLE_GOING_TO_DEMAND_ZONE, VEHICLE_GOING_TO_DESTINATION, VEHICLE_WAITING_AT_PICKUP, VEHICLE_DROPPING_OFF };

public class Vehicle : MonoBehaviour {
    public static RequestGrid requestGrid = null;
    public static float speed;
    public static float unitSize;
    private static int radiusToUnitSize = 12;

    private static float spritePersistTime = 5;

    private float spriteExistTime;

    private float searchTime;

    public static float timeRadius;
    public static float minimumScore;
    public static float tripDistanceWeight;
    public static float minimumSearchRadius;
    public static float maximumSearchRadius;
    public static float customSearchRadiusStep;
    public static float maximumDestinationRequestCount;
    public static float spritePersistance;

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
        searchTime = 0;
        spriteExistTime = 0;
        previousLocation = startingPosition;
        foreach(SpriteRenderer renderer in gameObject.GetComponentsInChildren<SpriteRenderer>())
        {
            renderer.enabled = renderer.gameObject.tag == "Vehicle";
        }
        distanceTravelledWithoutPassenger = 0;
        distanceTravelledWithPassenger = 0;
        requestsCompleted = 0;
        spriteExistTime = 0;
	}
	
	// Update is called once per frame
	void Update () {
        foreach(SpriteRenderer sprite in gameObject.GetComponentsInChildren<SpriteRenderer>())
        {
            if(sprite.gameObject.tag == "Thinking_Sprite" || sprite.gameObject.tag == "Waiting_Sprite" || sprite.gameObject.tag == "Match_Success_Sprite" || sprite.gameObject.tag == "Match_Failure_Sprite")
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
                    sprite.enabled = (sprite.gameObject.tag == "Thinking_Sprite" && spriteExistTime >= spritePersistTime) || (sprite.gameObject.tag == "Match_Failure_Sprite" && spriteExistTime < spritePersistTime) || sprite.gameObject.tag == "Vehicle";
                }
                ScanRequests();
                searchTime += GameObject.FindGameObjectWithTag("Simulation_Controller").GetComponent<SimulationController>().timeStep;
                break;
            case VEHICLE_STATE.VEHICLE_GOING_TO_PICKUP:
                foreach (SpriteRenderer renderer in gameObject.GetComponentsInChildren<SpriteRenderer>())
                {
                    renderer.enabled = renderer.gameObject.tag == "Vehicle" || (renderer.gameObject.tag == "Match_Success_Sprite" && spriteExistTime < spritePersistTime);
                    spriteExistTime += Time.deltaTime;
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
            case VEHICLE_STATE.VEHICLE_SCANNING_DEMAND:
                foreach (SpriteRenderer sprite in gameObject.GetComponentsInChildren<SpriteRenderer>())
                {
                    sprite.enabled = (sprite.gameObject.tag == "Thinking_Sprite" && spriteExistTime >= spritePersistTime) || (sprite.gameObject.tag == "Match_Failure_Sprite" && spriteExistTime < spritePersistTime) || sprite.gameObject.tag == "Vehicle";
                    spriteExistTime += Time.deltaTime;
                }
                if (ScanDemandZones())
                {
                    state = VEHICLE_STATE.VEHICLE_GOING_TO_DEMAND_ZONE;
                    spriteExistTime = 0;
                }
                else
                {
                    if (searchTime >= timeRadius)
                    {
                        StartScanningRequests();
                    }
                    else
                    {
                        searchTime += GameObject.FindGameObjectWithTag("Simulation_Controller").GetComponent<SimulationController>().timeStep;
                    }
                }
                break;
            case VEHICLE_STATE.VEHICLE_GOING_TO_DEMAND_ZONE:
                foreach (SpriteRenderer renderer in gameObject.GetComponentsInChildren<SpriteRenderer>())
                {
                    renderer.enabled = renderer.gameObject.tag == "Vehicle";
                }
                if (gameObject.transform.position == currentDestination)
                {
                    StartScanningRequests();
                }
                else
                {
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
                    if (dist >= 0 && dist < minimumSearchRadius * radiusToUnitSize)
                    {
                        if (!collidedRequestsRadius1.Contains(other.gameObject.GetComponent<Request>()))
                        {
                            collidedRequestsRadius1.Add(other.gameObject.GetComponent<Request>());
                        }
                    }
                    else if (dist >= minimumSearchRadius * radiusToUnitSize && dist < minimumSearchRadius * radiusToUnitSize + customSearchRadiusStep * radiusToUnitSize)
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
        float score = 0.0f;
        float lenToRequest = Vector3.Distance(gameObject.transform.position, request.requestLocation);
        float percentUtilization = (request.tripLength / (request.tripLength + lenToRequest)) * 10;
        float requestLengthValue = request.tripLength * tripDistanceWeight * 10;

        if(request.requestTime + timeRadius < currentTime + (lenToRequest * unitSize) / 0.00833333f)
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

    private bool ScanDemandZones()
    {
        int highestDemand = 0;
        Vector2 highestDemandLocation = new Vector2(-1, -1);
        int staleCount = 0;
        for (float xDeg = 0; xDeg < 2 * Mathf.PI; xDeg += Mathf.PI / 6) {
            float xPos = gameObject.transform.position.x + minimumSearchRadius * Mathf.Cos(xDeg);
            for(float yDeg = 0; yDeg < 2 * Mathf.PI; yDeg += Mathf.PI / 6)
            {
                float yPos = gameObject.transform.position.y + minimumSearchRadius * Mathf.Sin(yDeg);
                int demandAtLocation = requestGrid.getDemandAtLocation(new Vector2(xPos, yPos), currentTime);
                if (demandAtLocation > highestDemand)
                {
                    highestDemand = demandAtLocation;
                    highestDemandLocation = new Vector2(xPos, yPos);
                    staleCount = 0;
                }
                else if (demandAtLocation <= highestDemand)
                {
                    if(highestDemandLocation.x != -1 && highestDemandLocation.y != -1)
                    {
                        staleCount++;
                    }
                }

                if(staleCount >= 3)
                {
                    return false;
                }
            }
        }
        if(highestDemandLocation.x != -1 && highestDemandLocation.y != 1)
        {
            currentDestination = highestDemandLocation;
            return true;
        }
        return false;
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
            if (searchTime >= timeRadius)
            {
                StartScanningDemand();
            }
        }
        else
        {
            currentRequest = highestScoringRequest;
            state = VEHICLE_STATE.VEHICLE_GOING_TO_PICKUP;
            spriteExistTime = 0;
            currentDestination = currentRequest.requestLocation;
            currentRequest.matchedToVehicle = true;
            timeProjectedToArriveAtDestination = currentTime + (Vector3.Distance(gameObject.transform.position, currentDestination) * unitSize) / 0.00833333f;
            foreach (SpriteRenderer sprite in gameObject.GetComponentsInChildren<SpriteRenderer>())
            {
                sprite.enabled = sprite.gameObject.tag == "Vehicle";
            }
        }
    }
    
    private void StartScanningRequests()
    {
        state = VEHICLE_STATE.VEHICLE_SCANNING_REQUESTS;
        searchTime = 0;
        highestScore = minimumScore;
        highestScoringRequest = null;
        currentRequest = null;
    }

    private void StartScanningDemand()
    {
        searchTime = 0;
        spriteExistTime = 0;
        state = VEHICLE_STATE.VEHICLE_SCANNING_DEMAND;
    }

    private void DropOffRequest()
    {
        distanceTravelledWithPassenger += currentRequest.tripLength;
        previousLocation = currentRequest.destination;
        currentDestination = currentRequest.destination;
        currentRequest.droppedOff = true;
        requestsCompleted++;
        //currentRequest = null;
        StartScanningRequests();
    }


    private void PickUpRequest()
    {
        currentRequest.pickedUp = true;
        state = VEHICLE_STATE.VEHICLE_GOING_TO_DESTINATION;
        distanceTravelledWithoutPassenger += Vector2.Distance(previousLocation, currentDestination);
        currentDestination = currentRequest.destination;
        timeProjectedToArriveAtDestination = currentTime + (currentRequest.tripLength * unitSize)/ 0.00833333f;
        previousLocation = gameObject.transform.position;
    }
}
