using System.Collections.Generic;
using Godot;

public class TreeLayoutSimulator {

    private readonly float averagePathLength;
    private readonly float attractionStrength;
    private readonly float repulsionStrength;
    private readonly int simulationIterations;
    private readonly float minimumNodeDistance;
    private readonly float movementStrength;
    private readonly float nodePathClearance;

    public TreeLayoutSimulator(TreeLayoutSettings settings) {
        averagePathLength = settings.AveragePathLength;
        attractionStrength = settings.AttractionStrength;
        repulsionStrength = settings.RepulsionStrength;
        simulationIterations = settings.SimulationIterations;
        minimumNodeDistance = settings.MinimumNodeDistance;
        movementStrength = settings.MovementStrength;
        nodePathClearance = settings.NodePathClearance;
    }

    public void Simulate(
        TreeRoot tree,
        Dictionary<TreeNode, Vector2> positions
    ) {
        for(int i = 0; i < simulationIterations; i++) {
            Dictionary<TreeNode, Vector2> forces =
                CalculateForces(tree, positions);

            ApplyForces(tree, positions, forces);
        }

        positions[tree.Root] = Vector2.Zero;
    }

    private Dictionary<TreeNode, Vector2> CalculateForces(
        TreeRoot tree,
        Dictionary<TreeNode, Vector2> positions
    ) {
        Dictionary<TreeNode, Vector2> forces = new();

        foreach(TreeNode node in positions.Keys)
            forces[node] = Vector2.Zero;

        CalculateAttraction(
            tree.Root,
            positions,
            forces
        );

        CalculateRepulsion(
            positions,
            forces
        );

        CalculateNodePathRepulsion(
            tree,
            positions,
            forces
        );

        CalculatePathPathRepulsion(
            tree,
            positions,
            forces
        );

        return forces;
    }

    private void CalculateAttraction(
        TreeNode parent,
        Dictionary<TreeNode, Vector2> positions,
        Dictionary<TreeNode, Vector2> forces
    ) {
        Vector2 parentPosition = positions[parent];

        foreach(TreeNode child in parent.Children) {
            Vector2 childPosition = positions[child];

            Vector2 difference = childPosition - parentPosition;
            float distance = difference.Length();

            if(distance > 0.001f) {
                Vector2 direction = difference / distance;

                float force =
                    (distance - averagePathLength)
                    * attractionStrength;

                force = Mathf.Clamp(force, -10f, 10f);

                forces[parent] += direction * force;
                forces[child] -= direction * force;
            }

            CalculateAttraction(
                child,
                positions,
                forces
            );
        }
    }

    private void CalculateRepulsion(
        Dictionary<TreeNode, Vector2> positions,
        Dictionary<TreeNode, Vector2> forces
    ) {
        TreeNode[] nodes = new TreeNode[positions.Count];

        positions.Keys.CopyTo(nodes, 0);

        for(int i = 0; i < nodes.Length; i++) {
            for(int j = i + 1; j < nodes.Length; j++) {
                TreeNode first = nodes[i];
                TreeNode second = nodes[j];

                Vector2 difference =
                    positions[first] - positions[second];

                float distance = difference.Length();

                if(distance < 0.001f)
                    continue;

                Vector2 direction = difference / distance;

                float overlap =
                    minimumNodeDistance - distance;

                if(overlap <= 0f)
                    continue;

                float force =
                    overlap * repulsionStrength;

                force = Mathf.Min(force, 10f);

                forces[first] += direction * force;
                forces[second] -= direction * force;
            }
        }
    }

    private void CalculateNodePathRepulsion(
        TreeRoot tree,
        Dictionary<TreeNode, Vector2> positions,
        Dictionary<TreeNode, Vector2> forces
    ) {
        float minimumDistance =
            minimumNodeDistance + nodePathClearance;

        TreeNode[] nodes = new TreeNode[positions.Count];
        positions.Keys.CopyTo(nodes, 0);

        List<Connection> connections =
            GetConnections(tree);

        foreach(TreeNode node in nodes) {
            foreach(Connection connection in connections) {
                if(node == connection.parent ||
                   node == connection.child)
                    continue;

                Vector2 nodePosition = positions[node];

                Vector2 pathStart =
                    positions[connection.parent];

                Vector2 pathEnd =
                    positions[connection.child];

                Vector2 closestPoint =
                    GetClosestPointOnSegment(
                        nodePosition,
                        pathStart,
                        pathEnd
                    );

                Vector2 difference =
                    nodePosition - closestPoint;

                float distance = difference.Length();

                if(distance < 0.001f ||
                   distance >= minimumDistance)
                    continue;

                Vector2 direction =
                    difference / distance;

                float overlap =
                    minimumDistance - distance;

                float force =
                    overlap * repulsionStrength;

                force = Mathf.Min(force, 10f);

                forces[node] += direction * force;

                forces[connection.parent] -=
                    direction * force * 0.5f;

                forces[connection.child] -=
                    direction * force * 0.5f;
            }
        }
    }

