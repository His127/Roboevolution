using System.Collections.Generic;
using Godot;

public class LayoutCollisionResolver {

    private readonly float nodeDiameter;

    private const int ResolutionIterations = 5;

    public LayoutCollisionResolver(TreeLayoutSettings settings) {
        nodeDiameter = settings.NodeDiameter;
    }

    public void Resolve(
        TreeRoot tree,
        Dictionary<TreeNode, Vector2> positions
    ) {
        for(int i = 0; i < ResolutionIterations; i++) {
            bool changed = ResolveNodeNodeCollisions(
                tree,
                positions
            );

            positions[tree.Root] = Vector2.Zero;

            if(!changed)
                break;
        }
    }

    private bool ResolveNodeNodeCollisions(
        TreeRoot tree,
        Dictionary<TreeNode, Vector2> positions
    ) {
        bool changed = false;

        TreeNode[] nodes = new TreeNode[positions.Count];
        positions.Keys.CopyTo(nodes, 0);

        for(int i = 0; i < nodes.Length; i++) {
            if(nodes[i] == tree.Root)
                continue;

            for(int j = i + 1; j < nodes.Length; j++) {
                if(nodes[j] == tree.Root)
                    continue;

                if(ResolveNodePair(
                    nodes[i],
                    nodes[j],
                    positions
                )) {
                    changed = true;
                }
            }
        }

        return changed;
    }

    private bool ResolveNodePair(
        TreeNode first,
        TreeNode second,
        Dictionary<TreeNode, Vector2> positions
    ) {
        Vector2 difference =
            positions[first] - positions[second];

        float distance = difference.Length();

        if(distance >= nodeDiameter)
            return false;

        if(distance < 0.001f) {
            difference = Vector2.Right;
            distance = 1f;
        }

        Vector2 direction = difference / distance;

        float correction =
            (nodeDiameter - distance) / 2f;

        positions[first] += direction * correction;
        positions[second] -= direction * correction;

        return true;
    }
}