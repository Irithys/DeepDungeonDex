﻿using System;
using Dalamud.Game.ClientState;
using Dalamud.Plugin;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Command;
using Dalamud.Game;

namespace DeepDungeonDex
{
    public class Plugin : IDalamudPlugin
    {
        private DalamudPluginInterface pluginInterface;
        private Configuration config;
        private PluginUI ui;
        private ConfigUI cui;
        private GameObject previousTarget;
        private ClientState _clientState;
        private Condition _condition;
        private TargetManager _targetManager;
        private Framework _framework;
        private CommandManager _commands;

        public string Name => "DeepDungeonDex-CN";

        public Plugin(
            DalamudPluginInterface pluginInterface,
            ClientState clientState,
            CommandManager commands,
            Condition condition,
            Framework framework,
            //SeStringManager seStringManager,
            TargetManager targets)
        {
            this.pluginInterface = pluginInterface;
            this._clientState = clientState;
            this._condition = condition;
            this._framework = framework;
            this._commands = commands;
            this._targetManager = targets;

            this.config = (Configuration)this.pluginInterface.GetPluginConfig() ?? new Configuration();
            this.config.Initialize(this.pluginInterface);
            this.ui = new PluginUI(config, clientState);
            this.cui = new ConfigUI(config.Opacity, config.IsClickthrough, config.HideRedVulns, config.HideBasedOnJob, config);
            this.pluginInterface.UiBuilder.Draw += this.ui.Draw;
            this.pluginInterface.UiBuilder.Draw += this.cui.Draw;

            this._commands.AddHandler("/pddd", new CommandInfo(OpenConfig)
            {
                HelpMessage = "DeepDungeonDex config"
            });

            this._framework.Update += this.GetData;
        }

        public void OpenConfig(string command, string args)
        {
            cui.IsVisible = true;
        }

        public void GetData(Framework framework)
        {
            if (!this._condition[ConditionFlag.InDeepDungeon])
            {
                ui.IsVisible = false;
                return;
            }
            GameObject target = _targetManager.Target;

            TargetData t = new TargetData();
            if (!t.IsValidTarget(target))
            {
                ui.IsVisible = false;
                return;
            }
            else
            { 
                previousTarget = target;
                ui.IsVisible = true;
            }
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            this._commands.RemoveHandler("/pddd");

            this.pluginInterface.SavePluginConfig(this.config);

            this.pluginInterface.UiBuilder.Draw -= this.ui.Draw;
            this.pluginInterface.UiBuilder.Draw -= this.cui.Draw;

            this._framework.Update -= this.GetData;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
