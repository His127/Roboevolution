using System.Collections.Generic;
using Godot;

/// <summary>
/// Kräftebasierte Simulation für Baum-Layouts.
/// Voraussetzung: Die Startpositionen sind kreuzungsfrei (planar).
/// Jede Bewegung, die eine Kreuzung erzeugen würde, wird verkleinert
/// oder verworfen, dadurch bleibt das Layout planar.
/// </summary>
public class TreeLayoutSimulator {

    private const float MaxForce = 10f;
    private const float MinDistance = 0.001f;
    private const float TouchEpsilon = 1f;     // Kanten dichter als das zählen als Kreuzung
    private const int MaxStepAttempts = 6;     // Halbierungen pro Knoten und Iteration

    private readonly float nodeDiameter;
    private readonly float averagePathLength;
    private readonly float attractionStrength;
    private readonly float repulsionStrength;
    private readonly int simulationIterations;
    private readonly float minimumNodeDistance;
    private readonly float movementStrength;
    private readonly float nodePathClearance;

    // Pro Simulate-Aufruf gesetzt
    private List<Connection> connections;
    private Dictionary<TreeNode, List<int>> incident;
    private List<TreeNode> nodes;

    public TreeLayoutSimulator(TreeLayoutSettings settings) {
        nodeDiameter = settings.NodeDiameter;
        averagePathLength = settings.AveragePathLength;
        attractionStrength = settings.AttractionStrength;
        repulsionStrength = settings.RepulsionStrength;
        simulationIterations = settings.SimulationIterations;
        minimumNodeDistance = settings.MinimumNodeDistance;
        movementStrength = settings.MovementStrength;
        nodePathClearance = settings.NodePathClearance;
    }

    // ------------------------------------------------------------------
    // Einstieg
    // ------------------------------------------------------------------

    public void Simulate(
        TreeRoot tree,
        Dictionary<TreeNode, Vector2> positions
    ) {
        Prepare(tree, positions);

        positions[tree.Root] = Vector2.Zero;

        for(int i = 0; i < simulationIterations; i++) {
            Dictionary<TreeNode, Vector2> forces =
                CalculateForces(positions);

            ApplyForces(tree, positions, forces);
        }

        positions[tree.Root] = Vector2.Zero;
    }

    /// <summary>
    /// Prüft, ob das komplette Layout kreuzungsfrei ist (zum Debuggen).
    /// </summary>
    public bool IsPlanar(
        TreeRoot tree,
        Dictionary<TreeNode, Vector2> positions
    ) {
        Prepare(tree, positions);

        for(int i = 0; i < connections.Count; i++) {
            for(int j = i + 1; j < connections.Count; j++) {
                if(ShareNode(connections[i], connections[j]))
                    continue;

                if(EdgesConflict(connections[i], connections[j], positions))
                    return false;
            }
        }

        return true;
    }

    // ------------------------------------------------------------------
    // Vorbereitung (Kanten, Knotenliste, Inzidenz werden nur einmal gebaut)
    // ------------------------------------------------------------------

    private void Prepare(
        TreeRoot tree,
        Dictionary<TreeNode, Vector2> positions
    ) {
        connections = new List<Connection>();
        AddConnections(tree.Root, connections);

        nodes = new List<TreeNode>(positions.Keys);

        incident = new Dictionary<TreeNode, List<int>>();
        foreach(TreeNode node in nodes)
            incident[node] = new List<int>();

        for(int i = 0; i < connections.Count; i++) {
            incident[connections[i].parent].Add(i);
            incident[connections[i].child].Add(i);
        }
    }

    private void AddConnections(
        TreeNode parent,
        List<Connection> result
    ) {
        foreach(TreeNode child in parent.Children) {
            result.Add(new Connection(parent, child));
            AddConnections(child, result);
        }
    }

    // ------------------------------------------------------------------
    // Kräfte
    // ------------------------------------------------------------------

    private Dictionary<TreeNode, Vector2> CalculateForces(
        Dictionary<TreeNode, Vector2> positions
    ) {
        Dictionary<TreeNode, Vector2> forces = new();

        foreach(TreeNode node in nodes)
            forces[node] = Vector2.Zero;

        CalculateAttraction(positions, forces);
        CalculateNodeNodeRepulsion(positions, forces);
        CalculateNodePathRepulsion(positions, forces);
        CalculatePathPathRepulsion(positions, forces);

        return forces;
    }

    // Feder zwischen Eltern und Kindern auf Ziel-Kantenlänge
    private void CalculateAttraction(
        Dictionary<TreeNode, Vector2> positions,
        Dictionary<TreeNode, Vector2> forces
    ) {
        foreach(Connection c in connections) {
            Vector2 difference = positions[c.child] - positions[c.parent];
            float distance = difference.Length();

            if(distance < MinDistance)
                continue;

            Vector2 direction = difference / distance;

            float force = Mathf.Clamp(
                (distance - averagePathLength) * attractionStrength,
                -MaxForce,
                MaxForce
            );

            forces[c.parent] += direction * force;
            forces[c.child] -= direction * force;
        }
    }

