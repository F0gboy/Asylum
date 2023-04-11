using Sandbox.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sandbox.Event;


namespace Sandbox
{
	partial class MyPlayer : Player
	{
		private TimeSince timeSinceDropped;
		private TimeSince timeSinceJumpReleased;

		private Entity markedObject = null;

		string[] tags = { "Interact", "Hit", "paper1" };

		string interactTag = "Interact";

		/// <summary>
		/// The clothing container is what dresses the citizen
		/// </summary>
		public ClothingContainer Clothing = new();

		/// <summary>
		/// Default init
		/// </summary>
		public MyPlayer()
		{
			Inventory = new Inventory( this );
		}

		/// <summary>
		/// Initialize using this client
		/// </summary>
		public MyPlayer( IClient cl ) : this()
		{
			// Load clothing from client data
			Clothing.LoadFromClient( cl );
		}

		public override void Respawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			var pointLight = new PointLightEntity()
			{
				Position = Model.Bounds.Center,
				Range = 100,
				Color = Color.White,
				Brightness = 0.005f,
				QuadraticAttenuation = 500,
				Parent = this
			};

			

			Controller = new WalkController();

			if ( DevController is NoclipController )
			{
				DevController = null;
			}

			this.ClearWaterLevel();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			EnableTouch = true;
			EnableLagCompensation = true;
			Predictable = true;
			EnableHitboxes = true;

			Clothing.DressEntity( this );

			Inventory.Add( new Flashlight(), true );
			Inventory.Add( new Fists() );

			base.Respawn();
		}

		public override PawnController GetActiveController()
		{
			if ( DevController != null ) return DevController;

			return base.GetActiveController();
		}

		public override void Simulate( IClient cl )
		{
			base.Simulate( cl );
			
			var trace = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 85 )
				.WithAnyTags( tags )
				.Ignore( this )
				.Run();

			//Log.Warning( Model.Bounds.Size.x );

			//	DebugOverlay.TraceResult( trace );
			

			if ( trace.Hit && trace.Entity is Entity ent && !markedObject.IsValid() && ent.Tags.Has( interactTag ) )
			{
				markedObject = ent;

				var glow = ent.Components.GetOrCreate<Glow>();

				if ( ent is DoorEntity door ) glow.Color = door.Locked ? Color.Red : Color.Gray;
				else glow.Color = Color.Gray;

				glow.Width = 0.25f;
				glow.Enabled = true;
			}

			else if ( markedObject.IsValid() && trace.Entity != markedObject || !trace.Hit && markedObject.IsValid())
			{
				markedObject.Components.GetOrCreate<Glow>().Enabled = false;
				markedObject = null;
			}

			var controller = GetActiveController();
			if ( controller != null )
			{
				EnableSolidCollisions = !controller.HasTag( "noclip" );

				SimulateAnimation( controller );
			}

			TickPlayerUse();
			SimulateActiveChild( cl, ActiveChild );

			if ( Input.Released( InputButton.Jump ) )
			{
				if ( timeSinceJumpReleased < 0.3f )
				{
					if ( DevController is NoclipController )
					{
						DevController = null;
					}
					else
					{
						DevController = new NoclipController();
					}
				}

				timeSinceJumpReleased = 0;
			}

			if ( InputDirection.y != 0 || InputDirection.x != 0f )
			{
				timeSinceJumpReleased = 1;
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

		public override void StartTouch( Entity other )
		{
			if ( timeSinceDropped < 1 ) return;

			base.StartTouch( other );
		}

		public override float FootstepVolume()
		{
			return Velocity.WithZ( 0 ).Length.LerpInverse( 0.0f, 200.0f ) * 5.0f;
		}

		public override void FrameSimulate( IClient cl )
		{
			Camera.Rotation = ViewAngles.ToRotation();
			Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );

			Camera.Position = EyePosition;
			Camera.FirstPersonViewer = this;
			Camera.Main.SetViewModelCamera( 90f );
		}
	}
}
