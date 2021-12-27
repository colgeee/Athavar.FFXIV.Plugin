﻿// <copyright file="AddonSelectYesNoFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes.Features;

using System;
using System.Linq;
using System.Runtime.InteropServices;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Clicks;
using Athavar.FFXIV.Plugin.Module.Yes.BaseFeatures;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     AddonSelectYesNo feature.
/// </summary>
internal class AddonSelectYesNoFeature : OnSetupFeature
{
    private readonly YesModule module;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonSelectYesNoFeature" /> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule" />.</param>
    public AddonSelectYesNoFeature(YesModule module)
        : base(module.AddressResolver.AddonSelectYesNoOnSetupAddress, module) =>
        this.module = module;

    /// <inheritdoc />
    protected override string AddonName => "SelectYesNo";

    /// <inheritdoc />
    protected override unsafe void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
    {
        var dataPtr = (AddonSelectYesNoOnSetupData*)data;
        if (dataPtr == null)
        {
            return;
        }

        var text = this.module.LastSeenDialogText = this.module.GetSeStringText(dataPtr->TextPtr);
        PluginLog.Debug($"AddonSelectYesNo: text={text}");

        if (this.module.ForcedYesKeyPressed)
        {
            PluginLog.Debug("AddonSelectYesNo: Forced yes hotkey pressed");
            this.AddonSelectYesNoExecute(addon, true);
            return;
        }

        var zoneWarnOnce = true;
        var nodes = this.Configuration.GetAllNodes().OfType<TextEntryNode>();
        foreach (var node in nodes)
        {
            if (!node.Enabled || string.IsNullOrEmpty(node.Text))
            {
                continue;
            }

            if (!this.EntryMatchesText(node, text))
            {
                continue;
            }

            if (node.ZoneRestricted && !string.IsNullOrEmpty(node.ZoneText))
            {
                if (!this.module.TerritoryNames.TryGetValue(this.module.DalamudServices.ClientState.TerritoryType, out var zoneName))
                {
                    if (zoneWarnOnce && !(zoneWarnOnce = false))
                    {
                        PluginLog.Debug("Unable to verify Zone Restricted entry, ZoneID was not set yet");
                        this.module.ChatManager.PrintInformationMessage("Unable to verify Zone Restricted entry, change zones to update value");
                    }

                    zoneName = string.Empty;
                }

                if (!string.IsNullOrEmpty(zoneName) && this.EntryMatchesZoneName(node, zoneName))
                {
                    PluginLog.Debug($"AddonSelectYesNo: Matched on {node.Text} ({node.ZoneText})");
                    this.AddonSelectYesNoExecute(addon, node.IsYes);
                    return;
                }
            }
            else
            {
                PluginLog.Debug($"AddonSelectYesNo: Matched on {node.Text}");
                this.AddonSelectYesNoExecute(addon, node.IsYes);
                return;
            }
        }
    }

    private unsafe void AddonSelectYesNoExecute(IntPtr addon, bool yes)
    {
        if (yes)
        {
            var addonPtr = (AddonSelectYesno*)addon;
            var yesButton = addonPtr->YesButton;
            if (yesButton != null && !yesButton->IsEnabled)
            {
                PluginLog.Debug("AddonSelectYesNo: Enabling yes button");
                yesButton->AtkComponentBase.OwnerNode->AtkResNode.Flags ^= 1 << 5;
            }

            PluginLog.Debug("AddonSelectYesNo: Selecting yes");
            ClickSelectYesNo.Using(addon).Yes();
        }
        else
        {
            PluginLog.Debug("AddonSelectYesNo: Selecting no");
            ClickSelectYesNo.Using(addon).No();
        }
    }

    private bool EntryMatchesText(TextEntryNode node, string text) =>
        (node.IsTextRegex && (node.TextRegex?.IsMatch(text) ?? false)) ||
        (!node.IsTextRegex && text.Contains(node.Text));

    private bool EntryMatchesZoneName(TextEntryNode node, string zoneName) =>
        (node.ZoneIsRegex && (node.ZoneRegex?.IsMatch(zoneName) ?? false)) ||
        (!node.ZoneIsRegex && zoneName.Contains(node.ZoneText));

    [StructLayout(LayoutKind.Explicit, Size = 0x10)]
    private struct AddonSelectYesNoOnSetupData
    {
        [FieldOffset(0x8)]
        public readonly IntPtr TextPtr;
    }
}