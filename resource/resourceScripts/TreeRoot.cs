using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class TreeRoot : Resource {

    public Dictionary<string, BigNumber> Variables {get; set;} = new();

    public TreeNode Root {get; set;}
}