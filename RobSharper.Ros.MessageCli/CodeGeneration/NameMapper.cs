using System;
using System.Linq;
using RobSharper.Ros.MessageCli.CodeGeneration.Formatters;
using RobSharper.Ros.MessageCli.CodeGeneration.TemplateEngines;
using RobSharper.Ros.MessageParser;

namespace RobSharper.Ros.MessageCli.CodeGeneration
{
    public class NameMapper : INugetPackageNameResolver, IResourceNamingConvention, ITypeNameResolver, IBuiltInTypeChecker, IRosNamingConvention
    {
        private readonly string _packageName;
        private readonly ITemplateFormatter _packageNamingConvention;
        private readonly INameFormatter _fieldNameFormatter;
        private readonly INameFormatter _constantNameFormatter;

        public NameMapper(string packageName, ITemplateFormatter packageNamingConvention, 
            INameFormatter fieldNameFormatter = null, INameFormatter constantNameFormatter = null)
        {
            _packageName = packageName ?? throw new ArgumentNullException(nameof(packageName));
            _packageNamingConvention = packageNamingConvention ?? throw new ArgumentNullException(nameof(packageNamingConvention));
            _fieldNameFormatter = fieldNameFormatter ?? new AsIsFormatter();
            _constantNameFormatter = constantNameFormatter ?? new AsIsFormatter();
        }
         
        public virtual string ResolveNugetPackageName(string rosPackageName)
        {
            if (rosPackageName == null) throw new ArgumentNullException(nameof(rosPackageName));

            return GetNamespace(rosPackageName);
        }

        public virtual string ResolveNugetPackageName(RosTypeInfo rosType)
        {
            return ResolveNugetPackageName(rosType.PackageName);
        }

        public virtual string GetNamespace(string rosPackageName)
        {
            var data = new
            {
                Name = rosPackageName,
            };

            return _packageNamingConvention
                .Format(data)
                .Trim();
        }

        public string GetTypeName(string rosTypeName)
        {
            return GetTypeName(rosTypeName, DetailedRosMessageType.None);
        }

        public virtual string GetTypeName(string rosTypeName, DetailedRosMessageType messageType)
        {
            if (rosTypeName == null) throw new ArgumentNullException(nameof(rosTypeName));
            
            var typeName = rosTypeName.Trim();

            switch (messageType)
            {
                case DetailedRosMessageType.ActionGoal:
                    typeName += "Goal";
                    break;
                case DetailedRosMessageType.ActionResult:
                    typeName += "Result";
                    break;
                case DetailedRosMessageType.ActionFeedback:
                    typeName += "Feedback";
                    break;
                case DetailedRosMessageType.ServiceRequest:
                    typeName += "Request";
                    break;
                case DetailedRosMessageType.ServiceResponse:
                    typeName += "Response";
                    break;
            }

            if (typeName.Length > 0)
            {
                typeName = typeName.First().ToString().ToUpper() + typeName.Substring(1);
            }
            
            return typeName;
        }

        public virtual string GetFieldName(string rosIdentifier)
        {
            return _fieldNameFormatter.Format(rosIdentifier);
        }

        public virtual string GetConstantName(string rosIdentifier)
        {
            return  _constantNameFormatter.Format(rosIdentifier);
        }

        public string ResolveFullQualifiedTypeName(RosTypeInfo type)
        {
            return ResolveFullQualifiedTypeName(type, false);
        }
        
        public string ResolveFullQualifiedInterfaceName(RosTypeInfo type)
        {
            return ResolveFullQualifiedTypeName(type, true);
        }
        
        protected virtual string ResolveFullQualifiedTypeName(RosTypeInfo type, bool useInterface)
        {
            string typeString;

            if (type.IsBuiltInType)
            {
                var typeMapper = BuiltInTypeMapping.Create(type);
                typeString = typeMapper.Type.ToString();
            }
            else
            {
                var rosPackageName = type.PackageName ?? _packageName;
                var rosTypeName = type.TypeName;

                typeString = ResolveFullQualifiedTypeName(rosPackageName, rosTypeName);
            }

            if (type.IsArray)
            {
                if (useInterface)
                {
                    typeString = $"System.Collections.Generic.IList<{typeString}>";
                }
                else
                {
                    typeString = $"System.Collections.Generic.List<{typeString}>";
                }
            }

            return typeString;
        }

        protected virtual string ResolveFullQualifiedTypeName(string rosPackageName, string rosTypeName)
        {
            if (rosPackageName == null) throw new ArgumentNullException(nameof(rosPackageName));
            if (rosTypeName == null) throw new ArgumentNullException(nameof(rosTypeName));
             
            var namespaceName = GetNamespace(rosPackageName);
            var typeName = GetTypeName(rosTypeName);
            
            return $"{namespaceName}.{typeName}";
        }

        public virtual bool IsBuiltInType(RosTypeInfo rosType)
        {
            if (rosType == null) throw new ArgumentNullException(nameof(rosType));

            return rosType.IsBuiltInType;
        }

        public virtual string GetRosTypeName(string rosTypeName, DetailedRosMessageType messageType)
        {
            switch (messageType)
            {
                case DetailedRosMessageType.None:
                case DetailedRosMessageType.Message:
                case DetailedRosMessageType.Service:
                    return rosTypeName;
                case DetailedRosMessageType.ActionGoal:
                    return rosTypeName + "Goal";
                case DetailedRosMessageType.ActionResult:
                    return rosTypeName + "Result";
                case DetailedRosMessageType.ActionFeedback:
                    return rosTypeName + "Feedback";
                case DetailedRosMessageType.ServiceRequest:
                    return rosTypeName + "Request";
                case DetailedRosMessageType.ServiceResponse:
                    return rosTypeName + "Response";
                default:
                    throw new NotSupportedException($"Message type {messageType} is not supported.");
            }
        }
    }
}