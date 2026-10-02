using Godot;

public class TreeCreator{
    private TreeParser treeParser = new();
    private TreeLayoutProcessor processor = new(new());

    private UpgradeTree Tree;

    public TreeRoot TreeRoot {get; private set;}
    public TreeLayout layout {get; private set;}
    public CanvasLayer canvas {get; private set;}

    public static TreeCreator Create(UpgradeTree tree, CanvasLayer DisplayLayer){
        TreeCreator creator = new(){
            Tree = tree,
            canvas = DisplayLayer
        };
        creator.CreateTreeRoot();
        creator.CreateTreeLayout();
        return creator;
    }

    public void RecalculateTree(){
        CreateTreeLayout();
        CreateTreeLayout();
    }

    private void CreateTreeRoot(){
        TreeRoot = treeParser.ParseTree(Tree);
    }

    private void CreateTreeLayout(){
        layout = processor.ProcessTreeToLayout(TreeRoot);
    }


}