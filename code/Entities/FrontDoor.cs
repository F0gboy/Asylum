using Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Entities
{
	[HammerEntity, SupportsSolid]
	[RenderFields, VisGroup( VisGroup.Dynamic )]
	[Model]
	[Title( "FrontDoor" ), Category( "Lobby" ), Icon( "door_front" )]
	public partial class FrontDoor : KeyframeEntity, IUse
	{
		[Property( Title = "Activation distance" ), Category( "Settings" )]
		public int activationDist { get; set; } = 100;

		List<MyPlayer> players = new ();

		public bool IsUsable( Entity user )
		{
			var pawn = user.Client.Pawn as MyPlayer;
			return user is Player && activationDist > Vector3.DistanceBetween( pawn.EyePosition, Position ) && MyGame.gameIsDone;
		}

		public bool OnUse( Entity user )
		{
			if ( !user.IsValid() ) return false;

			if ( !players.Contains( user as MyPlayer ) )
			{
				players.Add( user as MyPlayer );
				MyGame.CreateNotification( To.Single( user as MyPlayer ), "You are ready to leave!" );
			}

			if ( players.Count == Game.Clients.Count ) BlackoutScreen( To.Everyone, "Victory" );

			return false;
		}

		[ClientRpc]
		public static void BlackoutScreen(string text)
		{
			_ = new Blackout( text );
		}

		public override void Spawn()
		{
			base.Spawn();
			SetupPhysicsFromModel( PhysicsMotionType.Static );
		}
	}
}
