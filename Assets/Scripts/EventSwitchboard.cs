using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace SpaceBattles
{
    //  I'm using the nomenclature for telephone switchboards
    //  Hopefully this page is still up: https://en.wikipedia.org/wiki/Telephone_switchboard#operation
    //  Basically the jacks were the sources and destinations (i.e. telephone sets) of a call 
    //   and the cords represented the caller from the source (front cord),
    //   and the callee from the target (rear cord)
    //  Togther the two cords formed a 'cord circuit'

    public class
    EventSwitchboard : MonoBehaviour
    {
        // -- Fields --
        // This monobehaviour is expected to be mounted directly
        // to the same gameobject as the target
        public MonoBehaviour Target;
        public List<CordCircuit> CordCircuits;
        public Type RegistryKeyType;

        // -- Delegates --

        // -- Events --

        // -- Enums --
        /// <summary>
        /// The type of the "jack" (either event or method),
        /// rather than the type of the 'target'
        /// (which is the classed object containing the "jack")
        /// </summary>
        public enum JackType { Event, Method };

        // -- Properties --

        // -- Methods --

        /// <summary>
        /// Actually links up the events and/or methods with one another
        /// </summary>
        public void ConnectCords(GameObjectRegistryModule registry)
        {
            MyContract.RequireFieldNotNull(Target, "Target");
            Type TargetType = Target.GetType();

            foreach (CordCircuit CordCircuit in CordCircuits)
            {
                // the parameter "provider" is ignored, but is still mandatory
                // what the fuck
                int RegistryKey = CordCircuit.SourceObjectRegistryKey;
                GameObject SourceObject = registry[RegistryKey];
                MyContract.RequireArgumentNotNull(
                    SourceObject,
                    PrintKey(RegistryKey, RegistryKeyType)
                      + " in the provided registry"
                );
                Component SourceScript
                    = SourceObject
                    .GetComponent(CordCircuit.SourceMonoBehaviourTypeName);
                MyContract.RequireFieldNotNull(SourceScript, "Source Script");

                Type SourceScriptType
                    = SourceScript.GetType();
                EventInfo SourceUnityEventInfo
                    = SourceScriptType.GetEvent(CordCircuit.SourceEventName);
                Debug.Assert(SourceUnityEventInfo != null,
                             "Source script "
                             + CordCircuit.SourceMonoBehaviourTypeName
                             + " must contain an event named "
                             + CordCircuit.SourceEventName);
                Debug.Log("Fields available on the target object: "
                        + PrintEvents(SourceScriptType));

                UnityAction OnSwitchTriggered
                    = CreateRearCord(CordCircuit.RearJackType,
                                     CordCircuit.RearJackName);

                SourceUnityEventInfo.AddEventHandler(SourceScript, OnSwitchTriggered);
            }
        }

        private UnityAction
        CreateRearCord
        (JackType rearJackType, string rearJackName)
        {
            switch (rearJackType)
            {
                case JackType.Event:
                    return CreateEventDelegate(rearJackName);
                case JackType.Method:
                    return CreateMethodDelegate(rearJackName);
                default:
                    throw new UnexpectedEnumValueException<JackType>(rearJackType);
            }
        }

        private UnityAction
        CreateEventDelegate
        (string eventName)
        {
            Type TargetType = Target.GetType();
            MyContract.RequireArgumentNotNull(TargetType, "targetType");
            MyContract.RequireFieldNotNull(Target, "Target");
            EventInfo TargetEventInfo
                    = TargetType.GetEvent(eventName);
            MyContract.RequireArgument(
                TargetEventInfo != null,
                 "Is a member of the Target object",
                 "Unity event " + eventName
            );
            MethodInfo RaiseEventMethod = TargetEventInfo.GetRaiseMethod();
            //Target.

            return delegate ()
            {
                Debug.Assert(Target != null, "Target is null");
                RaiseEventMethod.Invoke(Target, null);
            };
        }

        private UnityAction
        CreateMethodDelegate
        (string methodName)
        {
            Type TargetType = Target.GetType();
            MyContract.RequireArgumentNotNull(TargetType, "targetType");
            MyContract.RequireFieldNotNull(Target, "Target");
            MethodInfo TargetMethodInfo
                    = TargetType.GetMethod(methodName);
            MyContract.RequireArgument(
                TargetMethodInfo != null,
                 "Is a method of the Target object",
                 "Method " + methodName
            );

            return delegate ()
            {
                TargetMethodInfo.Invoke(Target, null);
            };
        }

        private string PrintKey(int key, Type KeyEnumType)
        {
            if (KeyEnumType != null && KeyEnumType.IsEnum)
            {
                return Enum.GetName(KeyEnumType, key);
            }
            else
            {
                return key.ToString();
            }
        }

        private string PrintFields (Type type)
        {
            IEnumerable<String> Fields
                = type.GetFields().Select(fi => fi.ToString());
            string returnstring = "(";
            foreach (string Field in Fields)
            {
                returnstring += "[";
                returnstring += Field;
                returnstring += "],";
            }
            returnstring += ")";
            return returnstring;
        }

        private string PrintEvents(Type type)
        {
            IEnumerable<String> Events
                = type.GetEvents().Select(ei => ei.ToString());
            string returnstring = "(";
            foreach (string EventName in Events)
            {
                returnstring += "[";
                returnstring += EventName;
                returnstring += "],";
            }
            returnstring += ")";
            return returnstring;
        }


        // -- Classes --

        [Serializable]
        public class CordCircuit
        {
            public int SourceObjectRegistryKey;
            public string SourceMonoBehaviourTypeName;
            public string SourceEventName;
            public JackType RearJackType;
            public string RearJackName;
        }
    }
}
