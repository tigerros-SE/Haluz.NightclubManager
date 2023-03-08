using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript {
	class Command {
		public string Name { get; set; }
		public string Description { get; set; }
		public Action Action { get; set; }

		public Command(string name, string description, Action action) {
			Name = name;
			Description = description;
			Action = action;
		}

		public override string ToString() {
			return $"{Name} - {Description}";
		}
	}

	partial class Program : MyGridProgram {
		#region mdk preserve
		const string NightclubExhaustGroupName = "Nightclub Exhausts";
		const string NightclubRotatingLightGroupName = "Nightclub Rotating Lights";
		const string NightclubNormalLightName = "Nightclub Normal Light";

		#endregion

		MyCommandLine _commandLine = new MyCommandLine();
		Dictionary<string, Command> _commands = new Dictionary<string, Command>(StringComparer.OrdinalIgnoreCase);
		bool isNightclubOn = false;

		public Program() {
			_commands["nightclub"] = new Command("nightclub", "Turn the nightclub party mode on/off.", Nightclub);
		}

		private void NightclubOff(List<IMyTerminalBlock> exhausts, List<IMyReflectorLight> lights) {
			for (int ei = 0; ei < exhausts.Count; ei++) {
				exhausts[ei].ApplyAction("OnOff_Off");
			}

			for (int rli = 0; rli < lights.Count; rli++) {
				IMyReflectorLight light = lights[rli];

				light.Enabled = light.CustomName == NightclubNormalLightName;
			}
		}

		private void NightclubOn(List<IMyTerminalBlock> exhausts, List<IMyReflectorLight> lights) {
			for (int ei = 0; ei < exhausts.Count; ei++) {
				exhausts[ei].ApplyAction("OnOff_On");
			}

			for (int rli = 0; rli < lights.Count; rli++) {
				IMyReflectorLight light = lights[rli];

				light.Enabled = light.CustomName != NightclubNormalLightName;
			}
		}

		public void Nightclub() {
			IMyBlockGroup exhaustGroup = GridTerminalSystem.GetBlockGroupWithName(NightclubExhaustGroupName);
			var exhausts = new List<IMyTerminalBlock>();
			exhaustGroup.GetBlocks(exhausts);

			IMyBlockGroup rotatingLightGroup = GridTerminalSystem.GetBlockGroupWithName(NightclubRotatingLightGroupName);
			var rotatingLights = new List<IMyReflectorLight>();
			rotatingLightGroup.GetBlocksOfType(rotatingLights);

			if (isNightclubOn) {
				NightclubOff(exhausts, rotatingLights);
			} else {
				NightclubOn(exhausts, rotatingLights);
			}

			isNightclubOn = !isNightclubOn;
		}

		public void Main(string argument, UpdateType updateSource) {
			if (_commandLine.TryParse(argument)) {
				Command command;

				string commandString = _commandLine.Argument(0);

				if (commandString == null) {
					Echo($"No command specified. Available commands: {string.Join(",", _commands.Values)}");
				} else if (_commands.TryGetValue(commandString, out command)) {
					command.Action();
				} else {
					Echo($"Unknown command {commandString}");
				}
			}
		}
	}
}
