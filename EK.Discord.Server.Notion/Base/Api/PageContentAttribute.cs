﻿using System.ComponentModel.DataAnnotations.Schema;

namespace EK.Discord.Server.Notion.Base.Api;

/// <summary>
///     Marks the property as containing the contents of the page
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class PageContentAttribute : NotMappedAttribute {

}