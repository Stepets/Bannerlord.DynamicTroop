﻿using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.LinQuick;
using static TaleWorlds.Core.ItemObject;

namespace Bannerlord.DynamicTroop;

public class Assignment {
	public static readonly EquipmentIndex[] WeaponSlots = {
															  EquipmentIndex.Weapon0,
															  EquipmentIndex.Weapon1,
															  EquipmentIndex.Weapon2,
															  EquipmentIndex.Weapon3
														  };

	private static int _counter;

	public readonly Equipment Equipment;

	public Assignment(CharacterObject character) {
		Index              = ++_counter;
		Character          = character;
		Equipment          = ArmyArmory.CreateEmptyEquipment();
		ReferenceEquipment = character.RandomBattleEquipment.Clone();
	}

	public int Index { get; }

	public bool IsAssigned { get; set; }

	public CharacterObject Character { get; }

	public Equipment ReferenceEquipment { get; }

	public bool IsShielded =>
		WeaponSlots.AnyQ(slot => Equipment.GetEquipmentFromSlot(slot) is {
																			 IsEmpty      : false,
																			 Item.ItemType: ItemTypeEnum.Shield
																		 });

	public bool CanBeShielded =>
		WeaponSlots.AnyQ(slot => Equipment.GetEquipmentFromSlot(slot) is {
																			 IsEmpty: false,
																			 Item: {
																				 ItemType: ItemTypeEnum
																					 .OneHandedWeapon
																			 } item
																		 } &&
								 !Global.CantUseWithShields(item));

	public bool IsArcher =>
		WeaponSlots.AnyQ(slot => Equipment.GetEquipmentFromSlot(slot) is { IsEmpty: false, Item: { } item } &&
								 Global.IsBow(item));

	public bool IsCrossBowMan =>
		WeaponSlots.AnyQ(slot => Equipment.GetEquipmentFromSlot(slot) is { IsEmpty: false, Item: { } item } &&
								 Global.IsCrossBow(item));

	public bool HaveThrown =>
		WeaponSlots.AnyQ(slot => Equipment.GetEquipmentFromSlot(slot) is { IsEmpty: false, Item: { } item } &&
								 Global.IsThrowing(item));

	public bool HaveTwoHandedWeaponOrPolearms =>
		WeaponSlots.AnyQ(slot => Equipment.GetEquipmentFromSlot(slot) is { IsEmpty: false, Item: { } item } &&
								 (Global.IsTwoHanded(item) || Global.IsPolearm(item)));

	public EquipmentIndex? EmptyWeaponSlot {
		get {
			foreach (var slot in WeaponSlots)
				if (Equipment.GetEquipmentFromSlot(slot).IsEmpty || Equipment.GetEquipmentFromSlot(slot).Item == null)
					return slot;

			return null;
		}
	}

	public bool IsMounted {
		get {
			var horse = ReferenceEquipment.GetEquipmentFromSlot(EquipmentIndex.Horse);
			return horse is { IsEmpty: false, Item: not null };
		}
	}

	public bool IsUnarmed() {
		return (Equipment.GetEquipmentFromSlot(EquipmentIndex.Weapon0).IsEmpty ||
				Equipment.GetEquipmentFromSlot(EquipmentIndex.Weapon0).Item == null) &&
			   (Equipment.GetEquipmentFromSlot(EquipmentIndex.Weapon1).IsEmpty ||
				Equipment.GetEquipmentFromSlot(EquipmentIndex.Weapon1).Item == null) &&
			   (Equipment.GetEquipmentFromSlot(EquipmentIndex.Weapon2).IsEmpty ||
				Equipment.GetEquipmentFromSlot(EquipmentIndex.Weapon2).Item == null) &&
			   (Equipment.GetEquipmentFromSlot(EquipmentIndex.Weapon3).IsEmpty ||
				Equipment.GetEquipmentFromSlot(EquipmentIndex.Weapon3).Item == null);
	}
}