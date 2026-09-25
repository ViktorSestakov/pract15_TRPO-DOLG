namespace ElectronicsShop.Services
{
    public class AuthService
    {
        public static bool IsManager { get; private set; }

        public static bool Login(string pin)
        {
            IsManager = pin == "1234";
            return IsManager;
        }

        public static void EnterVisitor()
        {
            IsManager = false;
        }

        public static void CheckManager()
        {
            if (!IsManager)
                throw new UnauthorizedAccessException("Изменять данные может только менеджер");
        }
    }
}
