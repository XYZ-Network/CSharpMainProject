using Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nodee
{
    public Vector2Int Point;
    public int Cost = 10;
    public int Estimate; //оценка растсояния до цели
    public int Value; // итоговое значение эврестической функции
    public Nodee Parent; //стрелочка
    public Nodee(Vector2Int point)
    {
       Point = point;
    }
    public Vector2Int GetPoint { get { return Point; } }
    public Vector2Int GetSetPoint
    {
        set
        {
            Point = value;
        }
        get { return Point; }
    }
    public void CalculateEstimate(Vector2Int targetPoint)
    {
        Estimate = Math.Abs(Point.x - targetPoint.x) + Math.Abs(Point.y-targetPoint.y);
    }

    public void CalculateValue()
    {
        Value = Cost + Estimate;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Nodee node) return false;

        return Point.x == node.Point.x && Point.y == node.Point.y;
    }

}

