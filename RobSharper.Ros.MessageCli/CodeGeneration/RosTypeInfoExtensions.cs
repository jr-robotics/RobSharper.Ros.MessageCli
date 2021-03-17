using System;
using RobSharper.Ros.MessageParser;

namespace RobSharper.Ros.MessageCli.CodeGeneration
{
    public static class RosTypeInfoExtensions
    {
        public static bool SupportsEqualityComparer(this RosTypeInfo type)
        {
            if (type.IsArray || !type.IsBuiltInType)
                return false;

            try
            {
                var typeMapper = BuiltInTypeMapping.Create(type);

                return typeMapper.Type == typeof(string) ||
                       (typeMapper.Type != typeof(double) && typeMapper.Type != typeof(float));
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }

        public static bool IsValueType(this RosTypeInfo type)
        {
            if (type.IsArray || !type.IsBuiltInType)
                return false;

            try
            {
                var typeMapper = BuiltInTypeMapping.Create(type);

                return typeMapper.Type.IsValueType;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }

        public static bool IsType(this RosTypeInfo rosType, Type expectedType)
        {
            if (rosType.IsArray || !rosType.IsBuiltInType)
                return false;

            try
            {
                var typeMapper = BuiltInTypeMapping.Create(rosType);
                return typeMapper.Type == expectedType;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }

        public static bool IsType<TExpected>(this RosTypeInfo rosType)
        {
            return IsType(rosType, typeof(TExpected));
        }
    }
}