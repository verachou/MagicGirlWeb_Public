// Copy from: https://stackoverflow.com/a/59094019

using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Http;


namespace MagicGirlWeb.Models.DataAnnotaions
{
  /// <summary>
  /// Validation attribute to assert an <see cref="IFormFile">IFormFile</see> property, field or parameter does not exceed a maximum size.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
  public sealed class FileSizeAttribute : ValidationAttribute
  {
    /// <summary>
    /// Gets the maximum acceptable size of the file.
    /// </summary>
    public long MaximumSize { get; private set; }

    /// <summary>
    /// Gets or sets the minimum acceptable size of the file.
    /// </summary>
    public int MinimumSize { get; set; }

    /// <summary>
    /// Constructor that accepts the maximum size of the file.
    /// </summary>
    /// <param name="maximumSize">The maximum size, inclusive.  It may not be negative.</param>
    public FileSizeAttribute(int maximumSize)
    {
      MaximumSize = maximumSize;
    }

    /// <summary>
    /// Override of <see cref="ValidationAttribute.IsValid(object)"/>
    /// </summary>
    /// <remarks>
    /// This method returns <c>true</c> if the <paramref name="value"/> is null.  
    /// It is assumed the <see cref="RequiredAttribute"/> is used if the value may not be null.
    /// </remarks>
    /// <param name="value">The value to test.</param>
    /// <returns><c>true</c> if the value is null or it's size is less than or equal to the set maximum size</returns>
    /// <exception cref="InvalidOperationException"> is thrown if the current attribute is ill-formed.</exception>
    public override bool IsValid(object value)
    {
      // Check the lengths for legality
      EnsureLegalSizes();

      // Automatically pass if value is null. RequiredAttribute should be used to assert a value is not null.
      // We expect a cast exception if the passed value was not an IFormFile.
      var length = value == null ? 0 : ((IFormFile)value).Length;

      return value == null || (length >= MinimumSize && length <= MaximumSize);
    }

    /// <summary>
    /// Override of <see cref="ValidationAttribute.FormatErrorMessage"/>
    /// </summary>
    /// <param name="name">The name to include in the formatted string</param>
    /// <returns>A localized string to describe the maximum acceptable size</returns>
    /// <exception cref="InvalidOperationException"> is thrown if the current attribute is ill-formed.</exception>
    
    // public override string FormatErrorMessage(string name)
    // {
    //   EnsureLegalSizes();

    //   string errorMessage = MinimumSize != 0 ? Resources.FileSizeAttribute_ValidationErrorIncludingMinimum : ErrorMessageString;

    //   // it's ok to pass in the minLength even for the error message without a {2} param since String.Format will just ignore extra arguments
    //   return string.Format(CultureInfo.CurrentCulture, errorMessage, name, _maximumSize, MinimumSize);
    // }

    /// <summary>
    /// Checks that MinimumSize and MaximumSize have legal values.  Throws InvalidOperationException if not.
    /// </summary>
    private void EnsureLegalSizes()
    {
      if (MaximumSize < 0)
      {
        //throw new InvalidOperationException(Resources.FileSizeAttribute_InvalidMaxSize);
        throw new InvalidOperationException(string.Format("MaximumSize {0} is lower than 0", MaximumSize));
      }

      if (MaximumSize < MinimumSize)
      {
        //throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.RangeAttribute_MinGreaterThanMax, MaximumSize, MinimumSize));
        throw new InvalidOperationException(string.Format("MaximumSize {0} is lower than MinimumSize {1}", MaximumSize, MinimumSize));
      }
    }
  }
}