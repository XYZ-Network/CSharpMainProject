using Model;
using Model.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;

public class A_zvezdochka : BaseUnitPath
{
    
    private Vector2Int _startPoint;
    private Vector2Int _endPoint;


    public A_zvezdochka(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
    {
        _startPoint = startPoint;
        _endPoint = endPoint;

    }
   

    protected override void Calculate()
    {
        List<Nodee> path1 = FindPath();
        if (path1 == null)
        {
            return;
        }
        //path1.RemoveAt(0);
       
        
        List<Vector2Int> finalka = new List<Vector2Int>();
        while (path1.Count > 0)
        {
            finalka.Add(path1[0].Point);
            path1.RemoveAt(0);
        }
        path = finalka.ToArray();
    }
    private List<Nodee> FindPath()
    {
        Nodee startNode = new Nodee(_startPoint);
        Nodee targetNode = new Nodee(_endPoint);
       List<Nodee> PlanB = new List<Nodee> { startNode };

        List<Nodee> openList = new List<Nodee> { startNode };
        List<Nodee> closedList = new List<Nodee>();
        bool hasloop = false;

        while (openList.Count > 0)
        {
            Nodee currentNode = openList[0];
            
            foreach (var node in openList)
            {
                if (node.Value < currentNode.Value)
                {
                    currentNode = node;
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);
            if (currentNode.Point.x == targetNode.Point.x && currentNode.Point.y == targetNode.Point.y || hasloop == true)
            {
                List<Nodee> path = new List<Nodee>();
                while (currentNode != null)
                {
                    path.Add(currentNode);
                    currentNode = currentNode.Parent;
                }
                path.Reverse();
                return path;
            }

            var nextStep = ThisWay(_endPoint, currentNode.Point);
       
           
            if (runtimeModel.IsTileWalkable(nextStep))
            {
                Nodee neighbor = new Nodee(nextStep);
                

                if (closedList.Contains(neighbor))
                {
                    hasloop = true;
                    neighbor.Parent = currentNode;
                    neighbor.CalculateEstimate(targetNode.Point);
                    neighbor.CalculateValue();

                    openList.Add(neighbor);
                    continue;
                }

                neighbor.Parent = currentNode;
                neighbor.CalculateEstimate(targetNode.Point);
                neighbor.CalculateValue();

                openList.Add(neighbor);
            }
           
            if(closedList.Count > 100) { break; }
            
        }
        
        return PlanB;
    }
    private Vector2Int ThisWay(Vector2Int end, Vector2Int cur)
    {
        var diff = end - cur;
        var stepDiff = diff.SignOrZero();
        var nextstep= cur + stepDiff;
        if (runtimeModel.IsTileWalkable(nextstep))
        {
            return nextstep;
        }
        if (stepDiff.sqrMagnitude > 1)
        {
            var partStep0 = cur + new Vector2Int(stepDiff.x, 0);
            if (runtimeModel.IsTileWalkable(partStep0))
                return partStep0;

            var partStep1 = cur + new Vector2Int(0, stepDiff.y);
            if (runtimeModel.IsTileWalkable(partStep1))
                return partStep1;
        }

        var sideStep0 = cur + new Vector2Int(stepDiff.y, -stepDiff.x);
        var shiftedStep0 = cur + (sideStep0 + stepDiff).SignOrZero();
        if (runtimeModel.IsTileWalkable(shiftedStep0))
            return shiftedStep0;

        var sideStep1 = cur + new Vector2Int(-stepDiff.y, stepDiff.x);
        var shiftedStep1 = cur + (sideStep1 + stepDiff).SignOrZero();
        if (runtimeModel.IsTileWalkable(shiftedStep1))
            return shiftedStep1;

        if (runtimeModel.IsTileWalkable(sideStep0))
            return sideStep0;

        if (runtimeModel.IsTileWalkable(sideStep1))
            return sideStep1;

        return cur;
    }

}
