using System.Collections.Generic;
using Godot;

public class TreeLayoutConnectionGenerator {

    public List<(TreeNode parent, TreeNode child, Vector2 start, Vector2 end)> Generate(
        TreeRoot tree,
        Dictionary<TreeNode, Vector2> positions
    ) {
        List<(TreeNode parent, TreeNode child, Vector2 start, Vector2 end)> connections = new();

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
        List<(TreeNode parent, TreeNode child, Vector2 start, Vector2 end)> connections
    ) {
        foreach(TreeNode child in parent.Children) {
            connections.Add((
                parent,
                child,
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
