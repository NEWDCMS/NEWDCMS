using System;

namespace DCMS.Core.AI
{

    /// <summary>
    /// K-Means 算法
    /// </summary>
    public class Clusterer
    {
        private int numClusters; // 集簇数量
        private int[] clustering; // index = a tuple, value = cluster ID 
        private double[][] centroids; // 每个簇的平均值（向量）
        private Random rnd; // 用于初始化 


        public Clusterer(int numClusters)
        {
            this.numClusters = numClusters;
            centroids = new double[numClusters][];
            rnd = new Random(0); // 任意种子
        }

        public int[] Cluster(double[][] data)
        {
            int numTuples = data.Length;
            int numValues = data[0].Length;
            clustering = new int[numTuples];

            for (int k = 0; k < numClusters; ++k) // allocate each centroid 
            {
                centroids[k] = new double[numValues];
            }

            InitRandom(data);

            bool changed = true; // change in clustering? 
            int maxCount = numTuples * 10; // sanity check 
            int ct = 0;
            while (changed == true && ct <= maxCount)
            {
                ++ct; // k-means typically converges very quickly 
                UpdateCentroids(data); // no effect if fail 
                changed = UpdateClustering(data); // no effect if fail 
            }

            int[] result = new int[numTuples];
            Array.Copy(clustering, result, clustering.Length);
            return result;
        }

        private void InitRandom(double[][] data)
        {
            int numTuples = data.Length;

            int clusterID = 0;
            for (int i = 0; i < numTuples; ++i)
            {
                clustering[i] = clusterID++;
                if (clusterID == numClusters)
                {
                    clusterID = 0;
                }
            }
            for (int i = 0; i < numTuples; ++i)
            {
                int r = rnd.Next(i, clustering.Length);
                int tmp = clustering[r];
                clustering[r] = clustering[i];
                clustering[i] = tmp;
            }
        }

        private void UpdateCentroids(double[][] data)
        {
            int[] clusterCounts = new int[numClusters];
            for (int i = 0; i < data.Length; ++i)
            {
                int clusterID = clustering[i];
                ++clusterCounts[clusterID];
            }

            // zero-out this.centroids so it can be used as scratch 
            for (int k = 0; k < centroids.Length; ++k)
            {
                for (int j = 0; j < centroids[k].Length; ++j)
                {
                    centroids[k][j] = 0.0;
                }
            }

            for (int i = 0; i < data.Length; ++i)
            {
                int clusterID = clustering[i];
                for (int j = 0; j < data[i].Length; ++j)
                {
                    centroids[clusterID][j] += data[i][j]; // accumulate sum 
                }
            }

            for (int k = 0; k < centroids.Length; ++k)
            {
                for (int j = 0; j < centroids[k].Length; ++j)
                {
                    centroids[k][j] /= clusterCounts[k]; // danger? 
                }
            }
        }

        private bool UpdateClustering(double[][] data)
        {
            // (re)assign each tuple to a cluster (closest centroid) 
            // returns false if no tuple assignments change OR 
            // if the reassignment would result in a clustering where 
            // one or more clusters have no tuples. 

            bool changed = false; // did any tuple change cluster? 

            int[] newClustering = new int[clustering.Length]; // proposed result 
            Array.Copy(clustering, newClustering, clustering.Length);

            double[] distances = new double[numClusters]; // from tuple to centroids

            for (int i = 0; i < data.Length; ++i) // walk through each tuple 
            {
                for (int k = 0; k < numClusters; ++k)
                {
                    distances[k] = Distance(data[i], centroids[k]);
                }

                int newClusterID = MinIndex(distances); // find closest centroid 
                if (newClusterID != newClustering[i])
                {
                    changed = true; // note a new clustering 
                    newClustering[i] = newClusterID; // accept update 
                }
            }

            if (changed == false)
            {
                return false; // no change so bail 
            }

            // check proposed clustering cluster counts 
            int[] clusterCounts = new int[numClusters];
            for (int i = 0; i < data.Length; ++i)
            {
                int clusterID = newClustering[i];
                ++clusterCounts[clusterID];
            }

            for (int k = 0; k < numClusters; ++k)
            {
                if (clusterCounts[k] == 0)
                {
                    return false; // bad clustering 
                }
            }

            Array.Copy(newClustering, clustering, newClustering.Length); // update 
            return true; // good clustering and at least one change 
        }

        // Euclidean distance between two vectors for UpdateClustering() 
        private static double Distance(double[] tuple, double[] centroid)
        {
            double sumSquaredDiffs = 0.0;
            for (int j = 0; j < tuple.Length; ++j)
            {
                sumSquaredDiffs += (tuple[j] - centroid[j]) * (tuple[j] - centroid[j]);
            }

            return Math.Sqrt(sumSquaredDiffs);
        }

        // helper for UpdateClustering() to find closest centroid 
        private static int MinIndex(double[] distances)
        {
            int indexOfMin = 0;
            double smallDist = distances[0];
            for (int k = 1; k < distances.Length; ++k)
            {
                if (distances[k] < smallDist)
                {
                    smallDist = distances[k];
                    indexOfMin = k;
                }
            }
            return indexOfMin;
        }
    }
}
