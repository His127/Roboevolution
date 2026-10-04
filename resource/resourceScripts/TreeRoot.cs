using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class TreeRoot : Resource {

    // public Dictionary<string, BigNumber> Variables {get; set;} = new();
    public TreeVariable[] Variables {get; set;}

    public TreeNode Root {get; set;}
}