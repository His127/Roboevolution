using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class TreeParser {
    
    public TreeRoot ParseTree(UpgradeTree tree){
        TreeRoot treeRoot = new();
        treeRoot.Variables = loadVariables(tree);
        treeRoot.Root = createRoot(tree);
        // PrintTree(treeRoot.Root);
        return treeRoot;
    }

    private Dictionary<string, BigNumber> loadVariables(UpgradeTree tree){
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

    private TreeNode createRoot(UpgradeTree tree) {
        TreeNode root = new() {
            Title = tree.Title,
            Description = "The Start of your Path",
            Effects = [],
            MaxLevel = 1,
            BasePrice = 0,
            PriceIncrease = 0,
            MinLevelForUnlock = 0,
            Children = createChildren(tree).ToList(),
        };

        return root;
    }

    private TreeNode[] createChildren(UpgradeTree tree) {
        string[] lines = tree.TreeStructure.Split("\n", false);

        List<TreeNode> children = new();
        List<TreeNode> parents = new();

        bool rootSkipped = false;

        foreach(string rawLine in lines) {
            if(string.IsNullOrWhiteSpace(rawLine))
                continue;

            int depth = rawLine.TakeWhile(c => c == '\t').Count();
            string line = rawLine.Trim();

            if(line.StartsWith("//"))
                continue;

            if(!rootSkipped) {
                if(line != "root")
                    throw new ArgumentException("The first entry of the UpgradeTree must be \"root\".");

                rootSkipped = true;
                continue;
            }

            TreeNode node = ParseNode(line);

            // Remove parents that are no longer relevant.
            while(parents.Count >= depth) {
                parents.RemoveAt(parents.Count - 1);
            }

            if(depth == 1) {
                children.Add(node);
            } else {
                TreeNode parent = parents[depth - 2];
                parent.Children.Add(node);
            }

            parents.Add(node);
        }

        if(!rootSkipped)
            throw new ArgumentException("The UpgradeTree does not contain a root.");

        return children.ToArray();
    }

    private TreeNode ParseNode(string line){
        TreeNode node;
        string[] arguments = line.Split(":", false);
        if(arguments.Length < 6)
            throw new ArgumentException($"Invalid UpgradeTree line: \"{line}\"");
        
        UpgradeEffect[] effects = createUpgradeEffects(arguments[2]);

        node = new(){
            Title = arguments[0],
            Description = arguments[1],
            Effects = effects,
            MaxLevel = int.Parse(arguments[3]),
            BasePrice = BigNumber.Parse(arguments[4]),
            PriceIncrease = BigNumber.Parse(arguments[5]),
            MinLevelForUnlock = arguments.Length == 7 ? int.Parse(arguments[6]) : 1
        };

        return node;
    }

    private UpgradeEffect[] createUpgradeEffects(string effects){
        List<UpgradeEffect> upgrades = new();
        string[] arguments = effects.Split(";", false);
        foreach(string s in arguments){
            string[] parts = s.Split(",", false);
            upgrades.Add(new(){
                Variable = parts[0],
                Increase = BigNumber.Parse(parts[1])
            });
        }
        return upgrades.ToArray();
    }

    private void PrintTree(TreeNode node, int depth = 0) {
        GD.Print($"{new string('\t', depth)}{node.Title}");

        foreach(TreeNode child in node.Children) {
            PrintTree(child, depth + 1);
        }
    }

}