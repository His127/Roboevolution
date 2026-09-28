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

    private static TreeNode createRoot(UpgradeTree tree){


        return new();
    }

}