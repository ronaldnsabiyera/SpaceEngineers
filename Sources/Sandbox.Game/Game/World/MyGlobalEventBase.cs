﻿using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Serializer;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using VRage;
using VRage.Utils;

namespace Sandbox.Game.World
{

    public class MyGlobalEventBase : IComparable
    {
        public bool IsOneTime
        {
            get
            {
                return !Definition.MinActivationTime.HasValue;
            }
        }
        public bool IsPeriodic
        {
            get
            {
                return !IsOneTime;
            }
        }
        public bool IsInPast
        {
            get
            {
                return ActivationTime.Ticks <= 0;
            }
        }
        public bool IsInFuture
        {
            get
            {
                return ActivationTime.Ticks > 0;
            }
        }
        public bool IsHandlerValid
        {
            get
            {
                return Action != null;
            }
        }

        public MyGlobalEventDefinition Definition { private set; get; }
        public GlobalEventHandler Action { private set; get; }
        public TimeSpan ActivationTime { private set; get; }
        public bool Enabled { get; set; }

        public virtual void InitFromDefinition(MyGlobalEventDefinition definition)
        {
            Definition = definition;
            Action = MyGlobalEventFactory.GetEventHandler(Definition.Id);
            if (Definition.FirstActivationTime.HasValue)
            {
                ActivationTime = Definition.FirstActivationTime.Value;
            }
            else
            {
                RecalculateActivationTime();
            }
            Enabled = true;
        }

        public virtual void Init(MyObjectBuilder_GlobalEventBase ob)
        {
            Definition = MyDefinitionManager.Static.GetEventDefinition(ob.DefinitionId);
            Action = MyGlobalEventFactory.GetEventHandler(Definition.Id);
            ActivationTime = TimeSpan.FromMilliseconds(ob.ActivationTimeMs);
            Enabled = ob.Enabled;
        }

        public virtual MyObjectBuilder_GlobalEventBase GetObjectBuilder()
        {
            var ob = Sandbox.Common.ObjectBuilders.Serializer.MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_GlobalEventBase>();
            ob.DefinitionId = Definition.Id;
            ob.ActivationTimeMs = ActivationTime.Ticks / TimeSpan.TicksPerMillisecond;
            ob.Enabled = Enabled;
            return ob;
        }

        public void RecalculateActivationTime()
        {
            if (Definition.MinActivationTime == Definition.MaxActivationTime)
            {
                ActivationTime = Definition.MinActivationTime.Value;
            }
            else
            {
                ActivationTime = MyUtils.GetRandomTimeSpan(Definition.MinActivationTime.Value, Definition.MaxActivationTime.Value);
            }
            MySandboxGame.Log.WriteLine("MyGlobalEvent.RecalculateActivationTime:");
            MySandboxGame.Log.WriteLine("Next activation in " + ActivationTime.ToString());
        }

        public void SetActivationTime(TimeSpan time)
        {
            ActivationTime = time;
        }

        public int CompareTo(object obj)
        {
            if (!(obj is MyGlobalEventBase))
            {
                Debug.Fail("Comparing a global event to a different class");
                return 0;
            }

            TimeSpan result = ActivationTime - (obj as MyGlobalEventBase).ActivationTime;
            if (result.Ticks == 0)
            {
                // Use RuntimeHelpers instead of Object.GetHashCode() to ensure that compiler always uses Object.GetHashCode
                return RuntimeHelpers.GetHashCode(this) - RuntimeHelpers.GetHashCode(obj);
            }
            else
            {
                return result.Ticks < 0 ? -1 : 1;
            }
        }
    }
}
