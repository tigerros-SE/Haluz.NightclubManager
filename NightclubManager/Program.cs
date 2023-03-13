namespace IngameScript
{
	using Sandbox.ModAPI.Ingame;
	using System;
	using System.Collections.Generic;
	using VRage.Game.ModAPI.Ingame.Utilities;
	using SEKit.Extensions;
	using SEKit.Classes;
	using SEKit.Enums;

	class Program : MyGridProgram
	{
		// Since this is not minified, adding "private" would take up space
		// ReSharper disable ArrangeTypeMemberModifiers
		#region mdk preserve
		// The nightclub exhaust group
		const string NightclubExhaustGroupName = "Nightclub Exhausts";
		// The nightclub light group. The "NightclubNormalLight" should be in this group.
		const string NightclubLightGroupName = "Nightclub Lights";
		// The nightclub normal light. This light will be turned off during "party mode" and turned on otherwise.
		// This is the last constant. Don't change anything below this!
		const string NightclubNormalLightName = "Nightclub Normal Light";
		#endregion
		// ReSharper restore ArrangeTypeMemberModifiers

		private readonly MyCommandLine _commandLine = new MyCommandLine();
		private readonly Dictionary<string, Command> _commands = new Dictionary<string, Command>(StringComparer.OrdinalIgnoreCase);
		private bool _isNightclubOn;

		public Program()
		{
			_commands["nightclub"] = new Command("nightclub", "Toggles the nightclub \"party mode\" (rotating lights and exhaust pipes).", Nightclub);
		}

		private void NightclubOff(List<IMyTerminalBlock> exhausts, List<IMyLightingBlock> lights)
		{
			for (int ei = 0; ei < exhausts.Count; ei++)
			{
				exhausts[ei].ApplyAction("OnOff_Off");
			}

			for (int rli = 0; rli < lights.Count; rli++)
			{
				IMyLightingBlock light = lights[rli];

				light.Enabled = light.CustomName == NightclubNormalLightName;
			}
		}

		private void NightclubOn(List<IMyTerminalBlock> exhausts, List<IMyLightingBlock> lights)
		{
			for (int ei = 0; ei < exhausts.Count; ei++)
			{
				exhausts[ei].ApplyAction("OnOff_On");
			}

			for (int rli = 0; rli < lights.Count; rli++)
			{
				IMyLightingBlock light = lights[rli];

				light.Enabled = light.CustomName != NightclubNormalLightName;
			}
		}

		public void Nightclub()
		{
			IMyBlockGroup exhaustGroup = GridTerminalSystem.GetBlockGroupWithName(NightclubExhaustGroupName);
			var exhausts = new List<IMyTerminalBlock>();
			exhaustGroup.GetBlocks(exhausts);

			IMyBlockGroup lightGroup = GridTerminalSystem.GetBlockGroupWithName(NightclubLightGroupName);
			var lights = new List<IMyLightingBlock>();
			lightGroup.GetBlocksOfType(lights);

			if (_isNightclubOn)
			{
				Me.WriteMessageToSurface(TextMessage.NewLine("Turning party mode off.", TextMessageType.Info), ProgrammableBlockSurfaceType.Display);
				NightclubOff(exhausts, lights);
			}
			else
			{
				Me.WriteMessageToSurface(TextMessage.NewLine("Turning party mode on.", TextMessageType.Info), ProgrammableBlockSurfaceType.Display);
				NightclubOn(exhausts, lights);
			}

			_isNightclubOn = !_isNightclubOn;
		}

		public void Main(string argument, UpdateType updateSource)
		{
			if (!_commandLine.TryParse(argument)) return;

			Command command;

			string commandString = _commandLine.Argument(0);

			if (commandString == null) 
			{
				Echo($"No command specified. Available commands: {string.Join(",", _commands.Values)}");
			}
			else if (_commands.TryGetValue(commandString, out command))
			{
				command.Action();
			}
			else
			{
				Echo($"Unknown command {commandString}");
			}
		}
	}
}