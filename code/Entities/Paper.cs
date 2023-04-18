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
	internal partial class Paper : KeyframeEntity, IUse
	{
		[Property( Title = "Activation distance" ), Category( "Settings" )]
		public int activationDist { get; set; } = 100;

		public bool IsUsable( Entity user )
		{
			var pawn = user.Client.Pawn as MyPlayer;
			return user is Player && activationDist > Vector3.DistanceBetween( pawn.EyePosition, Position );
		}

		public bool OnUse( Entity user )
		{
			OpenPaperClient( To.Single(user.Client.Pawn as MyPlayer), this );

			MyGame.AddPaperCollected( this );



			return false;
		}

		[ClientRpc]
		public void OpenPaperClient( Paper paper )
		{
			if ( MyGame.papers.AnyOpen() ) return;
			var num = Convert.ToInt32( paper.Name );
			MyGame.papers.Open( num );
		}

		public override void Spawn()
		{
			base.Spawn();

			SetupPhysicsFromModel( PhysicsMotionType.Static );
		}
	}
}
