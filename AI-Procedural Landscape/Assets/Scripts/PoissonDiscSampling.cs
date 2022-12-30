using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PoissonDiscSampling {
    public static List<Vector2> GeneratePoints(float radius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 15) {
        float cellSize = radius / Mathf.Sqrt(2);

        //This grid tells us, for each cell, what the index of each point in the point list that lies in the cell
        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];

        //This list holds all the points we generate
        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>();

        //Spawn the first point in the middle
        spawnPoints.Add(sampleRegionSize / 2);
        while (spawnPoints.Count > 0) {
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            Vector2 spawnCentre = spawnPoints[spawnIndex];

            bool candidateAccepted = false;

            for (int i = 0; i < numSamplesBeforeRejection; i++) {
                float angle = Random.value * Mathf.PI * 2;
                Vector2 direction = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                //We wanna be sure we're spawning the candidate outside the radius
                //that's why we use the radius as the minimum value
                Vector2 candidate = spawnCentre + direction * Random.Range(radius, 2 * radius);

                if (IsValid(candidate, sampleRegionSize, cellSize, radius, points, grid)) {
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
                    candidateAccepted = true;

                    break;
                }
            }
            if (!candidateAccepted) {
                spawnPoints.RemoveAt(spawnIndex);
            }
        }

        return points;
    }

    static bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid) {
        if (candidate.x >= 0 &&
            candidate.x < sampleRegionSize.x &&
            candidate.y >= 0 &&
            candidate.y < sampleRegionSize.y) {

            int cellX = (int)(candidate.x / cellSize);
            int cellY = (int)(candidate.y / cellSize);

            int searchStartX = Mathf.Max(0, cellX - 2);
            int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0)-1);

            int searchStartY = Mathf.Max(0, cellY - 2);
            int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

            for (int x = searchStartX; x <= searchEndX; x++) {
                for (int y = searchStartY; y <= searchEndY; y++){
                    int pointIndex = grid[x, y] - 1;
                    if (pointIndex != -1) {
                        float sqrDistance = (candidate - points[pointIndex]).sqrMagnitude;

                        if (sqrDistance < radius *radius) {
                            return false;
                        }

                    }
                }
            }
            return true;
        }

        return false;
    }
}
