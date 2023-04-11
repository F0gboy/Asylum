using Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Entities
{
	[HammerEntity, SupportsSolid]
	[RenderFields, VisGroup( VisGroup.Dynamic )]
	[Model]
	[Title( "Paper" ), Category( "Lobby" ), Icon( "radio_button_checked" )]
	internal class Paper : KeyframeEntity, IUse
	{
		[Property( Title = "Activation distance" ), Category( "Settings" )]
		public int activationDist { get; set; } = 100;

		public bool IsUsable( Entity user )
		{
			return user is Player && activationDist > Vector3.DistanceBetween( user.Position, Position );
		}

		public bool OnUse( Entity user )
		{
			var num = Convert.ToInt32( Name );

			//MyGame.papers.

			return false;
		}
	}
}
