// Copyright 2016 Renaud Paquay All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace mtsuite.CoreFileSystem.Utils {
  public static class ReflectionUtils {
    private static MemberInfo GetMemberInfoImpl(Type type, LambdaExpression lambda) {
      var member = lambda.Body as MemberExpression;
      if (member == null)
        throw new ArgumentException(string.Format(
          "Expression '{0}' refers to a method, not a property.",
          lambda));

      var memberInfo = member.Member;

      if (type != null) {
        if (type != memberInfo.ReflectedType &&
          !type.IsSubclassOf(memberInfo.ReflectedType))
          throw new ArgumentException(
            string.Format(
              "Expresion '{0}' refers to a property that is not from type {1}.",
              lambda,
              type));
      }

      return memberInfo;
    }

    private static PropertyInfo GetPropertyInfoImpl(Type type, LambdaExpression propertyLambda) {
      var memberInfo = GetMemberInfoImpl(type, propertyLambda);

      var propInfo = memberInfo as PropertyInfo;
      if (propInfo == null)
        throw new ArgumentException(string.Format(
          "Expression '{0}' refers to a field, not a property.",
          propertyLambda));

      return propInfo;
    }

    private static FieldInfo GetFieldInfoImpl(Type type, LambdaExpression propertyLambda) {
      var memberInfo = GetMemberInfoImpl(type, propertyLambda);

      var propInfo = memberInfo as FieldInfo;
      if (propInfo == null)
        throw new ArgumentException(string.Format(
          "Expression '{0}' refers to a propertty, not a field.",
          propertyLambda));

      return propInfo;
    }

    /// <summary>
    /// Return the <see cref="PropertyInfo"/> for an instance property.
    /// </summary>
    public static PropertyInfo GetPropertyInfo<TSource, TProperty>(
      TSource source,
      Expression<Func<TSource, TProperty>> propertyLambda) {
      Type type = typeof(TSource);
      return GetPropertyInfoImpl(type, propertyLambda);
    }

    /// <summary>
    /// Return the <see cref="FieldInfo"/> for a static property.
    /// </summary>
    public static FieldInfo GetFieldInfo<TProperty>(
      Expression<Func<TProperty>> propertyLambda) {
      return GetFieldInfoImpl(null, propertyLambda);
    }

    /// <summary>
    /// Return the <see cref="FieldInfo"/> for an instance property.
    /// </summary>
    public static FieldInfo GetFieldInfo<TSource, TProperty>(
      TSource source,
      Expression<Func<TSource, TProperty>> propertyLambda) {
      Type type = typeof(TSource);
      return GetFieldInfoImpl(type, propertyLambda);
    }

    /// <summary>
    /// Return the <see cref="PropertyInfo"/> for a static property.
    /// </summary>
    public static PropertyInfo GetPropertyInfo<TProperty>(
      Expression<Func<TProperty>> propertyLambda) {
      return GetPropertyInfoImpl(null, propertyLambda);
    }

    /// <summary>
    /// Return the <see cref="MemberInfo"/> for an instance member.
    /// </summary>
    public static MemberInfo GetMemberInfo<TSource, TMember>(
      TSource source,
      Expression<Func<TSource, TMember>> propertyLambda) {
      Type type = typeof(TSource);
      return GetMemberInfoImpl(type, propertyLambda);
    }

    /// <summary>
    /// Return the <see cref="MemberInfo"/> for a static member.
    /// </summary>
    public static MemberInfo GetMemberInfo<TMember>(
      Expression<Func<TMember>> lambda) {
      return GetMemberInfoImpl(null, lambda);
    }

    /// <summary>
    /// Return the property name for an instance property.
    /// </summary>
    public static string GetPropertyName<TSource, TProperty>(
      TSource source,
      Expression<Func<TSource, TProperty>> propertyLambda) {
      return GetPropertyInfoImpl(typeof(TSource), propertyLambda).Name;
    }

    /// <summary>
    /// Return the property name for an static property.
    /// </summary>
    public static string GetPropertyName<TProperty>(
      Expression<Func<TProperty>> propertyLambda) {
      return GetPropertyInfoImpl(null, propertyLambda).Name;
    }

    /// <summary>
    /// Return the property name for an instance property.
    /// </summary>
    public static string GetFieldName<TSource, TProperty>(
      TSource source,
      Expression<Func<TSource, TProperty>> propertyLambda) {
      return GetFieldInfoImpl(typeof(TSource), propertyLambda).Name;
    }

    /// <summary>
    /// Return the property name for an static property.
    /// </summary>
    public static string GetFieldName<TProperty>(
      Expression<Func<TProperty>> propertyLambda) {
        return GetFieldInfoImpl(null, propertyLambda).Name;
    }
  }
}