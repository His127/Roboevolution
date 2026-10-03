using Godot;
using System.Collections.Generic;

public class InitialPositionGenerator {

    private readonly float averagePathLength;
    private const float MaxWedge = Mathf.Pi * 0.95f; // Sektor < 180°, sonst Garantie weg

    public InitialPositionGenerator(TreeLayoutSettings settings) {
        averagePathLength = settings.AveragePathLength;
    }

    public Dictionary<TreeNode, Vector2> Generate(TreeRoot tree) {
        Dictionary<TreeNode, Vector2> positions = new();
        Dictionary<TreeNode, int> leaves = new();

        CountLeaves(tree.Root, leaves);

        positions[tree.Root] = Vector2.Zero;
        PlaceChildren(tree.Root, 0f, Mathf.Tau, 1, leaves, positions);

        return positions;
    }

    private int CountLeaves(TreeNode node, Dictionary<TreeNode, int> leaves) {
        int count = 0;
        foreach(TreeNode child in node.Children)
            count += CountLeaves(child, leaves);

        if(count == 0) count = 1;
        leaves[node] = count;
        return count;
    }

    private void PlaceChildren(
        TreeNode parent,
        float start,
        float end,
        int depth,
        Dictionary<TreeNode, int> leaves,
        Dictionary<TreeNode, Vector2> positions
    ) {
        if(parent.Children.Count == 0)
            return;

        // depth == 1 bedeutet: parent ist die Wurzel.
        // Die Wurzel darf den vollen Kreis nutzen, wenn sie mehrere Kinder hat.
        bool isRoot = depth == 1;
        float maxWedge =
            isRoot && parent.Children.Count > 1
                ? Mathf.Tau
                : Mathf.Pi;

        float width = end - start;
        if(width > maxWedge) {
            float center = (start + end) / 2f;
            start = center - maxWedge / 2f;
            end = center + maxWedge / 2f;
            width = maxWedge;
        }

        float total = leaves[parent];
        float angle = start;

        foreach(TreeNode child in parent.Children) {
            float share = width * leaves[child] / total;
            float mid = angle + share / 2f;

            positions[child] =
                Vector2.FromAngle(mid) * depth * averagePathLength;

            PlaceChildren(child, angle, angle + share,
                depth + 1, leaves, positions);

            angle += share;
        }
    }
}