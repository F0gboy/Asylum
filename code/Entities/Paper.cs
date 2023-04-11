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
	[Title( "Clock" ), Category( "Lobby" ), Icon( "radio_button_checked" )]
	internal class Paper : KeyframeEntity, IUse
	{
		public bool IsUsable( Entity user )
		{
			throw new NotImplementedException();
		}

		public bool OnUse( Entity user )
		{
			throw new NotImplementedException();
		}
	}
}
