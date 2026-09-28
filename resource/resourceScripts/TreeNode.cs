using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class TreeNode : Resource {
    public string Title {get; set;} = "";

    public string Description {get; set;} = "";

    public UpgradeEffect[] Effects {get; set;}

    public int MaxLevel {get; set;} = 0;

    public BigNumber BasePrice {get; set;} = new();

    public BigNumber PriceIncrease {get; set;} = new();

    public int MinLevelForUnlock {get; set;} = 1;

    public List<TreeNode> Children {get; set;} = new();
}