using UnityEngine;
using UnityEngine.AI;

namespace GameJam.Movement
{
    public static class FormationHelper
    {
        /// <summary>
        /// Generates formation positions in a grid pattern around a center point
        /// </summary>
        /// <param name="center">The center point (where player clicked)</param>
        /// <param name="unitCount">Number of units to arrange</param>
        /// <param name="spacing">Distance between units in the formation</param>
        /// <returns>Array of positions for each unit</returns>
        public static Vector3[] GetFormationPositions(
            Vector3 center,
            int unitCount,
            float spacing = 1.25f
        )
        {
            Vector3[] positions = new Vector3[unitCount];

            // Single unit - validate and return center
            if (unitCount == 1)
            {
                positions[0] = GetValidNavMeshPosition(center, spacing * 2f);
                return positions;
            }

            // Calculate grid dimensions (roughly square)
            int columns = Mathf.CeilToInt(Mathf.Sqrt(unitCount));
            int rows = Mathf.CeilToInt((float)unitCount / columns);

            int index = 0;
            for (int row = 0; row < rows && index < unitCount; row++)
            {
                for (int col = 0; col < columns && index < unitCount; col++)
                {
                    // Calculate offset from center
                    float xOffset = (col - (columns - 1) / 2f) * spacing;
                    float zOffset = (row - (rows - 1) / 2f) * spacing;

                    Vector3 desiredPosition = center + new Vector3(xOffset, 0, zOffset);

                    // Validate position is on NavMesh
                    positions[index] = GetValidNavMeshPosition(desiredPosition, spacing * 2f);
                    index++;
                }
            }

            return positions;
        }

        /// <summary>
        /// Finds the nearest valid NavMesh position to the desired position
        /// </summary>
        /// <param name="position">Desired position</param>
        /// <param name="searchRadius">How far to search for valid position</param>
        /// <returns>Valid NavMesh position, or original position if none found</returns>
        private static Vector3 GetValidNavMeshPosition(Vector3 position, float searchRadius)
        {
            NavMeshHit hit;

            // Try to find nearest point on NavMesh
            if (NavMesh.SamplePosition(position, out hit, searchRadius, NavMesh.AllAreas))
            {
                return hit.position;
            }

            // If no valid position found nearby, try expanding search
            if (NavMesh.SamplePosition(position, out hit, searchRadius * 3f, NavMesh.AllAreas))
            {
                return hit.position;
            }

            // Last resort: return original position (unit will try to get as close as possible)
            Debug.LogWarning($"Could not find valid NavMesh position near {position}");
            return position;
        }

        /// <summary>
        /// Alternative: Line formation (units in a horizontal line)
        /// </summary>
        public static Vector3[] GetLineFormation(Vector3 center, int unitCount, float spacing = 2f)
        {
            Vector3[] positions = new Vector3[unitCount];

            for (int i = 0; i < unitCount; i++)
            {
                float xOffset = (i - (unitCount - 1) / 2f) * spacing;
                Vector3 desiredPosition = center + new Vector3(xOffset, 0, 0);
                positions[i] = GetValidNavMeshPosition(desiredPosition, spacing * 2f);
            }

            return positions;
        }

        /// <summary>
        /// Alternative: Circular formation (units arranged in a circle)
        /// </summary>
        public static Vector3[] GetCircularFormation(
            Vector3 center,
            int unitCount,
            float radius = 3f
        )
        {
            Vector3[] positions = new Vector3[unitCount];

            if (unitCount == 1)
            {
                positions[0] = GetValidNavMeshPosition(center, radius);
                return positions;
            }

            float angleStep = 360f / unitCount;

            for (int i = 0; i < unitCount; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                float x = center.x + Mathf.Cos(angle) * radius;
                float z = center.z + Mathf.Sin(angle) * radius;

                Vector3 desiredPosition = new Vector3(x, center.y, z);
                positions[i] = GetValidNavMeshPosition(desiredPosition, radius * 0.5f);
            }

            return positions;
        }
    }
}
