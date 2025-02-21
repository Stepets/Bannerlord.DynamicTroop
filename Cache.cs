﻿#region

using System.Collections.Generic;
using System.Linq;
using log4net.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.ObjectSystem;

#endregion

namespace Bannerlord.DynamicTroop;

public static class Cache {
	private static readonly Dictionary<(ItemObject.ItemTypeEnum, int, CultureObject?), ItemObject[]> CachedItemsByType =
		new();

	private static readonly Dictionary<(int, CultureObject?), ItemObject[]> CachedItems = new();

	private static readonly Dictionary<(ItemObject.ItemTypeEnum, int, CultureCode?), ItemObject[]> CachedEquipmentByType = new();

	/// <summary>
	///     根据物品类型、层级和文化获取物品数组。
	/// </summary>
	/// <param name="itemType"> 物品的类型。 </param>
	/// <param name="tier">     物品的层级。 </param>
	/// <param name="culture">  物品的文化属性。如果为null，则不限制文化。 </param>
	/// <returns> 符合条件的物品数组。如果没有符合条件的物品，返回null。 </returns>
	public static ItemObject[]? GetItemsByTierAndCulture(int tier, CultureObject? culture) {
		var key = (tier, culture);

		if (!CachedItems.TryGetValue(key, out var items)) {
			// If not cached, generate and cache the list
			items = MBObjectManager.Instance.GetObjectTypeList<ItemObject>()
								   .WhereQ(item => item           != null   &&
												   (int)item.Tier <= tier   &&
												   ItemBlackList.Test(item) &&
												   (item.ItemType == ItemObject.ItemTypeEnum.Horse        ||
													item.ItemType == ItemObject.ItemTypeEnum.HorseHarness ||
													(ModSettings.Instance?.RemoveCivilianEquipmentsInRandom ?? false
														 ? !item.IsCivilian
														 : !item.ItemFlags.HasAnyFlag(ItemFlags.NotUsableByMale))) &&
												   Global.ItemTypes.ContainsQ(item.ItemType)                       &&
												   !item.IsCraftedByPlayer                                         &&
												   (!item.HasWeaponComponent ||
													!Global.InvalidWeaponClasses.ContainsQ(item.PrimaryWeapon
																							   .WeaponClass)) &&
												   (item.Culture == null || item.Culture == culture))
								   .ToArrayQ();

			CachedItems[key] = items;
		}

		return items;
	}

	public static ItemObject[]? GetItemsByTypeTierAndCulture(ItemObject.ItemTypeEnum itemType,
															 int                     tier,
															 CultureObject?          culture) {
		if (tier < 0) return null;

		var key = (itemType, tier, culture);
		if (!CachedItemsByType.TryGetValue(key, out var items)) {
			// If not cached, generate and cache the list
			items = EveryoneCampaignBehavior.ItemListByType[itemType]
											.WhereQ(item => item           != null   &&
															(int)item.Tier == tier   &&
															ItemBlackList.Test(item) &&
															!item.IsCraftedByPlayer  &&
															(item.ItemType == ItemObject.ItemTypeEnum.Horse        ||
															 item.ItemType == ItemObject.ItemTypeEnum.HorseHarness ||
															 (ModSettings.Instance?.RemoveCivilianEquipmentsInRandom ??
															  false
																  ? !item.IsCivilian
																  : !item.ItemFlags
																		 .HasAnyFlag(ItemFlags.NotUsableByMale))) &&
															(!item.HasWeaponComponent ||
															 !Global.InvalidWeaponClasses.ContainsQ(item.PrimaryWeapon
																										.WeaponClass)) &&
															(item.Culture == null || item.Culture == culture))
											.ToArrayQ();
			if (items == null || items.Length == 0) items = GetItemsByTypeTierAndCulture(itemType, tier - 1, culture);

			if (items == null) return null;

			CachedItemsByType[key] = items;
		}

		return items;
	}

	public static ItemObject[]? GetEquipmentByTypeTierAndCulture(ItemObject.ItemTypeEnum itemType, int tier, CultureCode? culture) {
		if (tier < 0) return null;

		var key = (itemType, tier, culture);
		if (!CachedEquipmentByType.TryGetValue(key, out var items))
		{
			Global.Log($"equipment by type {itemType} tier {tier} culture {culture}", Colors.Green, Level.Debug);
			List<ItemObject> tmp = new();
			foreach (var item in EveryoneCampaignBehavior.ItemListByType[itemType]) {
				if (item != null
				&& (int)item.Tier + 1 == tier
				&& ItemBlackList.Test(item)
				&& !item.IsCraftedByPlayer
				&& (item.ItemType == ItemObject.ItemTypeEnum.Horse
					|| item.ItemType == ItemObject.ItemTypeEnum.HorseHarness
					|| (item.HasArmorComponent 
						&& !((ModSettings.Instance?.RemoveCivilianEquipmentsInRandom ?? false) ? (item.IsCivilian && (item.Type == ItemObject.ItemTypeEnum.BodyArmor || item.Type == ItemObject.ItemTypeEnum.HeadArmor) && item.Tier < ItemObject.ItemTiers.Tier1) : item.ItemFlags.HasAnyFlag(ItemFlags.NotUsableByMale)))
					|| !item.HasArmorComponent)
				&& !(item.HasWeaponComponent && Global.InvalidWeaponClasses.ContainsQ(item.PrimaryWeapon.WeaponClass))
				&& (item.Culture?.GetCultureCode() == null || item.Culture.GetCultureCode() == culture)) {
					tmp.Add(item);
				}
			}
			// If not cached, generate and cache the list
			// items = EveryoneCampaignBehavior.ItemListByType[itemType]
			// 								.WhereQ(item => item != null 
			// 									&& (int)item.Tier+1 == tier 
			// 									&& ItemBlackList.Test(item)
			// 									&& !item.IsCraftedByPlayer
			// 									&& !(item.HasWeaponComponent && Global.InvalidWeaponClasses.ContainsQ(item.PrimaryWeapon.WeaponClass))
			// 									&& (item.Culture == null || item.Culture.GetCultureCode() == culture?.GetCultureCode())
			// 								)
			// 								.ToArrayQ();
			items = tmp.ToArrayQ();
			if (items == null || items.Length == 0) items = GetEquipmentByTypeTierAndCulture(itemType, tier - 1, culture);

			if (items == null) return null;

			Global.Log($"is {items.SelectQ(i => $"{i.Tier}#{i.Name}").Aggregate("", (a, b) => $"{a}, {b}")}", Colors.Green, Level.Debug);

			CachedEquipmentByType[key] = items;
		}

		return items;
	}
}