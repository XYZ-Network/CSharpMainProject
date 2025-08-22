using System;

public class GridNode
{
    public int X;
    public int Y;
    public int Cost = 3;
    public int Estimate;
    public int Value;
    public GridNode Parent;

    /// <summary>
    /// Манхэттенская эвристика
    /// </summary>
    public void CalculateEstimate(int targetX, int targetY)
    {
        Estimate = Math.Abs(X - targetX) + Math.Abs(Y - targetY);
    }

    /// <summary>
    /// Общая стоимость узла
    /// </summary>
    public void CalculateValue()
    {
        Value = Cost + Estimate;
    }
    public override bool Equals(object obj)
    {
        return obj is GridNode node && X == node.X && Y == node.Y;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
}