using Sandbox;

[Spawnable]
[Library( "weapon_flashlight", Title = "Flashlight" )]
partial class Flashlight : BaseWeapon
{
	public override string ViewModelPath => "weapons/rust_flashlight/v_rust_flashlight.vmdl";
	public override float SecondaryRate => 2.0f;

	protected virtual Vector3 LightOffset => 0;

	private SpotLightEntity worldLight;
	private SpotLightEntity viewLight;
	private PointLightEntity illuminatingLight;

	[Net, Local, Predicted]
	private bool LightEnabled { get; set; } = true;

	TimeSince timeSinceLightToggled;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );

		worldLight = CreateLight();
		worldLight.SetParent( this, "slide" );
		worldLight.EnableHideInFirstPerson = true;
		worldLight.Enabled = false;
	}

	public override void CreateViewModel()
	{
		base.CreateViewModel();

		viewLight = CreateLight();
		viewLight.SetParent( ViewModelEntity, "light", new Transform( ViewModelEntity.Rotation.Forward * (-10) ) );
		viewLight.EnableViewmodelRendering = true;
		viewLight.Enabled = LightEnabled;
	}

	private SpotLightEntity CreateLight()
	{
		var light = new SpotLightEntity
		{
			Enabled = true,
			DynamicShadows = true,
			Range = 512,
			Falloff = .5f,
			LinearAttenuation = 0.0f,
			QuadraticAttenuation = 1.0f,
			Brightness = .5f,
			Color = Color.White,
			InnerConeAngle = 20,
			OuterConeAngle = 40,
			FogStrength = 1.0f,
			Owner = Owner,
			LightCookie = Texture.Load( "materials/effects/lightcookie.vtex" )
		};

		return light;
	}

	public override void Simulate( IClient cl )
	{
		if ( cl == null )
			return;

		base.Simulate( cl );

		bool toggle = Input.Pressed( InputButton.Flashlight ) || Input.Pressed( InputButton.PrimaryAttack );

		if ( timeSinceLightToggled > 0.1f && toggle )
		{
			LightEnabled = !LightEnabled;

			PlaySound( LightEnabled ? "flashlight-on" : "flashlight-off" );

			if ( worldLight.IsValid() )
			{
				worldLight.Enabled = LightEnabled;
			}

			if ( viewLight.IsValid() )
			{
				viewLight.Enabled = LightEnabled;
			}

			timeSinceLightToggled = 0;
		}

		var localPawn = cl.Pawn as MyPlayer;

		if (!viewLight.IsValid() || localPawn == null) return;

		var trace = Trace.Ray( localPawn.EyePosition, localPawn.EyePosition + localPawn.EyeRotation.Forward * 512 )
			.Ignore( this )
			.Run();

		if ( trace.Hit && LightEnabled )
		{
			//Log.Info( MyGame.MapRange( trace.Distance, 0, 512, 0.005f, .025f ) );

			if ( illuminatingLight.IsValid() )
			{
				illuminatingLight.Position = trace.EndPosition - trace.Direction;
				illuminatingLight.Brightness = MyGame.MapRange( trace.Distance, 0, 256, 0.005f, .025f );
				return;

			}

			illuminatingLight = new PointLightEntity()
			{
				Position = trace.EndPosition,
				Range = 256,
				Color = Color.White,
				
				Brightness = MyGame.MapRange( trace.Distance, 0, 256, 0.005f, .025f ),
				LinearAttenuation = 0.001f,
				QuadraticAttenuation = 0
			};
		}

		else if ( !trace.Hit && LightEnabled && illuminatingLight.IsValid() || !LightEnabled )
		{
			illuminatingLight.Delete();
		}
	}

	public override bool CanReload()
	{
		return false;
	}

	private void Activate()
	{
		if ( worldLight.IsValid() )
		{
			worldLight.Enabled = LightEnabled;
		}
	}

	private void Deactivate()
	{
		if ( worldLight.IsValid() )
		{
			worldLight.Enabled = false;
		}
	}

	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );

		if ( Game.IsServer )
		{
			Activate();
		}
	}

	public override void ActiveEnd( Entity ent, bool dropped )
	{
		base.ActiveEnd( ent, dropped );

		if ( Game.IsServer )
		{
			if ( dropped )
			{
				Activate();
			}
			else
			{
				Deactivate();
			}
		}
	}

	public override void SimulateAnimator( CitizenAnimationHelper anim )
	{
		anim.HoldType = CitizenAnimationHelper.HoldTypes.Pistol;
		anim.Handedness = CitizenAnimationHelper.Hand.Right;
		anim.AimBodyWeight = 1.0f;
	}
}
