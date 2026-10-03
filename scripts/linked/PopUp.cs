using Godot;
using System;

public partial class PopUp : Node2D{

	[Export] OptionButton Button;


    public override void _Ready()
	{
		GameManager.Instance.currencyManager.currencyChange += update;  


	}

    public override void _ExitTree()
    {

		GameManager.Instance.currencyManager.currencyChange -= update;
		
    }

	private void update()
	{
		Button.SetItemText(0, $"Geld: {GameManager.Instance.currencyManager.Geld.ToDisplayFormat()}");
		Button.SetItemText(1, $"Schrott: {GameManager.Instance.currencyManager.Schrott.ToDisplayFormat()}");
		Button.SetItemText(2, $"R: {GameManager.Instance.currencyManager.R.ToDisplayFormat()}");
		Button.SetItemText(3, $"E: {GameManager.Instance.currencyManager.E.ToDisplayFormat()}");
	}


	


}
