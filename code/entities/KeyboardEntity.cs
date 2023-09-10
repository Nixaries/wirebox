using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Sandbox.UI;

[Library( "ent_wirekeyboard", Title = "Wire Keyboard" )]
public partial class KeyboardEntity : Prop, IUse, IWireOutputEntity
{
	public bool IsToggle { get; set; } = false;
	WirePortData IWireEntity.WirePorts { get; } = new WirePortData();

	[ConCmd.Server( "set_keyboard_button")]
	public static void SetKeyboardButton( string button, int keyCode, int netIdent )
	{
		KeyboardEntity keyboardEntity = FindByIndex( netIdent ) as KeyboardEntity;
		keyboardEntity.WireTriggerOutput( "Button", button );
		keyboardEntity.WireTriggerOutput( "Key Code", keyCode );
	}
	
	[ConCmd.Server( "set_keyboard_character")]
	public static void SetKeyboardCharacter( string character, int netIdent )
	{
		KeyboardEntity keyboardEntity = FindByIndex( netIdent ) as KeyboardEntity;
		keyboardEntity.WireTriggerOutput( "Character", character );
	}
	
	[ConCmd.Server( "set_keyboard_pressed")]
	public static void SetKeyboardPressed( bool pressed, int netIdent )
	{
		KeyboardEntity keyboardEntity = FindByIndex( netIdent ) as KeyboardEntity;
		keyboardEntity.WireTriggerOutput( "Pressed", pressed );
	}
	
	[ConCmd.Server("set_keyboard_active")]
	public static void SetKeyboardActive( bool active, int netIdent )
	{
		KeyboardEntity keyboardEntity = FindByIndex( netIdent ) as KeyboardEntity;
		keyboardEntity.WireTriggerOutput( "Active", active);
	}
	
	public bool IsUsable( Entity user )
	{
		return (user as IEntity) is SandboxPlayer;
	}
	
	public bool OnUse( Entity user )
	{
		if ( (user as IEntity) is SandboxPlayer player )
		{
			if ( player.Controller.GetType() != typeof( LockedPositionController ) )
			{
				GrabInputs( To.Single( player.Client ), this );
				player.EnableSolidCollisions = false;
				this.WireTriggerOutput( "Active", true );
			}
			else
			{
				player.EnableSolidCollisions = true;
				this.WireTriggerOutput( "Active", false );
			}
		}
		return false;
	}

	public void SetButton( string button, int keyCode )
	{
		ConsoleSystem.Run("set_keyboard_button", button, keyCode, this.NetworkIdent);
	}

	public void SetCharacter( string character )
	{
		ConsoleSystem.Run("set_keyboard_character", character, this.NetworkIdent);
	}

	public void SetPressed( bool pressed )
	{
		ConsoleSystem.Run( "set_keyboard_pressed", pressed, this.NetworkIdent );
	}

	public void SetActive( bool active )
	{
		ConsoleSystem.Run( "set_keyboard_active", active, this.NetworkIdent );
	}
	
	[ClientRpc]
	public static void GrabInputs(KeyboardEntity keyboardEntity)
	{
		keyboardEntity.WireTriggerOutput( "Active", true );
		
		if ( !Game.RootPanel.ChildrenOfType<InputGrabber>().Any() )
		{
			Game.RootPanel.AddChild<InputGrabber>();
		}

		InputGrabber inputGrabber = Game.RootPanel.ChildrenOfType<InputGrabber>().First();
		inputGrabber.activeKeyboard = keyboardEntity;
		
		InputFocus.Set( inputGrabber );
	}

	public PortType[] WireGetOutputs()
	{
		List<PortType> portTypes = new List<PortType>();
		portTypes.Add( PortType.String( "Button" ) );
		portTypes.Add( PortType.String( "Character" ) );
		portTypes.Add( PortType.Int( "Key Code" ) );
		portTypes.Add( PortType.Bool( "Pressed" ) );
		portTypes.Add( PortType.Bool( "Active" ) );

		return portTypes.ToArray();
	}
}
