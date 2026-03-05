using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SubgoalManager : MonoBehaviour
{
    public enum FactoryTypes
    {
        Start,
        End,
        Activation,
        Addition,
        Attention,
        FullDecoder,
        FullEncoder,
        Encoding,
        Multiplication,
        Network,
        Normalization,
        PickLast,
        PositionalEmbedder,
        Position,
        Scaling,
        Transposition
    }

    public static SubgoalManager Instance;

    [SerializeField] List<List<List<FactoryTypes>>> goals;

    [SerializeField] GameObject crawlerObject;
    Grid<GridBuildingSystem.GridObject> grid;
    List<(int, int)> startingPoints;
    public bool ReCrawl = false;
    public bool[] goalCheck;

    void Awake()
    {
        if (Instance != null)
            Destroy(this);
        Instance = this;
    }

    void Start()
    {
        goalCheck = new bool[goals.Count];
        startingPoints = new List<(int, int)>();
        grid = GridBuildingSystem.Instance.GetGrid();

        for (int x = 0; x < grid.Size.Item1; x++)
        {
            for (int y = 0; y < grid.Size.Item2; y++)
            {
                Building building = grid.GetGridObject(x, y).GetBuilding();
                if (building != null)
                {
                    if (building.GetBuildingTypeSO().nameString == "Import Factory")
                    {
                        startingPoints.Add((x, y));
                    }
                }
            }
        }

        TimeTickSystem.OnTick += CheckForCrawl;
    }

    private void CheckForCrawl(object sender, TimeTickSystem.TickEventArgs e)
    {
        if (ReCrawl)
        {
            // Rerun the crawl
            List<List<FactoryTypes>> crawls = Run();

            // Recheck for subgoals
            for (int i = 0; i < goals.Count; i++)
            {
                bool[] used = new bool[crawls.Count];

                foreach (var subgoal in goals[i])
                {
                    bool foundMatch = false;

                    for (int j = 0; j < crawls.Count; j++)
                    {
                        if (used[j]) continue;

                        if (ChainsMatch(crawls[j], subgoal))
                        {
                            used[j] = true;
                            foundMatch = true;
                            break;
                        }
                    }

                    if (foundMatch)
                        goalCheck[i] = true;
                }
            }

            // Stop further crawling this tick
            ReCrawl = false;
        }
    }

    public List<List<FactoryTypes>> Run()
    {
        List<List<FactoryTypes>> result = new List<List<FactoryTypes>>();

        foreach (var coord in startingPoints)
        {
            // Spawn a crawler at each start point
            SubgoalCrawler crawler = SpawnCrawler(coord);
            result.AddRange(crawler.Crawl());
        }

        // Return the crawler strings
        return result;
    }

    public SubgoalCrawler SpawnCrawler((int, int) startingPoint)
    {
        GameObject crawler = Instantiate(crawlerObject);
        SubgoalCrawler crawlerScript = crawler.GetComponent<SubgoalCrawler>();
        crawlerScript.StartingPoint = startingPoint;

        return crawlerScript;
    
    }

    void OnDestroy()
    {
        TimeTickSystem.OnTick -= CheckForCrawl;
    }
}
