using Model.Runtime.Projectiles;
using NUnit.Framework;
using UnityEngine;

public class Dz2Test : MonoBehaviour
{
    private const int SamplesOverTrajectory = 100;
    
    private const float ProjectileSpeed = 7f;

    [Test]
    public void TestProjectileTrajectory()
    {
        TestProjectileTrajectory(Vector2Int.zero, new Vector2Int(100, 0));
        TestProjectileTrajectory(Vector2Int.zero, new Vector2Int(0, 100));
        TestProjectileTrajectory(Vector2Int.zero, new Vector2Int(100, 100));
        TestProjectileTrajectory(new Vector2Int(50, 50), new Vector2Int(100, 100));
    }
    
    public void TestProjectileTrajectory(Vector2Int start, Vector2Int target)
    {
        var proj = new ArchToTileProjectile(null, target, 0, start);
        
        var totalDistance = Vector2.Distance(start, target);
        var timeToTarget = totalDistance / ProjectileSpeed;
        var maxHeight = 0.6f * totalDistance;

        var step = 1f / SamplesOverTrajectory;
        var timeStep = timeToTarget / SamplesOverTrajectory;
        var totalPassedTime = 0f;
        for (float t = 0; t <= 1; t += step)
        {
            totalPassedTime += timeStep;
            proj.Update(timeStep, totalPassedTime);
            var height = proj.Height;
            var refHeight = maxHeight * (-(t * 2 - 1) * (t * 2 - 1) + 1);
            
            Assert.AreEqual(refHeight, height, 0.1f, "Height is not as expected at t=" + t);
        }
    }
}
