using System.Collections.Generic;
using Godot;

public static class TreeParser {

    public static TreeRoot ParseTree(UpgradeTree tree){
        TreeRoot treeRoot = new();
        treeRoot.Variables = loadVariables(tree);
        treeRoot.Root = createRoot(tree);
        return treeRoot;
    }

    private static Dictionary<string, BigNumber> loadVariables(UpgradeTree tree){
        Dictionary<string, BigNumber> variables = new();
        string[] variableString = tree.Variables.Split(",", false);
        foreach(string variable in variableString){
            string name = variable.Trim();
            if(name.StartsWith("//")){
                string[] commentEnd = name.Split("\n");
                name = commentEnd[1];
            } 
            variables[name] = new();
            // GD.Print($"Stored Variable: {name}");
        }
        return variables;
    }

    private static TreeNode createRoot(UpgradeTree tree) {
        TreeNode root = new() {
            Title = tree.Title,
            Description = "The Start of your Path",
            Effects = [],
            MaxLevel = 1,
            BasePrice = 0,
            PriceIncrease = 0,
            MinLevelForUnlock = 0,
            // Children = createChildren(tree)
        };

        return root;
    }

    // private static TreeNode[] createChildren(UpgradeTree tree) {
    //     string[] lines = tree.TreeStructure.Split("\n", false);

    //     List<TreeNode> roots = new();
    //     List<TreeNode> parents = new();

    //     foreach(string rawLine in lines) {
    //         if(string.IsNullOrWhiteSpace(rawLine))
    //             continue;

    //         int depth = rawLine.TakeWhile(c => c == '\t').Count();
    //         string line = rawLine.Trim();

    //         TreeNode node = ParseNode(line);

    //         if(depth == 0) {
    //             roots.Add(node);
    //         } else {
    //             TreeNode parent = parents[depth - 1];
    //             parent.Children.Add(node);
    //         }

    //         if(parents.Count > depth)
    //             parents.RemoveRange(depth, parents.Count - depth);

    //         parents.Add(node);
    //     }

    //     return roots.ToArray();
    // }

}