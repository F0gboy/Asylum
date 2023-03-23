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
	[Title( "Basement key" ), Category( "Placeable" ), Icon( "radio_button_checked" )]
	public partial class Key : KeyframeEntity, IUse
	{
		[Property( Title = "Activation distance" ), Category( "Settings" )]
		public int activationDist { get; private set; } = 100;

		[Property( Title = "Name of door to open" ), Category( "Settings" )]
		public string doorOpenName { get; private set; } = "";

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/citizen_props/beachball.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		}

		public bool IsUsable( Entity user )
		{
			return user is Player && activationDist > Vector3.DistanceBetween( user.Position, Position );
		}

		public bool OnUse( Entity user )
		{
			Event.Run( "KeyCollected", this );

			Delete();

			return true;
		}
	}
}
