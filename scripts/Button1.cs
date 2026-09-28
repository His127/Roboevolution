using Godot;
using System;

public partial class Button1 : Button
{

	[Export]
	private TextEdit num1;
	[Export]
	private TextEdit num2;
	[Export]
	private OptionButton symbole;
	[Export]
	private RichTextLabel label;
	[Export(PropertyHint.MultilineText)]
	private string ResultFormat;


    public override void _Ready()
    {
        label.Text = "";
    }

	public void Calculate(){
		GD.Print("Calculating...");
		BigNumber a, b;
		string numA = num1.Text.ToLower();
		string numB = num2.Text.ToLower();
		if(numA.Contains("e")){
			string[] list = numA.Split("e");
			a = new(double.Parse(list[0]), int.Parse(list[1]));
		}else{
			a = new(double.Parse(numA));
		}
		if(numB.Contains("e")){
			string[] list1 = numB.Split("e");
			b = new(double.Parse(list1[0]), int.Parse(list1[1]));
		}else{
			b = new(double.Parse(numB));
		}
		BigNumber result = new();
		switch(symbole.Selected){
			case 0: // +
				result = a + b;
				break;
			case 1: // -
				result = a - b;
				break;
			case 2: // *
				result = a * b;
				break;
			case 3: // /
				result = a / b;
				break;
			default:
				throw new ArgumentException("No symbole used that is known");
		}

		string formatedString = ResultFormat.Replace("{text}", result.ToDisplayFormat());
		label.Text = formatedString;


	}


}
