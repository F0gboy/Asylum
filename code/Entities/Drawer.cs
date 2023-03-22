using Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Entities
{
	[HammerEntity, SupportsSolid]
	[RenderFields, VisGroup( VisGroup.Dynamic )]
	[Model( Archetypes = ModelArchetype.animated_model )]
	[Title( "Drawer" ), Category( "Placeable" ), Icon( "door_front" )]
	public partial class Drawer : KeyframeEntity, IUse
	{
		[Property( Title = "Activation distance" ), Category("Settings")]
		public int activationDist { get; set; } = 100;

		[Property( Title = "Time for move (Seconds)" ), Category( "Settings" )]
		public float time { get; set; } = 0.5f;

		[Property( Title = "Open sound" ), Category( "Sounds" )]
		public string openSound { get; set; } = "";

		[Property( Title = "Close sound" ), Category( "Sounds" )]
		public string closeSound { get; set; } = "";

		private Transform startTrans;
		private Transform endTrans;

		private Transform targetTrans;

		private bool open = false;
		private bool isMoving = false;

		public override void Spawn()
		{
			base.Spawn();

			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

			startTrans = Transform;

			var tempTrans = new Transform();
			tempTrans.Position = startTrans.Position + Rotation.Forward * (Model.RenderBounds.Size.x) * 0.75f;
			tempTrans.Rotation = startTrans.Rotation;
			tempTrans.Scale = startTrans.Scale;

			endTrans = tempTrans;
		}

		public bool IsUsable( Entity user )
		{
			return user is Player && activationDist > Vector3.DistanceBetween( user.Position, Position ) && !isMoving;
		}

		public bool OnUse( Entity user )
		{
			if ( isMoving ) return true;

			isMoving = true;

			open = !open;

			Sound playedSound;

			if ( open ) playedSound = PlaySound( openSound );
			else playedSound = PlaySound( closeSound );

			targetTrans = open ? endTrans : startTrans;

			var move = KeyframeTo( targetTrans, time );

			move.ContinueWith( task =>
			{
				if ( task.Result )
				{
					isMoving = false;
					playedSound.Stop();
				}
			} );

			return true;
		}
	}
}
