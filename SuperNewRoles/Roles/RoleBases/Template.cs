using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Roles.RoleBases;

public class Template : RoleBase<Template>
{
    public static Color color = Palette.CrewmateBlue;

    public Template()
    {
        RoleId = roleId = RoleId.DefaultRole;
        //以下いるもののみ変更
        HasTask = true;
        HasFakeTask = true;
        IsKiller = false;
        IsAssignRoleFirst = true;
        OptionId = 0;
        IsSHRRole = false;
        OptionType = CustomOptionType.Crewmate;
        IsChangeOutfitRole = false;
        CanUseVentOptionOn = false;
        CanUseVentOptionDefault = false;
        CanUseSaboOptionOn = false;
        CanUseSaboOptionDefault = false;
        IsImpostorViewOptionOn = false;
        IsImpostorViewOptionDefault = false;
        CoolTimeOptionOn = false;
        DurationTimeOptionOn = false;
        CoolTimeOptionMax = -1f;
        CoolTimeOptionMin = -1f;
        DurationTimeOptionMax = -1f;
        DurationTimeOptionMin = -1f;
    }

    public override void OnMeetingStart() { }
    public override void OnWrapUp() { }
    public override void FixedUpdate() { }
    public override void MeFixedUpdateAlive() { }
    public override void MeFixedUpdateDead() { }
    public override void OnKill(PlayerControl target) { }
    public override void OnDeath(PlayerControl killer = null) { }
    public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    public override void EndUseAbility() { }
    public override void ResetRole() { }
    public override void PostInit() { }
    public override void UseAbility() { base.UseAbility(); AbilityLimit--; if (AbilityLimit <= 0) EndUseAbility(); }
    public override bool CanUseAbility() { return base.CanUseAbility() && AbilityLimit <= 0; }

    //ボタンが必要な場合のみ(Buttonsの方に記述する必要あり)
    public static void MakeButtons(HudManager hm) { }
    public static void SetButtonCooldowns() { }

    public override void SetupMyOptions() { }

    public static void Clear()
    {
        players = new();
    }
}