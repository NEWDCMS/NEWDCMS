using FluentValidation.Validators;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using FluentValidation;

namespace DCMS.Web.Framework.Validators
{

    public class DecimalPropertyValidator<T, TProperty> : PropertyValidator<T, TProperty>
    {
        private readonly decimal _maxValue;

        public override string Name => "DecimalPropertyValidator";

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="maxValue">Maximum value</param>
        public DecimalPropertyValidator(decimal maxValue)
        {
            _maxValue = maxValue;
        }

        /// <summary>
        /// Is valid?
        /// </summary>
        /// <param name="context">Validation context</param>
        /// <returns>Result</returns>
        public override bool IsValid(ValidationContext<T> context, TProperty value)
        {
            if (decimal.TryParse(value.ToString(), out var propertyValue))
                return Math.Round(propertyValue, 3) < _maxValue;

            return false;
        }

        protected override string GetDefaultMessageTemplate(string errorCode) => "Decimal value is out of range";
    }

    //public class DecimalPropertyValidator : PropertyValidator
    //{
    //    private readonly decimal _maxValue;

    //    /// <summary>
    //    /// Ctor
    //    /// </summary>
    //    /// <param name="maxValue">Maximum value</param>
    //    public DecimalPropertyValidator(decimal maxValue) :
    //        base("Decimal value is out of range")
    //    {
    //        _maxValue = maxValue;
    //    }

    //    /// <summary>
    //    /// Is valid?
    //    /// </summary>
    //    /// <param name="context">Validation context</param>
    //    /// <returns>Result</returns>
    //    protected override bool IsValid(PropertyValidatorContext context)
    //    {
    //        if (decimal.TryParse(context.PropertyValue.ToString(), out decimal value))
    //        {
    //            return Math.Round(value, 3) < _maxValue;
    //        }

    //        return false;
    //    }
    //}
}