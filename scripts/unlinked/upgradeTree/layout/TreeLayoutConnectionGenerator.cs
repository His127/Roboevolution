using System.Collections.Generic;
using Godot;

public class TreeLayoutConnectionGenerator {

    public List<(Vector2 start, Vector2 end)> Generate(
        TreeRoot tree,
        Dictionary<TreeNode, Vector2> positions
    ) {
        List<(Vector2 start, Vector2 end)> connections = new();

        AddConnections(
            tree.Root,
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
