using UnityEngine;

[System.Serializable]
public class Route
{
    public Transform[] waypoints;
}
public class CarSpawner : MonoBehaviour
{
    public GameObject carPrefab;
    public Transform[] spawnPoints;
    public Route[] routes;

    public void SpawnCar()
    {
        int randomSpawn = Random.Range(0, spawnPoints.Length);
        GameObject carObj = Instantiate(carPrefab, spawnPoints[randomSpawn].position, spawnPoints[randomSpawn].rotation);

        CarAI carScript = carObj.GetComponent<CarAI>();

        if (randomSpawn < routes.Length)
        {
            carScript.currentRoute = routes[randomSpawn].waypoints;
            carScript.MoveToCurrentWaypoint();
        }
    }
}
