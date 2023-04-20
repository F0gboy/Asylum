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
	[Model( Archetypes = ModelArchetype.static_prop_model )]
	[Title( "Lock" ), Category( "Lock & Key" ), Icon( "door_front" )]
	public partial class Lock : KeyframeEntity, IUse
	{
		[Property( Title = "Activation distance" ), Category( "Settings" )]
		public int activationDist { get; private set; } = 100;

		[Property( Title = "Door to open" ), Category( "Settings" )]
		public EntityTarget door { get; private set; } = null;

		DoorEntity lockedDoor;

		public override void Spawn()
		{
			base.Spawn();

			SetupPhysicsFromModel( PhysicsMotionType.Static );
		}

		[Event.Entity.PostSpawn]
		public void PostSpawn()
		{
			var tempDoor = door.GetTarget();

			if ( tempDoor is DoorEntity ) lockedDoor = (DoorEntity) tempDoor;
		}

		public bool IsUsable( Entity user )
		{
			var pawn = user as MyPlayer;
			return user is Player && activationDist > Vector3.DistanceBetween( pawn.EyePosition, Position );
		}

		public bool OnUse( Entity user )
		{
			if ( !lockedDoor.IsValid() || !lockedDoor.Locked ) return false;

			Log.Info( lockedDoor.Name );
			Log.Info( door.Name );

			if ( !MyGame.keysCollected.Any( key => key.door.Name == lockedDoor.Name ) ) return false;

			lockedDoor.Locked = false;
			lockedDoor.Open();

			Delete();

			MyGame.CreateNotification( To.Single( user as MyPlayer ), "Door unlocked" );

			return true;
		}
	}
}
