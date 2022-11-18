// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace StackExchange.Redis.Extensions.System.Text.Json.NET7;

/// <summary>
///     Allows a client to customize the serialization.
/// </summary>
public interface ICacheSerializationContext
{
    /// <summary>
    ///     Returns the serialization context mapping
    /// </summary>
    /// <returns></returns>
    Dictionary<Type, JsonSerializerContext> GetContexts();
}