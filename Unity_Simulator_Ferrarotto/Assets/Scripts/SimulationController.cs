using System.Data;
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
    public float spritePersistance;
    private DataTable simData;

    //public Text tripLengthWeightWarningText;
    public Image mainUIContainer;
    public Image simUIContainer;
    public Text timeText;

    private bool simulationRunning;

    public void SetTimeRadius(string timeRadiusString)
    {
        if (timeRadiusString.Length != 0)
        {
            timeRadius = float.Parse(timeRadiusString) * 60.0f;
        }
        else
        {
            timeRadius = 15.0f * 60.0f;
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
            lateralSectionCount = 8;
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
            verticalSectionCount = 8;
        }
    }

    public string excelFileName;

    private int tripsCompleted;
    private float distanceTravelledWithPassenger;
    private float distanceTravelledWithoutPassenger;
    private float percentUtilization;

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
            timesToRun = 7200;
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
            //tripLengthWeightWarningText.gameObject.SetActive(tripLengthWeight > 0.01f);
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
            timeStep = 1;
        }
    }

    private RequestGrid grid;

    public GameObject vehiclePrefab;
    public GameObject requestPrefab;
    public GameObject requestGridPrefab;
    public GameObject gridSquarePrefab;

    public Image infoFrame;
    public Text paramName;
    public Text paramInfo;

    private void PrepareDataTable()
    {
        simData = new DataTable();
        DataColumn[] columns = {
            new DataColumn("Time Radius", System.Type.GetType("System.Int32")),
            new DataColumn("Section Size", System.Type.GetType("System.Int32")),
            new DataColumn("Map Size X", System.Type.GetType("System.Int32")),
            new DataColumn("Map Size Y", System.Type.GetType("System.Int32")),
            new DataColumn("Vehicle Count", System.Type.GetType("System.Int32")),
            new DataColumn("Request Goal", System.Type.GetType("System.Int32")),
            new DataColumn("Time Limit", System.Type.GetType("System.Int32")),
            new DataColumn("Minimum Score", System.Type.GetType("System.Decimal")),
            new DataColumn("Maximum Requests for Destination Penalty", System.Type.GetType("System.Int32")),
            new DataColumn("Minimum Search Radius", System.Type.GetType("System.Decimal")),
            new DataColumn("Maximum Search Radius", System.Type.GetType("System.Decimal")),
            new DataColumn("Weight of Trip Length", System.Type.GetType("System.Decimal")),
            new DataColumn("Result: Trips Completed", System.Type.GetType("System.Int32")),
            new DataColumn("Result: Distance Travelled with Passenger", System.Type.GetType("System.Decimal")),
            new DataColumn("Result: Distance Travelled without Passenger", System.Type.GetType("System.Decimal")),
            new DataColumn("Result: Utilization Percentage", System.Type.GetType("System.Decimal"))
        };
        simData.Columns.AddRange(columns);
    }

    private Aspose.Cells.Workbook PrepareWorkbook(string filePath)
    {
        Aspose.Cells.Workbook workbook = new Aspose.Cells.Workbook();
        workbook.Worksheets[0].Name = "Simulation Data";
        workbook.Worksheets[0].Cells.ImportDataTable(simData, true, 0, 0);
        workbook.Save(filePath);
        return workbook;
    }

    private Aspose.Cells.Workbook openWorkbook()
    {
        string filePath = System.IO.Directory.GetCurrentDirectory();
        filePath += "\\";
        filePath += excelFileName;
        if (System.IO.File.Exists(filePath))
        {
            return new Aspose.Cells.Workbook(filePath);
        }
        return PrepareWorkbook(filePath);
    }

    // Use this for initialization
    void Start () {
        currentTime = 0;
        simulationRunning = false;
        simUIContainer.gameObject.SetActive(false);
        PrepareDataTable();
        infoFrame.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
        //tripLengthWeightWarningText.gameObject.SetActive(tripLengthWeight > 0.01);
        if (currentTime < timesToRun)
        {
            currentTime += timeStep;
            int secondsToShow = (int)currentTime % 60;
            int minutesToShow = ((int)currentTime / 60) % 60;
            int hoursToShow = (((int)currentTime / 60) / 60) % 60;
            string sSecondsToShow = secondsToShow.ToString();
            if (secondsToShow < 10)
            {
                sSecondsToShow = "0" + sSecondsToShow;
            }
            string sMinutesToShow = minutesToShow.ToString();
            if (minutesToShow < 10)
            {
                sMinutesToShow = "0" + sMinutesToShow;
            }
            timeText.text = "Time: " + hoursToShow.ToString() + ":" + sMinutesToShow + ":" + sSecondsToShow;
        }
	}

    public void CreateRequestGrid()
    {
        RequestGrid.sectionSize = sectionSize;
        RequestGrid.lateralSectionCount = lateralSectionCount;
        RequestGrid.verticalSectionCount = verticalSectionCount;
        GameObject requestGrid = Instantiate(requestGridPrefab, Vector2.zero, Quaternion.identity);
        grid = requestGrid.GetComponent<RequestGrid>();
        grid.initializeMap();
        grid.FillDemandMap(maximumDestinationRequestCount, timesToRun);
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
            Vector2 startingLocation = new Vector2(Random.Range(-(lateralSectionCount / 2) * sectionSize, (lateralSectionCount / 2) * sectionSize), Random.Range(-(verticalSectionCount / 2) * sectionSize, (verticalSectionCount / 2) * sectionSize));
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
            Vector2 requestLocation = new Vector2(Random.Range(-(lateralSectionCount / 2) * sectionSize, (lateralSectionCount / 2) * sectionSize), Random.Range(-(verticalSectionCount / 2) * sectionSize, (verticalSectionCount / 2) * sectionSize));
            Vector2 destinationLocation = new Vector2(Random.Range(-(lateralSectionCount / 2) * sectionSize, (lateralSectionCount / 2) * sectionSize), Random.Range(-(verticalSectionCount / 2) * sectionSize, (verticalSectionCount / 2) * sectionSize));
            float tripLength = Vector2.Distance(requestLocation, destinationLocation);
            float requestTime = Random.Range(0, timesToRun);
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
        tripsCompleted = 0;
        distanceTravelledWithoutPassenger = 0;
        distanceTravelledWithPassenger = 0;
        percentUtilization = 0;
        currentTime = 0;
        CreateRequestGrid();
        CreateRequests();
        CreateVehicles();
        mainUIContainer.gameObject.SetActive(false);
        simUIContainer.gameObject.SetActive(true);
        timeText.text = "Current Time: 0.0s";
        simulationRunning = true;
        Camera.main.orthographicSize = sectionSize * lateralSectionCount/2 + sectionSize * verticalSectionCount/2;
    }

    private void destroyAllVehicles()
    {
        GameObject[] vehicles = GameObject.FindGameObjectsWithTag("Vehicle");
        for(int i = 0; i < vehicles.Length; i++)
        {
            Destroy(vehicles[i]);
        }
    }

    private void destroyAllRequests()
    {
        GameObject[] requests = GameObject.FindGameObjectsWithTag("Request");
        for(int i = 0; i < requests.Length; i++)
        {
            Destroy(requests[i]);
        }
    }

    private void destroyRequestGrid()
    {
        Destroy(grid.gameObject);
        grid = null;
    }

    public void EndSimulation()
    {
        GameObject[] vehicles = GameObject.FindGameObjectsWithTag("Vehicle");
        foreach(GameObject vehicle in vehicles)
        {
            tripsCompleted += vehicle.GetComponent<Vehicle>().requestsCompleted;
            distanceTravelledWithPassenger += vehicle.GetComponent<Vehicle>().distanceTravelledWithPassenger;
            distanceTravelledWithoutPassenger += vehicle.GetComponent<Vehicle>().distanceTravelledWithoutPassenger;
        }
        if ((distanceTravelledWithoutPassenger == 0 && distanceTravelledWithPassenger == 0) || tripsCompleted == 0)
        {
            percentUtilization = 0;
        }
        else
        {
            percentUtilization = ((distanceTravelledWithPassenger) / (distanceTravelledWithoutPassenger + distanceTravelledWithPassenger)) * 100.0f;
        }
        recordSimulationData();
        currentTime = 0;
        destroyAllVehicles();
        destroyAllRequests();
        destroyRequestGrid();
        simUIContainer.gameObject.SetActive(false);
        mainUIContainer.gameObject.SetActive(true);
        simulationRunning = false;
    }

    private void recordSimulationData()
    {
        DataRow row = simData.NewRow();
        row["Time Radius"] = timeRadius;
        row["Section Size"] = sectionSize;
        row["Map Size X"] = sectionSize * lateralSectionCount;
        row["Map Size Y"] = sectionSize * verticalSectionCount;
        row["Vehicle Count"] = vehicleCount;
        row["Request Goal"] = requestGoal;
        row["Time Limit"] = timesToRun;
        row["Minimum Score"] = minimumScore;
        row["Maximum Requests for Destination Penalty"] = maximumDestinationRequestCount;
        row["Minimum Search Radius"] = minSearchRadius;
        row["Maximum Search Radius"] = minSearchRadius + customSearchRadiusStep * 2;
        row["Weight of Trip Length"] = tripLengthWeight;
        row["Result: Trips Completed"] = tripsCompleted;
        row["Result: Distance Travelled with Passenger"] = distanceTravelledWithPassenger;
        row["Result: Distance Travelled without Passenger"] = distanceTravelledWithoutPassenger;
        row["Result: Utilization Percentage"] = percentUtilization;
        simData.Rows.Add(row);
    }

    public void outputSimulationData()
    {
        if (simData.Rows.Count > 0)
        {
            Aspose.Cells.Workbook workbook = openWorkbook();
            int rowToStartAt = workbook.Worksheets[0].Cells.MaxDataRow + 1;
            workbook.Worksheets[0].Cells.ImportDataTable(simData, false, rowToStartAt, 0);
            string filePath = System.IO.Directory.GetCurrentDirectory();
            filePath += "\\";
            filePath += excelFileName;
            workbook.Save(filePath);
            simData.Rows.Clear();
        }
    }

    private void destroyGrid()
    {
        GameObject[] squares = GameObject.FindGameObjectsWithTag("GridSquare");
        foreach(GameObject square in squares)
        {
            Destroy(square);
        }
    }

    private void createGrid()
    {
        for (int posX = -lateralSectionCount * sectionSize; posX < lateralSectionCount * sectionSize; posX += sectionSize)
        {
            for (int posY = -verticalSectionCount * sectionSize; posY < verticalSectionCount * sectionSize; posY += sectionSize)
            {
                GameObject square = Instantiate(gridSquarePrefab);
                
                square.transform.position = new Vector3(posX, posY);
            }
        }
    }

    public void showParamName(string sParamName)
    {
        paramName.text = sParamName;
    }

    public void showParamInfo(string sParamInfo)
    {
        infoFrame.gameObject.SetActive(true);
        paramInfo.text = sParamInfo;   
    }

    public void hideParamInfo()
    {
        infoFrame.gameObject.SetActive(false);
    }
}
