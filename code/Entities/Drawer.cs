using System.Reflection.Metadata;
using Editor;

namespace Sandbox.Entities
{
	[HammerEntity, SupportsSolid]
	[RenderFields, VisGroup( VisGroup.Dynamic )]
	[Model( Archetypes = ModelArchetype.animated_model )]
	[Title( "Drawer" ), Category( "Player interaction" ), Icon( "door_front" )]
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
			var pawn = user as MyPlayer;
			return user is Player && activationDist > Vector3.DistanceBetween( pawn.EyePosition, Position );
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
			foreach ( var child in Children )
			{
				Log.Info( child.Name );
				if (child is not KeyframeEntity keyFrameChild) continue;

				var posOffset = new Transform( keyFrameChild.Position - Position );
				var toPos = new Transform( targetTrans.Position + posOffset.Position );
				_ = keyFrameChild.KeyframeTo( toPos, time );
			}

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
