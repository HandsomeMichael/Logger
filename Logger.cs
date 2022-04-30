using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Diagnostics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;
using Terraria.UI.Chat;
using System.IO;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader.IO;
using Terraria.Localization;
using Terraria.Utilities;
using System.Reflection;
using MonoMod.RuntimeDetour.HookGen;
using Microsoft.Xna.Framework.Audio;
using Terraria.Audio;
using Terraria.Graphics.Capture;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using ReLogic.Graphics;
using System.Runtime;
using Microsoft.Xna.Framework.Input;
using Terraria.Graphics.Shaders;

namespace Logger
{
	public class GiveLogTrack : ModCommand
	{
		public override CommandType Type
			=> CommandType.Chat;

		public override string Command
			=> "GiveLogTrack";

		public override string Usage
			=> "/GiveLogTrack <player | npc | projectile> <index>";

		public override string Description
			=> "Track an entity field values. the argument are <player || npc || projectile> <index>";

		public override void Action(CommandCaller caller, string input, string[] args) {
			int whoami = int.Parse(args[1]);
			string test = args[0].ToUpper();
			int context = -1;
			if (test == "PLAYER") {context = TextEntityTrack.player;}
			else if (test == "NPC") {context = TextEntityTrack.npc;}
			else {context = TextEntityTrack.projectile;}
			TextEntityTrack.GiveLog(caller.Player,whoami,context);
		}
	}
	public class GiveLogEntity : ModCommand
	{
		public override CommandType Type
			=> CommandType.Chat;

		public override string Command
			=> "GiveLogEntity";

		public override string Usage
			=> "/GiveLogEntity <player | npc | projectile> <index>";

		public override string Description
			=> "Give an entity log item , the argument are <player || npc || projectile> <index>";

		public override void Action(CommandCaller caller, string input, string[] args) {
			int whoami = int.Parse(args[0]);
			string test = args[0].ToUpper();
			List<string> p = new List<string>();
			if (test == "PLAYER") {p = Logger.LogField(Main.player[whoami],false);}
			else if (test == "NPC") {p = Logger.LogField(Main.npc[whoami],false);}
			else {p = Logger.LogField(Main.projectile[whoami],false);}

			Item dummy = new Item();
			dummy.SetDefaults(ModContent.ItemType<Text>());
			if (dummy.modItem is Text t) {t.log = p;}
			caller.Player.QuickSpawnClonedItem(dummy);
		}
	}
	public class EntityList : ModCommand
	{
		public override CommandType Type
			=> CommandType.Chat;

		public override string Command
			=> "EntityList";

		public override string Usage
			=> "/EntityList <player | npc | projectile>";

		public override string Description
			=> "Print out any active entity pos , and names. the argument are <player || npc || projectile || any>";

