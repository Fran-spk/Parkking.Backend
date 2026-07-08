using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Reflection;
namespace Parkking.Models.Enums
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());

            var attribute = field?
                .GetCustomAttribute<DescriptionAttribute>();

            return attribute?.Description ?? value.ToString();
        }
    }
}
