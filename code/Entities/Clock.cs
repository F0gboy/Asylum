using Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Entities
{

	using Sandbox;
	using Editor;

	/// <summary>
	/// This entity defines the spawn point of the player in first person shooter gamemodes.
	/// </summary>
	[Library( "info_player_start" ), HammerEntity]
	[Title( "Player Spawnpoint" ), Category( "Player" ), Icon( "place" )]
	[Model]
	public class SpawnPoint : Entity, IUse
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
