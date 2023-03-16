using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sandbox.Event;

namespace Sandbox
{
	internal class MyPlayer : Player
	{
		public ClothingContainer Clothing = new();

		public MyPlayer() : base()
		{
			Inventory = new Inventory( this );
		}

		public MyPlayer( IClient client ) : this()
		{
			SetModel( "models/citizen/citizen.vmdl" );
			Clothing.LoadFromClient(client);
		}

		public override void Respawn()
		{
			Log.Info( "Spawner spiller" );
			Clothing.DressEntity( this );

			Controller = new WalkController();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			Inventory.Add( new Flashlight(), true );

			base.Respawn();
		}

		public override PawnController GetActiveController()
		{
			if ( DevController != null ) return DevController;

			return base.GetActiveController();
		}

		public override void Simulate( IClient client )
		{
			base.Simulate( client );

			var controller = GetActiveController();
			if ( controller != null )
			{
				EnableSolidCollisions = !controller.HasTag( "noclip" );

				SimulateAnimation( controller );
			}
		}

		Entity lastWeapon;

		void SimulateAnimation( PawnController controller )
		{
			if ( controller == null )
				return;

			// where should we be rotated to
			var turnSpeed = 0.02f;

			Rotation rotation;

			// If we're a bot, spin us around 180 degrees.
			if ( Client.IsBot )
				rotation = ViewAngles.WithYaw( ViewAngles.yaw + 180f ).ToRotation();
			else
				rotation = ViewAngles.ToRotation();

			var idealRotation = Rotation.LookAt( rotation.Forward.WithZ( 0 ), Vector3.Up );
			Rotation = Rotation.Slerp( Rotation, idealRotation, controller.WishVelocity.Length * Time.Delta * turnSpeed );
			Rotation = Rotation.Clamp( idealRotation, 45.0f, out var shuffle ); // lock facing to within 45 degrees of look direction

			CitizenAnimationHelper animHelper = new CitizenAnimationHelper( this );

			animHelper.WithWishVelocity( controller.WishVelocity );
			animHelper.WithVelocity( controller.Velocity );
			animHelper.WithLookAt( EyePosition + EyeRotation.Forward * 100.0f, 1.0f, 1.0f, 0.5f );
			animHelper.AimAngle = rotation;
			animHelper.FootShuffle = shuffle;
			animHelper.DuckLevel = MathX.Lerp( animHelper.DuckLevel, controller.HasTag( "ducked" ) ? 1 : 0, Time.Delta * 10.0f );
			animHelper.VoiceLevel = (Game.IsClient && Client.IsValid()) ? Client.Voice.LastHeard < 0.5f ? Client.Voice.CurrentLevel : 0.0f : 0.0f;
			animHelper.IsGrounded = GroundEntity != null;
			animHelper.IsSitting = controller.HasTag( "sitting" );
			animHelper.IsNoclipping = controller.HasTag( "noclip" );
			animHelper.IsClimbing = controller.HasTag( "climbing" );
			animHelper.IsSwimming = this.GetWaterLevel() >= 0.5f;
			animHelper.IsWeaponLowered = false;

			if ( controller.HasEvent( "jump" ) ) animHelper.TriggerJump();
			if ( ActiveChild != lastWeapon ) animHelper.TriggerDeploy();

			if ( ActiveChild is BaseCarriable carry )
			{
				carry.SimulateAnimator( animHelper );
			}
			else
			{
				animHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
				animHelper.AimBodyWeight = 0.5f;
			}

			lastWeapon = ActiveChild;
		}
	}
}
