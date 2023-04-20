using Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Entities
{

	[HammerEntity, SupportsSolid]
	[RenderFields, VisGroup( VisGroup.Dynamic )]
	[Model( Archetypes = ModelArchetype.static_prop_model )]
	[Title( "Combination lock" ), Category( "Lock & Key" ), Icon( "door_front" )]
	public partial class CombinationLock : KeyframeEntity, IUse
	{
		[Property( Title = "Activation distance" ), Category( "Settings" )]
		public int activationDist { get; private set; } = 100;

		[Property( Title = "Door to open" ), Category( "Settings" )]
		public EntityTarget door { get; private set; } = null;

		private static DoorEntity lockedDoor;

		private static DoorEntity clientDoor;

		public override void Spawn()
		{
			base.Spawn();

			SetupPhysicsFromModel( PhysicsMotionType.Static );
		}

		[Event.Entity.PostSpawn]
		public void PostSpawn()
		{
			var tempDoor = door.GetTarget();

			if ( tempDoor is DoorEntity ) lockedDoor = (DoorEntity)tempDoor;
		}

		public bool IsUsable( Entity user )
		{
			var pawn = user as MyPlayer;
			return user is Player && activationDist > Vector3.DistanceBetween( pawn.EyePosition, Position );
		}

		public bool OnUse( Entity user )
		{
			OpenKeypadClient( To.Single( user as MyPlayer ), lockedDoor );

			return true;
		}

		[ClientRpc]
		public static void OpenKeypadClient( DoorEntity door )
		{
			Keypad keypad = MyGame.keypad;

			clientDoor = door;

			if ( keypad.visible ) return;
			keypad.ToggleKeypad();
		}

		[ConCmd.Server]
		public static void ClientBoolToServer( bool correct )
		{
			if (!correct) return;
			lockedDoor.Locked = false;
			lockedDoor.Open();

			FindByName( "CombinationLock" )?.Delete();
		}

		public void DeleteLock()
		{
			this.Delete();
		}
		public static void TryOpenDoor()
		{
			if ( !MyGame.keypad.IsCodeCorrect() ) return;

			_ = new Notifications( "Door unlocked" );

			ClientBoolToServer( MyGame.keypad.IsCodeCorrect() );
		}
	}
}
