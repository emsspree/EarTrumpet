﻿using EarTrumpet.DataModel.Storage;
using EarTrumpet.Extensibility;
using EarTrumpet.Actions.DataModel;
using EarTrumpet.Actions.DataModel.Processing;
using EarTrumpet.Actions.DataModel.Serialization;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace EarTrumpet.Actions
{
    [Export(typeof(IAddonLifecycle))]
    public class Addon : IAddonLifecycle
    {
        public static Addon Current { get; private set; }
        public static string Namespace => "EarTrumpet.Actions";
        public LocalVariablesContainer LocalVariables { get; private set; }

        public EarTrumpetAction[] Actions
        {
            get => _actions;
            set
            {
                _settings.Set(c_actionsSettingKey, value);
                LoadAndRegister();
            }
        }

        public AddonInfo Info
        {
            get => new AddonInfo
            {
                DisplayName = Properties.Resources.MyActionsText,
                PublisherName = "File-New-Project",
                Id = "EarTrumpet.project-eta",
                HelpLink = "https://github.com/File-New-Project/EarTrumpet",
                AddonVersion = new System.Version(1, 0, 0, 0),
            };
        }

        private readonly string c_actionsSettingKey = "ActionsData";
        private EarTrumpetAction[] _actions = new EarTrumpetAction[] { };
        private ISettingsBag _settings = StorageFactory.GetSettings(Namespace);
        private TriggerManager _triggerManager = new TriggerManager();

        public void OnApplicationLifecycleEvent(ApplicationLifecycleEvent evt)
        {
            if (evt == ApplicationLifecycleEvent.Startup2)
            {
                Current = this;
                LocalVariables = new LocalVariablesContainer(_settings);

                _triggerManager.Triggered += OnTriggered;
                LoadAndRegister();

                _triggerManager.OnEvent(ApplicationLifecycleEvent.Startup);
            }
            else if (evt == ApplicationLifecycleEvent.Shutdown)
            {
                _triggerManager.OnEvent(ApplicationLifecycleEvent.Shutdown);
            }
        }

        private void LoadAndRegister()
        {
            _triggerManager.Clear();
            _actions = _settings.Get(c_actionsSettingKey, new EarTrumpetAction[] { });
            _actions.SelectMany(a => a.Triggers).ToList().ForEach(t => _triggerManager.Register(t));
        }

        public void Import(string fileName)
        {
            var imported = Serializer.FromString<EarTrumpetAction[]>(File.ReadAllText(fileName)).ToList();
            foreach(var imp in imported)
            {
                imp.Id = Guid.NewGuid();
            }
            imported.AddRange(Actions);
            Actions = imported.ToArray();
        }

        public string Export()
        {
            return _settings.Get(c_actionsSettingKey, "");
        }

        private void OnTriggered(BaseTrigger trigger)
        {
            var action = Actions.FirstOrDefault(a => a.Triggers.Contains(trigger));
            if (action != null && action.Conditions.All(c => ConditionProcessor.IsMet(c)))
            {
                TriggerAction(action);
            }
        }

        public void TriggerAction(EarTrumpetAction action)
        {
            action.Actions.ToList().ForEach(a => ActionProcessor.Invoke(a));
        }
    }
}