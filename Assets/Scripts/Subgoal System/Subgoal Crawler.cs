using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SubgoalManager;

public class SubgoalCrawler : MonoBehaviour
{
    public (int, int) StartingPoint;
    List<(int,int)> visited = new List<(int, int)>();

    public List<List<FactoryTypes>> Crawl()
    {
        (int, int) currentTile = StartingPoint;
        List<List<FactoryTypes>> result = new List<List<FactoryTypes>>();
        List<FactoryTypes> run = new List<FactoryTypes>();

        while(true)
        {
            // Check current tile
            GridBuildingSystem.GridObject cell = GridBuildingSystem.Instance.GetGrid().GetGridObject(currentTile.Item1, currentTile.Item2);
            if (cell == null) break;

            Building building = cell.GetBuilding();
            if (building == null) break;

            // Break if there is a loop in the crawl
            if (visited.Contains(currentTile)) break;
            visited.Add(currentTile);

            if (building is Conveyor)
            {
                switch (building.GetDir())
                {
                    case BuildingTypeSO.Dir.Down:
                        currentTile = (currentTile.Item1, currentTile.Item2 - 1);
                        continue;
                    case BuildingTypeSO.Dir.Left:
                        currentTile = (currentTile.Item1 - 1, currentTile.Item2);
                        continue;
                    case BuildingTypeSO.Dir.Up:
                        currentTile = (currentTile.Item1, currentTile.Item2 + 1);
                        continue;
                    case BuildingTypeSO.Dir.Right:
                        currentTile = (currentTile.Item1 + 1, currentTile.Item2);
                        continue;
                }
            }
            else if (building is AFactory)
            {
                string buildingName = building.GetBuildingTypeSO().nameString;
                if (buildingName == "Duplication Factory")
                {
                    // Split crawl up in both output cells
                    SubgoalCrawler childLeft = SubgoalManager.Instance.SpawnCrawler(((AFactory)building).OutputCells[0]);
                    SubgoalCrawler childRight = SubgoalManager.Instance.SpawnCrawler(((AFactory)building).OutputCells[1]);
                    
                    // Run both crawlers
                    List<List<FactoryTypes>> resultLeft = childLeft.Crawl();
                    List<List<FactoryTypes>> resultRight = childRight.Crawl();

                    if (run.Count > 0)
                    {
                        // Append existing run at the start of each of the childrens crawls
                        foreach (var crawlLeft in resultLeft)
                        {
                            List<FactoryTypes> copy = new List<FactoryTypes>(run);
                            copy.AddRange(crawlLeft);
                            result.Add(copy);
                        }
                        foreach (var crawlRight in resultRight)
                        {
                            List<FactoryTypes> copy = new List<FactoryTypes>(run);
                            copy.AddRange(crawlRight);
                            result.Add(copy);
                        }
                    }
                    else
                    {
                        result.AddRange(resultLeft);
                        result.AddRange(resultRight);
                    }

                    // Return the result
                    return result;
                }
                else if (buildingName == "Export Factory")
                {
                    run.Add(FactoryTypes.End);
                    break;
                }
                else if (buildingName == "Import Factory")
                {
                    run.Add(FactoryTypes.Start);
                    currentTile = ((AFactory)building).OutputCells[0];
                    continue;
                }
                else
                {
                    // Append factory name in run
                    run.Add(MapType(buildingName));
                    currentTile = ((AFactory)building).OutputCells[0];
                    continue;
                }
            }
        }

        result.Add(run);
        return result;
    }

    private FactoryTypes MapType(string name)
    {
        switch(name)
        {
            default:
            case "Activation Factory":
                return FactoryTypes.Activation;
            case "Addition Factory":
                return FactoryTypes.Addition;
            case "Attention Factory":
                return FactoryTypes.Attention;
            case "Decoder":
                return FactoryTypes.FullDecoder;
            case "Encoder":
                return FactoryTypes.FullEncoder;
            case "Encoding Factory":
                return FactoryTypes.Encoding;
            case "Multiplication Factory":
                return FactoryTypes.Multiplication;
            case "Network Factory":
                return FactoryTypes.Network;
            case "Normalization Factory":
                return FactoryTypes.Normalization;
            case "Pick Last":
                return FactoryTypes.PickLast;
            case "Positional Embedder":
                return FactoryTypes.PositionalEmbedder;
            case "Positional Factory":
                return FactoryTypes.Position;
            case "Scaling Factory":
                return FactoryTypes.Scaling;
            case "Transposition Factory":
                return FactoryTypes.Transposition;

        }
    }
}
