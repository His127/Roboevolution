using System;
using System.Collections.Generic;
using Godot;

public class TreeLayoutProcessor {

    [Export]
    private float nodeDiameter = 50f;

    [Export]
    private float averagePathLength = 150f;

    private float NodeRadius => nodeDiameter / 2f;

    // Extra space between two nodes.
    private float MinimumNodeDistance => nodeDiameter * 1.1f;

    // Extra space around paths.
    [Export]
    private float nodePathClearance = 30f;

    public TreeLayout ProcessTreeToLayout(TreeRoot tree) {
        Dictionary<TreeNode, Vector2> positions = GeneratePositions(tree);

        return new TreeLayout {
            positions = positions,
            connections = GenerateConnections(tree, positions)
        };
    }

    private Dictionary<TreeNode, Vector2> GeneratePositions(TreeRoot tree) {
        Dictionary<TreeNode, Vector2> positions = new();

        List<(Vector2 start, Vector2 end)> connections = new();

        positions[tree.Root] = Vector2.Zero;

        int childCount = tree.Root.Children.Count;

        for(int i = 0; i < childCount; i++) {
            Vector2 direction = GetRootDirection(i, childCount);

            PlaceNode(
                tree.Root,
                tree.Root.Children[i],
                positions,
                connections,
                direction
            );
        }

        return positions;
    }

    private void PlaceNode(
        TreeNode parent,
        TreeNode node,
        Dictionary<TreeNode, Vector2> positions,
        List<(Vector2 start, Vector2 end)> connections,
        Vector2 preferredDirection
    ) {
        Vector2 parentPosition = positions[parent];

        // GD.Print(
        //     $"Placing '{node.Title}' " +
        //     $"at parent position {parentPosition}"
        // );

        Vector2 position = FindFreePosition(
            parentPosition,
            preferredDirection,
            positions,
            connections
        );

        positions[node] = position;

        // The path to this node is now part of the layout.
        connections.Add((
            parentPosition,
            position
        ));

        int childCount = node.Children.Count;

        for(int i = 0; i < childCount; i++) {
            Vector2 direction = GetChildDirection(
                preferredDirection,
                i,
                childCount
            );

            PlaceNode(
                node,
                node.Children[i],
                positions,
                connections,
                direction
            );
        }
    }

    private Vector2 FindFreePosition(
        Vector2 parentPosition,
        Vector2 preferredDirection,
        Dictionary<TreeNode, Vector2> positions,
        List<(Vector2 start, Vector2 end)> connections
    ) {
        Vector2[] directions = GenerateCandidateDirections(
            preferredDirection
        );

        // Keep increasing the distance until a valid position is found.
        for(int distanceStep = 1; distanceStep <= 100; distanceStep++) {
            float distance = averagePathLength * GetDistanceMultiplier(distanceStep);

            foreach(Vector2 direction in directions) {
                Vector2 position = parentPosition + direction * distance;

                if(IsPositionFree(
                    position,
                    parentPosition,
                    positions,
                    connections
                )) {
                    return position;
                }
            }
        }

        throw new InvalidOperationException(
            $"Could not find a free position for node near {parentPosition}. " +
            $"Existing nodes: {positions.Count}, " +
            $"Existing paths: {connections.Count}"
        );
    }

    private float GetDistanceMultiplier(int distanceStep) {
        return 0.75f + distanceStep * 0.5f;
    }

    private Vector2[] GenerateCandidateDirections(
        Vector2 preferredDirection
    ) {
        List<Vector2> directions = new();

        float baseAngle = preferredDirection.Angle();

        // Try the complete 360 degrees in 5 degree steps.
        for(float angle = 0f; angle < Mathf.Tau; angle += Mathf.DegToRad(5f)) {
            directions.Add(
                Vector2.FromAngle(baseAngle + angle)
            );
        }

        return directions.ToArray();
    }

    private bool IsPositionFree(
        Vector2 position,
        Vector2 parentPosition,
        Dictionary<TreeNode, Vector2> positions,
        List<(Vector2 start, Vector2 end)> connections
    ) {
        // Check the new node against all existing nodes.
        foreach(Vector2 otherPosition in positions.Values) {
            if(otherPosition.DistanceTo(parentPosition) < 0.01f)
                continue;

            if(position.DistanceTo(otherPosition) < MinimumNodeDistance)
                return false;
        }

        // Check the new path against existing nodes.
        foreach(Vector2 otherPosition in positions.Values) {
            if(otherPosition.DistanceTo(parentPosition) < 0.01f)
                continue;

            if(SegmentIntersectsCircle(
                parentPosition,
                position,
                otherPosition,
                NodeRadius + nodePathClearance
            )) {
                return false;
            }
        }

        // Check the new node against existing paths.
        foreach((Vector2 start, Vector2 end) connection in connections) {
            bool sharesParent =
                connection.start.DistanceTo(parentPosition) < 0.01f ||
                connection.end.DistanceTo(parentPosition) < 0.01f;

            if(sharesParent)
                continue;

            if(SegmentIntersectsCircle(
                connection.start,
                connection.end,
                position,
                NodeRadius + nodePathClearance
            )) {
                return false;
            }
        }

        // Check the new path against existing paths.
        foreach((Vector2 start, Vector2 end) connection in connections) {
            bool sharesParent =
                connection.start.DistanceTo(parentPosition) < 0.01f ||
                connection.end.DistanceTo(parentPosition) < 0.01f;

            if(sharesParent)
                continue;

            if(SegmentsIntersect(
                parentPosition,
                position,
                connection.start,
                connection.end
            )) {
                return false;
            }
        }

        return true;
    }

