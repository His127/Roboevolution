using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class UpgradeEffect :Resource {

    public BigNumber Variable {get; set;} = new();

    public BigNumber Increase {get; set;} = new();
}