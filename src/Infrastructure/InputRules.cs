using System.Globalization;
using ElectronicsShop.Models;

namespace ElectronicsShop.Infrastructure
{
    public class InputRules
    {
        public const decimal MaxPrice = 9999999999.99m;

        public static bool TryDecimal(string? text, int places, out decimal value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text))
                return false;

            string number = text.Trim().Replace(',', '.');
            bool result = decimal.TryParse(number,
                NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture, out value);

            if (!result || decimal.Round(value, places) != value)
                return false;

            return true;
        }

        public static bool TryPriceRange(string from, string to,
            out decimal? minimum, out decimal? maximum, out string error)
        {
            minimum = null;
            maximum = null;
            error = "";

            if (!string.IsNullOrWhiteSpace(from))
            {
                decimal number;
                if (!TryDecimal(from, 2, out number) || number < 0 || number > MaxPrice)
                {
                    error = "Цена от: число от 0 до 9 999 999 999,99, до 2 знаков после запятой";
                    return false;
                }
                minimum = number;
            }

            if (!string.IsNullOrWhiteSpace(to))
            {
                decimal number;
                if (!TryDecimal(to, 2, out number) || number < 0 || number > MaxPrice)
                {
                    error = "Цена до: число от 0 до 9 999 999 999,99, до 2 знаков после запятой";
                    return false;
                }
                maximum = number;
            }

            if (minimum.HasValue && maximum.HasValue && minimum.Value > maximum.Value)
            {
                error = "Цена от не должна быть больше цены до";
                return false;
            }

            return true;
        }

        public static bool Matches(Product product, string search, int categoryId,
            int brandId, decimal? minimum, decimal? maximum)
        {
            if (!product.Name.Contains(search.Trim(), StringComparison.CurrentCultureIgnoreCase))
                return false;

            if (categoryId != 0 && product.CategoryId != categoryId)
                return false;

            if (brandId != 0 && product.BrandId != brandId)
                return false;

            if (minimum.HasValue && product.Price < minimum.Value)
                return false;

            if (maximum.HasValue && product.Price > maximum.Value)
                return false;

            return true;
        }

        public static void CheckName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100)
                throw new ArgumentException("Введите название от 1 до 100 символов");
        }

        public static void CheckProduct(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name) || product.Name.Trim().Length > 200)
                throw new ArgumentException("Введите название товара, не более 200 символов");

            if (string.IsNullOrWhiteSpace(product.Description) || product.Description.Trim().Length > 2000)
                throw new ArgumentException("Введите описание, не более 2000 символов");

            if (product.Price < 0 || product.Price > MaxPrice || decimal.Round(product.Price, 2) != product.Price)
                throw new ArgumentException("Цена должна быть от 0 до 9 999 999 999,99, до 2 знаков после запятой");

            if (product.Stock < 0)
                throw new ArgumentException("Остаток не может быть отрицательным");

            if (product.Rating < 0 || product.Rating > 5 || decimal.Round(product.Rating, 1) != product.Rating)
                throw new ArgumentException("Рейтинг должен быть от 0 до 5, до 1 знака после запятой");

            if (product.CategoryId <= 0 || product.BrandId <= 0)
                throw new ArgumentException("Выберите категорию и бренд");
        }
    }
}
