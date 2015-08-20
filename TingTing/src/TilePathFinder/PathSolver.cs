//#define LOG

using System;
using System.Collections.Generic;
using Pathfinding.Datastructures;
using System.Threading;
using TingTing;
using GameTypes;

namespace Pathfinding
{
    public class PathSolver
    {
        private void TryQueueNewTile(PointTileNode pNewNode, PathLink pLink, AStarStack pNodesToVisit, PointTileNode pGoal)
        {
            PointTileNode previousNode = pLink.GetOtherNode(pNewNode);
            float linkDistance = pLink.distance;
            float newPathCost = previousNode.pathCostHere + pNewNode.baseCost + linkDistance;
            
            if (pNewNode.linkLeadingHere == null || (pNewNode.pathCostHere > newPathCost)) {
                pNewNode.distanceToGoal = pNewNode.DistanceTo(pGoal) * 2f;
                pNewNode.pathCostHere = newPathCost;
                pNewNode.linkLeadingHere = pLink;
                pNodesToVisit.Push(pNewNode);
            }
        }
	
        public Path FindPath(PointTileNode pStart, PointTileNode pGoal, RoomRunner pNetwork, bool pReset)
        {
#if DEBUG
			if(pNetwork == null) {
				throw new Exception("pNetwork is null");
			}
#endif
			if (pStart == null || pGoal == null) {
                return new Path(new PointTileNode[] {}, 0f, PathStatus.DESTINATION_UNREACHABLE, 0);
			}

			if (pStart == pGoal) {
                return new Path(new PointTileNode[] {}, 0f, PathStatus.ALREADY_THERE, 0);
			}

            int testCount = 0;
			
			if(pReset) {
            	pNetwork.Reset();
			}
			
            pStart.isStartNode = true;
            pGoal.isGoalNode = true;
            List<PointTileNode> resultNodeList = new List<PointTileNode>();
            
            PointTileNode currentNode = pStart;
            PointTileNode goalNode = pGoal;
            
            currentNode.visited = true;
            currentNode.linkLeadingHere = null;
            AStarStack nodesToVisit = new AStarStack();
            PathStatus pathResult = PathStatus.NOT_CALCULATED_YET;
            testCount = 1;
            
            while (pathResult == PathStatus.NOT_CALCULATED_YET) {
                foreach (PathLink l in currentNode.links) {
                    PointTileNode otherNode = l.GetOtherNode(currentNode);
                    
                    if (!otherNode.visited) {
                        TryQueueNewTile(otherNode, l, nodesToVisit, goalNode);
                    }
                }
                
                if (nodesToVisit.Count == 0) {
                    pathResult = PathStatus.DESTINATION_UNREACHABLE;
                }
                else {
                    currentNode = nodesToVisit.Pop();
                    testCount++;

#if LOG
                    D.Log("testing new node: " + currentNode);
#endif
                    currentNode.visited = true;
                    
                    if (currentNode == goalNode) {
                        pathResult = PathStatus.FOUND_GOAL;
                    }
                }
            }
            
            // Path finished, collect
            float tLength = 0;

            if (pathResult == PathStatus.FOUND_GOAL) {
                tLength = currentNode.pathCostHere;
                
                while (currentNode != pStart) {
                    resultNodeList.Add((PointTileNode)currentNode);
                    currentNode = currentNode.linkLeadingHere.GetOtherNode(currentNode);
                }
                
                resultNodeList.Add((PointTileNode)currentNode);
                resultNodeList.Reverse();
            }
            
            return new Path(resultNodeList.ToArray(), tLength, pathResult, testCount);
        }

    }
}
