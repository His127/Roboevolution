using Godot;

public partial class JunkPile : TextureButton {
    [Export]
    private JunkDrop[] junkDrops;

    [Export]
    private PackedScene junkPiece;

	[Export]
	RichTextLabel label;

    [Export]
    RichTextLabel lable_county;

    [Export]
    int anfangswert = 10;
    
    int counter;

	[Export]
	TextEdit maxMultiplier;
	[Export]
	VSlider progress;

    private RandomNumberGenerator random = new();

    public override void _Ready(){
       
        counter = anfangswert;
        lable_county.Text = $"[pulse]{counter}";
        label.Text = $"{GameManager.Instance.currencyManager.Schrott.ToDisplayFormat()} Trash Collected";

    }


    public void Clicked() {

        counter--;
        if(counter <= 0){

        JunkDrop selected = GetRandomDrop();

        if(selected == null)
            return;

        JunkPice piece = junkPiece.Instantiate<JunkPice>();
		piece.Position = Size / 2;
        piece.Direction = new Vector2(
            random.RandfRange(-1f, 1f),
            random.RandfRange(-1f, 1f)
        ).Normalized();
		piece.Lifetime += random.RandfRange(-0.5f, 0.5f);
        piece.Texture = selected.Texture;
		piece.value = BigNumber.Parse(selected.Value);
		piece.trashCollected += JunkCollected;

        AddChild(piece);

        counter = anfangswert;

        }

        lable_county.Text = $"[pulse]{counter}";

    }

    private JunkDrop GetRandomDrop() {
        float totalChance = 0;

        foreach(JunkDrop drop in junkDrops)
            totalChance += drop.DropChance;

        if(totalChance <= 0)
            return null;

        float value = random.RandfRange(0, totalChance);

        foreach(JunkDrop drop in junkDrops)
        {
            value -= drop.DropChance;

            if(value <= 0)
                return drop;
        }

        return junkDrops[^1];
    }

	private void JunkCollected(string value){ 
		BigNumber trashValue = BigNumber.Parse(value);
		BigNumber multiplier = BigNumber.Parse(maxMultiplier.Text);
		multiplier *= progress.Value;
		trashValue *= multiplier;

		GameManager.Instance.currencyManager.Schrott += trashValue;
		label.Text = $"{GameManager.Instance.currencyManager.Schrott.ToDisplayFormat()} Trash Collected";
	}
}