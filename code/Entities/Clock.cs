using Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Entities
{

	[HammerEntity]
	[Title( "Ding Dong" ), Category( "Placeable" ), Icon( "place" )]
	[EditorModel( "models/bomberbrick/bomberbrick.vmdl_c" )]
	public class Clock : Entity, IUse
	{
		public bool IsUsable( Entity user )
		{
			if ( Vector3.DistanceBetween( user.Position, Position ) < 5 ) return true;

			return false;
		}

		public bool OnUse( Entity user )
		{
			Log.Info( "Virker" );
			return false;
		}
	}
}
