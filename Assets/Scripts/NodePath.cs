
using UnityEngine;

public class NodePath
{
    public readonly Vector2Int position;

    public int cost { get; private set; } = 1;
    public int estimate { get; private set; }
    public int value { get; private set; }

    public NodePath parent = null;

    public NodePath(Vector2Int position)
    {
        this.position = position;
    }

    public void CalculateEstimate(Vector2Int targetPosition)
    {
        estimate = Mathf.Abs(position.x - targetPosition.x) + Mathf.Abs(position.y - targetPosition.y);
    }

    public void CalculateValue()
    {
        value = cost + estimate;
    }

    public override bool Equals(object obj)
    {
        if (obj is not NodePath node)
        {
            return false;
        }

        return position.x == node.position.x && position.y == node.position.y;
    }
}

