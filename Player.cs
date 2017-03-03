using System.Collections.Generic;

public class Player
{
    public Player()
    {
        ArmyList = new List<Army>();
    }

    public List<Army> ArmyList { get; private set; }

    public void AddArmy(Army army)
    {
        // create new army object
        ArmyList.Add(army);
    }

    public bool CanMoveArmy(int armyId, Pos toPos)
    {
        if (armyId >= ArmyList.Count)
        {
            return false;
        }

        Army army = ArmyList[armyId];
        if (army.DistanceTo(toPos) > army.MoveRange)
        {
            return false;
        }

        return true;
    }

    public bool ArmyExists(int armyId)
    {
        if (armyId >= ArmyList.Count)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public Pos ArmyPosition(int armyId)
    {
        return ArmyList[armyId].Position;
    }

    public void MoveArmy(int armyId, Pos toPos)
    {
        ArmyList[armyId].Position = toPos;
    }

    public void RemoveArmy(Army army)
    {
        if (ArmyList.Contains(army))
        {
            ArmyList.Remove(army);
        }
    }
}