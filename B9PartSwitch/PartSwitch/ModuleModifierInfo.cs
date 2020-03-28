using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using B9PartSwitch.Fishbones;
using B9PartSwitch.Fishbones.Context;
using B9PartSwitch.Fishbones.Parsers;
using B9PartSwitch.PartSwitch.PartModifiers;

namespace B9PartSwitch
{
    public class ModuleModifierInfo : IContextualNode
    {
        public sealed class CannotParseFieldException : Exception
        {
            private CannotParseFieldException(string message) : base(message) { }
            private CannotParseFieldException(string message, Exception innerException): base(message, innerException) { }

            public static CannotParseFieldException CannotFindParser(string fieldName, Type fieldType)
            {
                fieldName.ThrowIfNullArgument(nameof(fieldName));
                fieldType.ThrowIfNullArgument(nameof(fieldType));

                string message = $"Could not find a suitable way to parse type {fieldType.Name} for field {fieldName}";
                return new CannotParseFieldException(message);
            }

            public static CannotParseFieldException ExceptionWhileParsing(string fieldName, Type fieldType, Exception innerException)
            {
                fieldName.ThrowIfNullArgument(nameof(fieldName));
                fieldType.ThrowIfNullArgument(nameof(fieldType));
                innerException.ThrowIfNullArgument(nameof(innerException));

                string message = $"Exception while parsing type {fieldType.Name} for field {fieldName}";
                return new CannotParseFieldException(message, innerException);
            }
        }

        public static readonly ReadOnlyCollection<Type> INVALID_MODULES_FOR_DATA_LOADING = new ReadOnlyCollection<Type>(new Type[] {
            typeof(ModulePartVariants),
            typeof(ModuleB9PartSwitch),
            typeof(ModuleB9PartInfo),
            typeof(ModuleB9DisableTransform),
        });

        public static readonly ReadOnlyCollection<string> INVALID_MODULES_NAMES_FOR_DATA_LOADING = new ReadOnlyCollection<string>(new string[] {
            "FSfuelSwitch",
            "FSmeshSwitch",
            "FStextureSwitch",
            "FStextureSwitch2",
            "InterstellarFuelSwitch",
            "InterstellarMeshSwitch",
            "InterstellarTextureSwitch",
        });

        public static readonly ReadOnlyCollection<Type> INVALID_MODULES_FOR_DISABLING = new ReadOnlyCollection<Type>(new Type[] {
            typeof(ModulePartVariants),
            typeof(ModuleB9PartSwitch),
            typeof(ModuleB9PartInfo),
            typeof(ModuleB9DisableTransform),
        });

        public static readonly ReadOnlyCollection<string> INVALID_MODULES_NAMES_FOR_DISABLING = new ReadOnlyCollection<string>(new string[] {
            "FSfuelSwitch",
            "FSmeshSwitch",
            "FStextureSwitch",
            "FStextureSwitch2",
            "InterstellarFuelSwitch",
            "InterstellarMeshSwitch",
            "InterstellarTextureSwitch",
        });

        [NodeData(name = "IDENTIFIER")]
        public ConfigNode identifierNode;

        [NodeData]
        public bool moduleActive = true;

        [NodeData(name = "DATA")]
        public ConfigNode dataNode;

        public void Load(ConfigNode node, OperationContext context) => this.LoadFields(node, context);
        public void Save(ConfigNode node, OperationContext context) => this.SaveFields(node, context);

        public IEnumerable<IPartModifier> CreatePartModifiers(Part part, PartModule parentModule, BaseEventDetails moduleDataChangedEventDetails)
        {
            part.ThrowIfNullArgument(nameof(part));
            parentModule.ThrowIfNullArgument(nameof(parentModule));
            moduleDataChangedEventDetails.ThrowIfNullArgument(nameof(moduleDataChangedEventDetails));

            if (identifierNode.IsNull()) throw new Exception("module modifier must have an IDENTIFIER node");

            if (!(identifierNode.GetValue("name") is string moduleName))
                throw new Exception("IDENTIFIER node does not have a name value");

            moduleName = moduleName.Trim();

            if (moduleName == string.Empty) throw new Exception ("IDENTIFIER node has a blank name");

            PartModule module = FindModule(part, parentModule, moduleName);

            if (dataNode.IsNotNull())
            {
                if (!(module.part.partInfo is AvailablePart partInfo)) throw new InvalidOperationException($"partInfo is null on part {part.name}");
                if (!(partInfo.partConfig is ConfigNode partConfig)) throw new InvalidOperationException($"partInfo.partConfig is null on part {partInfo.name}");

                ConfigNode originalNode = FindPrefabNode(module, moduleName);

                if (INVALID_MODULES_FOR_DATA_LOADING.Any(type => module.GetType().Implements(type)))
                    throw new InvalidOperationException($"Cannot modify data on {module.GetType()}");
                else if (INVALID_MODULES_NAMES_FOR_DATA_LOADING.Any(moduleTypeName => module.GetType().Name == moduleTypeName))
                    throw new InvalidOperationException($"Cannot modify data on {module.GetType()}");
                else if (module is ModuleEnginesFX moduleEnginesFX)
                {
                    yield return new ModuleDataHandlerBasic(module, originalNode, dataNode, moduleDataChangedEventDetails);
                    if (dataNode.GetValue("flameoutEffectName") is string flameoutEffectName)
                        yield return new EffectDeactivator(part, moduleEnginesFX.flameoutEffectName, flameoutEffectName);
                    if (dataNode.GetValue("runningEffectName") is string runningEffectName)
                        yield return new EffectDeactivator(part, moduleEnginesFX.runningEffectName, runningEffectName);
                    if (dataNode.GetValue("powerEffectName") is string powerEffectName)
                        yield return new EffectDeactivator(part, moduleEnginesFX.powerEffectName, powerEffectName);
                    if (dataNode.GetValue("engageEffectName") is string engageEffectName)
                        yield return new EffectDeactivator(part, moduleEnginesFX.engageEffectName, engageEffectName);
                    if (dataNode.GetValue("disengageEffectName") is string disengageEffectName)
                        yield return new EffectDeactivator(part, moduleEnginesFX.disengageEffectName, disengageEffectName);
                    if (dataNode.GetValue("directThrottleEffectName") is string directThrottleEffectName)
                        yield return new EffectDeactivator(part, moduleEnginesFX.directThrottleEffectName, directThrottleEffectName);
                    if (dataNode.GetValue("spoolEffectName") is string spoolEffectName)
                        yield return new EffectDeactivator(part, moduleEnginesFX.spoolEffectName, spoolEffectName);
                }
                else if (module is ModuleDeployableSolarPanel && dataNode.HasValue("chargeRate"))
                {
                    // if output resources are already initialized ModuleDeployableSolarPanel won't do it again
                    // therefore, wipe all output resources before loading new data
                    // order matters here since the output resources must be empty when new data is loaded
                    yield return new ModuleOutputResourceResetter(module);
                    yield return new ModuleDataHandlerBasic(module, originalNode, dataNode, moduleDataChangedEventDetails);
                }
                else
                    yield return new ModuleDataHandlerBasic(module, originalNode, dataNode, moduleDataChangedEventDetails);
            }

            if (!moduleActive)
            {
                if (INVALID_MODULES_FOR_DISABLING.Any(type => module.GetType().Implements(type)))
                    throw new InvalidOperationException($"Cannot disable {module.GetType()}");
                else if (INVALID_MODULES_NAMES_FOR_DISABLING.Any(moduleTypeName => module.GetType().Name == moduleTypeName))
                    throw new InvalidOperationException($"Cannot disable {module.GetType()}");

                yield return new ModuleDeactivator(module, parentModule);
            }
        }

