using System.Text;
using TingTing;

namespace Pathfinding
{
    public enum PathStatus
    {
        NOT_CALCULATED_YET,
        DESTINATION_UNREACHABLE,
        FOUND_GOAL,
        ALREADY_THERE
    }

    public struct Path
    {
        public PathStatus status;
        public float pathLength;
        public PointTileNode[] nodes;
        public int pathSearchTestCount;

		public Path(PointTileNode[] pNodes, float pPathLength, PathStatus pStatus, int pPathSearchTestCount)
        {
            nodes = pNodes;
            pathLength = pPathLength;
            status = pStatus;
            pathSearchTestCount = pPathSearchTestCount;
        }

        public static Path EMPTY {
            get {
                return new Path(new PointTileNode[0], 0f, PathStatus.NOT_CALCULATED_YET, 0);
            }
        }
        
        public PointTileNode LastNode {
            get {
                return nodes[nodes.Length - 1];
            }
        }
                
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Path: \n[ ");
            
            foreach (PointTileNode ipn in nodes) {
                sb.Append(ipn.ToString() + ",\n");
            }
            
            sb.Append("]");
            return sb.ToString();
        }

		public override bool Equals(object pOther)
		{
			if(!(pOther is Path)) return false;
			var other = (Path)pOther;
			if(status != other.status) return false;
			else if(pathLength != other.pathLength) return false;

			for(int i = 0; i < pathLength; i++) {
                if((System.IEquatable<PointTileNode>)nodes[i] != (System.IEquatable<PointTileNode>)other.nodes[i]) return false;
			}

			return true;
		}

		public static bool operator ==(Path a, Path b) {
			return a.Equals(b);
		}

		public static bool operator !=(Path a, Path b) {
			return !a.Equals(b);
		}
    }
}