    private void CalculatePathPathRepulsion(
    TreeRoot tree,
    Dictionary<TreeNode, Vector2> positions,
    Dictionary<TreeNode, Vector2> forces
) {
    List<Connection> connections =
        GetConnections(tree);

    float minimumDistance =
        nodePathClearance;

    for(int i = 0; i < connections.Count; i++) {
        for(int j = i + 1; j < connections.Count; j++) {
            Connection first = connections[i];
            Connection second = connections[j];

            if(ShareNode(first, second))
                continue;

            Vector2 firstStart =
                positions[first.parent];

            Vector2 firstEnd =
                positions[first.child];

            Vector2 secondStart =
                positions[second.parent];

            Vector2 secondEnd =
                positions[second.child];

            if(SegmentsIntersect(
                firstStart,
                firstEnd,
                secondStart,
                secondEnd
            )) {
                ResolvePathIntersection(
                    first,
                    second,
                    positions,
                    forces
                );

                continue;
            }

            float distance = GetSegmentDistance(
                firstStart,
                firstEnd,
                secondStart,
                secondEnd
            );

            if(distance >= minimumDistance)
                continue;

            Vector2 firstPoint =
                GetClosestPointOnSegment(
                    secondStart,
                    firstStart,
                    firstEnd
                );

            Vector2 secondPoint =
                GetClosestPointOnSegment(
                    firstStart,
                    secondStart,
                    secondEnd
                );

            Vector2 difference =
                firstPoint - secondPoint;

            if(difference.LengthSquared() < 0.001f)
                continue;

            Vector2 direction =
                difference.Normalized();

            float overlap =
                minimumDistance - distance;

            float force =
                overlap * repulsionStrength;

            force = Mathf.Min(force, 10f);

            forces[first.parent] +=
                direction * force * 0.25f;

            forces[first.child] +=
                direction * force * 0.25f;

            forces[second.parent] -=
                direction * force * 0.25f;

            forces[second.child] -=
                direction * force * 0.25f;
        }
    }
}

private bool SegmentsIntersect(
    Vector2 firstStart,
    Vector2 firstEnd,
    Vector2 secondStart,
    Vector2 secondEnd
) {
    float firstSide1 = Cross(
        firstEnd - firstStart,
        secondStart - firstStart
    );

    float firstSide2 = Cross(
        firstEnd - firstStart,
        secondEnd - firstStart
    );

    float secondSide1 = Cross(
        secondEnd - secondStart,
        firstStart - secondStart
    );

    float secondSide2 = Cross(
        secondEnd - secondStart,
        firstEnd - secondStart
    );

    return firstSide1 * firstSide2 < 0f &&
           secondSide1 * secondSide2 < 0f;
}

private float Cross(Vector2 first, Vector2 second) {
    return first.X * second.Y -
           first.Y * second.X;
}

private void ResolvePathIntersection(
    Connection first,
    Connection second,
    Dictionary<TreeNode, Vector2> positions,
    Dictionary<TreeNode, Vector2> forces
) {
    Vector2 firstDirection =
        positions[first.child] -
        positions[first.parent];

    Vector2 secondDirection =
        positions[second.child] -
        positions[second.parent];

    if(firstDirection.LengthSquared() < 0.001f ||
       secondDirection.LengthSquared() < 0.001f)
        return;

    Vector2 normal =
        new Vector2(
            -secondDirection.Y,
            secondDirection.X
        ).Normalized();

    float force = Mathf.Min(
        repulsionStrength * 5f,
        10f
    );

    forces[first.parent] +=
        normal * force * 0.5f;

    forces[first.child] +=
        normal * force * 0.5f;

    forces[second.parent] -=
        normal * force * 0.5f;

    forces[second.child] -=
        normal * force * 0.5f;
}

    private bool ShareNode(
        Connection first,
        Connection second
    ) {
        return first.parent == second.parent ||
               first.parent == second.child ||
               first.child == second.parent ||
               first.child == second.child;
    }

    private float GetSegmentDistance(
        Vector2 firstStart,
        Vector2 firstEnd,
        Vector2 secondStart,
        Vector2 secondEnd
    ) {
        Vector2 firstClosest =
            GetClosestPointOnSegment(
                secondStart,
                firstStart,
                firstEnd
            );

        Vector2 secondClosest =
            GetClosestPointOnSegment(
                firstStart,
                secondStart,
                secondEnd
            );

        return firstClosest.DistanceTo(secondClosest);
    }

    private Vector2 GetClosestPointOnSegment(
        Vector2 point,
        Vector2 start,
        Vector2 end
    ) {
        Vector2 segment = end - start;

        float lengthSquared =
            segment.LengthSquared();

        if(lengthSquared < 0.001f)
            return start;

        float t =
            (point - start).Dot(segment)
            / lengthSquared;

        t = Mathf.Clamp(t, 0f, 1f);

        return start + segment * t;
    }

    private List<Connection> GetConnections(TreeRoot tree) {
        List<Connection> connections = new();

        AddConnections(
            tree.Root,
            connections
        );

        return connections;
    }

    private void AddConnections(
        TreeNode parent,
        List<Connection> connections
    ) {
        foreach(TreeNode child in parent.Children) {
            connections.Add(new(parent, child));

            AddConnections(
                child,
                connections
            );
        }
    }

    private void ApplyForces(
        TreeRoot tree,
        Dictionary<TreeNode, Vector2> positions,
        Dictionary<TreeNode, Vector2> forces
    ) {
        foreach(TreeNode node in positions.Keys) {
            if(node == tree.Root)
                continue;

            Vector2 force = forces[node];

            if(force.Length() > 10f)
                force = force.Normalized() * 10f;

            Vector2 movement =
                force * movementStrength;

            positions[node] += movement;
        }
    }

    private struct Connection {
        public TreeNode parent;
        public TreeNode child;

        public Connection(
            TreeNode parent,
            TreeNode child
        ) {
            this.parent = parent;
            this.child = child;
        }
    }
}