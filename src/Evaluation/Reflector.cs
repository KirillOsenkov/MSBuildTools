using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

public static class Reflector
{
    public static object GetStaticFieldValue(this Type type, string fieldName)
    {
        FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        if (fieldInfo == null)
        {
            throw new FieldAccessException("Field " + fieldName + " was not found on type " + type.ToString());
        }

        object result = fieldInfo.GetValue(null);
        return result;
    }

    public static object GetStaticPropertyValue(this Type type, string propertyName)
    {
        PropertyInfo propertyInfo = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (propertyInfo == null)
        {
            throw new ArgumentException("Property " + propertyName + " was not found on type " + type.ToString());
        }

        object result = propertyInfo.GetValue(null, null);
        return result;
    }

    public static void SetStaticPropertyValue<TType, TProperty>(string propertyName, TProperty propertyValue)
    {
        var propertyInfo = typeof(TType).GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (propertyInfo == null)
        {
            throw new ArgumentException("Property " + propertyName + " was not found on type " + typeof(TType).ToString());
        }

        propertyInfo.SetValue(null, propertyValue);
    }

    public static void SetStaticPropertyValue<T>(this Type type, string propertyName, T propertyValue)
    {
        var propertyInfo = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (propertyInfo == null)
        {
            throw new ArgumentException("Property " + propertyName + " was not found on type " + type.ToString());
        }

        propertyInfo.SetValue(null, propertyValue);
    }

    public static void SetStaticFieldValue(this Type type, string fieldName, object value)
    {
        FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        if (fieldInfo == null)
        {
            throw new FieldAccessException("Field " + fieldName + " was not found on type " + type.ToString());
        }

        fieldInfo.SetValue(null, value);
    }

    public static IEnumerable<FieldInfo> GetAllFields(this Type type, bool includeInstance = true)
    {
        return type.GetMembers(includeInstance, (t, b) => t.GetFields(b));
    }

    public static IEnumerable<PropertyInfo> GetAllProperties(this Type type, bool includeInstance = true)
    {
        return type.GetMembers(includeInstance, (t, b) => t.GetProperties(b));
    }

    public static IEnumerable<EventInfo> GetAllEvents(this Type type, bool includeInstance = true)
    {
        return type.GetMembers(includeInstance, (t, b) => t.GetEvents(b));
    }

    public static IReadOnlyList<T> GetMembers<T>(this Type type, bool includeInstance, Func<Type, BindingFlags, IReadOnlyList<T>> getter)
        where T : MemberInfo
    {
        var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        if (includeInstance)
        {
            bindingFlags |= BindingFlags.Instance;
        }

        var list = new List<T>();
        var names = new HashSet<string>();

        Type current = type;
        while (current != null)
        {
            var members = getter(current, bindingFlags);
            foreach (var member in members)
            {
                if (names.Add(member.Name))
                {
                    list.Add(member);
                }
            }

            current = current.BaseType;
        }

        return list.ToArray();
    }

    public static FieldType GetStaticFieldValue<FieldType>(this Type type, string fieldName)
    {
        FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        object result = fieldInfo.GetValue(null);
        return (FieldType)result;
    }

    public static object TryGetFieldValue(this object instance, string fieldName)
    {
        FieldInfo fieldInfo = GetField(instance, fieldName);
        if (fieldInfo == null)
        {
            return null;
        }

        object result = fieldInfo.GetValue(instance);
        return result;
    }

    public static object GetFieldValue(this object instance, string fieldName)
    {
        FieldInfo fieldInfo = GetField(instance, fieldName);
        object result = fieldInfo.GetValue(instance);
        return result;
    }

    public static object GetPropertyOrFieldValue(this object instance, string memberName, object missingValue = null)
    {
        var fieldInfo = GetField(instance, memberName);
        if (fieldInfo != null)
        {
            return fieldInfo.GetValue(instance);
        }

        return GetPropertyValue(instance, memberName, throwOnMissing: false, missingValue: missingValue);
    }

    public static FieldType GetFieldValue<FieldType>(this object instance, string fieldName)
    {
        return (FieldType)GetFieldValue(instance, fieldName);
    }

    public static void SetFieldValue(this object instance, string fieldName, object value, Type instanceType = null)
    {
        var field = instance.GetField(fieldName, instanceType);
        if (field == null)
        {
            throw new FieldAccessException("Field " + fieldName + " was not found on object " + Convert.ToString(instance));
        }

        field.SetValue(instance, value);
    }

    public static FieldInfo GetField(this object instance, string fieldName, Type instanceType = null)
    {
        var type = instanceType ?? instance.GetType();
        FieldInfo fieldInfo = null;
        while (type != null)
        {
            fieldInfo = type.GetField(
                fieldName,
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.Static);
            if (fieldInfo != null)
            {
                break;
            }

            type = type.BaseType;
        }

        return fieldInfo;
    }