    // Knoten stoßen Knoten ab
    private void CalculateNodeNodeRepulsion(
        Dictionary<TreeNode, Vector2> positions,
        Dictionary<TreeNode, Vector2> forces
    ) {
        for(int i = 0; i < nodes.Count; i++) {
            for(int j = i + 1; j < nodes.Count; j++) {
                TreeNode first = nodes[i];
                TreeNode second = nodes[j];

                Vector2 difference = positions[first] - positions[second];
                float distance = difference.Length();

                if(distance < MinDistance || distance >= minimumNodeDistance)
                    continue;

                Vector2 direction = difference / distance;

                float force = Mathf.Min(
                    (minimumNodeDistance - distance) * repulsionStrength,
                    MaxForce
                );

                forces[first] += direction * force;
                forces[second] -= direction * force;
            }
        }
    }

    // Knoten stoßen fremde Kanten ab
    private void CalculateNodePathRepulsion(
        Dictionary<TreeNode, Vector2> positions,
        Dictionary<TreeNode, Vector2> forces
    ) {
        // Halber Knotendurchmesser + Abstand zur Kante
        float minimumDistance = nodeDiameter / 2f + nodePathClearance;

        foreach(TreeNode node in nodes) {
            Vector2 nodePosition = positions[node];

            foreach(Connection c in connections) {
                if(node == c.parent || node == c.child)
                    continue;

                Vector2 start = positions[c.parent];
                Vector2 end = positions[c.child];

                Vector2 closest =
                    GetClosestPointOnSegment(nodePosition, start, end);

                Vector2 difference = nodePosition - closest;
                float distance = difference.Length();

                if(distance < MinDistance || distance >= minimumDistance)
                    continue;

                Vector2 direction = difference / distance;

                float force = Mathf.Min(
                    (minimumDistance - distance) * repulsionStrength,
                    MaxForce
                );

                forces[node] += direction * force;
                forces[c.parent] -= direction * force * 0.5f;
                forces[c.child] -= direction * force * 0.5f;
            }
        }
    }

    // Kanten halten Abstand zu Kanten (nur Abstand, Kreuzungen
    // werden in ApplyForces verhindert)
    private void CalculatePathPathRepulsion(
        Dictionary<TreeNode, Vector2> positions,
        Dictionary<TreeNode, Vector2> forces
    ) {
        for(int i = 0; i < connections.Count; i++) {
            for(int j = i + 1; j < connections.Count; j++) {
                Connection first = connections[i];
                Connection second = connections[j];

                if(ShareNode(first, second))
                    continue;

                Vector2 firstStart = positions[first.parent];
                Vector2 firstEnd = positions[first.child];
                Vector2 secondStart = positions[second.parent];
                Vector2 secondEnd = positions[second.child];

                GetClosestPair(
                    firstStart, firstEnd,
                    secondStart, secondEnd,
                    out Vector2 firstPoint,
                    out Vector2 secondPoint
                );

                Vector2 difference = firstPoint - secondPoint;
                float distance = difference.Length();

                if(distance < MinDistance || distance >= nodePathClearance)
                    continue;

                Vector2 direction = difference / distance;

                float force = Mathf.Min(
                    (nodePathClearance - distance) * repulsionStrength,
                    MaxForce
                );

                forces[first.parent] += direction * force * 0.25f;
                forces[first.child] += direction * force * 0.25f;
                forces[second.parent] -= direction * force * 0.25f;
                forces[second.child] -= direction * force * 0.25f;
            }
        }
    }

    // ------------------------------------------------------------------
    // Bewegung mit Kreuzungsschutz
    // ------------------------------------------------------------------

    private void ApplyForces(
        TreeRoot tree,
        Dictionary<TreeNode, Vector2> positions,
        Dictionary<TreeNode, Vector2> forces
    ) {
        foreach(TreeNode node in nodes) {
            if(node == tree.Root)
                continue;

            Vector2 force = forces[node];

            if(force.Length() > MaxForce)
                force = force.Normalized() * MaxForce;

            Vector2 oldPosition = positions[node];
            Vector2 step = force * movementStrength;

            if(step.LengthSquared() < MinDistance * MinDistance)
                continue;

            bool accepted = false;

            for(int attempt = 0; attempt < MaxStepAttempts; attempt++) {
                positions[node] = oldPosition + step;

                if(!CreatesConflict(node, positions)) {
                    accepted = true;
                    break;
                }

                step *= 0.5f;
            }

            if(!accepted)
                positions[node] = oldPosition;
        }
    }

    // Prüft nur die Kanten, die am bewegten Knoten hängen,
    // gegen alle Kanten, mit denen sie keinen Knoten teilen.
    private bool CreatesConflict(
        TreeNode node,
        Dictionary<TreeNode, Vector2> positions
    ) {
        foreach(int index in incident[node]) {
            Connection mine = connections[index];

            for(int k = 0; k < connections.Count; k++) {
                Connection other = connections[k];

                if(ShareNode(mine, other))
                    continue;

                if(EdgesConflict(mine, other, positions))
                    return true;
            }
        }

        return false;
    }

