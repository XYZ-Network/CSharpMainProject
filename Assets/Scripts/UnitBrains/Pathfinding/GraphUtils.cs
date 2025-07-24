using System.Collections.Generic;
using System.Linq;

namespace UnitBrains.Pathfinding
{
    /// <summary>
    /// Represents a graph structure used for A* pathfinding.
    /// </summary>
    /// <remarks>
    /// If the graph dynamically generates nodes rather than taking them from some internal storage,
    /// ensure that <typeparamref name="TNode"/> correctly overrides <c>Equals</c> and <c>GetHashCode</c>.
    /// This is necessary so that nodes with identical attributes (e.g., coordinates) are treated as equal.
    /// Improper equality implementation may result in incorrect behavior or infinite loops.
    /// </remarks>
    /// <typeparam name="TNode">The type representing a node in the graph. Must be non-nullable.</typeparam>
    public interface IGraph<TNode> where TNode : notnull
    {
        /// <summary>
        /// Returns a collection of nodes that are directly reachable from the specified node.
        /// </summary>
        /// <param name="node">The node whose neighbors should be retrieved.</param>
        /// <returns>An enumerable, providing neighboring nodes.</returns>
        IEnumerable<TNode> GetNeighbors(TNode node);

        /// <summary>
        /// Returns the cost of moving from the given node to one of its neighbors.
        /// </summary>
        /// <param name="node">The current node.</param>
        /// <param name="neighbor">A neighboring node of the current node.</param>
        /// <returns>The movement cost between the two nodes.</returns>
        float GetEdgeCost(TNode node, TNode neighbor);

        /// <summary>
        /// Returns the estimated cost (heuristic) from the given node to the target node.
        /// </summary>
        /// <param name="node">The node from which to estimate the cost.</param>
        /// <param name="targetNode">The target node for the heuristic estimate.</param>
        /// <returns>An integer representing the estimated cost to reach the target.</returns>
        float GetHeuristic(TNode node, TNode targetNode);
    }

    public static class GraphUtils
    {
        /// <summary>
        /// Finds the shortest path from a start node to a target node using the A* search algorithm.
        /// </summary>
        /// <typeparam name="TNode">The type representing a graph node.</typeparam>
        /// <param name="graph">The graph to search, providing neighbors, edge costs, and heuristics.</param>
        /// <param name="startNode">The node from which the pathfinding begins.</param>
        /// <param name="targetNode">The destination node to reach.</param>
        /// <returns>
        /// A list of nodes representing the shortest path from <paramref name="startNode"/> to
        /// <paramref name="targetNode"/>, where the next node is first element in the list and target
        /// node is the last one. If no path is found, returns an empty list.
        /// </returns>
        /// <remarks>
        /// If the <paramref name="graph"/> dynamically generates nodes rather than taking them from internal storage,
        /// ensure that <typeparamref name="TNode"/> correctly overrides <c>Equals</c> and <c>GetHashCode</c>.
        /// Improper equality implementation may result in incorrect behavior or infinite loops.
        /// </remarks>
        public static List<TNode> FindPathAStar<TNode>(IGraph<TNode> graph, TNode startNode, TNode targetNode)
            where TNode : notnull
        {
            // We'll use priority queue to avoid manual iteration to find the best node to visit.
            PriorityQueue<TNode, float> pQueue = new PriorityQueue<TNode, float>();

            // For specific node `N` contains total movement cost from `startNode` to `N`.  
            Dictionary<TNode, float> totalCosts = new();

            // Contains information about our traversal. Key is a node and value is neighbor node which we came from.
            // This map will be used at the end to restore found path.
            Dictionary<TNode, TNode> visitMap = new();

            pQueue.Enqueue(startNode, 0);
            totalCosts[startNode] = 0;

            while (pQueue.Count > 0)
            {
                TNode currentNode = pQueue.Dequeue();

                if (currentNode.Equals(targetNode))
                {
                    break;
                }

                IEnumerable<TNode> neighbors = graph.GetNeighbors(currentNode);
                float currentCost = totalCosts[currentNode];

                foreach (TNode neighbor in neighbors)
                {
                    float newNeighborCost = currentCost + graph.GetEdgeCost(currentNode, neighbor);

                    if (!totalCosts.TryGetValue(neighbor, out float existingCost) || newNeighborCost < existingCost)
                    {
                        totalCosts[neighbor] = newNeighborCost;

                        float priority = newNeighborCost + graph.GetHeuristic(neighbor, targetNode);
                        pQueue.Enqueue(neighbor, priority);
                        visitMap[neighbor] = currentNode;
                    }
                }
            }

            // Now build the path.
            Stack<TNode> foundPath = new();
            TNode node = targetNode;

            while (!node.Equals(startNode) && visitMap.TryGetValue(node, out TNode prev))
            {
                foundPath.Push(node);
                node = prev;
            }

            if (foundPath.Count > 0)
            {
                foundPath.Push(node);                
            }

            return foundPath.ToList();
        }
    }
}