		public override void Action(CommandCaller caller, string input, string[] args) {
			string test = "ANY";
			if (args.Length > 0) {
				test = args[0].ToUpper();
			}
			if (test == "ANY") {
				foreach (var item in Main.player){
					if (item != null && item.active ) {
						Main.NewText("playerlog ( "+item.whoAmI+" ) [ "+item.name+ " ] { "+item.position.ToString()+" }");
					}
				}
				foreach (var item in Main.npc){
					if (item != null && item.active) {
						Main.NewText("npclog ( "+item.type+" , "+item.whoAmI+" ) [ "+item.FullName+ " ] { "+item.position.ToString()+" }");
					}
				}
				foreach (var item in Main.projectile){
					if (item != null && item.active) {
						Main.NewText("projlog ( "+item.type+" , "+item.whoAmI+" ) [ "+item.Name+ " ] { "+item.position.ToString()+" }");
					}
				}
			}
			else if (test == "PLAYER") {
				foreach (var item in Main.player){
					if (item != null && item.active ) {
						Main.NewText("playerlog ( "+item.whoAmI+" ) [ "+item.name+ " ] { "+item.position.ToString()+" }");
					}
				}
			}
			else if (test == "NPC") {
				foreach (var item in Main.npc){
					if (item != null && item.active) {
						Main.NewText("npclog ( "+item.type+" , "+item.whoAmI+" ) [ "+item.FullName+ " ] { "+item.position.ToString()+" }");
					}
				}
			}
			else {
				foreach (var item in Main.projectile){
					if (item != null && item.active) {
						Main.NewText("projlog ( "+item.type+" , "+item.whoAmI+" ) [ "+item.Name+ " ] { "+item.position.ToString()+" }");
					}
				}
			}
		}
	}
	public class LogNPC : GlobalNPC
	{
		public static void Log(NPC npc,string context = "none") {
			if (npc == null) {return;}
			Main.NewText("projlog ( "+npc.type+" , "+npc.whoAmI+" ) [ "+npc.FullName+ " ] { context : "+context+" }");
			Main.NewText("ai0 : "+npc.ai[0]);
			Main.NewText("ai1 : "+npc.ai[1]);
			Main.NewText("ai2 : "+npc.ai[2]);
			Main.NewText("localAIs : "+npc.localAI[0]+" , "+npc.localAI[1]);
			Main.NewText("timeLeft : "+npc.timeLeft);
		}
		public override bool PreAI(NPC npc) {
			if (MyConfig.get.npc_preai) Log(npc,"preai");
			return base.PreAI(npc);
		}
		public override void NPCLoot(NPC npc) {
			if (MyConfig.get.npc_npcloot) Log(npc,"NPCLoot");
		}
	}
	public class LogProj : GlobalProjectile
	{
		public static void Log(Projectile projectile,string context = "none") {
			if (projectile == null) {return;}
			Main.NewText("projlog ( "+projectile.type+" , "+projectile.whoAmI+" ) [ "+projectile.Name+ " ] { context : "+context+" }");
			Main.NewText("ai0 : "+projectile.ai[0]);
			Main.NewText("ai1 : "+projectile.ai[1]);
			Main.NewText("localAIs : "+projectile.localAI[0]+" , "+projectile.localAI[1]);
			Main.NewText("timeLeft : "+projectile.timeLeft);
		}
		public override bool PreAI(Projectile projectile) {
			if (MyConfig.get.proj_preai) Log(projectile,"preai");
			return base.PreAI(projectile);
		}
		public override bool PreKill(Projectile projectile, int timeLeft) {
			if (MyConfig.get.proj_prekill) Log(projectile,"prekil");
			return base.PreKill(projectile,timeLeft);
		}
	}
	public class TextEntityTrack : Text
	{
		public static void GiveLog(Player player,int track,int context) {
			if (context == -1) {return;}
			Item dummy = new Item();
			dummy.SetDefaults(ModContent.ItemType<TextEntityTrack>());
			if (dummy.modItem is TextEntityTrack t) {
				t.track = track;
				t.context = context;
			}
			player.QuickSpawnClonedItem(dummy);
		}
		public int track = -1;
		public int context;
		public const int player = 0;
		public const int projectile = 1;
		public const int npc = 2;
		public const int modProjectile = 3;
		public const int modNPC = 4;
		public override bool CanRightClick() => true;
		public override bool ConsumeItem(Player player) => false;
		public override void RightClick(Player player) {
			int cnt = context;
			if (context == projectile) {context = modProjectile;}
			else if (context == modProjectile) {context = projectile;}
			else if (context == npc) {context = modNPC;}
			else if (context == modNPC) {context = npc;}
			if (cnt != context) {Main.NewText($"context changed to {cnt}");}
		}
		public override void UpdateInventory(Player plyr) {
			if (track == -1) {
				item.TurnToAir();
				return;
			}
			item.color = Color.Yellow;
			if (context == player) {log = Logger.LogField(Main.player[track],false);}
			else if (context == projectile) {log = Logger.LogField(Main.projectile[track],false);}
			else if (context == npc) {log = Logger.LogField(Main.npc[track],false);}
			else if (context == modProjectile) {log = Logger.LogField(Main.projectile[track].modProjectile,false);}
			else if (context == modNPC) {log = Logger.LogField(Main.npc[track].modNPC,false);}
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			string c = "unknown";
			if (context == player) {c = "player";}
			else if (context == npc) {c = "npc";}
			else if (context == projectile) {c = "projectile";}
			else if (context == modProjectile) {c = "modProjectile";}
			else if (context == modNPC) {c = "modNPC";}
			base.ModifyTooltips(tooltips);
			if (context != player) {
				tooltips.Insert(1,new TooltipLine(mod,"amogussussybakisadssf","Use right click to change context"));
			}
			tooltips.Insert(1,new TooltipLine(mod,"amogussussybakisadssf",$"tracking : {track}, context : {c}"));
		}
	}
	public class Text : ModItem
	{
		public override string Texture => "Logger/Text";
		public List<string> log = new List<string>() {"no text"};
		public override bool CloneNewInstances => true;

