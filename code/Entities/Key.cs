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
	[Model( Archetypes = ModelArchetype.physics_prop_model )]
	[Title( "Basement key" ), Category( "Lock & Key" ), Icon( "door_front" )]
	public partial class Key : ModelEntity, IUse
	{
		[Property( Title = "Activation distance" ), Category( "Settings" )]
		public int activationDist { get; private set; } = 100;

		[Property( Title = "Door to open" ), Category( "Settings" )]
		public EntityTarget door { get; private set; } = null;

		public override void Spawn()
		{
			base.Spawn();

			SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
		}

		public bool IsUsable( Entity user )
		{
			var pawn = user as MyPlayer;
			return user is Player && activationDist > Vector3.DistanceBetween( pawn.EyePosition, Position );
		}

		public bool OnUse( Entity user )
		{
			Event.Run( "KeyCollected", this );

			Delete();

			return true;
		}
	}
}
