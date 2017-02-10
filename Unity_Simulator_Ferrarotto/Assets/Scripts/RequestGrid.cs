using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequestGrid : MonoBehaviour {
    public static int sectionSize;
    public static int lateralSectionCount;
    public static int verticalSectionCount;

    private SortedDictionary<int, SortedDictionary<int, List<Request>>> requestMap = new SortedDictionary<int, SortedDictionary<int, List<Request>>>();

	// Use this for initialization
	void Start () {
        for (int x = 0; x <= lateralSectionCount * sectionSize; x += sectionSize)
        {
            int timesUsed = 0;
            for (int y = 0; y <= verticalSectionCount * sectionSize; y += sectionSize)
            {
                if (timesUsed == 0)
                {
                    requestMap.Add(x, new SortedDictionary<int, List<Request>>());
                }
                requestMap[x].Add(y, new List<Request>());
                timesUsed++;
            }
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

        if (latitudeToUse < 0)
        {
            latitudeToUse = 0;
        }
        else if (latitudeToUse > verticalSectionCount * sectionSize)
        {
            latitudeToUse = verticalSectionCount * sectionSize;
        }

        if (longitudeToUse < 0)
        {
            longitudeToUse = 0;
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

    public void AddRequest(Request request)
    {
        Vector2 coordToUse = normalizeCoordinates(request.requestLocation);
    }
}
