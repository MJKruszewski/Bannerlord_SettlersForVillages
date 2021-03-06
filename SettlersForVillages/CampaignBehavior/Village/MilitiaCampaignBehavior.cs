﻿using System;
using System.Collections.Generic;
using SettlersForVillages.Models;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;

namespace SettlersForVillages.CampaignBehavior.Village
{
    class MilitiaCampaignBehavior : CampaignBehaviorBase
    {
        private static readonly string VillageMilitiaMenu = "village_militia_rec_menu";

        private static readonly List<MilitiaTierModel> TierMilitiaList = new List<MilitiaTierModel>
        {
            new MilitiaTierModel(5000, 10),
            new MilitiaTierModel(10000, 25),
            new MilitiaTierModel(15000, 50),
            new MilitiaTierModel(20000, 75),
            new MilitiaTierModel(25000, 100),
        };

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, AddGameMenus);
        }

        private void AddGameMenus(CampaignGameStarter campaignGameSystemStarter)
        {
            try
            {
                campaignGameSystemStarter.AddGameMenuOption(
                    "village",
                    "village_militia_rec_menu_button",
                    Main.Localization.GetTranslation(Localization.MilitiaMenuVillage),
                    args =>
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Recruit;

                        return Campaign.Current.CurrentMenuContext.GameMenu.StringId != VillageMilitiaMenu &&
                               Settlement.CurrentSettlement.IsVillage &&
                               Settlement.CurrentSettlement.OwnerClan == Clan.PlayerClan;
                    },
                    args => { GameMenu.SwitchToMenu(VillageMilitiaMenu); },
                    false,
                    4
                );

                campaignGameSystemStarter.AddGameMenu(
                    VillageMilitiaMenu,
                    Main.Localization.GetTranslation(Localization.MilitiaMenuVillageDescription),
                    null
                );

                AddTiers(campaignGameSystemStarter);
                AddLeaveButtons(campaignGameSystemStarter);
            }
            catch (Exception e)
            {
                Logger.logError(e);
            }
        }

        private static void AddTiers(CampaignGameStarter campaignGameSystemStarter)
        {
            foreach (var tier in TierMilitiaList)
            {
                campaignGameSystemStarter.AddGameMenuOption(
                    VillageMilitiaMenu,
                    tier.GetOptionId(),
                    tier.GetOptionText(),
                    args =>
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Trade;

                        return true;
                    },
                    args =>
                    {
                        try
                        {
                            AddMilitia(tier.GoldPrice, tier.MilitiaToAdd);
                        }
                        catch (Exception e)
                        {
                            Logger.logError(e);
                        }
                    }
                );
            }
        }

        private static void AddLeaveButtons(CampaignGameStarter campaignGameSystemStarter)
        {
            campaignGameSystemStarter.AddGameMenuOption(
                VillageMilitiaMenu,
                "village_militia_rec_leave",
                Main.Localization.GetTranslation(Localization.MenuLeave),
                args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                args => { GameMenu.SwitchToMenu("village"); }
            );
        }

        private static void AddMilitia(int goldPrice, float militiaToAdd)
        {
            if (goldPrice > Hero.MainHero.Gold)
            {
                Logger.DisplayInfoMsg(Main.Localization.GetTranslation(Localization.MilitiaActionLackOfGold));
                return;
            }

            if (militiaToAdd >= Settlement.CurrentSettlement.Village.Hearth)
            {
                Logger.DisplayInfoMsg(Main.Localization.GetTranslation(Localization.MilitiaActionLackOfVillagers));
                return;
            }

            GiveGoldAction.ApplyForCharacterToSettlement(Hero.MainHero, Settlement.CurrentSettlement, goldPrice);
            Settlement.CurrentSettlement.Village.Hearth -= militiaToAdd;
            Settlement.CurrentSettlement.Militia += militiaToAdd;
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}