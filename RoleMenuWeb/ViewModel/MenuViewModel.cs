namespace RoleMenuWeb.ViewModel
{
    public class MenuViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? ParentMenuId { get; set; }
        public Guid? NavigationMenuId { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public bool Permitted { get; set; }
    }
}
