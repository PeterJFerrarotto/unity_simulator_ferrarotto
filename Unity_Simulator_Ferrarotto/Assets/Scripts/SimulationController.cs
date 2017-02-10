using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimulationController : MonoBehaviour {
    public float timeRadius;
    public int sectionSize;
    public int lateralSectionCount;
    public int verticalSectionCount;
    public float currentTime;

    public Text tripLengthWeightWarningText;
    public Image mainUIContainer;
    public Image simUIContainer;
    public Text timeText;

    private bool simulationRunning = false;

    public void SetTimeRadius(string timeRadiusString)
    {
        if (timeRadiusString.Length != 0)
        {
            timeRadius = float.Parse(timeRadiusString);
        }
        else
        {
            timeRadius = 10;
        }
    }

    public void SetSectionSize(string sectionSizeString)
    {
        if (sectionSizeString.Length != 0)
        {
            sectionSize = int.Parse(sectionSizeString);
        }
        else
        {
            sectionSize = 5;
        }
    }

    public void SetLateralSectionCount(string lateralSectionCountString)
    {
        if (lateralSectionCountString.Length != 0)
        {
            lateralSectionCount = int.Parse(lateralSectionCountString);
        }
        else
        {
            lateralSectionCount = 4;
        }
    }

    public void SetVerticalSectionCount(string verticalSectionCountString)
    {
        if (verticalSectionCountString.Length != 0)
        {
            verticalSectionCount = int.Parse(verticalSectionCountString);
        }
        else
        {
            verticalSectionCount = 4;
        }
    }

    //How long a second is for the simulation. 1 = 1 loop, 10 = 10 loops, 100 = 100 loops.
    public float speed;
    public float unitSize;

    public void SetSpeed(string speedString)
    {
        if (speedString.Length != 0)
        {
            speed = float.Parse(speedString);
        }
        else
        {
            speed = 30;
        }
    }

    public void SetUnitSize(string unitSizeString)
    {
        if (unitSizeString.Length != 0)
        {
            unitSize = float.Parse(unitSizeString);
        }
        else
        {
            unitSize = 10;
        }
    }

    public int vehicleCount;
    public int requestGoal;
    public int timesToRun;
    public int minimumScore;
    public int maximumDestinationRequestCount;
    public float minSearchRadius;
    public float customSearchRadiusStep;
    public float tripLengthWeight;
    public float timeStep;
    
    public void SetVehicleCount(string vehicleCountString)
    {
        if (vehicleCountString.Length != 0)
        {
            vehicleCount = int.Parse(vehicleCountString);
        }
        else
        {
            vehicleCount = 5;
        }
    }

    public void SetRequestGoal(string requestGoalString)
    {
        if (requestGoalString.Length != 0)
        {
            requestGoal = int.Parse(requestGoalString);
        }
        else
        {
            requestGoal = 10;
        }
    }
    
    public void SetTimesToRun(string timesToRunString)
    {
        if (timesToRunString.Length != 0)
        {
            timesToRun = int.Parse(timesToRunString);
        }
        else
        {
            timesToRun = 100;
        }
    }

    public void SetMinimumScore(string minimumScoreString)
    {
        if (minimumScoreString.Length != 0)
        {
            minimumScore = int.Parse(minimumScoreString);
        }
        else
        {
            minimumScore = 30;
        }
    }

    public void SetMaximumDestinationRequestCount(string maximumDestinationRequestCountString)
    {
        if (maximumDestinationRequestCountString.Length != 0)
        {
            maximumDestinationRequestCount = int.Parse(maximumDestinationRequestCountString);
        }
        else
        {
            maximumDestinationRequestCount = 30;
        }
    }

    public void SetMinimumSearchRadius(string minSearchRadiusString)
    {
        if (minSearchRadiusString.Length != 0)
        {
            minSearchRadius = float.Parse(minSearchRadiusString);
        }
        else
        {
            minSearchRadius = 1;
        }
    }

    public void SetCustomSearchRadiusStep(string customSearchRadiusStepString)
    {
        if (customSearchRadiusStepString.Length != 0)
        {
            customSearchRadiusStep = float.Parse(customSearchRadiusStepString);
        }
        else
        {
            customSearchRadiusStep = 1;
        }
    }

    public void SetTripLengthWeight(string tripLengthWeightString)
    {
        if (tripLengthWeightString.Length != 0)
        {
            tripLengthWeight = float.Parse(tripLengthWeightString);
            tripLengthWeightWarningText.gameObject.SetActive(tripLengthWeight > 0.01f);
        }
        else
        {
            tripLengthWeight = 0.002f;
        }
    }

    public void SetTimeStep(string timeStepString)
    {
        if (timeStepString.Length != 0)
        {
            timeStep = float.Parse(timeStepString);
        }
        else
        {
            timeStep = 0.1f;
        }
    }

    private RequestGrid grid;

    public GameObject vehiclePrefab;
    public GameObject requestPrefab;
    public GameObject requestGridPrefab;

    // Use this for initialization
    void Start () {
        currentTime = 0;
        simUIContainer.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
        tripLengthWeightWarningText.gameObject.SetActive(tripLengthWeight > 0.01);
        currentTime += timeStep;
        int secondsToShow = (int)currentTime % 60;
        int minutesToShow = ((int)currentTime / 60) % 60;
        int hoursToShow = (((int)currentTime / 60) / 60) % 60;
        string sSecondsToShow = secondsToShow.ToString();
        if(secondsToShow < 10)
        {
            sSecondsToShow = "0" + sSecondsToShow;
        }
        string sMinutesToShow = minutesToShow.ToString();
        if(minutesToShow < 10)
        {
            sMinutesToShow = "0" + sMinutesToShow;
        }
        timeText.text = "Time: " + hoursToShow.ToString() + ":" + sMinutesToShow + ":" + sSecondsToShow;
	}

    public void CreateRequestGrid()
    {
        RequestGrid.sectionSize = sectionSize;
        RequestGrid.lateralSectionCount = lateralSectionCount;
        RequestGrid.verticalSectionCount = verticalSectionCount;
        GameObject requestGrid = Instantiate(requestGridPrefab, Vector2.zero, Quaternion.identity);
        grid = requestGrid.GetComponent<RequestGrid>();
    }

    public void CreateVehicles()
    {
        Vehicle.requestGrid = grid;
        Vehicle.customSearchRadiusStep = customSearchRadiusStep;
        Vehicle.minimumSearchRadius = minSearchRadius;
        Vehicle.maximumSearchRadius = minSearchRadius + customSearchRadiusStep * 2;
        Vehicle.maximumDestinationRequestCount = maximumDestinationRequestCount;
        Vehicle.tripDistanceWeight = tripLengthWeight;
        Vehicle.timeRadius = timeRadius;
        Vehicle.speed = speed;
        Vehicle.unitSize = unitSize;
        for (int i = 0; i < vehicleCount; i++)
        { 
            Vector2 startingLocation = new Vector2(Random.Range(-lateralSectionCount * sectionSize, lateralSectionCount * sectionSize), Random.Range(-verticalSectionCount * sectionSize, verticalSectionCount * sectionSize));
            GameObject newVehicle = Instantiate(vehiclePrefab, startingLocation, Quaternion.identity);
            //newVehicle.SetActive(false);
            newVehicle.GetComponent<Vehicle>().startingPosition = startingLocation;
        }
    }

    public void CreateRequests()
    {
        Request.timeRadius = timeRadius;
        for (int i = 0; i < requestGoal * 5; i++)
        {
            Vector2 requestLocation = new Vector2(Random.Range(-lateralSectionCount * sectionSize, lateralSectionCount * sectionSize), Random.Range(-verticalSectionCount * sectionSize, verticalSectionCount * sectionSize));
            Vector2 destinationLocation = new Vector2(Random.Range(-lateralSectionCount * sectionSize, lateralSectionCount * sectionSize), Random.Range(-verticalSectionCount * sectionSize, verticalSectionCount * sectionSize));
            float tripLength = Vector2.Distance(requestLocation, destinationLocation);
            float requestTime = Random.Range(Time.time, Time.time + timesToRun);
            GameObject newRequest = Instantiate(requestPrefab, requestLocation, Quaternion.identity);
            newRequest.GetComponent<Request>().requestLocation = requestLocation;
            newRequest.GetComponent<Request>().destination = destinationLocation;
            newRequest.GetComponent<Request>().requestTime = requestTime;
            newRequest.GetComponent<Request>().tripLength = tripLength;
            grid.AddRequest(newRequest.GetComponent<Request>());
        }
    }


    public void StartSimulation()
    {
        currentTime = 0;
        CreateRequestGrid();
        CreateRequests();
        CreateVehicles();
        mainUIContainer.gameObject.SetActive(false);
        simUIContainer.gameObject.SetActive(true);
        timeText.text = "Current Time: 0.0s";
        simulationRunning = true;
    }
}
