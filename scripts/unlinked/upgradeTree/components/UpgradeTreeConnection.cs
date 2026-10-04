using Godot;

public partial class UpgradeTreeConnection : Line2D {
    private TreeNode parent;
    private TreeNode child;

    public void Setup(TreeNode parent, TreeNode child, Vector2 start, Vector2 end) {
        this.parent = parent;
        this.child = child;

        Points = [start, end];

        parent.StateChanged += OnStateChanged;
        child.StateChanged += OnStateChanged;

        UpdateVisibility();
    }

    private void OnStateChanged(NodeState oldState, NodeState newState) {
        UpdateVisibility();
    }

    private void UpdateVisibility() {
        bool parentUnlocked = parent.State != NodeState.INVISIBLE;

        bool childUnlocked = child.State != NodeState.INVISIBLE;

        Visible = parentUnlocked && childUnlocked;
    }

    public override void _ExitTree() {
        parent.StateChanged -= OnStateChanged;
        child.StateChanged -= OnStateChanged;
    }
}