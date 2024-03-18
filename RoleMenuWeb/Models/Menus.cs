using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RoleMenuWeb.Models
{
    public class Menus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid? ParentMenuId { get; set; }

        [ForeignKey("NavigationMenu")]
        public Guid? NavigationMenuId { get; set; }
        public virtual NavigationMenu NavigationMenu { get; set; }
        public string ControllerName { get; set; }

        public string ActionName { get; set; }

        [NotMapped]
        public bool Permitted { get; set; }
    }
}
