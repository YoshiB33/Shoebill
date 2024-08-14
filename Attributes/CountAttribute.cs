using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace Shoebill.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
    AllowMultiple = false)]
public class CountAttribute(char item, int maximumCount) : ValidationAttribute(() => "The field {0} must be a string with a maximum item count of {1}.")
{
    public char Item { get; } = item;
    public int MaximumCount { get; } = maximumCount;
    public int MinimumCount { get; }

    public override bool IsValid(object? value)
    {
        EnsureLegalCounts();

        if (value == null)
        {
            return true;
        }

        int count = ((string)value).Count(c => c == Item);
        return count >= MinimumCount && count <= MaximumCount;
    }

    public override string FormatErrorMessage(string name)
    {
        EnsureLegalCounts();


        bool useErrorMessageWithMinimum = MinimumCount != 0 && ErrorMessage != null && ErrorMessage != string.Empty;
        string errorMessage = useErrorMessageWithMinimum
            ? "The field {0} must be a string with a minimum item count of {2} and a maximum item count of {1}."
            : ErrorMessageString;

        return string.Format(CultureInfo.CurrentCulture, errorMessage, name, MaximumCount, MinimumCount);
    }

    private void EnsureLegalCounts()
    {
        if (MaximumCount < 0)
        {
            throw new InvalidOperationException("The maximum count is less than 0");
        }

        if (MaximumCount < MinimumCount)
        {
            throw new InvalidOperationException("The minimum count is greater than the maximum count");
        }
    }
}
