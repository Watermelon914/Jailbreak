﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Extensions;

public static class PlayerExtensions
{
    public static CsTeam GetTeam(this CCSPlayerController controller)
    {
        return (CsTeam)controller.TeamNum;
    }

    public static bool IsReal(this CCSPlayerController? player)
    {
        //  Do nothing else before this:
        //  Verifies the handle points to an entity within the global entity list.
        if (player == null)
            return false;
        if (!player.IsValid)
            return false;

        if (player.Connected != PlayerConnectedState.PlayerConnected)
            return false;

        if (player.IsBot || player.IsHLTV)
            return false;

        return true;
    }

    public static void Teleport(this CCSPlayerController player, CCSPlayerController target)
    {
        if (!player.IsReal() || !target.IsReal())
            return;

        var playerPawn = player.Pawn.Value;
        if (playerPawn == null)
            return;

        var targetPawn = target.Pawn.Value;
        if (targetPawn == null)
            return;

        if (targetPawn is { AbsRotation: not null, AbsOrigin: not null })
            Teleport(player, targetPawn.AbsOrigin, targetPawn.AbsRotation);
    }

    public static void Teleport(this CCSPlayerController player, Vector pos, QAngle? rot = null)
    {
        if (!player.IsReal())
            return;

        var playerPawn = player.Pawn.Value;
        if (playerPawn == null)
            return;

        playerPawn.Teleport(pos, rot ?? playerPawn.AbsRotation!, new Vector());
    }

    public static void Freeze(this CCSPlayerController player)
    {
        if (!player.Pawn.IsValid || player.Connected != PlayerConnectedState.PlayerConnected)
            return;

        if (player.Pawn.Value == null)
            return;

        player.Pawn.Value.Freeze();
    }

    public static void UnFreeze(this CCSPlayerController player)
    {
        if (!player.Pawn.IsValid || player.Connected != PlayerConnectedState.PlayerConnected)
            return;

        if (player.Pawn.Value == null)
            return;

        player.Pawn.Value.UnFreeze();
    }

    public static void Freeze(this CBasePlayerPawn pawn)
    {
        pawn.MoveType = MoveType_t.MOVETYPE_OBSOLETE;

        Schema.SetSchemaValue(pawn.Handle, "CBaseEntity", "m_nActualMoveType", 1);
        Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
    }

    public static void UnFreeze(this CBasePlayerPawn pawn)
    {
        pawn.MoveType = MoveType_t.MOVETYPE_WALK;

        Schema.SetSchemaValue(pawn.Handle, "CBaseEntity", "m_nActualMoveType", 2);
        Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
    }

    public static void SetHp(this CCSPlayerController controller, int health = 100)
    {
        if (health <= 0 || !controller.PawnIsAlive || controller.PlayerPawn.Value == null) return;

        controller.Health = health;
        controller.PlayerPawn.Value.Health = health;

        if (health > 100)
        {
            controller.MaxHealth = health;
            controller.PlayerPawn.Value.MaxHealth = health;
        }

        var weaponServices = controller.PlayerPawn.Value!.WeaponServices;
        if (weaponServices == null) return;

        controller.GiveNamedItem("weapon_healthshot");

        foreach (var weapon in weaponServices.MyWeapons)
            if (weapon.IsValid && weapon.Value!.DesignerName == "weapon_healthshot")
            {
                weapon.Value.Remove();
                break;
            }
    }
}