using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicsShop.Models
{
    public partial class Product
    {
        [NotMapped]
        public bool IsLowStock
        {
            get { return Stock < 10; }
        }

        [NotMapped]
        public string TagsText
        {
            get
            {
                var names = new List<string>();
                foreach (var tag in Tags.OrderBy(t => t.Name))
                    names.Add("#" + tag.Name);

                return string.Join(" ", names);
            }
        }
    }
}