        private PartModule FindModule(Part part, PartModule parentModule, string moduleName)
        {
            PartModule matchedModule = null;

            foreach (PartModule module in part.Modules)
            {
                if (module == parentModule) continue;

                if (!IsMatch(module, moduleName)) continue;

                if (matchedModule.IsNotNull()) throw new Exception("Found more than one matching module");

                matchedModule = module;
            }

            if (matchedModule.IsNull()) throw new Exception("Could not find matching module");

            return matchedModule;
        }

        private ConfigNode FindPrefabNode(PartModule module, string moduleName)
        {
            if (!(module.part.partInfo is AvailablePart partInfo)) throw new InvalidOperationException($"partInfo is null on part {module.part.name}");
            if (!(partInfo.partConfig is ConfigNode partConfig)) throw new InvalidOperationException($"partInfo.partConfig is null on part {partInfo.name}");

            ConfigNode matchedNode = null;

            foreach (ConfigNode subNode in partConfig.nodes)
            {
                if (subNode.name != "MODULE") continue;

                if (!NodeMatchesModule(module, moduleName, subNode)) continue;

                if (matchedNode.IsNotNull()) throw new Exception("Found more than one matching module node");

                matchedNode = subNode;
            }

            if (matchedNode.IsNull()) throw new Exception("Could not find matching module node");

            return matchedNode;
        }

        private bool IsMatch(PartModule module, string moduleName)
        {
            if (module.GetType().Name != moduleName) return false;

            foreach (ConfigNode.Value value in identifierNode.values)
            {
                if (value.name == "name") continue;

                if (module.Fields[value.name] is BaseField baseField)
                {
                    IValueParser parser;
                    object parsedValue;

                    try
                    {
                        parser = DefaultValueParseMap.Instance.GetParser(baseField.FieldInfo.FieldType);
                    }
                    catch (ParseTypeNotRegisteredException)
                    {
                        throw CannotParseFieldException.CannotFindParser(baseField.name, baseField.FieldInfo.FieldType);
                    }

                    try
                    {
                        parsedValue = parser.Parse(value.value);
                    }
                    catch (Exception ex)
                    {
                        throw CannotParseFieldException.ExceptionWhileParsing(baseField.name, baseField.FieldInfo.FieldType, ex);
                    }

                    if (!object.Equals(parsedValue, baseField.GetValue(module))) return false;
                }
                else if (module is CustomPartModule cpm && value.name == "moduleID")
                {
                    if (cpm.moduleID != value.value) return false;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private bool NodeMatchesModule(PartModule module, string moduleName, ConfigNode node)
        {
            if (node.GetValue("name") != moduleName) return false;

            foreach (ConfigNode.Value value in identifierNode.values)
            {
                if (value.name == "name") continue;

                if (!(node.GetValue(value.name) is string testValue)) return false;

                if (module.Fields[value.name] is BaseField baseField)
                {
                    object parsedValue, nodeValue;

                    try
                    {
                        IValueParser parser = DefaultValueParseMap.Instance.GetParser(baseField.FieldInfo.FieldType);
                        parsedValue = parser.Parse(value.value);
                        nodeValue = parser.Parse(testValue);
                    }
                    catch (ParseTypeNotRegisteredException)
                    {
                        throw CannotParseFieldException.CannotFindParser(baseField.name, baseField.FieldInfo.FieldType);
                    }
                    catch (Exception ex)
                    {
                        throw CannotParseFieldException.ExceptionWhileParsing(baseField.name, baseField.FieldInfo.FieldType, ex);
                    }

                    if (!object.Equals(parsedValue, nodeValue)) return false;
                }
                else if (module is CustomPartModule && value.name == "moduleID")
                {
                    if (node.GetValue("moduleID") != value.value) return false;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}
