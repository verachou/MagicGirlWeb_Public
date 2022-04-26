// Copy from: https://stackoverflow.com/a/64519418 , https://stackoverflow.com/a/59094019

using System;
using System.IO;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace MagicGirlWeb.Models.DataAnnotaions
{
  /// <summary>
  /// Validation attribute to assert an <see cref="IFormFile">IFormFile</see> property, field or parameter has a specific extention.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
  public sealed class AllowFileExtensionsAttribute : ValidationAttribute
  {
    public string Extensions;

    public AllowFileExtensionsAttribute(string extensions)
    {
      Extensions = extensions;
    }

    /// <summary>
    /// Override of <see cref="ValidationAttribute.IsValid(object)"/>
    /// </summary>
    /// <remarks>
    /// This method returns <c>true</c> if the <paramref name="value"/> is null.  
    /// It is assumed the <see cref="RequiredAttribute"/> is used if the value may not be null.
    /// </remarks>
    /// <param name="value">The value to test.</param>
    /// <returns><c>true</c> if the value is null or it's extension is not invluded in the set extensionss</returns>
    private string ExtensionsNormalized
    {
      get
      {
        return Extensions.Replace(" ", "", StringComparison.Ordinal).ToUpperInvariant();
      }
    }

    public override bool IsValid(object value)
    {
      // Automatically pass if value is null. RequiredAttribute should be used to assert a value is not null.
      // We expect a cast exception if the passed value was not an IFormFile.
      if (value == null)
        return true;
      var extension = Path.GetExtension(((IFormFile)value).FileName).ToUpperInvariant().Replace(".", "");
      return value == null || ExtensionsNormalized.Contains(extension);
    }

    // public string GetErrorMessage()
    // {
    //   return $"Your image's filetype is not valid.";
    // }
  }
}