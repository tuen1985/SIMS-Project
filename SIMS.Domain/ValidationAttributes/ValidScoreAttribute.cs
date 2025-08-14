using System;
using System.ComponentModel.DataAnnotations;
using SIMS.Domain;

namespace SIMS.Domain.ValidationAttributes
{
    public class ValidScoreAttribute : ValidationAttribute
    {
        private readonly float _minScore;
        private readonly float _maxScore;

        public ValidScoreAttribute(float minScore, float maxScore)
        {
            _minScore = minScore;
            _maxScore = maxScore;
            ErrorMessage = $"The score must be between {_minScore} and {_maxScore}.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is float score)
            {
                if (score < _minScore || score > _maxScore)
                {
                    return new ValidationResult(ErrorMessage);
                }
            }

            return ValidationResult.Success;
        }
    }
}