    // Kreuzung oder (fast) Berührung
    private bool EdgesConflict(
        Connection first,
        Connection second,
        Dictionary<TreeNode, Vector2> positions
    ) {
        Vector2 a1 = positions[first.parent];
        Vector2 a2 = positions[first.child];
        Vector2 b1 = positions[second.parent];
        Vector2 b2 = positions[second.child];

        if(SegmentsIntersect(a1, a2, b1, b2))
            return true;

        return GetSegmentDistance(a1, a2, b1, b2) < TouchEpsilon;
    }

    // ------------------------------------------------------------------
    // Geometrie
    // ------------------------------------------------------------------

    private bool ShareNode(Connection first, Connection second) {
        return first.parent == second.parent ||
               first.parent == second.child ||
               first.child == second.parent ||
               first.child == second.child;
    }

    private bool SegmentsIntersect(
        Vector2 firstStart,
        Vector2 firstEnd,
        Vector2 secondStart,
        Vector2 secondEnd
    ) {
        Vector2 firstDir = firstEnd - firstStart;
        Vector2 secondDir = secondEnd - secondStart;

        float d1 = Cross(firstDir, secondStart - firstStart);
        float d2 = Cross(firstDir, secondEnd - firstStart);
        float d3 = Cross(secondDir, firstStart - secondStart);
        float d4 = Cross(secondDir, firstEnd - secondStart);

        return d1 * d2 < 0f && d3 * d4 < 0f;
    }

    private float Cross(Vector2 first, Vector2 second) {
        return first.X * second.Y - first.Y * second.X;
    }

    private float GetSegmentDistance(
        Vector2 firstStart,
        Vector2 firstEnd,
        Vector2 secondStart,
        Vector2 secondEnd
    ) {
        // Bei nicht kreuzenden Segmenten liegt der kürzeste Abstand
        // immer an mindestens einem Endpunkt.
        float d1 = GetClosestPointOnSegment(secondStart, firstStart, firstEnd)
            .DistanceTo(secondStart);
        float d2 = GetClosestPointOnSegment(secondEnd, firstStart, firstEnd)
            .DistanceTo(secondEnd);
        float d3 = GetClosestPointOnSegment(firstStart, secondStart, secondEnd)
            .DistanceTo(firstStart);
        float d4 = GetClosestPointOnSegment(firstEnd, secondStart, secondEnd)
            .DistanceTo(firstEnd);

        return Mathf.Min(Mathf.Min(d1, d2), Mathf.Min(d3, d4));
    }

    // Nächstes Punktepaar zweier (nicht kreuzender) Segmente
    private void GetClosestPair(
        Vector2 firstStart,
        Vector2 firstEnd,
        Vector2 secondStart,
        Vector2 secondEnd,
        out Vector2 firstPoint,
        out Vector2 secondPoint
    ) {
        float best = float.MaxValue;
        firstPoint = firstStart;
        secondPoint = secondStart;

        // Kandidat 1 und 2: Endpunkte des zweiten Segments auf das erste
        TryPair(
            GetClosestPointOnSegment(secondStart, firstStart, firstEnd),
            secondStart, ref best, ref firstPoint, ref secondPoint);
        TryPair(
            GetClosestPointOnSegment(secondEnd, firstStart, firstEnd),
            secondEnd, ref best, ref firstPoint, ref secondPoint);

        // Kandidat 3 und 4: Endpunkte des ersten Segments auf das zweite
        TryPair(
            firstStart,
            GetClosestPointOnSegment(firstStart, secondStart, secondEnd),
            ref best, ref firstPoint, ref secondPoint);
        TryPair(
            firstEnd,
            GetClosestPointOnSegment(firstEnd, secondStart, secondEnd),
            ref best, ref firstPoint, ref secondPoint);
    }

    private void TryPair(
        Vector2 a,
        Vector2 b,
        ref float best,
        ref Vector2 bestA,
        ref Vector2 bestB
    ) {
        float d = a.DistanceSquaredTo(b);

        if(d < best) {
            best = d;
            bestA = a;
            bestB = b;
        }
    }

    private Vector2 GetClosestPointOnSegment(
        Vector2 point,
        Vector2 start,
        Vector2 end
    ) {
        Vector2 segment = end - start;
        float lengthSquared = segment.LengthSquared();

        if(lengthSquared < MinDistance)
            return start;

        float t = (point - start).Dot(segment) / lengthSquared;
        t = Mathf.Clamp(t, 0f, 1f);

        return start + segment * t;
    }

    private struct Connection {
        public TreeNode parent;
        public TreeNode child;

        public Connection(TreeNode parent, TreeNode child) {
            this.parent = parent;
            this.child = child;
        }
    }
}