using System;

public class Nodes
{
    public int x;
    public int y;
    public int cost = 10;
    public int estimate;
    public int value;
    public Nodes parent;

    public Nodes(int x, int y)
    {
        this.x = x;
        this.y = y;        
    }

    public void CalculateEstimate(int targetX, int targetY)
    {
        estimate = Math.Abs(x - targetX) + Math.Abs(y - targetY);
    }

    public void CalculateValue()
    {
        value = cost + estimate;
    }

    public override bool Equals(object obj)
    {
        if (obj is not Nodes node) return false;

        return x == node.x && y == node.y;
    }
}
