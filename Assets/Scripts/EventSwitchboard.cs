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
        public List<CordCircuit> CordCircuits
            = new List<CordCircuit>();
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
        public void ConnectCords(RegistryModule<GameObject> registry)
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

                UnityEvent SourceEvent
                    = RetrieveUnityEvent(
                        CordCircuit.SourceUnityEventName,
                        SourceScript
                      );

                UnityAction OnSwitchTriggered
                    = CreateRearCord(CordCircuit.RearJackType,
                                     CordCircuit.RearJackName);

                SourceEvent.AddListener(OnSwitchTriggered);

                Debug.Log("Connecting Source "
                         + CordCircuit.SourceObjectRegistryKey
                         + "\t" + CordCircuit.SourceMonoBehaviourTypeName
                         + "\t" + CordCircuit.SourceUnityEventName
                         + "\nTo target: "
                         + CordCircuit.RearJackName);
            }
        }

        private static UnityEvent RetrieveUnityEvent(string eventName, Component eventHostScript)
        {
            Type HostScriptType = eventHostScript.GetType();
            FieldInfo UnityEventInfo
                = HostScriptType.GetField(eventName);
            Debug.Assert(UnityEventInfo != null,
                         "Source script "
                         + HostScriptType.ToString()
                         + " must contain a Unity event named "
                         + eventName);
            //Debug.Log("Fields available on the target object: "
            //        + PrintEvents(SourceScriptType));
            System.Object UnityEventUncasted
                = UnityEventInfo.GetValue(eventHostScript);
            
            Debug.Assert(UnityEventUncasted != null);
            Debug.Assert(UnityEventUncasted.GetType()
                          == typeof(UnityEvent));

            return (UnityEvent)UnityEventUncasted;
        }

        private UnityAction
        CreateRearCord
        (JackType rearJackType, string rearJackName)
        {
            switch (rearJackType)
            {
                case JackType.Event:
                    return CreateUnityEventDelegate(rearJackName);
                case JackType.Method:
                    return CreateMethodDelegate(rearJackName);
                default:
                    throw new UnexpectedEnumValueException<JackType>(rearJackType);
            }
        }

        private UnityAction
        CreateUnityEventDelegate
        (string eventName)
        {
            MyContract.RequireFieldNotNull(Target, "Target");
            UnityEvent EventToTrigger
                = RetrieveUnityEvent(eventName, Target);
            
            return delegate ()
            {
                EventToTrigger.Invoke();
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
        /// I use UnityEvents for both source and destination events
        /// because fields are much easier to manage through reflection
        /// than pure c# events
        public class CordCircuit
        {
            public int SourceObjectRegistryKey;
            public string SourceMonoBehaviourTypeName;
            public string SourceUnityEventName;
            public JackType RearJackType;
            public string RearJackName;
        }
    }
}
