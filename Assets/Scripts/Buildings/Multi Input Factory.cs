using System.Collections.Generic;
using System.Linq;
using Utilities.ML;

public class MultiInputFactory : AFactory
{
    protected List<(bool, Matrix?)> setFlags = new List<(bool, Matrix?)>();
    protected List<WorldItem> outputs = new List<WorldItem>();

    new protected void Start()
    {
        base.Start();

        foreach ((int, int) cell in InputCells)
        {
            setFlags.Add((false, null));
        }
    }

    public override void AddFromInput(WorldItem item, (int, int) cell)
    {
        for (int i = 0; i < setFlags.Count; i++)
        {
            if (cell == InputCells[i] && !setFlags[i].Item1)
            {
                if (coroutineOnAction == null) StartAnimation();
                setFlags[i] = (true, item.state);
            }
        }

        item.MoveTo(Center);
        item.DestroyOnArrival();
    }

    public override WorldItem RemoveFromOutput((int, int) cell)
    {
        // Check if outputs list is empty, return null
        if (outputs.Count <= 0) return null;

        // if not pop first item
        WorldItem item = outputs.First();
        outputs.RemoveAt(0);

        if (outputs.Count <= 0) StopAnimation();

        return item;
    }

    protected override void Action(object sender, TimeTickSystem.TickEventArgs e) {}

    protected override void FillCellLists()
    {
        switch(dir)
        {
            default:
            case BuildingTypeSO.Dir.Down:
                OutputCells.Add((cellX, cellY - 1));
                InputCells.Add((cellX, cellY + 1));
                InputCells.Add((cellX - 1, cellY));
                break;
            case BuildingTypeSO.Dir.Left:
                OutputCells.Add((cellX - 1, cellY));
                InputCells.Add((cellX + 1, cellY));
                InputCells.Add((cellX, cellY + 1));
                break;
            case BuildingTypeSO.Dir.Up:
                OutputCells.Add((cellX, cellY + 1));
                InputCells.Add((cellX, cellY - 1));
                InputCells.Add((cellX + 1, cellY));
                break;
            case BuildingTypeSO.Dir.Right:
                OutputCells.Add((cellX + 1, cellY));
                InputCells.Add((cellX - 1, cellY));
                InputCells.Add((cellX, cellY - 1));
                break;
        }
    }

    public override bool Occupied((int, int) cell)
    {
        for (int i = 0; i < setFlags.Count; i++)
        {
            if (cell == InputCells[i]) return setFlags[i].Item1 || broken;
        }
        return broken || !InputCells.Contains(cell);
    }

    protected override void Reset()
    {
        for (int i = 0; i < setFlags.Count; i++)
        {
            setFlags[i] = (false, null);
        }
        outputs.Clear();
    }
}