    public static object GetPropertyValue(this object instance, string propertyName, bool throwOnMissing = true, object missingValue = null)
    {
        Type type = instance.GetType();
        PropertyInfo propertyInfo = type.GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        if (propertyInfo == null)
        {
            if (throwOnMissing)
            {
                throw new ArgumentException("Property " + propertyName + " was not found on type " + type.ToString());
            }
            else
            {
                return missingValue;
            }
        }

        object result = propertyInfo?.GetValue(instance, null);
        return result;
    }

    public static void SetPropertyValue<T>(this object instance, string propertyName, T value)
    {
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        Type type = instance.GetType();
        PropertyInfo propertyInfo = type.GetProperty(propertyName, flags);
        if (propertyInfo == null)
        {
            throw new ArgumentException("Property " + propertyName + " was not found on type " + type.ToString());
        }

        // Workaround for Reflection bug 791391
        if (propertyInfo.DeclaringType != type)
        {
            type = propertyInfo.DeclaringType;
            propertyInfo = type.GetProperty(propertyName, flags);
        }

        propertyInfo.SetValue(instance, value, flags, null, null, null);
    }

    public static PropertyType GetPropertyValue<PropertyType>(this object instance, string propertyName)
    {
        return (PropertyType)GetPropertyValue(instance, propertyName);
    }

    public static object InvokeMethod(this object instance, string methodName, params object[] arguments)
    {
        return InvokeMethod(instance.GetType(), instance, methodName, arguments);
    }

    public static object InvokeMethod(Type type, object instance, string methodName, params object[] arguments)
    {
        MethodInfo methodInfo = null;
        BindingFlags bindingFlags =
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Static |
            BindingFlags.Instance;

        if (arguments == null)
        {
            arguments = Array.Empty<Type>();
        }

        methodInfo = type.GetMethod(
            methodName,
            bindingFlags,
            null,
            arguments.Select(a => a.GetType()).ToArray(),
            null);
        if (methodInfo == null)
        {
            methodInfo = type.GetMethod(
                methodName,
                bindingFlags);
        }

        object result = methodInfo.Invoke(instance, arguments);
        return result;
    }

    public static object InvokeStaticMethod(this Type type, string methodName)
    {
        return InvokeStaticMethod(type, methodName, null);
    }

    public static object InvokeStaticMethod(this Type type, string methodName, params object[] arguments)
    {
        var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        var methodInfo = type.GetMethods(bindingFlags)
            .Where(m => m.Name == methodName && m.GetParameters().Length == arguments.Length)
            .FirstOrDefault();
        if (methodInfo == null)
        {
            return null;
        }

        object result = methodInfo.Invoke(null, arguments);
        return result;
    }

    public static Type FindType(string typeName)
    {
        Type result = null;
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            result = FindType(assembly, typeName);
            if (result != null)
            {
                return result;
            }
        }

        return result;
    }

    public static Type FindType(Assembly assembly, string typeName)
    {
        var result = assembly.GetType(typeName);
        if (result == null)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.Name == typeName)
                {
                    result = type;
                }
            }
        }

        if (result == null)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.Name.Contains(typeName))
                {
                    result = type;
                }
            }
        }

        return result;
    }

    public static Type FindType(string assemblyName, string typeName)
    {
        Assembly assembly = FindAssembly(assemblyName);
        return FindType(assembly, typeName);
    }

    public static Assembly FindAssembly(string assemblyName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.GetName().Name == assemblyName)
            {
                return assembly;
            }
        }

        return null;
    }

    public static object FindObjectOfType(System.Collections.ObjectModel.ReadOnlyCollection<KeyValuePair<object, object>> readOnlyCollection, string typeNameSubstring)
    {
        foreach (var obj in readOnlyCollection)
        {
            var value = obj.Value;
            if (value.GetType().FullName.Contains(typeNameSubstring))
            {
                return value;
            }
        }

        return null;
    }

    public static object FindObjectOfType(IEnumerable objects, string typeNameSubstring)
    {
        foreach (var obj in objects)
        {
            if (obj.GetType().FullName.Contains(typeNameSubstring))
            {
                return obj;
            }
        }

        return null;
    }

    public static Func<T1, T2, TResult> CreateConstructorDelegate<T1, T2, TResult>()
    {
        var ctor = typeof(TResult).GetConstructor(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance,
            null,
            new[] { typeof(T1), typeof(T2) },
            null);
        return CreateConstructorDelegate<T1, T2, TResult>(ctor);
    }

    public static Func<T1, T2, TResult> CreateConstructorDelegate<T1, T2, TResult>(ConstructorInfo ctor)
    {
        var type = typeof(TResult);
        var parameterTypes = new[] { typeof(T1), typeof(T2) };

        var dynamicMethod = new DynamicMethod(
            "",
            typeof(TResult),
            parameterTypes,
            type);
        ILGenerator il = dynamicMethod.GetILGenerator();

        il.DeclareLocal(type);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Newobj, ctor);
        il.Emit(OpCodes.Ret);

        var func = (Func<T1, T2, TResult>)dynamicMethod.CreateDelegate(typeof(Func<T1, T2, TResult>));
        return func;
    }
}