using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robot
{
    class Map
    {
        #region Variables

        public static int maxInt = 0XFFFF;
        public static int waypointNum = 4;
        public static int[,] adjM = new int[waypointNum, waypointNum];

        public static ArrayList pointsPath = new ArrayList(waypointNum);

        public struct WayPoint
        {
            public double x;
            public double y;
        }
        public static WayPoint[] waypoints = new WayPoint[waypointNum];

        #endregion

        #region Config edges and waypoints

        internal static void Init()
        {
            for (int x = 0; x < waypointNum; x++)
            {
                for (int y = 0; y < waypointNum; y++)
                {
                    adjM[x, y] = maxInt;
                }
            }

            // Generate the map, need to be changed
            AddEdge(0, 1, 250);
            AddEdge(1, 2, 350);
            AddEdge(1, 3, 250);

            // Add waypoints to the map
            AddWaypoints(0, -253, 338);
            AddWaypoints(1, 0, 338);
            AddWaypoints(2, 369, 338);
            AddWaypoints(3, 30, 70);
        }
        #endregion

        #region Add points and edges

        internal static void AddWaypoints(int id, double x, double y)
        {
            waypoints[id].x = x;
            waypoints[id].y = y;
        }

        internal static void AddEdge(int start, int end, int len)
        {
            adjM[start, end] = len;
            adjM[end, start] = len;
        }

        #endregion

        #region Create path

        internal static void CreatePath(int start, int end)
        {
            ArrayList S = new ArrayList(waypointNum);
            ArrayList U = new ArrayList(waypointNum);
            ArrayList reversePath = new ArrayList(waypointNum);
            int[] distance = new int[waypointNum];
            int[] prev = new int[waypointNum];

            pointsPath.Clear();

            S.Add(start);
            for (int i = 0; i < waypointNum; i++)
            {
                distance[i] = adjM[start, i];
                prev[i] = start;
                if (i != start)
                {
                    U.Add(i);
                }
            }
            distance[start] = 0;
            int Count = U.Count;
            while (Count > 0)
            {
                int min_index = (int)U[0];
                // find the closest point
                foreach (int r in U)
                {
                    if (distance[r] < distance[min_index])
                        min_index = r;
                }
                S.Add(min_index);
                U.Remove(min_index);

                // Update the previous points for each U
                foreach (int r in U)
                {
                    if (distance[r] > distance[min_index] + adjM[min_index, r])
                    {
                        distance[r] = distance[min_index] + adjM[min_index, r];
                        prev[r] = min_index;
                    }
                    else
                    {
                        distance[r] = distance[r];
                    }
                }
                Count = U.Count;
            }

            // Generate path
            int prePoint = prev[end];
            reversePath.Add(end);
            while (prePoint != start)
            {
                reversePath.Add(prePoint);
                prePoint = prev[prePoint];
            }
            reversePath.Add(start);
            for (int j = reversePath.Count - 1; j >= 0; j--)
            {
                pointsPath.Add(reversePath[j]);
            }
            pointsPath.Remove(pointsPath[0]);
        }
        
        #endregion
    }
}
