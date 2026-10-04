using System.Collections.Generic;
using Godot;

public class TreeLayoutProcessor {

    private readonly TreeLayoutSettings settings;

    public TreeLayoutProcessor(TreeLayoutSettings settings) {
        this.settings = settings;
    }

    public TreeLayout ProcessTreeToLayout(TreeRoot tree) {
        InitialPositionGenerator positionGenerator =
            new(settings);

        Dictionary<TreeNode, Vector2> positions =
            positionGenerator.Generate(tree);

        TreeLayoutSimulator simulator =
            new(settings);

        simulator.Simulate(
            tree,
            positions
        );

        // LayoutCollisionResolver collisionResolver =
        //     new(settings);

        // collisionResolver.Resolve(
        //     tree,
        //     positions
        // );

        TreeLayoutConnectionGenerator connectionGenerator =
            new();

        List<(TreeNode parent, TreeNode child, Vector2 start, Vector2 end)> connections =
            connectionGenerator.Generate(
                tree,
                positions
            );

        return new TreeLayout {
            positions = positions,
            connections = connections
        };
    }
}