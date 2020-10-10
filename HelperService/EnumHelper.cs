using System;

namespace HelperService
{
    /// <summary>
    /// 枚举类型帮助方法
    /// </summary>
    public static class EnumHelper
    {  
        /// <summary>
        /// 枚举解析
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="enumString">枚举值名称</param>
        /// <param name="defaultValue">枚举默认值</param>
        /// <returns>枚举值</returns>
        public static T ParseEnum<T>(string enumString, T defaultValue) where T : struct
        {
            if (!typeof (T).IsEnum)
            {
                throw new Exception("T must be a enum type!");
            }

            T returnValue;
            if (!Enum.TryParse(enumString, true, out returnValue))
            {
                returnValue = defaultValue;
            }

            return returnValue;
        }
    }
}