    private bool SegmentIntersectsCircle(
        Vector2 start,
        Vector2 end,
        Vector2 circleCenter,
        float radius
    ) {
        Vector2 segment = end - start;

        if(segment.LengthSquared() == 0)
            return start.DistanceTo(circleCenter) <= radius;

        float t = (circleCenter - start).Dot(segment)
            / segment.LengthSquared();

        t = Mathf.Clamp(t, 0f, 1f);

        Vector2 closestPoint = start + segment * t;

        return closestPoint.DistanceTo(circleCenter) <= radius;
    }

    private bool SegmentsIntersect(
        Vector2 startA,
        Vector2 endA,
        Vector2 startB,
        Vector2 endB
    ) {
        const float epsilon = 0.001f;

        float orientation1 = Cross(
            endA - startA,
            startB - startA
        );

        float orientation2 = Cross(
            endA - startA,
            endB - startA
        );

        float orientation3 = Cross(
            endB - startB,
            startA - startB
        );

        float orientation4 = Cross(
            endB - startB,
            endA - startB
        );

        // General intersection.
        if(
            ((orientation1 > epsilon && orientation2 < -epsilon) ||
             (orientation1 < -epsilon && orientation2 > epsilon)) &&
            ((orientation3 > epsilon && orientation4 < -epsilon) ||
             (orientation3 < -epsilon && orientation4 > epsilon))
        ) {
            return true;
        }

        // Collinear intersection.
        if(Mathf.Abs(orientation1) <= epsilon &&
           IsPointOnSegment(startB, startA, endA))
            return true;

        if(Mathf.Abs(orientation2) <= epsilon &&
           IsPointOnSegment(endB, startA, endA))
            return true;

        if(Mathf.Abs(orientation3) <= epsilon &&
           IsPointOnSegment(startA, startB, endB))
            return true;

        if(Mathf.Abs(orientation4) <= epsilon &&
           IsPointOnSegment(endA, startB, endB))
            return true;

        return false;
    }

    private float Cross(Vector2 a, Vector2 b) {
        return a.X * b.Y - a.Y * b.X;
    }

    private bool IsPointOnSegment(
        Vector2 point,
        Vector2 start,
        Vector2 end
    ) {
        return point.X >= Mathf.Min(start.X, end.X) - 0.001f &&
               point.X <= Mathf.Max(start.X, end.X) + 0.001f &&
               point.Y >= Mathf.Min(start.Y, end.Y) - 0.001f &&
               point.Y <= Mathf.Max(start.Y, end.Y) + 0.001f;
    }

    private Vector2 GetRootDirection(int index, int childCount) {
        if(childCount == 0)
            return Vector2.Right;

        float angle = Mathf.Tau * index / childCount;

        return Vector2.FromAngle(angle);
    }

    private Vector2 GetChildDirection(
        Vector2 parentDirection,
        int index,
        int childCount
    ) {
        if(childCount == 0)
            return parentDirection;

        if(childCount == 1)
            return parentDirection;

        float parentAngle = parentDirection.Angle();

        float spread = Mathf.Pi / 2f;

        float angle = parentAngle
            - spread / 2f
            + spread * index / (childCount - 1);

        return Vector2.FromAngle(angle);
    }

    private List<(Vector2 start, Vector2 end)> GenerateConnections(
        TreeRoot root,
        Dictionary<TreeNode, Vector2> positions
    ) {
        List<(Vector2 start, Vector2 end)> connections = new();

        AddConnections(
            root.Root,
            positions,
            connections
        );

        return connections;
    }

    private void AddConnections(
        TreeNode parent,
        Dictionary<TreeNode, Vector2> positions,
        List<(Vector2 start, Vector2 end)> connections
    ) {
        foreach(TreeNode child in parent.Children) {
            connections.Add((
                positions[parent],
                positions[child]
            ));

            AddConnections(
                child,
                positions,
                connections
            );
        }
    }
}