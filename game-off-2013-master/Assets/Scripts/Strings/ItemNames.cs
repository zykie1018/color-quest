using UnityEngine;
using System.Collections;

/*
 * This string class allows us to look up an item in an
 * inventory by name. These strings must match the itemNames
 * on our Items/ prefabs.
 */
public class ItemNames
{
	// Meter Upgrades
	public static string RED_FILLS_FASTER = "RedFillsFaster";
	public static string GREEN_FILLS_FASTER = "GreenFillsFaster";
	public static string BLUE_FILLS_FASTER = "BlueFillsFaster";
	
	// Magnet Upgrades
	public static string RED_MAGNET = "RedMagnet";
	public static string GREEN_MAGNET = "GreenMagnet";
	public static string BLUE_MAGNET = "BlueMagnet";
	
	// Ability Upgrades
	public static string RED_MORE_EFFECTIVE = "EfficientLaser";
	public static string GREEN_MORE_EFFECTIVE = "StrongerShields";
	public static string BLUE_MORE_EFFECTIVE = "BetterReflexes";
	
	// Power Upgrades
	public static string RED_LASTS_LONGER = "RedLastsLonger";
	public static string GREEN_LASTS_LONGER = "GreenLastsLonger";
	public static string BLUE_LASTS_LONGER = "BlueLastsLonger";
	
	// Consumables
	public static string BOOST = "Boost";
	public static string SUPERBOOST = "SuperBoost";
}
