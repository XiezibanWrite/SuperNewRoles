﻿using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using SuperNewRoles.Patches;
using UnityEngine;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomOption;

namespace SuperNewRoles.Roles
{
    class SpeedBooster { 
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.SpeedBoosterBoostButton.MaxTimer = RoleClass.SpeedBooster.CoolTime;
            RoleClass.SpeedBooster.ButtonTimer = DateTime.Now;
        }
        public static void BoostStart()
        {
            RoleClass.SpeedBooster.IsSpeedBoost = true;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetSpeedBoost, Hazel.SendOption.Reliable, -1);
            writer.Write(true);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            CustomRPC.RPCProcedure.SetSpeedBoost(true,PlayerControl.LocalPlayer.PlayerId);
            SpeedBooster.ResetCoolDown();
        }
        public static void ResetSpeed()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetSpeedBoost, Hazel.SendOption.Reliable, -1);
            writer.Write(false);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            CustomRPC.RPCProcedure.SetSpeedBoost(false, PlayerControl.LocalPlayer.PlayerId);
        }
        public static void SpeedBoostEnd()
        {
            ResetSpeed();
        }
        public static bool IsSpeedBooster(PlayerControl Player)
        {
            if (RoleClass.SpeedBooster.SpeedBoosterPlayer.IsCheckListPlayerControl(Player))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void EndMeeting()
        {

            HudManagerStartPatch.SpeedBoosterBoostButton.MaxTimer = RoleClass.SpeedBooster.CoolTime;
            RoleClass.SpeedBooster.ButtonTimer = DateTime.Now;
            ResetSpeed();
        }
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
        public static class PlayerPhysicsSpeedPatch
        {
            public static void Postfix(PlayerPhysics __instance)
            {
                if (__instance.AmOwner && __instance.myPlayer.CanMove && GameData.Instance && (RoleClass.EvilSpeedBooster.IsBoostPlayers[__instance.myPlayer.PlayerId] || RoleClass.SpeedBooster.IsBoostPlayers[__instance.myPlayer.PlayerId]))
                {
                    __instance.body.velocity *= RoleClass.SpeedBooster.Speed;
                }

            }
        }
    }
}
