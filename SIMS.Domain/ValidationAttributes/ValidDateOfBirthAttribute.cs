using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;

namespace SIMS.Domain.ValidationAttributes
{
    public class ValidDateOfBirthAttribute : ValidationAttribute
    {
        private readonly int _minAge;
        private readonly int _maxAge;

        public ValidDateOfBirthAttribute(int minAge, int maxAge)
        {
            _minAge = minAge;
            _maxAge = maxAge;
            ErrorMessage = $"Tuổi của sinh viên phải từ {_minAge} đến {_maxAge}.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime dateOfBirth)
            {
                int age = DateTime.Today.Year - dateOfBirth.Year;

                // Điều chỉnh lại tuổi nếu chưa đến sinh nhật trong năm nay
                if (dateOfBirth.Date > DateTime.Today.AddYears(-age))
                {
                    age--;
                }

                if (age < _minAge || age > _maxAge)
                {
                    return new ValidationResult(ErrorMessage);
                }
            }

            return ValidationResult.Success;
        }
    }
}