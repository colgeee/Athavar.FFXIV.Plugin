﻿// <copyright file="JsonParseException.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Exceptions;

public class JsonParseException : AthavarPluginException
{
    public JsonParseException(string message)
        : base(message)
    {
    }
}