		public static int scroll;
		public override void ModifyTooltips(List<TooltipLine> tooltips) {

			int a = 0;
			foreach (var i in log){
				tooltips.Add(new TooltipLine(mod,"asdf"+a,i));
				a++;
			}

			int maxLine = 20;
			List<TooltipLine> cachedLine = new List<TooltipLine>();
			for (int i = 0; i < tooltips.Count; i++) {
				var tt = tooltips[i];
				if ((tt.mod == "Terraria" && tt.Name.Contains("Tooltip")) || (tt.mod != "Terraria" && !tt.isModifier && !tt.isModifierBad)) {
					cachedLine.Add(tt);
				}
			}
			int num25 = PlayerInput.ScrollWheelDelta / 120;
			int sped = MyConfig.get.text_speed;
			if (Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift)) {sped *= 5;}
			if (num25 < 0) {scroll += sped;}
			if (num25 > 0) {scroll -= sped;}
			if (scroll < 0) {scroll = 0;}
			if (scroll > cachedLine.Count-2) {scroll = cachedLine.Count-2;}
			if (cachedLine.Count > maxLine + 1) {
				int lineRemove = cachedLine.Count - maxLine - 1;
				
				lineRemove -= scroll;

				if (scroll > 0) {
					lineRemove -= 1;
					for (int z = 0; z < scroll+1; z++){
						tooltips.Remove(cachedLine[z]);
					}
				}
				for (int b = cachedLine.Count - 1; b >= cachedLine.Count - lineRemove - 1 ; b--){
					if (b < cachedLine.Count) {
						tooltips.Remove(cachedLine[b]);
					}
				}
			}

		}
		public static Vector2 Size(string longestText) {
			var snippets = ChatManager.ParseMessage(longestText, Color.White).ToArray();
			return ChatManager.GetStringSize(Main.fontMouseText, snippets, Vector2.One);
		}
		public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset) {
			if ((line.mod == "Terraria" && line.Name.Contains("Tooltip")) || (line.mod != "Terraria" && !line.isModifier && !line.isModifierBad)) {
				if (line.Name != "amogussussybakisadssf") {
					Color color = Color.Red;
					if (line.text.Contains('#')) {
						color = Color.Pink;
					}
					Vector2 messageSize = Size(line.text);
					int width = (int)messageSize.X + 3;
					if (200 > width) {width = 200;}
					Utils.DrawInvBG(Main.spriteBatch, new Rectangle(
						line.X - (35/10),
						line.Y - (42/10),
						width,
						(int)messageSize.Y),color*0.4f);
				}
			}
			return base.PreDrawTooltipLine(line,ref yOffset);
		}
		public override void SetDefaults() {
			item.width = item.height = 10;
			item.rare = 1;
		}
	}
	public class MyConfig : ModConfig
	{
		public static void SaveConfig(){
			typeof(ConfigManager).GetMethod("Save", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[1] { get });
		}

		public override ConfigScope Mode => ConfigScope.ClientSide;
		public static MyConfig get => ModContent.GetInstance<MyConfig>();

		[Header("maongus")]

		[DefaultValue(false)]
		public bool proj_preai;

		[DefaultValue(false)]
		public bool proj_prekill;

		[DefaultValue(false)]
		public bool npc_preai;

		[DefaultValue(false)]
		public bool npc_npcloot;

		[DefaultValue(2)]
		public int text_speed;

	}
	public class Logger : Mod
	{
		public override void UpdateUI(GameTime gameTime) {
			if (Main.HoverItem.IsAir) {Text.scroll = 0;}
		}
		public static List<string> LogField(object obj,bool newtext = true) {
			if (obj == null) {
				if (newtext) {Main.NewText("error erorr , obj is null",Color.Red);}
				return new List<string>(){"error"};
			}
			var p = obj.GetType().GetFields();
			List<string> text = new List<string>();
			text.Add("field loggin : "+obj.GetType().Name);
			if (obj is NPC npc) {
				if (newtext)Main.NewText("Name : "+npc.FullName,Color.Red);
				text.Add("Name : "+npc.FullName);
			}
			if (obj is Projectile projectile) {
				if (newtext)Main.NewText("Name : "+projectile.Name,Color.Red);
				text.Add("Name : "+projectile.Name);
			}
			foreach (var item in p.OrderBy(type => type.Name)){
				if (item != null && !item.IsStatic) {
					object b = item.GetValue(obj);
					if (b != null) {
						if (b.GetType().IsArray) {
							Array array = (Array)b;
							int count = 0;
							if (array.Length == 254 || array.Length == 255 || array.Length == 256) {
								string bext = item.Name+$"[#{Main.myPlayer}] : "+array.GetValue(Main.myPlayer).ToString();
								if (newtext) {Main.NewText(bext);}
								text.Add(bext);
							}
							else {
								foreach (var i in array){
									string bext = item.Name+$"[#{count}] : "+i.ToString();
									if (newtext) {Main.NewText(bext);}
									text.Add(bext);
									count++;
								}
							}
							continue;
						}
						string ga = item.Name+" : "+b.ToString();
						if (newtext) {Main.NewText(ga);}
						text.Add(ga);
					}
				}
			}
			return text;
		}
	}
}