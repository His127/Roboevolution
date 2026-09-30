using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class TreeLayout : Resource {

    public Dictionary<TreeNode, Vector2> positions {get; set;}

    public List<(Vector2 start, Vector2 end)> connections {get; set;}


}