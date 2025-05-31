using System;

public class AStarNode
{
    public int x;
    public int y;
    public int Cost = 10;
    public int Estimate;
    public int Value;
    public AStarNode Parent;

    public AStarNode(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public void CalculateEstimate(int targetX, int targetY)
    {
        Estimate = Math.Abs(x - targetX) + Math.Abs(y - targetY);
    }

    public void CalculateValue()
    {
        Value = Cost + Estimate;
    }

    public override bool Equals(object obj)
    {
        if (obj is not AStarNode node) return false;

        return x == node.x && y == node.y;
    }
}