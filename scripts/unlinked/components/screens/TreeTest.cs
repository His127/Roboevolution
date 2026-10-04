using Godot;

public partial class TreeTest : BaseScreen {

    [Export]
    CanvasLayer canvas;

    public override void _Ready() {
        base._Ready();
        GameManager.Instance.treeManager.AddCanvas(canvas);
        GameManager.Instance.treeManager.LoadTree(0);

    }

}