using Godot;
using System.Collections.Generic;

public class InitialPositionGenerator {

    private readonly float averagePathLength;

    public InitialPositionGenerator(TreeLayoutSettings settings) {
        averagePathLength = settings.AveragePathLength;
    }

    public Dictionary<TreeNode, Vector2> Generate(TreeRoot tree) {
        Dictionary<TreeNode, Vector2> positions = new();

        positions[tree.Root] = Vector2.Zero;

        for(int i = 0; i < tree.Root.Children.Count; i++) {
            Vector2 direction = GetRootDirection(
                i,
                tree.Root.Children.Count
            );

            PlaceNode(
                tree.Root.Children[i],
                positions,
                Vector2.Zero,
                direction
            );
        }

        return positions;
    }

    private void PlaceNode(
        TreeNode node,
        Dictionary<TreeNode, Vector2> positions,
        Vector2 parentPosition,
        Vector2 parentDirection
    ) {
        Vector2 position =
            parentPosition + parentDirection * averagePathLength;

        positions[node] = position;

        for(int i = 0; i < node.Children.Count; i++) {
            Vector2 direction = GetChildDirection(
                parentDirection,
                i,
                node.Children.Count
            );

            PlaceNode(
                node.Children[i],
                positions,
                position,
                direction
            );
        }
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

        float angle =
            parentAngle
            - spread / 2f
            + spread * index / (childCount - 1);

        return Vector2.FromAngle(angle);
    }
}
