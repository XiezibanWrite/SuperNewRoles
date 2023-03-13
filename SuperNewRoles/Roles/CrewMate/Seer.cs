using System;
using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.ReplayManager;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Impostor.MadRole;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.Neutral.FriendRoles;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOption;
using static SuperNewRoles.Modules.CustomOptionHolder;
namespace SuperNewRoles.Roles.Crewmate;

public class Seer : RoleBase<Seer>
{
    public static Color color = new Color32(97, 178, 108, byte.MaxValue);

    public Seer()
    {
        RoleId = roleId = RoleId.Seer;
        //以下いるもののみ変更
        OptionId = 320;
        IsSHRRole = true;
        OptionType = CustomOptionType.Crewmate;
    }

    public override void OnMeetingStart() { }
    public override void OnWrapUp()
    {
        var role = PlayerControl.LocalPlayer.GetRole();
        if (role is RoleId.Seer or RoleId.MadSeer or RoleId.EvilSeer or RoleId.SeerFriends or RoleId.JackalSeer or RoleId.SidekickSeer)
        {
            List<Vector3> DeadBodyPositions = new();
            bool limitSoulDuration = false;
            float soulDuration = 0f;
            switch (role)
            {
                case RoleId.Seer:
                    DeadBodyPositions = deadBodyPositions;
                    deadBodyPositions = new List<Vector3>();
                    limitSoulDuration = SeerLimitSoulDuration.GetBool();
                    soulDuration = SeerSoulDuration.GetFloat();
                    if (mode is not 0 and not 2) return;
                    break;
                case RoleId.JackalSeer:
                case RoleId.SidekickSeer:
                    DeadBodyPositions = RoleClass.JackalSeer.deadBodyPositions;
                    RoleClass.JackalSeer.deadBodyPositions = new List<Vector3>();
                    limitSoulDuration = RoleClass.JackalSeer.limitSoulDuration;
                    soulDuration = RoleClass.JackalSeer.soulDuration;
                    if (RoleClass.JackalSeer.mode is not 0 and not 2) return;
                    break;
            }
            foreach (Vector3 pos in DeadBodyPositions)
            {
                GameObject soul = new();
                soul.transform.position = pos;
                soul.layer = 5;
                var rend = soul.AddComponent<SpriteRenderer>();
                rend.sprite = GetSoulSprite();

                if (limitSoulDuration)
                {
                    FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(soulDuration, new Action<float>((p) =>
                    {
                        if (rend != null)
                        {
                            var tmp = rend.color;
                            tmp.a = Mathf.Clamp01(1 - p);
                            rend.color = tmp;
                        }
                        if (p == 1f && rend != null && rend.gameObject != null) UnityEngine.Object.Destroy(rend.gameObject);
                    })));
                }
            }
        }
    }
    public override void FixedUpdate() { }
    public override void MeFixedUpdateAlive() { }
    public override void MeFixedUpdateDead() { }
    public override void OnKill(PlayerControl target) { }
    public override void OnDeath(PlayerControl killer = null) { }
    public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    public override void EndUseAbility() { }
    public override void ResetRole() { }
    public override void PostInit() { deadBodyPositions = new(); }
    public override void UseAbility() { base.UseAbility(); AbilityLimit--; if (AbilityLimit <= 0) EndUseAbility(); }
    public override bool CanUseAbility() { return base.CanUseAbility() && AbilityLimit <= 0; }

    //ボタンが必要な場合のみ(Buttonsの方に記述する必要あり)
    public static void MakeButtons(HudManager hm) { }
    public static void SetButtonCooldowns() { }

    // CustomOption Start
    public static CustomOption SeerMode;
    public static CustomOption SeerLimitSoulDuration;
    public static CustomOption SeerSoulDuration;

    public override void SetupMyOptions()
    {
        SeerMode = Create(OptionId, false, CustomOptionType.Crewmate, "SeerMode", new string[] { "SeerModeBoth", "SeerModeFlash", "SeerModeSouls" }, RoleOption); OptionId++;
        SeerLimitSoulDuration = Create(OptionId, false, CustomOptionType.Crewmate, "SeerLimitSoulDuration", false, RoleOption); OptionId++;
        SeerSoulDuration = Create(OptionId, false, CustomOptionType.Crewmate, "SeerSoulDuration", 15f, 0f, 120f, 5f, SeerLimitSoulDuration, format: "unitCouples");
    }
    // CustomOption End

    // RoleClass Start
    public List<Vector3> deadBodyPositions
    {
        get { return ReplayData.CanReplayCheckPlayerView ? GetValueVector3("_deadBodyPositions") : _deadBodyPositions; }
        set { if (ReplayData.CanReplayCheckPlayerView) SetValueVector3("_deadBodyPositions", value); else _deadBodyPositions = value; }
    }
    private List<Vector3> _deadBodyPositions;
    public static int mode;

    public static Sprite GetSoulSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Soul.png", 500f);

    public static void Clear()
    {
        players = new();
        mode = ModeHandler.IsMode(ModeId.SuperHostRoles) ? 1 : SeerMode.GetSelection();
    }

    // RoleClass End

    /*========== ShowFlash Start ==========*/

    private static SpriteRenderer FullScreenRenderer;
    private static HudManager Renderer;
    public static void ShowFlash_ClearAndReload()
    {
        FullScreenRenderer = GameObject.Instantiate(FastDestroyableSingleton<HudManager>.Instance.FullScreen, FastDestroyableSingleton<HudManager>.Instance.transform);
        Renderer = FastDestroyableSingleton<HudManager>.Instance;
    }

    /** <summary>
        画面を光らせる
        </summary>
        <param name="color">
        (new Color("r値" / 255f, "g値" / 255f, "b値" / 255f))
        あるいはUnityのcolorコード指定で色を選択
        </param>
        <param name="duration">
        color色に画面を光らせはじめ、終わるまでの時間(duration/2秒時に指定色に光る)
        </param>
    **/
    public static void ShowFlash(Color color, float duration = 1f)
    {
        if (Renderer == null || FullScreenRenderer == null) return;
        FullScreenRenderer.gameObject.SetActive(true);
        FullScreenRenderer.enabled = true;
        Renderer.StartCoroutine(Effects.Lerp(duration, new Action<float>((p) =>
        {
            if (p < 0.5)
            {
                if (FullScreenRenderer != null)
                {
                    FullScreenRenderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(p * 2 * 0.75f));
                }
            }
            else
            {
                if (FullScreenRenderer != null)
                {
                    FullScreenRenderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01((1 - p) * 2 * 0.75f));
                }
            }
            if (p == 1f && FullScreenRenderer != null)
            {
                FullScreenRenderer.enabled = true;
                FullScreenRenderer.gameObject.SetActive(false);
                Logger.Info("発動待機状態に戻しました。", "SetActive(false)");
            }
        })));
    }

    /*========== ShowFlash End ==========*/

    /*========== Seers inherent ability Start ==========*/

    public static class MurderPlayerPatch
    {
        public static void Postfix([HarmonyArgument(0)] PlayerControl target)
        {
            var role = PlayerControl.LocalPlayer.GetRole();
            if (role is RoleId.Seer or RoleId.MadSeer or RoleId.EvilSeer or RoleId.SeerFriends or RoleId.JackalSeer or RoleId.SidekickSeer)
            {
                bool ModeFlag = false;
                switch (role)
                {
                    case RoleId.Seer:
                        if (local.deadBodyPositions != null) local.deadBodyPositions.Add(target.transform.position);
                        ModeFlag = mode <= 1;
                        break;
                    case RoleId.MadSeer:
                        if (MadSeer.local.deadBodyPositions != null) MadSeer.local.deadBodyPositions.Add(target.transform.position);
                        ModeFlag = MadSeer.mode <= 1;
                        break;
                    case RoleId.EvilSeer:
                        if (EvilSeer.local.deadBodyPositions != null) EvilSeer.local.deadBodyPositions.Add(target.transform.position);
                        ModeFlag = EvilSeer.mode <= 1;
                        break;
                    case RoleId.SeerFriends:
                        if (SeerFriends.local.deadBodyPositions != null) SeerFriends.local.deadBodyPositions.Add(target.transform.position);
                        ModeFlag = SeerFriends.mode <= 1;
                        break;
                    case RoleId.JackalSeer:
                    case RoleId.SidekickSeer:
                        if (RoleClass.JackalSeer.deadBodyPositions != null) RoleClass.JackalSeer.deadBodyPositions.Add(target.transform.position);
                        ModeFlag = RoleClass.JackalSeer.mode <= 1;
                        break;
                }
                if (PlayerControl.LocalPlayer.IsAlive() && CachedPlayer.LocalPlayer.PlayerId != target.PlayerId && ModeFlag)
                {
                    ShowFlash(new Color(42f / 255f, 187f / 255f, 245f / 255f));
                }
            }
        }
        public static void ShowFlash_SHR(PlayerControl target)
        {
            List<List<PlayerControl>> seers = new() {
                    allPlayers,
                    MadSeer.allPlayers,
                    EvilSeer.allPlayers,
                    SeerFriends.allPlayers,
                    RoleClass.JackalSeer.JackalSeerPlayer,
                };
            foreach (var p in seers)
            {
                if (p == null) continue;
                foreach (var p2 in p)
                {
                    if (p2 == null) continue;
                    if (!p2.IsMod())
                    {
                        p2.ShowReactorFlash(1.5f);
                        Logger.Info($"非導入者で尚且つ[ {p2.GetRole()} ]である{p2.GetDefaultName()}に死の点滅を発生させました。", "MurderPlayer");
                    }
                }
            }
        }
    }

    /*========== Seers inherent ability End ==========*/
}