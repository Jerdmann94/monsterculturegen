using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    public List<ACulturalEvent> events = new List<ACulturalEvent>();
    public List<CultureStartingData> startingData = new List<CultureStartingData>();
    public GameObject prefab;
    private Dictionary<string, Color> cultureColorDictionary = new Dictionary<string, Color>();
    // Dictionary to store generated random colors for cultures
    Dictionary<string, Vector3> cultureCenterPositions = new Dictionary<string, Vector3>();
    public GameObject linePrefab; // A prefab with a LineRenderer component attached.

void Start()
{
    System.Random random = new System.Random();
    var generator = new CultureGenMaster();
    var tuple = generator.Generate(events, startingData);
    var map = tuple.Item1;
    var cultures = tuple.Item2;

    // Dictionary to store assigned random colors for cultures
    Dictionary<string, Color> cultureColorDictionary = new Dictionary<string, Color>();
    // Dictionary to store center positions of cultures
    Dictionary<string, Vector3> cultureCenterPositions = new Dictionary<string, Vector3>();

    // Step 1: Instantiate tiles and populate culture colors and positions
    foreach (var tile in map)
    {
        // Skip if the culture is null
        if (tile.culture == null)
            continue;

        if (!cultures.Contains(tile.culture))
            continue;
        // Assign random colors to new cultures
        if (!cultureColorDictionary.ContainsKey(tile.culture.name))
        {
            Color randomColor = new Color(
                (float)random.NextDouble(),
                (float)random.NextDouble(),
                (float)random.NextDouble()
            );

            cultureColorDictionary.Add(tile.culture.name, randomColor);
        }

        // Instantiate the prefab at the tile location
        var obj = Instantiate(prefab, new Vector3(tile.myX, tile.myY, 0), Quaternion.identity);

        // Retrieve the SpriteRenderer from the instantiated prefab
        var spriteRenderer = obj.GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            // Assign the specific color for the culture
            spriteRenderer.color = cultureColorDictionary[tile.culture.name];
        }

        // Store the center position of the culture if it's not already recorded
        if (!cultureCenterPositions.ContainsKey(tile.culture.name))
        {
            cultureCenterPositions[tile.culture.name] = new Vector3(tile.myX, tile.myY, 0);
        }
    }

    // Step 2: Draw lines between cultures using LineRenderer
    foreach (var culture in cultures)
    {
        // If the culture doesn't have a position, skip it
        if (!cultureCenterPositions.ContainsKey(culture.name))
            continue;

        // Get the position of the current culture
        Vector3 culturePosition = cultureCenterPositions[culture.name];

        // FRIEND LINKS: Draw green lines to friends
        foreach (var friend in culture.friends)
        {
            if (cultureCenterPositions.ContainsKey(friend.name))
            {
                Vector3 friendPosition = cultureCenterPositions[friend.name];
                DrawLine(culturePosition, friendPosition, Color.green);
            }
        }

        // ENEMY LINKS: Draw red lines to enemies
        foreach (var enemy in culture.enemies)
        {
            if (cultureCenterPositions.ContainsKey(enemy.name))
            {
                Vector3 enemyPosition = cultureCenterPositions[enemy.name];
                DrawLine(culturePosition, enemyPosition, Color.red);
            }
        }
    }
}

    void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        // Instantiate the prefab with LineRenderer
        GameObject line = Instantiate(linePrefab);

        // Get the LineRenderer component
        var lineRenderer = line.GetComponent<LineRenderer>();

        if (lineRenderer != null)
        {
            // Set the start and end positions
            lineRenderer.positionCount = 2; // Line has 2 points (start and end)
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);

            // Set the line color
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;

            // Set additional line properties (optional)
            lineRenderer.startWidth = 0.3f; // Line width
            lineRenderer.endWidth = 0.3f;
        }
    }

}
