//*********************************************************************************************
//* File             :   Reflection.cs
//* Author           :   Rout, Sukanta 
//* Date             :   2/1/2024
//* Description      :   Initial version
//* Version          :   1.0
//*-------------------------------------------------------------------------------------------
//* dd-MMM-yyyy	: Version 1.x, Changed By : xxx
//*
//*                 - 1)
//*                 - 2)
//*                 - 3)
//*                 - 4)
//*
//*********************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Sukanta.Reflection
{
    /// <summary>
    /// Helper functions that make refelection handling easier
    /// </summary>
    public static class Reflections
    {
        private static MethodInfo methodDefinition = typeof(ArrayConverter).GetMethod("ParseToArray", BindingFlags.NonPublic | BindingFlags.Static);
        private static MethodInfo methodDefinitionArray = typeof(ArrayConverter).GetMethod("ParseArrayToArray", BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// Get location of the entry assembly
        /// </summary>
        /// <returns></returns>
        public static DirectoryInfo GetEntryAssemblyLocation()
        {
            return new FileInfo(Assembly.GetEntryAssembly().Location).Directory;
        }

        /// <summary>
        /// Get location of the entry assembly
        /// </summary>
        /// <returns></returns>
        public static DirectoryInfo GetCurrentAssemblyLocation()
        {
            return new FileInfo(Assembly.GetCallingAssembly().Location).Directory;
        }

        /// <summary>
        /// Gets the entry assemblies comment xml file name
        /// </summary>
        /// <returns></returns>
        public static string GetApplicationCommentFile()
        {
            return $"{Assembly.GetEntryAssembly().GetName().Name}.xml";
        }

        /// <summary>
        /// Get application version string
        /// </summary>
        /// <returns></returns>
        public static string GetAppVersion()
        {
            try
            {
                Assembly assembly = Assembly.GetEntryAssembly();
                Version version = assembly.GetName().Version;
                return $"{version.Major}.{version.Minor}";
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Get AppVersion based on name
        /// </summary>
        /// <param name="binaryPath"></param>
        /// <returns></returns>
        public static string GetAppVersion(string binaryPath)
        {
            try
            {
                Assembly assembly = LoadPhysicalFile(binaryPath);
                Version version = assembly.GetName().Version;
                return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Get the file info with root relative path arguments
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static FileInfo GetRootRelativeFile(params string[] path)
        {
            List<string> args = new List<string>
      {
        GetEntryAssemblyLocation().FullName
      };
            args.AddRange(path);
            return new FileInfo(GetFullPath(args.ToArray()));
        }

        /// <summary>
        /// Get combined full path
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static string GetFullPath(params string[] args)
        {
            return Path.GetFullPath(Path.Combine(args));
        }

        /// <summary>
        /// Get the file info with root relative path arguments
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static FileInfo GetAssemblyRelativeFile(params string[] path)
        {
            List<string> args = new List<string>
      {
        GetCurrentAssemblyLocation().FullName
      };
            args.AddRange(path);
            return new FileInfo(GetFullPath(args.ToArray()));
        }

        /// <summary>
        /// Get root relative dir
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static DirectoryInfo GetRootRelativeDir(params string[] path)
        {
            List<string> args = new List<string>
      {
        GetEntryAssemblyLocation().FullName
      };
            args.AddRange(path);
            return new DirectoryInfo(GetFullPath(args.ToArray()));
        }

        /// <summary>
        /// Get public concreate implementations of the specified type defined in the assembly
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetImplementation<T>(this Assembly assembly)
        {
            return GetImplementation(assembly, typeof(T));
        }

        public static IEnumerable<Type> GetImplementation(this Assembly assembly, Type type)
        {
            return assembly.GetTypes().Where(t => t.IsPublic && !t.IsSealed && !t.IsAbstract && !t.IsInterface && type.IsAssignableFrom(t));
        }

        /// <summary>
        /// Load assembly file from physical location
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Assembly LoadPhysicalFile(string path)
        {
#pragma warning disable S3885
            return Assembly.LoadFrom(path);
#pragma warning restore S3885
        }

        /// <summary>
        /// Create type from specified assembly
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assembly"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static T CreateInstance<T>(this Assembly assembly, string typeName, params object[] args)
        {
            Type type = assembly.GetType(typeName);

            if (type == null)
            {
                throw new InvalidOperationException($"Type {typeName} not found in assemby {assembly.FullName}");
            }

            return (T)Activator.CreateInstance(type, args);
        }

        /// <summary>
        /// Create type from assembly file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assemblyPath"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static T CreateInstanceFromFile<T>(string assemblyPath, string typeName)
        {
            Assembly assem = LoadPhysicalFile(assemblyPath);
            return CreateInstance<T>(assem, typeName);
        }

        /// <summary>
        /// Get properies with specified attributes
        /// </summary>
        /// <returns></returns>
        public static List<Tuple<PropertyInfo, Attribute>> GetPropertyAttributes<T>(this Type probeType) where T : Attribute
        {
            List<Tuple<PropertyInfo, Attribute>> configProperties = new List<Tuple<PropertyInfo, Attribute>>();

            PropertyInfo[] allProperties = probeType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in allProperties)
            {
                T configAttribute = property.GetCustomAttribute<T>();

                if (configAttribute != null)
                {
                    configProperties.Add(new Tuple<PropertyInfo, Attribute>(property, configAttribute));
                }
            }

            return configProperties;
        }

        /// <summary>
        /// Set value by reflection
        /// </summary>
        /// <param name="inputObject"></param>
        /// <param name="propertyInfo"></param>
        /// <param name="propertyVal"></param>
        /// <returns></returns>
        public static bool ReflectionSetValue(this object inputObject, PropertyInfo propertyInfo, object propertyVal)
        {
            //find the property type
            Type propertyType = propertyInfo.PropertyType;

            //Convert.ChangeType does not handle conversion to nullable types
            //if the property type is nullable, we need to get the underlying type of the property
            Type targetType = IsNullableType(propertyType) ? Nullable.GetUnderlyingType(propertyType) : propertyType;

            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                //if it's null, just set the value from the reserved word null, and return
                if (propertyVal == null)
                {
                    propertyInfo.SetValue(inputObject, null, null);
                    return false;
                }
            }
            else if (targetType.IsEnum)
            {
                HandleEnum(targetType, ref propertyVal);
            }
            else if (propertyType.IsArray && propertyVal != null)
            {
                HandleArray(propertyInfo, ref propertyVal, propertyType, inputObject);
            }
            else
            {
                //Returns an System.Object with the specified System.Type and whose value is
                //equivalent to the specified object.
                propertyVal = Convert.ChangeType(propertyVal, targetType);
            }

            object currentVal = propertyInfo.GetValue(inputObject);

            if (currentVal == null && propertyVal == null)
            {
                return false;
            }

            if ((currentVal == null) || (propertyVal == null) || !currentVal.Equals(propertyVal))
            {
                //Set the value of the property
                propertyInfo.SetValue(inputObject, propertyVal, null);
                return true;
            }

            return false;

        }

        /// <summary>
        /// Check if value has changed from proevois value
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool HasChanged(this PropertyInfo propertyInfo, object prevValue, object currentValue)
        {
            return prevValue.Equals(currentValue);
        }


        /// <summary>
        /// Handle array type
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="propertyVal"></param>
        /// <param name="propertyType"></param>
        /// <param name="inputObject"></param>
        private static void HandleArray(PropertyInfo propertyInfo, ref object propertyVal, Type propertyType, object inputObject)
        {
            string stringProp = propertyVal as string;

            if (stringProp != null)
            {
                object arrayValue = ((string)propertyVal).ToMemberArray(propertyType.GetElementType());
                propertyInfo.SetValue(inputObject, arrayValue, null);
            }
            else
            {
                object array = ((Array)propertyVal).ToMemberArray(propertyType.GetElementType());
                propertyInfo.SetValue(propertyVal, array, null);
            }
        }

        /// <summary>
        /// Handle Enumeration
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="propertyVal"></param>
        private static void HandleEnum(Type targetType, ref object propertyVal)
        {
            Type enumType = Enum.GetUnderlyingType(targetType);

            bool isParsed = false;

            try
            {
                propertyVal = Enum.Parse(targetType, propertyVal.ToString());
                isParsed = true;
            }
            catch
            {
                isParsed = false;
                //Ignore parse error
            }

            if (!isParsed)
            {
                object convertEnumValue = Convert.ChangeType(propertyVal, enumType);

                if (Enum.IsDefined(targetType, convertEnumValue))
                {
                    propertyVal = Enum.Parse(targetType, propertyVal.ToString());
                }
            }
        }


        /// <summary>
        /// Coverts a comma separated array to arry of type T
        /// </summary>
        /// <param name="arrayText"></param>
        /// <param name="membertype"></param>
        /// <returns></returns>
        public static object ToMemberArray(this string arrayText, Type membertype)
        {
            MethodInfo method = methodDefinition.MakeGenericMethod(membertype);
            return method.Invoke(null, new object[] { arrayText });
        }

        /// <summary>
        /// Coverts a comma separated array to arry of type T
        /// </summary>
        /// <param name="arrayText"></param>
        /// <param name="membertype"></param>
        /// <returns></returns>
        public static object ToMemberArray(this Array array, Type membertype)
        {
            MethodInfo method = methodDefinitionArray.MakeGenericMethod(membertype);
            return method.Invoke(null, new object[] { array });
        }

        /// <summary>
        /// Check if type is nullable
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }


        /// <summary>
        /// Get das attributes
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static Dictionary<T, PropertyInfo> GetAttribProperties<T>(this object element) where T : Attribute
        {
            Dictionary<T, PropertyInfo> propItems = new Dictionary<T, PropertyInfo>();

            IEnumerable<PropertyInfo> props = element.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(T)));

            foreach (PropertyInfo prop in props)
            {
                T configAttribute = prop.GetCustomAttribute<T>(true);

                if (configAttribute != null)
                {
                    propItems.Add(configAttribute, prop);
                }

            }

            return propItems;
        }


    }
}
