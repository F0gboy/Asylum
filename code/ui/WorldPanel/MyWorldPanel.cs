using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

partial class MyWorldPanel : WorldPanel
{
	public MyWorldPanel(string text, Vector3 position)
	{
		var w = 1040;
		var h = 760;
		PanelBounds = new Rect( -(w / 2), -(h / 2), w, h );

		Position = position;

		StyleSheet.Load( "/UI/Styles/WorldPanel.scss" );

		Add.Label( text, "text" );

		//Add.Label( "Hole 1", "hole" );
		//Add.Label( "Coooool", "name" );
		//Add.Label( "Par 3", "par" );
	}

	public override void Tick()
	{
		base.Tick();

		var w = 1040;
		var h = 760;
		PanelBounds = new Rect( -(w / 2), -(h / 2), w, h );
	}
}
