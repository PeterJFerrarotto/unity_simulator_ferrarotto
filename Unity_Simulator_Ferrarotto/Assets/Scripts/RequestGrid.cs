using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequestGrid : MonoBehaviour {
    public static int sectionSize;
    public static int lateralSectionCount;
    public static int verticalSectionCount;

    private bool initialized;

    public bool Initialized
    {
        get
        {
            return initialized;
        }
    }

    private SortedDictionary<int, SortedDictionary<int, List<Request>>> requestMap = new SortedDictionary<int, SortedDictionary<int, List<Request>>>();

    private SortedDictionary<int, SortedDictionary<int, SortedDictionary<int, int>>> demandMap = new SortedDictionary<int, SortedDictionary<int, SortedDictionary<int, int>>>();

    // Use this for initialization
    void Start () {
        initializeMap();
	}

    public void initializeMap()
    {
        if (!initialized)
        {
            for (int x = -(lateralSectionCount / 2) * sectionSize; x <= (lateralSectionCount / 2) * sectionSize; x += sectionSize)
            {
                int timesUsed = 0;
                for (int y = -(verticalSectionCount / 2) * sectionSize; y <= (verticalSectionCount / 2) * sectionSize; y += sectionSize)
                {
                    if (timesUsed == 0)
                    {
                        requestMap.Add(x, new SortedDictionary<int, List<Request>>());
                    }
                    requestMap[x].Add(y, new List<Request>());
                    timesUsed++;
                }
            }
            initialized = true;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private Vector2 normalizeCoordinates(Vector2 coordinates)
    {
        int latitudeToUse = (int)coordinates.x;
        int longitudeToUse = (int)coordinates.y;

        //This could later be re-worked into ranged SQL queries.
        if (latitudeToUse % sectionSize > sectionSize / 2)
        {
            latitudeToUse += sectionSize - (latitudeToUse % sectionSize);
        }
        else if (latitudeToUse % sectionSize <= sectionSize / 2)
        {
            latitudeToUse -= latitudeToUse % sectionSize;
        }

        if (longitudeToUse % sectionSize > sectionSize / 2)
        {
            longitudeToUse += sectionSize - (longitudeToUse % sectionSize);
        }
        else if (longitudeToUse % sectionSize <= sectionSize / 2)
        {
            longitudeToUse -= longitudeToUse % sectionSize;
        }

        if (latitudeToUse < -verticalSectionCount * sectionSize)
        {
            latitudeToUse = -verticalSectionCount * sectionSize;
        }
        else if (latitudeToUse > verticalSectionCount * sectionSize)
        {
            latitudeToUse = verticalSectionCount * sectionSize;
        }

        if (longitudeToUse < -lateralSectionCount * sectionSize)
        {
            longitudeToUse = -lateralSectionCount * sectionSize;
        }
        else if (longitudeToUse > lateralSectionCount * sectionSize)
        {
            longitudeToUse = lateralSectionCount * sectionSize;
        }
        return new Vector2(longitudeToUse, latitudeToUse);
    }

    public int getRequestsAtLocation(Vector2 location)
    {
        Vector2 coordToUse = normalizeCoordinates(location);
        return requestMap[(int)coordToUse.x][(int)coordToUse.y].Count;
    }

    public int getDemandAtLocation(Vector2 location, float time)
    {
        int hourToUse = (int)time / 360;
        Vector2 coordToUse = normalizeCoordinates(location);
        return demandMap[(int)coordToUse.x][(int)coordToUse.y][hourToUse];
    }

    public void AddRequest(Request request)
    {
        Vector2 coordToUse = normalizeCoordinates(request.requestLocation);
        requestMap[(int)coordToUse.x][(int)coordToUse.y].Add(request);
    }

    public void FillDemandMap(int maxDemandAmount, int maxTime)
    {
        int hourCount = maxTime / 360;
        for (int x = -(lateralSectionCount / 2) * sectionSize; x <= (lateralSectionCount / 2) * sectionSize; x += sectionSize)
        {
            int timesUsed = 0;
            for(int y = -(verticalSectionCount / 2) * sectionSize; y <= (verticalSectionCount / 2) * sectionSize; y += sectionSize)
            {
                if(timesUsed == 0)
                {
                    demandMap.Add(x, new SortedDictionary<int, SortedDictionary<int, int>>());
                }
                timesUsed++;

                int timesUsed2 = 0;
                for(int t = 0; t <= hourCount; t++)
                {
                    if(timesUsed2 == 0)
                    {
                        demandMap[x].Add(y, new SortedDictionary<int, int>());
                    }
                    timesUsed2++;
                    demandMap[x][y].Add(t, (int)Random.Range(0, maxDemandAmount));
                }
            }
        }
    }
}
